using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using telemetry_device.compactCollection;
using telemetry_device_main.decryptor;
using telemetry_device_main.icds;
namespace telemetry_device
{
    class TelemetryDevice
    {
        const string NETWORK_DEVICE_NAME = @"\Device\NPF_{026FB3A3-275B-4791-91BE-BB5CC388A4D7}";

        private PipeLine _pipeLine;
        public TelemetryDevice()
        {
            _pipeLine = new PipeLine();
        }

        public async Task RunAsync()
        {
            Task.Run(() => { ListenForPackets(); });
            // prevents program from ending
            await Task.Delay(-1);
        }

        private  void OnPacketArrival(object sender, PacketCapture e)
        {
            byte[] rawBits = new byte[e.Data.Length];
            for (int i = 0; i < e.Data.Length; i++)
                rawBits[i] = e.Data[i];
            var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, rawBits);
           
            _pipeLine.PushToBuffer(packet);
        }

        public async Task ListenForPackets()
        {
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            if (devices.Count == 0)
                return;
                
            int i = 0;

            foreach (var dev in devices)
            {
                if (dev.Name == NETWORK_DEVICE_NAME)
                    break;
                i++;
            }
            LibPcapLiveDevice device = (LibPcapLiveDevice)devices[i];

            device.OnPacketArrival += new PacketArrivalEventHandler(OnPacketArrival);

            int readTimeoutMilliseconds = 1000;
            device.Open(mode: DeviceModes.Promiscuous | DeviceModes.DataTransferUdp | DeviceModes.NoCaptureLocal, read_timeout: readTimeoutMilliseconds);

            device.StartCapture();

            Console.ReadLine();

            device.StopCapture();
        }
    }
}
