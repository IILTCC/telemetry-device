using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device.compactCollection
{
    enum MetricType
    {
        SniffingTime,
        PacketDropRate,
        FlightBoxDownCorruptedPacket,
        FlightBoxUpCorruptedPacket,
        FiberBoxDownCorruptedPacket,
        FiberBoxUpCorruptedPacket,
        FlightBoxDownDecryptTime,
        FlightBoxUpDecryptTime,
        FiberBoxDownDecryptTime,
        FiberBoxUpDecryptTime,        
        FlightBoxDownKafkaSend,
        FlightBoxUpKafkaSend,
        FiberBoxDownKafkaSend,
        FiberBoxUpKafkaSend,
    }
}
