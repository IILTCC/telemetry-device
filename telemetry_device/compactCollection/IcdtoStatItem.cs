using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device.compactCollection
{
    class IcdtoStatItem
    {
        public SingleStatisticType CorruptedPacket { get; set; }
        public SingleStatisticType KafkaUploadTime { get; set; }
        public SingleStatisticType DecryptTime { get; set; }
        public IcdtoStatItem(SingleStatisticType corruptedPacket,SingleStatisticType kafkaUploadTime, SingleStatisticType decryptTime)
        {
            this.CorruptedPacket = corruptedPacket;
            this.KafkaUploadTime = kafkaUploadTime;
            this.DecryptTime = decryptTime;
        }

    }
}
