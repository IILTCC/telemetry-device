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
        const int PORT = 50000;
        TcpClient simulator;
        NetworkStream stream;

        public TelemetryDevice()
        {
            //this._icdDictionary = new ConcurrentDictionary<IcdTypes, dynamic>();

            _pipeLine = new PipeLine();


        }

        public async Task RunAsync()
        {
            Task.Run(() => { ListenForPackets(); });
            // prevents program from ending
            await Task.Delay(-1);
        }

        private  void device_OnPacketArrival(object sender, PacketCapture e)
        {
            //var udp = (UdpPacket) e.GetPacket().Ext

            byte[] rawBits = new byte[e.Data.Length];
            for (int i = 0; i < e.Data.Length; i++)
                rawBits[i] = e.Data[i];
            var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, rawBits);
            
            var ipPacket = packet.Extract<IPPacket>();
            if(ipPacket.Protocol == PacketDotNet.ProtocolType.Udp )
            {
                var udpPacket = packet.Extract<UdpPacket>();
                //Console.WriteLine("dest prot "+udpPacket.DestinationPort);
                //Console.WriteLine("src port "+udpPacket.SourcePort);
                //Console.WriteLine("=================="+ udpPacket.PayloadData);
                //if(udpPacket.DestinationPort == SIMULATOR_DEST_PORT)
                //    printBytes(udpPacket.PayloadData);
            }
            //Console.WriteLine("payload "+packet.PayloadPacket);
            //Console.WriteLine("ip packet "+ipPacket);
            //Console.WriteLine();
            _pipeLine.PushToBuffer(new BufferBlockItem(rawBits, e.GetPacket().LinkLayerType));
        }

        public async Task ListenForPackets()
        {
            byte[] packetData = new byte[8192];
            while (true)
            {
                CaptureDeviceList devices = CaptureDeviceList.Instance;
                if (devices.Count == 0)
                {
                    return;
                }
                int i = 0;

                foreach (var dev in devices)
                {
                    if (dev.Name == NETWORK_DEVICE_NAME)
                        break;
                    i++;
                }
                LibPcapLiveDevice device = (LibPcapLiveDevice)devices[i];

                // Register our handler function to the 'packet arrival' event
                device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);

                // Open the device for capturing
                int readTimeoutMilliseconds = 1000;
                device.Open(mode: DeviceModes.Promiscuous | DeviceModes.DataTransferUdp | DeviceModes.NoCaptureLocal, read_timeout: readTimeoutMilliseconds);

                // Start the capturing process
                device.StartCapture();

                // Wait for 'Enter' from the user.
                Console.ReadLine();

                // Stop the capturing process
                device.StopCapture();

                Console.WriteLine("-- Capture stopped.");

                // Print out the device statistics
                Console.WriteLine(device.Statistics.ToString());
                Console.ReadLine();



                // get inital data of the packet pos 0,1 size and 2 type of packet
                //await stream.ReadAsync(packetData, 0, 3);
                //byte[] size = new byte[2];
                //size[0] = packetData[0];
                //size[1] = packetData[1];
                //int type = packetData[2];

                //// receive the entire final packet
                //byte[] receivedPacket = new byte[BitConverter.ToInt16(size)];
                //await stream.ReadAsync(receivedPacket, 0, BitConverter.ToInt16(size));

                //_pipeLine.PushToBuffer(new BufferBlockItem(receivedPacket, _icdDictionary[(IcdTypes)type]));
            }
        }
    }
}
