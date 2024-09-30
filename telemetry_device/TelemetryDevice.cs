using simulator_main.icd;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using telemetry_device_main.decryptor;
using telemetry_device_main.icds;
namespace telemetry_device
{
    class TelemetryDevice
    {
        const int PORT = 50000;
        TcpClient simulator;
        NetworkStream stream;
        private Dictionary<IcdTypes, Type> IcdDictionary;
        const string FILE_TYPE = ".json";
        const string REPO_PATH = "../../../icd_repo/";
        public TelemetryDevice()
        {
            this.IcdDictionary = new Dictionary<IcdTypes, Type>();
            IcdDictionary.Add(IcdTypes.FlightBoxDownIcd, typeof(FlightBoxIcd));
            IcdDictionary.Add(IcdTypes.FlightBoxUpIcd, typeof(FlightBoxIcd));
            IcdDictionary.Add(IcdTypes.FiberBoxDownIcd, typeof(FiberBoxIcd));
            IcdDictionary.Add(IcdTypes.FiberBoxUpIcd, typeof(FiberBoxIcd));

        }
        public async Task ConnectAsync()
        {
            IPAddress ipAddr = new IPAddress(new byte[] { 127, 0, 0, 1 });
            TcpListener socket = new TcpListener(ipAddr, PORT);
            socket.Start(1);
            simulator = await socket.AcceptTcpClientAsync();
            stream = simulator.GetStream();
        }
        public async Task RunAsync()
        {
            await ConnectAsync();

            Task.Run(() => { ListenForPackets(); });
            // prevents program from ending
            await Task.Delay(-1);
        }
        public void ProccessPackets(IcdTypes type, byte[]packet)
        {
            Type genericIcdType = typeof(IcdPacketDecryptor<>).MakeGenericType(IcdDictionary[type]);
            dynamic icdInstance = Activator.CreateInstance(genericIcdType);
            try
            {
                string jsonText = File.ReadAllText(REPO_PATH + type.ToString() + FILE_TYPE);
                Dictionary<string, (int, bool)> retDict = icdInstance.DecryptPacket(packet, jsonText);
            }
            catch (Exception)
            {
                return;
            }
        }

        public async Task ListenForPackets()
        {
            byte[] packetData = new byte[8192];
            int counter = 0;
            while (true)
            {
                // get inital data of the packet pos 0,1 size and 2 type of packet
                await stream.ReadAsync(packetData, 0, 3);
                byte[] size = new byte[2];
                size[0] = packetData[0];
                size[1] = packetData[1];
                int type = packetData[2];

                // receive the entire final packet
                byte[] receivedPacket = new byte[BitConverter.ToInt16(size)];
                await stream.ReadAsync(receivedPacket, 0, BitConverter.ToInt16(size));

                ProccessPackets((IcdTypes)type, receivedPacket);
            }
        }
    }
}
