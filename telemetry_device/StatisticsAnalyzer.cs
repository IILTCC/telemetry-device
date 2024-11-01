using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using telemetry_device.compactCollection;
using telemetry_device_main.icds;

namespace telemetry_device
{
    class StatisticsAnalyzer
    {
        private static StatisticsAnalyzer _instance;
        private Dictionary<SingleStatisticType,Statistic> _singleStatistics;
        private Dictionary<MultiStatisticType,MultiStatistic> _multiStatistics;
        private StatisticsAnalyzer()
        {
            _singleStatistics = new Dictionary<SingleStatisticType, Statistic>();
            _multiStatistics = new Dictionary<MultiStatisticType, MultiStatistic>();
            foreach(SingleStatisticType statType in Enum.GetValues(typeof(SingleStatisticType)))
                _singleStatistics.Add(statType, new Statistic());

            foreach (MultiStatisticType statType in Enum.GetValues(typeof(MultiStatisticType)))
                _multiStatistics.Add(statType, new MultiStatistic());
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
        public void UpdateStatistic(SingleStatisticType statType,int value)
        {
            _singleStatistics[statType].AddCounter(value);
        }
        public void UpdateStatistic(MultiStatisticType statType,IcdTypes icdType,int value)
        {
            _multiStatistics[statType].AddCounter(icdType, value);
        }
        public Dictionary<string,float> GetDataDictionary()
        {
            Dictionary<string, float> avgDict = new Dictionary<string, float>();
            foreach(SingleStatisticType key in _singleStatistics.Keys)
            {
                avgDict.Add(key.ToString(), _singleStatistics[key].GetAvg());
            }
            foreach(MultiStatisticType key in _multiStatistics.Keys)
            {
                foreach(IcdTypes icdType in Enum.GetValues(typeof(IcdTypes)))
                {
                    avgDict.Add(key.ToString()+"-"+icdType.ToString(),_multiStatistics[key].GetAvg(icdType));
                }
            }
            return avgDict;
        }
    }
}
