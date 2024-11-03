using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using telemetry_device_main.icds;

namespace telemetry_device.compactCollection
{
    class IcdStatistics
    {
        private Dictionary<IcdTypes, GlobalStatistics> _statistics;
        public IcdStatistics()
        {
            _statistics = new Dictionary<IcdTypes, GlobalStatistics>();
            foreach(IcdTypes icdType in Enum.GetValues(typeof(IcdTypes)))
            {
                _statistics.Add(icdType, new GlobalStatistics());
            }
        }
        public void AddCounter(IcdTypes icdType,int increment)
        {
            _statistics[icdType].AddCounter(increment);
        }
        public float GetAvg(IcdTypes icdType)
        {
            return _statistics[icdType].GetAvg();
        }
    }
}
