using PacketDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using telemetry_device.compactCollection;
using telemetry_device_main.decryptor;
using telemetry_device_main.icds;

namespace telemetry_device
{
    class PipeLine
    {
        public bool canGo = true;
        private ConcurrentDictionary<IcdTypes, dynamic> _icdDictionary;
        const int SIMULATOR_DEST_PORT = 50_000;

        const string FILE_TYPE = ".json";
        const string REPO_PATH = "../../../icd_repo/";
        private BufferBlock<BufferBlockItem> _pullerBlock;
        private TransformBlock<BufferBlockItem, TransformBlockItem> _peelPacket;
        private TransformBlock<TransformBlockItem, Dictionary<string, (int, bool)>> _decryptBlock;
        public PipeLine()
        {


            this._icdDictionary = new ConcurrentDictionary<IcdTypes, dynamic>();
            _peelPacket = new TransformBlock<BufferBlockItem, TransformBlockItem>(PeelPacket, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 }); ;
            _pullerBlock = new BufferBlock<BufferBlockItem>(new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 }); ;
            _decryptBlock = new TransformBlock<TransformBlockItem, Dictionary<string, (int, bool)>>(ProccessPackets, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 }); ;
            _pullerBlock.LinkTo(_peelPacket);
            _peelPacket.LinkTo(_decryptBlock);

            //_pullerBlock.LinkTo(_decryptBlock);

            (IcdTypes, Type)[] icdTypes = new (IcdTypes, Type)[4] {
                (IcdTypes.FiberBoxDownIcd,typeof(FiberBoxDownIcd)),
                (IcdTypes.FiberBoxUpIcd, typeof(FiberBoxUpIcd)),
                (IcdTypes.FlightBoxDownIcd, typeof(FlightBoxDownIcd)),
                (IcdTypes.FlightBoxUpIcd, typeof(FlightBoxUpIcd))};

            foreach ((IcdTypes, Type) icdInitialization in icdTypes)
            {
                string jsonText = File.ReadAllText(REPO_PATH + icdInitialization.Item1.ToString() + FILE_TYPE);
                Type genericIcdType = typeof(IcdPacketDecryptor<>).MakeGenericType(icdInitialization.Item2);
                _icdDictionary.TryAdd(icdInitialization.Item1, Activator.CreateInstance(genericIcdType, new object[] { jsonText }));
            }
        }
        public void printBytes(byte[] arr)
        {
            Console.WriteLine();
            foreach (byte item in arr)
                Console.Write(Convert.ToString(item, 2).PadLeft(8, '0') + " ");
            Console.WriteLine();
        }
        private TransformBlockItem PeelPacket(BufferBlockItem bufferBlockItem)
        {
            var packet = Packet.ParsePacket(bufferBlockItem.PacketLayer, bufferBlockItem.PacketCap);

            var ipPacket = packet.Extract<IPPacket>();
            if (ipPacket.Protocol == ProtocolType.Udp)
            {
                Console.WriteLine("----------------------------");
                var udpPacket = packet.Extract<UdpPacket>();
                Console.WriteLine("dest prot " + udpPacket.DestinationPort);
                Console.WriteLine("src port " + udpPacket.SourcePort);
                //Console.WriteLine("==================" + udpPacket.PayloadData);
                if (udpPacket.DestinationPort == SIMULATOR_DEST_PORT)
                {

                    printBytes(udpPacket.PayloadData);
                    Console.WriteLine();
                    byte[] packetData = new byte[udpPacket.PayloadData.Length - 3];
                    for (int i = 0; i < packetData.Length; i++)
                        packetData[i] = udpPacket.PayloadData[i + 3];
                    printBytes(packetData);
                    int type = udpPacket.PayloadData[2];
                    Console.WriteLine("type " + (IcdTypes)type);
                    return new TransformBlockItem((IcdTypes)type, packetData);
                }
            }
            // need to add fileter
            return new TransformBlockItem(IcdTypes.FiberBoxDownIcd, null);
        }
        public void printDict(Dictionary<string, (int, bool)> dict)
        {
            foreach (var item in dict.Keys)
                Console.WriteLine(item.PadRight(20) + " " + dict[item].Item1 + " " + dict[item].Item2);
        }
        public Dictionary<string, (int, bool)> ProccessPackets(TransformBlockItem transformItem)
        {



            if (transformItem.PacketData == null)
            {
                canGo = true;
                return null;
            }
            try
            {
                Dictionary<string, (int, bool)> decryptedParamDict = _icdDictionary[transformItem.PacketType].DecryptPacket(transformItem.PacketData);
                //return null;
                printDict(decryptedParamDict);
                canGo = true;
                return decryptedParamDict;
            }
            catch (Exception ex)
            {
                canGo = true;
                return null;
            }
        }
        public void PushToBuffer(BufferBlockItem blockItem)
        {
            if(canGo)
            {
                canGo = false;
                this._pullerBlock.Post(blockItem);
            }
        }
    }
}
