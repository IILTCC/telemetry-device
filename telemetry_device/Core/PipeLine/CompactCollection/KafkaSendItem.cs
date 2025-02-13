using System;
using System.Collections.Generic;

namespace telemetry_device.Core.PipeLine.CompactCollection
{
    class KafkaSendItem
    {
        public DateTime PacketTime { get; set; }
        public Dictionary<string, (int, bool)> ParamDict { get; set; }
        public KafkaSendItem(DateTime packetTime , Dictionary<string,(int,bool)> paramDict)
        {
            PacketTime = packetTime;
            ParamDict = paramDict;
        }
    }
}
