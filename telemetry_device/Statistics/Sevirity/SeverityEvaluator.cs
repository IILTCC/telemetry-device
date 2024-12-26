using telemetry_device.compactCollection;
using telemetry_device.Settings;
using telemetry_device_main.Enums;

namespace telemetry_device.Statistics.Sevirity
{
    class SeverityEvaluator
    {
        private readonly StatisticsSeveritySettings _settings;
        public SeverityEvaluator()
        {
            ConfigProvider configProvider = ConfigProvider.Instance;
            _settings = configProvider.ProvideStatisticSeverity();
        }
        public StatisticsSeverity EvaluateSeverity(GlobalStatisticType type, float value)
        {
            switch(type)
            {
                case GlobalStatisticType.PacketDropRate:
                    return _settings.PacketDropRate.Evaluate(value);
                case GlobalStatisticType.SniffingTime:
                    return _settings.SniffingTime.Evaluate(value);
            }
            return StatisticsSeverity.Bad;
        }
        public StatisticsSeverity EvaluateSeverity(IcdStatisticType type, float value)
        {
            switch (type)
            {
                case IcdStatisticType.CorruptedPacket:
                    return _settings.CorruptedPacket.Evaluate(value);
                case IcdStatisticType.DecodeTime:
                    return _settings.DecodeTime.Evaluate(value);
                case IcdStatisticType.KafkaUploadTime:
                    return _settings.KafkaUploadTime.Evaluate(value);
            }
            return StatisticsSeverity.Bad;
        }
    }
}
