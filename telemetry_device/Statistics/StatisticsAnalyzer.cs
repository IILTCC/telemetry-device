using System;
using System.Collections.Generic;
using telemetry_device.compactCollection;
using telemetry_device.Statistics.CompactCollection;
using telemetry_device.Statistics.Sevirity;
using telemetry_device_main.icds;

namespace telemetry_device
{
    class StatisticsAnalyzer
    {
        private static StatisticsAnalyzer _instance;
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
        private readonly Dictionary<GlobalStatisticType,GlobalStatistics> _globalStatistics;
        private readonly Dictionary<IcdStatisticType,IcdStatistics> _icdStatistics;
        private StatisticsAnalyzer()
        {
            _globalStatistics = new Dictionary<GlobalStatisticType, GlobalStatistics>();
            _icdStatistics = new Dictionary<IcdStatisticType, IcdStatistics>();

            InitializeDicts();
        }

        private void InitializeDicts()
        {
            foreach (GlobalStatisticType statType in Enum.GetValues(typeof(GlobalStatisticType)))
                _globalStatistics.Add(statType, new GlobalStatistics());
            foreach (IcdStatisticType statType in Enum.GetValues(typeof(IcdStatisticType)))
                _icdStatistics.Add(statType, new IcdStatistics());
        }

        public void UpdateStatistic(GlobalStatisticType statType,double value)
        {
            _globalStatistics[statType].AddValue(value);
        }

        public void UpdateStatistic(IcdStatisticType statType,IcdTypes icdType,double value)
        {
            _icdStatistics[statType].AddValue(icdType, value);
        }

        public Dictionary<StatisticDictionaryKey, StatisticsDictionaryValue> GetDataDictionary()
        {
            Dictionary<StatisticDictionaryKey, StatisticsDictionaryValue> avgDict = new Dictionary<StatisticDictionaryKey, StatisticsDictionaryValue>();
            foreach(GlobalStatisticType key in _globalStatistics.Keys)
            {
                StatisticDictionaryKey dictionaryKey = new StatisticDictionaryKey(key);
                double lastValue = _globalStatistics[key].GetLastValue();
                StatisticsDictionaryValue dictionaryValue = new StatisticsDictionaryValue( _globalStatistics[key].EvalSevirity(lastValue), lastValue);
                avgDict.Add(dictionaryKey, dictionaryValue);
            }

            foreach(IcdStatisticType key in _icdStatistics.Keys)
                foreach(IcdTypes icdType in Enum.GetValues(typeof(IcdTypes)))
                {
                    StatisticDictionaryKey dictionaryKey = new StatisticDictionaryKey(icdType,key);
                    double lastValue = _icdStatistics[key].GetLast(icdType);
                    StatisticsDictionaryValue dictionaryValue = new StatisticsDictionaryValue(_icdStatistics[key].EvalSevirity(icdType, lastValue), lastValue);
                    avgDict.Add(dictionaryKey, dictionaryValue);
                }
            
            return avgDict;
        }
    }
}
