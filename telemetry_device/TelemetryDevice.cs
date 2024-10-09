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
using telemetry_device.compactCollection;
using telemetry_device_main.decryptor;
using telemetry_device_main.icds;
namespace telemetry_device
{
    class TelemetryDevice
    {
        const int MAX_THREADPOOL_SIZE = 10;
        const int PORT = 50000;
        TcpClient simulator;
        NetworkStream stream;
        private ConcurrentDictionary<IcdTypes, IcdDictionaryItem> _icdDictionary;
        const string FILE_TYPE = ".json";
        const string REPO_PATH = "../../../icd_repo/";
        private ConcurrentQueue<PacketQueueItem> _packetQueue;

        public TelemetryDevice()
        {
            this._icdDictionary = new ConcurrentDictionary<IcdTypes, IcdDictionaryItem>();

            (IcdTypes,Type)[] icdTypes = new (IcdTypes, Type)[4] {
                (IcdTypes.FiberBoxDownIcd,typeof(FiberBoxDownIcd)),
                (IcdTypes.FiberBoxUpIcd, typeof(FiberBoxUpIcd)),
                (IcdTypes.FlightBoxDownIcd, typeof(FlightBoxDownIcd)),
                (IcdTypes.FlightBoxUpIcd, typeof(FlightBoxUpIcd))};

            foreach((IcdTypes,Type) icdInitialization in icdTypes)
            {
                string jsonText = File.ReadAllText(REPO_PATH + icdInitialization.Item1.ToString() + FILE_TYPE);
                Type genericIcdType = typeof(IcdPacketDecryptor<>).MakeGenericType(icdInitialization.Item2);
                _icdDictionary.TryAdd(icdInitialization.Item1,new IcdDictionaryItem(Activator.CreateInstance(genericIcdType), jsonText));
            }

            _packetQueue = new ConcurrentQueue<PacketQueueItem>();
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
            ThreadPool.SetMaxThreads(MAX_THREADPOOL_SIZE, MAX_THREADPOOL_SIZE);

            Thread proccessPacket = new Thread(ProccessPackets);
            Thread listenPacket = new Thread(ListenForPackets);

            proccessPacket.Start();
            listenPacket.Start();

        }
        public void ProccessPackets()
        {
            while (true)
            {
                if (_packetQueue.Count > 0)
                {
                    PacketQueueItem packetData;
                    _packetQueue.TryDequeue(out packetData);
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
        public void AnalayzePacket(PacketQueueItem packetData)
        {
            dynamic icdInstance = _icdDictionary[packetData.IcdType].IcdObject;
            Dictionary<string, (int, bool)> decryptedParamDict = icdInstance.DecryptPacket(packetData.PacketData, _icdDictionary[packetData.IcdType].IcdJson);
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
                
                _packetQueue.Enqueue(new PacketQueueItem((IcdTypes)type,receivedPacket));
            }
        }
    }
}
