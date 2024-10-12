using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using telemetry_device_main.icds;

namespace telemetry_device.compactCollection
{
    class BufferBlockItem
    {

        public BufferBlockItem(byte[] packetCap, LinkLayers packetLayer)
        {
            this.PacketLayer = packetLayer;
            this.PacketCap = packetCap;
            //this.IcdDecryptObject = icdDecryptObject;
        }

        public byte[] PacketCap { get; set; }
        public LinkLayers PacketLayer { get; set; }
        //public dynamic IcdDecryptObject { get; set; }
    }
}
