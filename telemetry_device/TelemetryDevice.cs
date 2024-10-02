using simulator_main.icd;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
        private Dictionary<IcdTypes, (dynamic,string)> IcdDictionary;
        const string FILE_TYPE = ".json";
        const string REPO_PATH = "../../../icd_repo/";
        ConcurrentQueue<(IcdTypes, byte[])> packetQueue;
        public TelemetryDevice()
        {
            this.IcdDictionary = new Dictionary<IcdTypes, (dynamic, string)>();
            (IcdTypes,Type)[] icdTypes = new (IcdTypes, Type)[4] {
                (IcdTypes.FiberBoxDownIcd,typeof(FiberBoxDownIcd)),
                (IcdTypes.FiberBoxUpIcd, typeof(FiberBoxUpIcd)),
                (IcdTypes.FlightBoxDownIcd, typeof(FlightBoxDownIcd)),
                (IcdTypes.FlightBoxUpIcd, typeof(FlightBoxUpIcd))};
            foreach((IcdTypes,Type) type in icdTypes)
            {
                string jsonText = File.ReadAllText(REPO_PATH + type.Item1.ToString() + FILE_TYPE);
                Type genericIcdType = typeof(IcdPacketDecryptor<>).MakeGenericType(type.Item2);
                IcdDictionary.Add(type.Item1,(Activator.CreateInstance(genericIcdType), jsonText));
            }
            //IcdDictionary.Add(IcdTypes.FlightBoxDownIcd, (typeof(FlightBoxIcd),""));
            //IcdDictionary.Add(IcdTypes.FlightBoxUpIcd, (typeof(FlightBoxIcd),""));
            //IcdDictionary.Add(IcdTypes.FiberBoxDownIcd, (typeof(FiberBoxIcd),""));
            //IcdDictionary.Add(IcdTypes.FiberBoxUpIcd, (typeof(FiberBoxIcd),""));
            packetQueue = new ConcurrentQueue<(IcdTypes, byte[])>();
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
            Thread listenForPackets = new Thread(ProccessPackets);
            Task.Run(() => { ListenForPackets(); });
            listenForPackets.Start();
            // prevents program from ending
            await Task.Delay(-1);
        }
        public void ProccessPackets()
        {
            while (true)
            {
                Console.WriteLine(packetQueue.Count);
                if (packetQueue.Count > 0)
                {

                    (IcdTypes, byte[]) packetData;

                    packetQueue.TryDequeue(out packetData);
                    Console.WriteLine("processing packet----------------------------------------------------------");
                    try
                    {
                        dynamic icdInstance = IcdDictionary[packetData.Item1].Item1;
                        //string jsonText = File.ReadAllText(REPO_PATH + packetData.Item1.ToString() + FILE_TYPE);
                        Dictionary<string, (int, bool)> retDict = icdInstance.DecryptPacket(packetData.Item2, IcdDictionary[packetData.Item1].Item2);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return;
                    }
                }
                Thread.Sleep(200);

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
                int type = packetData[2];

                // receive the entire final packet
                byte[] receivedPacket = new byte[BitConverter.ToInt16(size)];
                await stream.ReadAsync(receivedPacket, 0, BitConverter.ToInt16(size));


                Console.WriteLine("received packet");
                
                packetQueue.Enqueue(((IcdTypes)type,receivedPacket));
            }
        }
    }
}
