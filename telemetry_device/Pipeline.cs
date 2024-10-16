﻿using Microsoft.Extensions.Configuration;
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
        const string FILE_TYPE = ".json";
        const string REPO_PATH = "../../../icd_repo/";

        private ActionBlock<Packet> _disposedPackets;
        private BufferBlock<Packet> _pullerBlock;
        private TransformBlock<Packet, TransformBlockItem> _extractPacketData;
        private TransformBlock<TransformBlockItem, Dictionary<string, (int, bool)>> _decryptBlock;

        private ConcurrentDictionary<IcdTypes, dynamic> _icdDictionary;

        private TelemetryDeviceSettings _telemetryDeviceSettings;

        public PipeLine(TelemetryDeviceSettings telemetryDeviceSettings)
        {
            _telemetryDeviceSettings = telemetryDeviceSettings;

            this._icdDictionary = new ConcurrentDictionary<IcdTypes, dynamic>();

            // acts as clearing buffer endpoint for unwanted packets
            _disposedPackets = new ActionBlock<Packet>((Packet packet) => { });

            _extractPacketData = new TransformBlock<Packet, TransformBlockItem>(ExtractPacketData);
            _pullerBlock = new BufferBlock<Packet>();
            _decryptBlock = new TransformBlock<TransformBlockItem, Dictionary<string, (int, bool)>>(ProccessPackets);

            // packets either continue to the rest of the blocks or stop at the action block
            _pullerBlock.LinkTo(_extractPacketData, FilterPacket);
            _pullerBlock.LinkTo(_disposedPackets, (Packet packet) => { return !FilterPacket(packet); });

            _extractPacketData.LinkTo(_decryptBlock);

            InitializeIcdDictionary();
        }

        private void InitializeIcdDictionary()
        {
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

        // returns true if includes correct dest port and correct protocol
        private bool FilterPacket(Packet packet)
        {
            IPPacket ipPacket = packet.Extract<IPPacket>();
            if (ipPacket.Protocol == ProtocolType.Udp)
            {
                UdpPacket udpPacket = packet.Extract<UdpPacket>();
                if (udpPacket.DestinationPort == _telemetryDeviceSettings.SimulatorDestPort)
                    return true;
            }
            return false;
        }

        private TransformBlockItem ExtractPacketData(Packet packet)
        {
            var udpPacket = packet.Extract<UdpPacket>();

            // remove header bytes
            byte[] packetData = new byte[udpPacket.PayloadData.Length - 3];
            for (int i = 0; i < packetData.Length; i++)
                packetData[i] = udpPacket.PayloadData[i + 3];

            int type = udpPacket.PayloadData[2];

            return new TransformBlockItem((IcdTypes)type, packetData);
        }

        private Dictionary<string, (int, bool)> ProccessPackets(TransformBlockItem transformItem)
        {
            try
            {
                Dictionary<string, (int, bool)> decryptedParamDict = _icdDictionary[transformItem.PacketType].DecryptPacket(transformItem.PacketData);
                return decryptedParamDict;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void PushToBuffer(Packet packet)
        {
            this._pullerBlock.Post(packet);
        }
    }
}
