using telemetry_device_main.Enums;

namespace telemetry_device.Statistics.Sevirity
{
    class SingleStatisticSeverity
    {
        public float GoodWarnSplit { get; set; }
        public float WarnBadSplit { get; set; }
        public bool IsHigherBetter { get; set; }

        public StatisticsSeverity Evaluate(float value)
        {
            if(IsHigherBetter)
            {
                if (value > GoodWarnSplit)
                    return StatisticsSeverity.Good;
                if (value < GoodWarnSplit && value >= WarnBadSplit)
                    return StatisticsSeverity.Warn;
            }
            else
            {
                if (value <= GoodWarnSplit)
                    return StatisticsSeverity.Good;
                if (value > GoodWarnSplit && value <= WarnBadSplit)
                    return StatisticsSeverity.Warn;
            }
            return StatisticsSeverity.Bad;
        }
    }
}
