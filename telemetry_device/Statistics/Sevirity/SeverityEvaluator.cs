using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using telemetry_device.compactCollection;
using telemetry_device_main.Enums;

namespace telemetry_device.Statistics.Sevirity
{
    class SeverityEvaluator
    {
        private StatisticsSeveritySettings _settings;
        public SeverityEvaluator(StatisticsSeveritySettings settings)
        {
            _settings = settings;
        }
        public StatisticsSeverity EvaluateSeverity(GlobalStatisticType type, int value)
        {
            switch(type)
            {
                case GlobalStatisticType.PacketDropRate:
                    return _settings.PacketDropRate.Evaluate(value);
                case GlobalStatisticType.SniffingTime:
                    return _settings.SniffingTime.Evaluate(value);
            }
            return StatisticsSeverity.Normal;
        }
        public StatisticsSeverity EvaluateSeverity(IcdStatisticType type, int value)
        {
            switch (type)
            {
                case IcdStatisticType.CorruptedPacket:
                    return _settings.CorruptedPacket.Evaluate(value);
                case IcdStatisticType.DecryptTime:
                    return _settings.DecryptTime.Evaluate(value);
                case IcdStatisticType.KafkaUploadTime:
                    return _settings.KafkaUploadTime.Evaluate(value);
            }
            return StatisticsSeverity.Normal;
        }
    }
}
