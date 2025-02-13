using PacketDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks.Dataflow;
using telemetry_device.compactCollection;
using telemetry_device.Core.PipeLine.CompactCollection;
using telemetry_device_main;
using telemetry_device_main.decodeor;
using telemetry_device_main.Enums;
using telemetry_device_main.icds;

namespace telemetry_device
{
    class PipeLine
    {
        private readonly BufferBlock<Packet> _pullerBlock;
        private readonly ActionBlock<Packet> _disposedPackets;
        private readonly TransformBlock<Packet, ToDecodePacketItem> _extractPacketData;
        private readonly TransformBlock<ToDecodePacketItem, SendToKafkaItem> _decodeFiberBoxUp;
        private readonly TransformBlock<ToDecodePacketItem, SendToKafkaItem> _decodeFiberBoxDown;
        private readonly TransformBlock<ToDecodePacketItem, SendToKafkaItem> _decodeFlightBoxUp;
        private readonly TransformBlock<ToDecodePacketItem, SendToKafkaItem> _decodeFlightBoxDown;
        private readonly ActionBlock<SendToKafkaItem> _sendToKafka;

        private readonly ConcurrentDictionary<IcdTypes, IDecodePacket> _icdDictionary;
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
            _icdDictionary = new ConcurrentDictionary<IcdTypes, IDecodePacket>();

            // acts as clearing buffer endpoint for unwanted packets 100 acts as precentage for bad packed
            _disposedPackets = new ActionBlock<Packet>(DisposedPackets);
            _extractPacketData = new TransformBlock<Packet, ToDecodePacketItem>(ExtractPacketData);
            _pullerBlock = new BufferBlock<Packet>();
            _decodeFiberBoxDown = new TransformBlock<ToDecodePacketItem, SendToKafkaItem>(DecodePacket);
            _decodeFiberBoxUp = new TransformBlock<ToDecodePacketItem, SendToKafkaItem>(DecodePacket);
            _decodeFlightBoxDown = new TransformBlock<ToDecodePacketItem, SendToKafkaItem>(DecodePacket);
            _decodeFlightBoxUp = new TransformBlock<ToDecodePacketItem, SendToKafkaItem>(DecodePacket);
            _sendToKafka = new ActionBlock<SendToKafkaItem>(SendParamToKafka);

            ConfigurePipelineLinks();
            InitializeIcdDictionary();
            _logger.LogInfo("Succesfuly initalized all icds", LogId.Initated);
        }
        private void ConfigurePipelineLinks()
        {
            IcdTypes[] icdTypesArray = (IcdTypes[])Enum.GetValues(typeof(IcdTypes));

            _pullerBlock.LinkTo(_extractPacketData, IsPacketAppropriate);
            _pullerBlock.LinkTo(_disposedPackets, (Packet packet) => { return !IsPacketAppropriate(packet); });
            TransformBlock<ToDecodePacketItem,SendToKafkaItem> [] decodeors = new TransformBlock<ToDecodePacketItem, SendToKafkaItem>[4] {_decodeFiberBoxDown,_decodeFiberBoxUp,_decodeFlightBoxDown,_decodeFlightBoxUp };
            for (int decodeIndex = 0; decodeIndex < decodeors.Length; decodeIndex++)
            {
                IcdTypes icdType = icdTypesArray[decodeIndex];
                _extractPacketData.LinkTo(decodeors[decodeIndex], (ToDecodePacketItem ToDecodePacketItem) => { return DistributeByPort(ToDecodePacketItem, icdType); });
                decodeors[decodeIndex].LinkTo(_sendToKafka);
            }
        }

        private void InitializeIcdDictionary()
        {
            string FiberBoxDownJson = File.ReadAllText(Consts.REPO_PATH + IcdTypes.FiberBoxDownIcd.ToString() + Consts.FILE_TYPE);
            string FiberBoxUpJson = File.ReadAllText(Consts.REPO_PATH + IcdTypes.FiberBoxUpIcd.ToString() + Consts.FILE_TYPE);
            string FlightBoxDownJson = File.ReadAllText(Consts.REPO_PATH + IcdTypes.FlightBoxDownIcd.ToString() + Consts.FILE_TYPE);
            string FlightBoxUpJson = File.ReadAllText(Consts.REPO_PATH + IcdTypes.FlightBoxUpIcd.ToString() + Consts.FILE_TYPE);

            IDecodePacket FiberBoxDowndecodeor = new FiberBoxdecodeor<FiberBoxDownIcd>(FiberBoxDownJson);
            IDecodePacket FiberBoxUpdecodeor = new FiberBoxdecodeor<FiberBoxUpIcd>(FiberBoxUpJson);
            IDecodePacket FlightBoxDowndecodeor = new FlightBoxdecodeor<FlightBoxDownIcd>(FlightBoxDownJson);
            IDecodePacket FlightBoxUpdecodeor = new FlightBoxdecodeor<FlightBoxUpIcd>(FlightBoxUpJson);

            _icdDictionary.TryAdd(IcdTypes.FiberBoxDownIcd,FiberBoxDowndecodeor);
            _icdDictionary.TryAdd(IcdTypes.FiberBoxUpIcd,FiberBoxUpdecodeor);
            _icdDictionary.TryAdd(IcdTypes.FlightBoxDownIcd,FlightBoxDowndecodeor);
            _icdDictionary.TryAdd(IcdTypes.FlightBoxUpIcd,FlightBoxUpdecodeor);
        }

        public void PushToBuffer(Packet packet)
        {
            _pullerBlock.Post(packet);
        }

        private bool DistributeByPort(ToDecodePacketItem ToDecodePacketItem,IcdTypes icdType)
        {
            return ToDecodePacketItem.PacketPort == _telemetryDeviceSettings.SimulatorDestPort+(int)icdType;
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
                    if (udpPacket!=null && udpPacket.DestinationPort >= minPortNumber && udpPacket.DestinationPort < maxPortNumber)
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

        private ToDecodePacketItem ExtractPacketData(Packet packet)
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
            
            return new ToDecodePacketItem((IcdTypes)type, packetData,udpPacket.DestinationPort,dateTime);
        }

        private SendToKafkaItem DecodePacket(ToDecodePacketItem transformItem)
        {
            try
            {
                DateTime beforedecode = DateTime.Now;
                Dictionary<string, (int paramValue, bool wasErrorFound)> decodeedParamDict = _icdDictionary[transformItem.PacketType].DecodePacket(transformItem.PacketData);
                double decodeTime = (double)DateTime.Now.Subtract(beforedecode).TotalMilliseconds;
                _statAnalyze.UpdateStatistic(IcdStatisticType.DecodeTime, transformItem.PacketType, decodeTime);
                   
                _statAnalyze.UpdateStatistic(IcdStatisticType.CorruptedPacket, transformItem.PacketType, CalcErrorCount(decodeedParamDict));
                return new SendToKafkaItem(transformItem.PacketType,decodeedParamDict,transformItem.PacketTime);
            }
            catch (Exception ex)
            {
                _logger.LogError("Tried decode and send to kafka -"+ex.Message,LogId.ErrorDecode);
                return null;
            }
        }

        public int CalcErrorCount(Dictionary<string, (int paramValue, bool wasErrorFound)> errorDict)
        {
            foreach ((int paramValue, bool wasErrorFound) param in errorDict.Values)
                if (param.wasErrorFound)
                    return 1;
            return 0;
        }

        private void SendParamToKafka(SendToKafkaItem sendToKafkaItem)
        {
            DateTime beforeDecode = DateTime.Now;
            KafkaSendItem kafkaSend = new KafkaSendItem(sendToKafkaItem.PacketTime,sendToKafkaItem.ParamDict);
            _kafkaConnection.SendFrameToKafka(sendToKafkaItem.PacketType.ToString(),kafkaSend);
            int decodeTime = (int)DateTime.Now.Subtract(beforeDecode).TotalMilliseconds;

            _statAnalyze.UpdateStatistic(IcdStatisticType.KafkaUploadTime,sendToKafkaItem.PacketType, decodeTime);

            _kafkaConnection.SendStatisticsToKafka(_statAnalyze.GetDataDictionary());
        }
    }
}
