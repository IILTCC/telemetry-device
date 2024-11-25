using telemetry_device_main.Enums;

namespace telemetry_device.Statistics.CompactCollection
{
    class StatisticsDictionaryValue
    {
        public StatisticsSeverity Sevirity { get; set; }
        public float Value { get; set; }
        public StatisticsDictionaryValue(StatisticsSeverity sevirity, float value)
        {
            Sevirity = sevirity;
            Value = value;
        }
    }
}
