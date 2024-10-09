using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
        private BufferBlock<BufferBlockItem> _sourceBlock;
        private TransformBlock<BufferBlockItem, Dictionary<string, (int, bool)>> _decryptBlock;
        const int PORT = 50000;
        TcpClient simulator;
        NetworkStream stream;
        private ConcurrentDictionary<IcdTypes, IcdDictionaryItem> _icdDictionary;
        const string FILE_TYPE = ".json";
        const string REPO_PATH = "../../../icd_repo/";
        public TelemetryDevice()
        {
            _sourceBlock = new BufferBlock<BufferBlockItem>();
            _decryptBlock = new TransformBlock<BufferBlockItem, Dictionary<string, (int,bool)>>(ProccessPackets);
            //ActionBlock<Dictionary<string, PacketItem>> sendBlock = new ActionBlock<Dictionary<string, PacketItem>>();

            this._icdDictionary = new ConcurrentDictionary<IcdTypes, IcdDictionaryItem>();

            (IcdTypes, Type)[] icdTypes = new (IcdTypes, Type)[4] {
                (IcdTypes.FiberBoxDownIcd,typeof(FiberBoxDownIcd)),
                (IcdTypes.FiberBoxUpIcd, typeof(FiberBoxUpIcd)),
                (IcdTypes.FlightBoxDownIcd, typeof(FlightBoxDownIcd)),
                (IcdTypes.FlightBoxUpIcd, typeof(FlightBoxUpIcd))};

            foreach ((IcdTypes, Type) icdInitialization in icdTypes)
            {
                string jsonText = File.ReadAllText(REPO_PATH + icdInitialization.Item1.ToString() + FILE_TYPE);
                Type genericIcdType = typeof(IcdPacketDecryptor<>).MakeGenericType(icdInitialization.Item2);
                _icdDictionary.TryAdd(icdInitialization.Item1, new IcdDictionaryItem(Activator.CreateInstance(genericIcdType), jsonText));
            }
            _sourceBlock.LinkTo(_decryptBlock);
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
        public Dictionary<string,(int,bool)> ProccessPackets(BufferBlockItem bufferItem)
        {            
            try
            {
                dynamic icdInstance = _icdDictionary[bufferItem.IcdType].IcdObject;
                Dictionary<string, (int, bool)> decryptedParamDict = icdInstance.DecryptPacket(bufferItem.PacketData, _icdDictionary[bufferItem.IcdType].IcdJson);
                Console.WriteLine("proccessed packet");
                return decryptedParamDict;
            }
            catch (Exception ex)
            {
                return null;
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

                _sourceBlock.Post(new BufferBlockItem(receivedPacket, (IcdTypes)type));
            }
        }
    }
}
