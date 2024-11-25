using telemetry_device_main.Enums;

namespace telemetry_device.Statistics.Sevirity
{
    class SingleStatisticSeverity
    {
        public float GoodMin { get; set; }
        public float GoodMax { get; set; }
        public float BadMin { get; set; }
        public float BadMax { get; set; }        
        public float NormalMin { get; set; }
        public float NormalMax { get; set; }

        public StatisticsSeverity Evaluate(float value)
        {
            if (value >= GoodMin && value < GoodMax)
                return StatisticsSeverity.Good;
            if (value >= BadMin && value < BadMax)
                return StatisticsSeverity.Bad;
            if (value >= NormalMin && value < NormalMax)
                return StatisticsSeverity.Normal;
            return StatisticsSeverity.Normal;
        }
    }
}
