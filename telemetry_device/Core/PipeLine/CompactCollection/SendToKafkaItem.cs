using System.Collections.Generic;
using telemetry_device_main.icds;

namespace telemetry_device.compactCollection
{
    class SendToKafkaItem
    {
        public IcdTypes PacketType { get; set; }
        public Dictionary <string,(int,bool)> ParamDict { get; set; }
        public SendToKafkaItem( IcdTypes packetType, Dictionary<string,(int,bool)> paramDict)
        {
            this.PacketType = packetType;
            this.ParamDict = paramDict;
        }
    }
}
