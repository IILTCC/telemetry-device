using System;
using System.Collections.Generic;
using telemetry_device_main.Enums;
using telemetry_device_main.icds;

namespace telemetry_device.compactCollection
{
    class IcdStatistics
    {
        private readonly Dictionary<IcdTypes, GlobalStatistics> _statistics;
        public IcdStatistics()
        {
            _statistics = new Dictionary<IcdTypes, GlobalStatistics>();
            foreach(IcdTypes icdType in Enum.GetValues(typeof(IcdTypes)))
            {
                _statistics.Add(icdType, new GlobalStatistics());
            }
        }

        public void AddValue(IcdTypes icdType,double value)
        {
            _statistics[icdType].AddValue(value);
        }

        public double GetLast(IcdTypes icdType)
        {
            return _statistics[icdType].GetLastValue();
        }
        public StatisticsSeverity EvalSevirity(IcdTypes icdType, double value)
        {
            return _statistics[icdType].EvalSevirity(value);
        }
    }
}
