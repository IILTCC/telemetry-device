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
        public BufferBlockItem(byte[] packetData, IcdTypes icdType)
        {
            this.PacketData = packetData;
            this.IcdType = icdType;
        }

        public byte[] PacketData { get; set; }
        public IcdTypes IcdType { get; set; }
    }
}
