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
        public BufferBlockItem(byte[] packetData, dynamic icdDecryptObject)
        {
            this.PacketData = packetData;
            this.IcdDecryptObject = icdDecryptObject;
        }

        public byte[] PacketData { get; set; }
        public dynamic IcdDecryptObject { get; set; }
    }
}
