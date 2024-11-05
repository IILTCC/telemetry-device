using PacketDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks.Dataflow;
using telemetry_device.compactCollection;
using telemetry_device_main.decryptor;
using telemetry_device_main.icds;

namespace telemetry_device
{
    class PipeLine
    {
        private const string FILE_TYPE = ".json";
        private const string REPO_PATH = "../../../icd_repo/";
        private const int HEADER_SIZE = 25;
        private const string TIMESTAMP_FORMAT = "dd,MM,yyyy,HH,mm,ss,ffff";

        private readonly ActionBlock<Packet> _disposedPackets;
        private readonly BufferBlock<Packet> _pullerBlock;
        private readonly TransformBlock<Packet, ToDecryptPacketItem> _extractPacketData;
        private readonly TransformBlock<ToDecryptPacketItem, SendToKafkaItem> _decryptBlock;
        private readonly ActionBlock<SendToKafkaItem> _sendToKafka;

        private readonly ConcurrentDictionary<IcdTypes, dynamic> _icdDictionary;
        private readonly TelemetryDeviceSettings _telemetryDeviceSettings;
        private readonly KafkaConnection _kafkaConnection;
        private readonly TelemetryLogger _logger;
        private readonly StatisticsAnalyzer _statAnalyze;
        public PipeLine(TelemetryDeviceSettings telemetryDeviceSettings,KafkaConnection kafkaConnection)
        {
            _logger = TelemetryLogger.Instance;
            _statAnalyze = StatisticsAnalyzer.Instance;
            _telemetryDeviceSettings = telemetryDeviceSettings;
            _kafkaConnection = kafkaConnection;
            _icdDictionary = new ConcurrentDictionary<IcdTypes, dynamic>();

            // acts as clearing buffer endpoint for unwanted packets 100 acts as precentage for bad packed
            _disposedPackets = new ActionBlock<Packet>(DisposedPackets);
            _extractPacketData = new TransformBlock<Packet, ToDecryptPacketItem>(ExtractPacketData);
            _pullerBlock = new BufferBlock<Packet>();
            _decryptBlock = new TransformBlock<ToDecryptPacketItem, SendToKafkaItem>(ProccessPackets);
            _sendToKafka = new ActionBlock<SendToKafkaItem>(SendParamToKafka);

            ConfigurePipelineLinks();
            InitializeIcdDictionary();
            _logger.LogInfo("Succesfuly initalized all icds");
        }
        private void ConfigurePipelineLinks()
        {
            _pullerBlock.LinkTo(_extractPacketData, IsPacketAppropriate);
            _pullerBlock.LinkTo(_disposedPackets, (Packet packet) => { return !IsPacketAppropriate(packet); });
            _extractPacketData.LinkTo(_decryptBlock);
            _decryptBlock.LinkTo(_sendToKafka);
        }

        private void InitializeIcdDictionary()
        {
            (IcdTypes, Type)[] icdTypes = new (IcdTypes, Type)[4] {
                (IcdTypes.FiberBoxDownIcd,typeof(FiberBoxDownIcd)),
                (IcdTypes.FiberBoxUpIcd, typeof(FiberBoxUpIcd)),
                (IcdTypes.FlightBoxDownIcd, typeof(FlightBoxDownIcd)),
                (IcdTypes.FlightBoxUpIcd, typeof(FlightBoxUpIcd))};

            foreach ((IcdTypes, Type) icdInitialization in icdTypes)
            {
                string jsonText = File.ReadAllText(REPO_PATH + icdInitialization.Item1.ToString() + FILE_TYPE);
                Type genericIcdType = typeof(IcdPacketDecryptor<>).MakeGenericType(icdInitialization.Item2);
                _icdDictionary.TryAdd(icdInitialization.Item1, Activator.CreateInstance(genericIcdType, new object[] { jsonText }));
            }
        }

        public void PushToBuffer(Packet packet)
        {
            _pullerBlock.Post(packet);
        }

        // returns true if includes correct dest port and correct protocol
        private bool IsPacketAppropriate(Packet packet)
        {
            IPPacket ipPacket = packet.Extract<IPPacket>();
            if (ipPacket != null)
            {
                if (ipPacket.Protocol == ProtocolType.Udp)
                {
                    UdpPacket udpPacket = packet.Extract<UdpPacket>();
                    if (udpPacket.DestinationPort == _telemetryDeviceSettings.SimulatorDestPort)
                        return true;
                }
            }
            return false;
        }

        public void DisposedPackets(Packet packet)
        {
            const int BAD_PACKET_PRECENTAGE = 100;
            _statAnalyze.UpdateStatistic(GlobalStatisticType.PacketDropRate, BAD_PACKET_PRECENTAGE);
        }

        private ToDecryptPacketItem ExtractPacketData(Packet packet)
        {
            const int TYPE_SIZE = 1;
            const int TIMESTAMP_SIZE = 24;
            const int GOOD_PACKET_PRECENTAGE = 0;

            _statAnalyze.UpdateStatistic(GlobalStatisticType.PacketDropRate, GOOD_PACKET_PRECENTAGE);

            var udpPacket = packet.Extract<UdpPacket>();
            // remove header bytes
            byte[] packetData = new byte[udpPacket.PayloadData.Length - HEADER_SIZE];
            byte[] typeBytes = new byte[TYPE_SIZE];
            byte[] timestampBytes = new byte[TIMESTAMP_SIZE];
            List<byte[]> packetParams = new List<byte[]>() {typeBytes,timestampBytes,packetData};
            int packetOffset = 0;
            foreach(byte[] param in packetParams)
                for (int paramIndex = 0; paramIndex < param.Length; paramIndex++)
                    param[paramIndex] = udpPacket.PayloadData[packetOffset++];

            int type = typeBytes[0];
            string timestamp = Encoding.ASCII.GetString(timestampBytes);

            // the format in which the timestamp is in the packet
            DateTime dateTime = DateTime.ParseExact(timestamp, TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
            int sinffingTime = (int)DateTime.Now.Subtract(dateTime).TotalMilliseconds;
            _statAnalyze.UpdateStatistic(GlobalStatisticType.SniffingTime, sinffingTime);
            
            return new ToDecryptPacketItem((IcdTypes)type, packetData);
        }

        private SendToKafkaItem ProccessPackets(ToDecryptPacketItem transformItem)
        {
            try
            {
                DateTime beforeDecrypt = DateTime.Now;
                Dictionary<string, (int paramValue, bool wasErrorFound)> decryptedParamDict = _icdDictionary[transformItem.PacketType].DecryptPacket(transformItem.PacketData);
                int decryptTime = (int)DateTime.Now.Subtract(beforeDecrypt).TotalMilliseconds;

                _statAnalyze.UpdateStatistic(IcdStatisticType.DecryptTime, transformItem.PacketType, decryptTime);
                _statAnalyze.UpdateStatistic(IcdStatisticType.CorruptedPacket, transformItem.PacketType, CalcErrorCount(decryptedParamDict));
                return new SendToKafkaItem(transformItem.PacketType,decryptedParamDict);
            }
            catch (Exception ex)
            {
                _logger.LogError("Tried decrypt and send to kafka -"+ex.Message);
                return null;
            }
        }

        public int CalcErrorCount(Dictionary<string, (int paramValue, bool wasErrorFound)> errorDict)
        {
            int errorCounter = 0;
            foreach ((int paramValue, bool wasErrorFound) param in errorDict.Values)
                if (param.wasErrorFound)
                    errorCounter++;
            return errorCounter;
        }

        private void SendParamToKafka(SendToKafkaItem sendToKafkaItem)
        {
            DateTime beforeDecrypt = DateTime.Now;
            _kafkaConnection.SendFrameToKafka(sendToKafkaItem.PacketType.ToString(),sendToKafkaItem.ParamDict);
            int decryptTime = (int)DateTime.Now.Subtract(beforeDecrypt).TotalMilliseconds;

            _statAnalyze.UpdateStatistic(IcdStatisticType.KafkaUploadTime,sendToKafkaItem.PacketType, decryptTime);
            _kafkaConnection.SendStatisticsToKafka(_statAnalyze.GetDataDictionary());
        }

    }
}
