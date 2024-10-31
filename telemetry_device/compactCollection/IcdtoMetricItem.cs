using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device.compactCollection
{
    class IcdtoMetricItem
    {
        public MetricType CorruptedPacket { get; set; }
        public MetricType KafkaSend { get; set; }
        public MetricType DecryptTime { get; set; }
        public IcdtoMetricItem(MetricType corruptedPacket,MetricType kafkaSend, MetricType decryptTime)
        {
            this.CorruptedPacket = corruptedPacket;
            this.KafkaSend = kafkaSend;
            this.DecryptTime = decryptTime;
        }

    }
}
