using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using telemetry_device_main.icds;

namespace telemetry_device.compactCollection
{
    class PacketQueueItem
    {
        public PacketQueueItem(IcdTypes icdType , byte[] packetData)
        {
            this.IcdType = icdType;
            this.PacketData = packetData;
        }
        public IcdTypes IcdType { get; set; }
        public byte[] PacketData { get; set; }
    }
}
