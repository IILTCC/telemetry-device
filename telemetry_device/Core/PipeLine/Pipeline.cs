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
using telemetry_device_main;
using telemetry_device.Statistics.Sevirity;

namespace telemetry_device
{
    class PipeLine
    {
        private readonly BufferBlock<Packet> _pullerBlock;
        private readonly ActionBlock<Packet> _disposedPackets;
        private readonly TransformBlock<Packet, ToDecryptPacketItem> _extractPacketData;
        private readonly TransformBlock<ToDecryptPacketItem, SendToKafkaItem> _decryptFiberBoxUp;
        private readonly TransformBlock<ToDecryptPacketItem, SendToKafkaItem> _decryptFiberBoxDown;
        private readonly TransformBlock<ToDecryptPacketItem, SendToKafkaItem> _decryptFlightBoxUp;
        private readonly TransformBlock<ToDecryptPacketItem, SendToKafkaItem> _decryptFlightBoxDown;
        private readonly ActionBlock<SendToKafkaItem> _sendToKafka;

        private readonly ConcurrentDictionary<IcdTypes, IDecryptPacket> _icdDictionary;
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
            _icdDictionary = new ConcurrentDictionary<IcdTypes, IDecryptPacket>();

            // acts as clearing buffer endpoint for unwanted packets 100 acts as precentage for bad packed
            _disposedPackets = new ActionBlock<Packet>(DisposedPackets);
            _extractPacketData = new TransformBlock<Packet, ToDecryptPacketItem>(ExtractPacketData);
            _pullerBlock = new BufferBlock<Packet>();
            _decryptFiberBoxDown = new TransformBlock<ToDecryptPacketItem, SendToKafkaItem>(DecryptPacket);
            _decryptFiberBoxUp = new TransformBlock<ToDecryptPacketItem, SendToKafkaItem>(DecryptPacket);
            _decryptFlightBoxDown = new TransformBlock<ToDecryptPacketItem, SendToKafkaItem>(DecryptPacket);
            _decryptFlightBoxUp = new TransformBlock<ToDecryptPacketItem, SendToKafkaItem>(DecryptPacket);
            _sendToKafka = new ActionBlock<SendToKafkaItem>(SendParamToKafka);

            ConfigurePipelineLinks();
            InitializeIcdDictionary();
            _logger.LogInfo("Succesfuly initalized all icds");
        }
        private void ConfigurePipelineLinks()
        {
            IcdTypes[] icdTypesArray = (IcdTypes[])Enum.GetValues(typeof(IcdTypes));

            _pullerBlock.LinkTo(_extractPacketData, IsPacketAppropriate);
            _pullerBlock.LinkTo(_disposedPackets, (Packet packet) => { return !IsPacketAppropriate(packet); });
            TransformBlock<ToDecryptPacketItem,SendToKafkaItem> [] decryptors = new TransformBlock<ToDecryptPacketItem, SendToKafkaItem>[4] {_decryptFiberBoxDown,_decryptFiberBoxUp,_decryptFlightBoxDown,_decryptFlightBoxUp };
            for (int decryptIndex = 0; decryptIndex < decryptors.Length; decryptIndex++)
            {
                IcdTypes icdType = icdTypesArray[decryptIndex];
                _extractPacketData.LinkTo(decryptors[decryptIndex], (ToDecryptPacketItem toDecryptPacketItem) => { return DistributeByPort(toDecryptPacketItem, icdType); });
                decryptors[decryptIndex].LinkTo(_sendToKafka);
            }
        }

        private void InitializeIcdDictionary()
        {
            string FiberBoxDownJson = File.ReadAllText(Consts.REPO_PATH + IcdTypes.FiberBoxDownIcd.ToString() + Consts.FILE_TYPE);
            string FiberBoxUpJson = File.ReadAllText(Consts.REPO_PATH + IcdTypes.FiberBoxUpIcd.ToString() + Consts.FILE_TYPE);
            string FlightBoxDownJson = File.ReadAllText(Consts.REPO_PATH + IcdTypes.FlightBoxDownIcd.ToString() + Consts.FILE_TYPE);
            string FlightBoxUpJson = File.ReadAllText(Consts.REPO_PATH + IcdTypes.FlightBoxUpIcd.ToString() + Consts.FILE_TYPE);

            IDecryptPacket FiberBoxDownDecryptor = new FiberBoxDecryptor<FiberBoxDownIcd>(FiberBoxDownJson);
            IDecryptPacket FiberBoxUpDecryptor = new FiberBoxDecryptor<FiberBoxUpIcd>(FiberBoxUpJson);
            IDecryptPacket FlightBoxDownDecryptor = new FlightBoxDecryptor<FlightBoxDownIcd>(FlightBoxDownJson);
            IDecryptPacket FlightBoxUpDecryptor = new FlightBoxDecryptor<FlightBoxUpIcd>(FlightBoxUpJson);

            _icdDictionary.TryAdd(IcdTypes.FiberBoxDownIcd,FiberBoxDownDecryptor);
            _icdDictionary.TryAdd(IcdTypes.FiberBoxUpIcd,FiberBoxUpDecryptor);
            _icdDictionary.TryAdd(IcdTypes.FlightBoxDownIcd,FlightBoxDownDecryptor);
            _icdDictionary.TryAdd(IcdTypes.FlightBoxUpIcd,FlightBoxUpDecryptor);
        }

        public void PushToBuffer(Packet packet)
        {
            _pullerBlock.Post(packet);
        }

        private bool DistributeByPort(ToDecryptPacketItem toDecryptPacketItem,IcdTypes icdType)
        {
            return toDecryptPacketItem.PacketPort == _telemetryDeviceSettings.SimulatorDestPort+(int)icdType;
        }
        
        // returns true if includes correct dest port and correct protocol
        private bool IsPacketAppropriate(Packet packet)
        {
            int minPortNumber = _telemetryDeviceSettings.SimulatorDestPort;
            int maxPortNumber = _telemetryDeviceSettings.SimulatorDestPort + Enum.GetNames(typeof(IcdTypes)).Length;

            IPPacket ipPacket = packet.Extract<IPPacket>();
            if (ipPacket != null)
            {
                if (ipPacket.Protocol == ProtocolType.Udp)
                {
                    UdpPacket udpPacket = packet.Extract<UdpPacket>();
                    if (udpPacket.DestinationPort >= minPortNumber && udpPacket.DestinationPort < maxPortNumber)
                        return true;
                }
            }
            return false;
        }

        public void DisposedPackets(Packet packet)
        {
            _statAnalyze.UpdateStatistic(GlobalStatisticType.PacketDropRate, Consts.BAD_PACKET_PRECENTAGE);
        }

        // loads all the packet data to different arrays
        private void LoadByteToArray(ref byte[] packetData , ref byte[] typeBytes , ref byte[] timestampBytes,UdpPacket udpPacket)
        {
            List<byte[]> packetParams = new List<byte[]>() { typeBytes, timestampBytes, packetData };
            int packetOffset = 0;
            foreach (byte[] param in packetParams)
                for (int paramIndex = 0; paramIndex < param.Length; paramIndex++)
                    param[paramIndex] = udpPacket.PayloadData[packetOffset++];
        }

        private ToDecryptPacketItem ExtractPacketData(Packet packet)
        {
            _statAnalyze.UpdateStatistic(GlobalStatisticType.PacketDropRate, Consts.GOOD_PACKET_PRECENTAGE);

            UdpPacket udpPacket = packet.Extract<UdpPacket>();
            byte[] packetData = new byte[udpPacket.PayloadData.Length - Consts.HEADER_SIZE];
            byte[] typeBytes = new byte[Consts.TYPE_SIZE];
            byte[] timestampBytes = new byte[Consts.TIMESTAMP_SIZE];

            LoadByteToArray(ref packetData,ref typeBytes,ref timestampBytes,udpPacket);

            int type = typeBytes[Consts.TYPE_PLACE];
            string timestamp = Encoding.ASCII.GetString(timestampBytes);

            DateTime dateTime = DateTime.ParseExact(timestamp, Consts.TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
            int sinffingTime = (int)DateTime.Now.Subtract(dateTime).TotalMilliseconds;
            _statAnalyze.UpdateStatistic(GlobalStatisticType.SniffingTime, sinffingTime);
            
            return new ToDecryptPacketItem((IcdTypes)type, packetData,udpPacket.DestinationPort);
        }

        private SendToKafkaItem DecryptPacket(ToDecryptPacketItem transformItem)
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
