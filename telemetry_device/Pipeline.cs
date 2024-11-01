using Microsoft.Extensions.Configuration;
using PacketDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private const int HEADER_SIZE = 27;
        private const string TIMESTAMP_FORMAT = "dd,MM,yyyy,HH,mm,ss,ffff";


        private ActionBlock<Packet> _disposedPackets;
        private BufferBlock<Packet> _pullerBlock;
        private TransformBlock<Packet, TransformBlockItem> _extractPacketData;
        private TransformBlock<TransformBlockItem, SendToKafkaItem> _decryptBlock;
        private ActionBlock<SendToKafkaItem> _sendToKafka;

        private ConcurrentDictionary<IcdTypes, dynamic> _icdDictionary;

        private TelemetryDeviceSettings _telemetryDeviceSettings;

        private KafkaConnection _kafkaConnection;
        private TelemetryLogger _logger;
        private StatisticsAnalyzer _statAnalyze;
        private readonly Dictionary<IcdTypes,IcdtoStatItem> _icdToMetric;
        public PipeLine(TelemetryDeviceSettings telemetryDeviceSettings,KafkaConnection kafkaConnection)
        {
            _logger = TelemetryLogger.Instance;
            _statAnalyze = StatisticsAnalyzer.Instance;

            _telemetryDeviceSettings = telemetryDeviceSettings;
            _kafkaConnection = kafkaConnection;

            this._icdDictionary = new ConcurrentDictionary<IcdTypes, dynamic>();
            _icdToMetric = new Dictionary<IcdTypes, IcdtoStatItem>();
            // acts as clearing buffer endpoint for unwanted packets 100 acts as precentage for bad packed
            _disposedPackets = new ActionBlock<Packet>((Packet packet) => { _statAnalyze.UpdateStatistic(SingleStatisticType.PacketDropRate, 100); });

            _extractPacketData = new TransformBlock<Packet, TransformBlockItem>(ExtractPacketData);
            _pullerBlock = new BufferBlock<Packet>();
            _decryptBlock = new TransformBlock<TransformBlockItem, SendToKafkaItem>(ProccessPackets);
            _sendToKafka = new ActionBlock<SendToKafkaItem>(SendParamToKafka);

            // packets either continue to the rest of the blocks or stop at the action block
            _pullerBlock.LinkTo(_extractPacketData, IsPacketAppropriate);
            _pullerBlock.LinkTo(_disposedPackets, (Packet packet) => { return !IsPacketAppropriate(packet); });

            _extractPacketData.LinkTo(_decryptBlock);
            _decryptBlock.LinkTo(_sendToKafka);
            InitializeIcdDictionary();
            _logger.LogInfo("Succesfuly initalized all icds");
        }
        private void SendParamToKafka(SendToKafkaItem sendToKafkaItem)
        {
            DateTime beforeDecrypt = DateTime.Now;
            _kafkaConnection.SendFrameToKafka(sendToKafkaItem.PacketType.ToString(),sendToKafkaItem.ParamDict);

            int decryptTime = (int)DateTime.Now.Subtract(beforeDecrypt).TotalMilliseconds;

            _statAnalyze.UpdateStatistic(MultiStatisticType.KafkaUploadTime,sendToKafkaItem.PacketType, decryptTime);

            _kafkaConnection.SendStatisticsToKafka(_statAnalyze.GetDataDictionary());
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

        private TransformBlockItem ExtractPacketData(Packet packet)
        {
            // acts as precentage for good packet
            _statAnalyze.UpdateStatistic(SingleStatisticType.PacketDropRate, 0);

            var udpPacket = packet.Extract<UdpPacket>();

            // remove header bytes
            byte[] packetData = new byte[udpPacket.PayloadData.Length - HEADER_SIZE];
            for (int i = 0; i < packetData.Length; i++)
                packetData[i] = udpPacket.PayloadData[i + HEADER_SIZE];

            int type = udpPacket.PayloadData[2];
            byte[] timestampBytes = new byte[24];
            for (int i = 0; i < timestampBytes.Length; i++)
                timestampBytes[i] = udpPacket.PayloadData[i + 3];

            string timestamp = Encoding.ASCII.GetString(timestampBytes);

            // the format in which the timestamp is in the packet
            DateTime dateTime = DateTime.ParseExact(timestamp, TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
            int sinffingTime = (int)DateTime.Now.Subtract(dateTime).TotalMilliseconds;
            _statAnalyze.UpdateStatistic(SingleStatisticType.SniffingTime, sinffingTime);
            
            return new TransformBlockItem((IcdTypes)type, packetData);
        }

        private SendToKafkaItem ProccessPackets(TransformBlockItem transformItem)
        {
            try
            {
                DateTime beforeDecrypt = DateTime.Now;

                Dictionary<string, (int, bool)> decryptedParamDict = _icdDictionary[transformItem.PacketType].DecryptPacket(transformItem.PacketData);
                int decryptTime = (int)DateTime.Now.Subtract(beforeDecrypt).TotalMilliseconds;

                _statAnalyze.UpdateStatistic(MultiStatisticType.DecryptTime, transformItem.PacketType, decryptTime);

                int errorCounter = 0;
                foreach ((int, bool) param in decryptedParamDict.Values)
                    if (param.Item2)
                        errorCounter++;

                _statAnalyze.UpdateStatistic(MultiStatisticType.CorruptedPacket, transformItem.PacketType, errorCounter);
                return new SendToKafkaItem(transformItem.PacketType,decryptedParamDict);
            }
            catch (Exception ex)
            {
                _logger.LogError("Tried decrypt and send to kafka -"+ex.Message);
                return null;
            }
        }
        public void PushToBuffer(Packet packet)
        {
            this._pullerBlock.Post(packet);
        }
    }
}
