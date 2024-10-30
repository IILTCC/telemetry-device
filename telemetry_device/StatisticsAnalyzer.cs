using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device
{
    class StatisticsAnalyzer
    {
        private static StatisticsAnalyzer _instance;

        private StatisticsAnalyzer()
        {
        }
        public static StatisticsAnalyzer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StatisticsAnalyzer();
                }
                return _instance;
            }
        }
    }
}
