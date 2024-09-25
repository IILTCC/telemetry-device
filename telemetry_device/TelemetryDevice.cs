using simulator_main.icd;
using System;
using System.Collections;
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
        Queue<(IcdTypes, byte[])> packetQueue;
        private Dictionary<IcdTypes,(Type,string)> IcdDictionary;
        const string FILE_TYPE = ".json";
        const string REPO_PATH = "../../../icd_repo/";
        public TelemetryDevice()
        {
            this.IcdDictionary = new Dictionary<IcdTypes,(Type,string)>();
            IcdDictionary.Add(IcdTypes.FlightBoxDown, (typeof(FlightBoxIcd), "FlightBoxUpIcd"));
            IcdDictionary.Add(IcdTypes.FlightBoxUp, (typeof(FlightBoxIcd), "FlightboxUpIcd"));
            IcdDictionary.Add(IcdTypes.FiberBoxDown, (typeof(FiberBoxIcd), "FiberBoxDownIcd"));
            IcdDictionary.Add(IcdTypes.FiberBoxUp, (typeof(FiberBoxIcd), "FiberBoxUpIcd"));
            packetQueue = new Queue<(IcdTypes, byte[])>();
        }
        public async Task ConnectAsync()
        {
            Console.WriteLine("listing");
            IPAddress ipAddr = new IPAddress(new byte[] { 127,0,0,1});

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
            await Task.Run(() => { ProccessPackets(); });
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
        public async Task ProccessPackets()
        {
            while(true)
            {
                if(packetQueue.Count>0)
                {
                    Console.WriteLine("new packet");
                    IcdTypes packetType;
                    byte[] packetData;
                    (packetType, packetData) = packetQueue.Dequeue();
                    Console.WriteLine("type "+packetType);
                    printByteArray(packetData);
                    Type genericIcdType = typeof(IcdPacketDecryptor<>).MakeGenericType(IcdDictionary[packetType].Item1);
                    dynamic icdInstance = Activator.CreateInstance(genericIcdType);
                    try
                    {

                    string jsonText = File.ReadAllText(REPO_PATH + IcdDictionary[packetType].Item2 + FILE_TYPE);
                    Dictionary<string, (int, bool)>  retDict = icdInstance.DecryptPacket(packetData, jsonText);
                    Console.WriteLine("after dictionary");
                    PrintDictionary(retDict);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }
        public async Task ListenForPackets()
        {
            byte[] packetData = new byte[8192];
            while(true)
            {
                await stream.ReadAsync(packetData,0,3);
                Console.WriteLine("got packet size "+ packetData[0]+" "+ packetData[1]+" "+packetData[3]);
                byte[] size = new byte[2];
                size[0] = packetData[0];
                size[1] = packetData[1];
                
                Console.WriteLine("packet length "+BitConverter.ToInt16(size));
                int type = packetData[3];
                Console.WriteLine("packet type " + type);
                byte[] receivedPacket = new byte[BitConverter.ToInt16(size)];
                Console.WriteLine("before");
                await stream.ReadAsync(receivedPacket,0,BitConverter.ToInt16(size));
                packetQueue.Enqueue(((IcdTypes)type,receivedPacket));

            }
        }
    }
}
