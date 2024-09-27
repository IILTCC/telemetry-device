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
using telemetry_device.async_queue;
namespace telemetry_device
{
    class TelemetryDevice
    {
        const int PORT = 50000;
        TcpClient simulator;
        NetworkStream stream;
        AsyncQueueu<(IcdTypes, byte[])> PacketQueue;
        ConcurrentQueue<(IcdTypes, byte[])> packetQueue;
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
            PacketQueue = new AsyncQueueu<(IcdTypes, byte[])>();
            packetQueue = new ConcurrentQueue<(IcdTypes, byte[])>();
        }
        public async Task ConnectAsync()
        {
            Console.WriteLine("listing");
            IPAddress ipAddr = new IPAddress(new byte[] { 127, 0, 0, 1 });

            TcpListener socket = new TcpListener(ipAddr, PORT);
            socket.Start(1);
            simulator = await socket.AcceptTcpClientAsync();
            stream = simulator.GetStream();
            Console.WriteLine("found target");
        }
        public async Task RunAsync()
        {

            await ConnectAsync();

            Task.Run(() => { ListenForPackets(); });
            await Task.Delay(-1);
        }
        public static void printByteArray(byte[] array)
        {
            foreach (var item in array)
            {
                Console.Write(Convert.ToString(item, 2).PadLeft(8, '0') + " ");
            }
            Console.WriteLine();
        }
        public static void PrintDictionary(Dictionary<string, (int, bool)> dict)
        {
            foreach (var item in dict.Keys)
            {
                Console.WriteLine("key " + item + " equals " + dict[item].Item1 + " " + dict[item].Item2);
            }
        }
        public void ProccessPackets(IcdTypes type, byte[]packet)
        {
            printByteArray(packet);
            Type genericIcdType = typeof(IcdPacketDecryptor<>).MakeGenericType(IcdDictionary[type].Item1);
            dynamic icdInstance = Activator.CreateInstance(genericIcdType);
            try
            {
                string jsonText = File.ReadAllText(REPO_PATH + IcdDictionary[type].Item2 + FILE_TYPE);
                Dictionary<string, (int, bool)> retDict = icdInstance.DecryptPacket(packet, jsonText);
                PrintDictionary(retDict);
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
                await stream.ReadAsync(packetData, 0, 3);
                byte[] size = new byte[2];
                size[0] = packetData[0];
                size[1] = packetData[1];
                int type = packetData[3];
                byte[] receivedPacket = new byte[BitConverter.ToInt16(size)];
                await stream.ReadAsync(receivedPacket, 0, BitConverter.ToInt16(size));
                ProccessPackets((IcdTypes)type, receivedPacket);
            }
        }
    }
}
