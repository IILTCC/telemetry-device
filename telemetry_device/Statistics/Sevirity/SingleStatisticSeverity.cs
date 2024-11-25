using telemetry_device_main.Enums;

namespace telemetry_device.Statistics.Sevirity
{
    class SingleStatisticSeverity
    {
        public int GoodMin { get; set; }
        public int GoodMax { get; set; }
        public int BadMin { get; set; }
        public int BadMax { get; set; }        
        public int NormalMin { get; set; }
        public int NormalMax { get; set; }

        public StatisticsSeverity Evaluate(int value)
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
