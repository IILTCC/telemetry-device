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
        private ConcurrentDictionary<IcdTypes, (dynamic,string)> IcdDictionary;
        const string FILE_TYPE = ".json";
        const string REPO_PATH = "../../../icd_repo/";
        private ConcurrentQueue<(IcdTypes, byte[])> PacketQueue;

        public TelemetryDevice()
        {
            this.IcdDictionary = new ConcurrentDictionary<IcdTypes, (dynamic, string)>();

            (IcdTypes,Type)[] icdTypes = new (IcdTypes, Type)[4] {
                (IcdTypes.FiberBoxDownIcd,typeof(FiberBoxDownIcd)),
                (IcdTypes.FiberBoxUpIcd, typeof(FiberBoxUpIcd)),
                (IcdTypes.FlightBoxDownIcd, typeof(FlightBoxDownIcd)),
                (IcdTypes.FlightBoxUpIcd, typeof(FlightBoxUpIcd))};

            // intializing on cretion all types of IcdPacketDecryptor
            foreach((IcdTypes,Type) icdDescript in icdTypes)
            {
                string jsonText = File.ReadAllText(REPO_PATH + icdDescript.Item1.ToString() + FILE_TYPE);
                Type genericIcdType = typeof(IcdPacketDecryptor<>).MakeGenericType(icdDescript.Item2);
                IcdDictionary.TryAdd(icdDescript.Item1,(Activator.CreateInstance(genericIcdType), jsonText));
            }

            PacketQueue = new ConcurrentQueue<(IcdTypes, byte[])>();
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
            ThreadPool.SetMaxThreads(10, 10);

            Thread proccessPacket = new Thread(ProccessPackets);
            Thread listenPacket = new Thread(ListenForPackets);

            proccessPacket.Start();
            listenPacket.Start();

        }
        public void ProccessPackets()
        {
            while (true)
            {
                Console.WriteLine(PacketQueue.Count);
                if (PacketQueue.Count > 0)
                {
                    (IcdTypes, byte[]) packetData;
                    PacketQueue.TryDequeue(out packetData);
                    try
                    {
                        // split work between worker thread on threadpool
                        Task.Run(()=>AnalayzePacket(packetData));
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
            }
        }
        public void AnalayzePacket((IcdTypes, byte[]) packetData)
        {
            dynamic icdInstance = IcdDictionary[packetData.Item1].Item1;
            Dictionary<string, (int, bool)> retDict = icdInstance.DecryptPacket(packetData.Item2, IcdDictionary[packetData.Item1].Item2);
        }
        public async void ListenForPackets()
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
                
                PacketQueue.Enqueue(((IcdTypes)type,receivedPacket));
            }
        }
    }
}
