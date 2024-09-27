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
        private Dictionary<IcdTypes, (Type, string)> IcdDictionary;
        const string FILE_TYPE = ".json";
        const string REPO_PATH = "../../../icd_repo/";
        public TelemetryDevice()
        {
            this.IcdDictionary = new Dictionary<IcdTypes, (Type, string)>();
            IcdDictionary.Add(IcdTypes.FlightBoxDown, (typeof(FlightBoxIcd), "FlightBoxUpIcd"));
            IcdDictionary.Add(IcdTypes.FlightBoxUp, (typeof(FlightBoxIcd), "FlightboxUpIcd"));
            IcdDictionary.Add(IcdTypes.FiberBoxDown, (typeof(FiberBoxIcd), "FiberBoxDownIcd"));
            IcdDictionary.Add(IcdTypes.FiberBoxUp, (typeof(FiberBoxIcd), "FiberBoxUpIcd"));
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
            await Task.Delay(-1);
        }
        public void ProccessPackets(IcdTypes type, byte[]packet)
        {
            Type genericIcdType = typeof(IcdPacketDecryptor<>).MakeGenericType(IcdDictionary[type].Item1);
            dynamic icdInstance = Activator.CreateInstance(genericIcdType);
            try
            {
                string jsonText = File.ReadAllText(REPO_PATH + IcdDictionary[type].Item2 + FILE_TYPE);
                Dictionary<string, (int, bool)> retDict = icdInstance.DecryptPacket(packet, jsonText);
            }
            catch (Exception ex)
            {
                return;
            }
        }
        public async Task ListenForPackets()
        {
            byte[] packetData = new byte[8192];
            while (true)
            {
                // get inital data of the packet pos 0,1 size and 2 type of packet
                await stream.ReadAsync(packetData, 0, 3);
                byte[] size = new byte[2];
                size[0] = packetData[0];
                size[1] = packetData[1];
                int type = packetData[3];

                // receive the entire final packet
                byte[] receivedPacket = new byte[BitConverter.ToInt16(size)];
                await stream.ReadAsync(receivedPacket, 0, BitConverter.ToInt16(size));

                ProccessPackets((IcdTypes)type, receivedPacket);
                
            }
        }
    }
}
