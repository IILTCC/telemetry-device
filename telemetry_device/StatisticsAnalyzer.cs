using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using telemetry_device.compactCollection;
namespace telemetry_device
{
    class StatisticsAnalyzer
    {
        private static StatisticsAnalyzer _instance;
        private Dictionary<MetricType,Metric> _metrics;
        private StatisticsAnalyzer()
        {
            _metrics = new Dictionary<MetricType, Metric>();
            foreach(MetricType statType in Enum.GetValues(typeof(MetricType)))
            {
                _metrics.Add(statType, new Metric());
            }
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
        public void UpdateMetric(MetricType metric,int value)
        {
            _metrics[metric].AddCounter(value);
        }
        public Dictionary<MetricType,float> GetDataDictionary()
        {
            Dictionary<MetricType, float> avgDict = new Dictionary<MetricType, float>();
            foreach(MetricType key in _metrics.Keys)
            {
                avgDict.Add(key, _metrics[key].GetAvg());
            }
            return avgDict;
        }
    }
}
