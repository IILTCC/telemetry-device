using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using telemetry_device_main.icds;

namespace telemetry_device.compactCollection
{
    class TransformBlockItem
    {
        public TransformBlockItem(IcdTypes packetType, byte[] packetData)
        {
            this.PacketType = packetType;
            this.PacketData = packetData;
        }
        public IcdTypes PacketType { get; set; }
        public byte[] PacketData { get; set; }
    }
}
