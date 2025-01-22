using telemetry_device_main.Enums;

namespace telemetry_device.Statistics.CompactCollection
{
    class StatisticsDictionaryValue
    {
        public StatisticsSeverity Sevirity { get; set; }
        public double Value { get; set; }
        public StatisticsDictionaryValue(StatisticsSeverity sevirity, double value)
        {
            Sevirity = sevirity;
            Value = value;
        }
    }
}
