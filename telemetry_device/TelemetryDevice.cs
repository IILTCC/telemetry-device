using HealthCheck;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using telemetry_device.Settings;
using telemetry_device_main;
using telemetry_device_main.Enums;

namespace telemetry_device
{
    class TelemetryDevice
    {
        private readonly HealthCheckEndPoint _healthCheck;
        private readonly PipeLine _pipeLine;
        private readonly TelemetryDeviceSettings _telemetryDeviceSettings;
        private readonly KafkaConnection _kafkaConnection;
        private readonly TelemetryLogger _logger;
        private readonly HealthCheckSettings _healthCheckSettings;
        public TelemetryDevice()
        {
            ConfigProvider configProvider = ConfigProvider.Instance;
            _telemetryDeviceSettings = configProvider.ProvideTelemetrySettings();
            _healthCheckSettings = configProvider.ProvideHealthCheckSettings();
            _kafkaConnection = new KafkaConnection();
            _kafkaConnection.WaitForKafkaConnection();
            _healthCheck = new HealthCheckEndPoint();
            _logger = TelemetryLogger.Instance;
            _pipeLine = new PipeLine(_telemetryDeviceSettings,_kafkaConnection);

            _logger.LogInfo("Connection established to kafka", LogId.ConnectionSuccesful);
        }

        public async Task RunAsync()
        {
            Task.Run(() => { ListenForPackets(); });
            Task.Run(() => { _healthCheck.StartUp(_healthCheckSettings); });
            
            // prevents program from ending
            await Task.Delay(-1);
        }


        public async Task ListenForPackets()
        {
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            int deviceIndex = 0;
            foreach (var dev in devices)
            {
                if (dev.Name == Consts.NETWORK_DEVICE_NAME)
                    break;
                deviceIndex++;
            }
            LibPcapLiveDevice device = (LibPcapLiveDevice)devices[deviceIndex];
            device.OnPacketArrival += new PacketArrivalEventHandler(OnPacketArrival);

            int readTimeoutMilliseconds = _telemetryDeviceSettings.TelemetryReadTimeout;
            device.Open(mode: DeviceModes.Promiscuous | DeviceModes.DataTransferUdp | DeviceModes.NoCaptureLocal, read_timeout: readTimeoutMilliseconds);
            _logger.LogInfo("Starting sniffing packets", LogId.StartUp);

            device.StartCapture();
            Console.ReadLine();
            device.StopCapture();
        }

        private  void OnPacketArrival(object sender, PacketCapture e)
        {
            byte[] rawBits = new byte[e.Data.Length];
            for (int bitIndex = 0; bitIndex < e.Data.Length; bitIndex++)
                rawBits[bitIndex] = e.Data[bitIndex];
            Packet packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, rawBits);
           
            _pipeLine.PushToBuffer(packet);
        }
    }
}
