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
        public static StatisticsAnalyzer Instance(StatisticsSeveritySettings statisticsSeveritySettings)
        {
            {
                if (_instance == null)
                {
                    _instance = new StatisticsAnalyzer(statisticsSeveritySettings);
                }
                return _instance;
            }
        }
        private readonly Dictionary<GlobalStatisticType,GlobalStatistics> _globalStatistics;
        private readonly Dictionary<IcdStatisticType,IcdStatistics> _icdStatistics;
        private readonly SeverityEvaluator _severityEvaluator;
        private StatisticsAnalyzer(StatisticsSeveritySettings statisticsSeveritySettings)
        {
            _severityEvaluator = new SeverityEvaluator(statisticsSeveritySettings);
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

        public void UpdateStatistic(GlobalStatisticType statType,int value)
        {
            _globalStatistics[statType].AddCounter(value);
        }

        public void UpdateStatistic(IcdStatisticType statType,IcdTypes icdType,int value)
        {
            _icdStatistics[statType].AddCounter(icdType, value);
        }

        public Dictionary<StatisticDictionaryKey, StatisticsDictionaryValue> GetDataDictionary()
        {
            Dictionary<StatisticDictionaryKey, StatisticsDictionaryValue> avgDict = new Dictionary<StatisticDictionaryKey, StatisticsDictionaryValue>();
            foreach(GlobalStatisticType key in _globalStatistics.Keys)
            {
                StatisticDictionaryKey dictionaryKey = new StatisticDictionaryKey(key);
                float currentAvg = _globalStatistics[key].GetAvg();
                StatisticsDictionaryValue dictionaryValue = new StatisticsDictionaryValue( _severityEvaluator.EvaluateSeverity(key,(int)currentAvg), currentAvg);
                avgDict.Add(dictionaryKey, dictionaryValue);
            }

            foreach(IcdStatisticType key in _icdStatistics.Keys)
                foreach(IcdTypes icdType in Enum.GetValues(typeof(IcdTypes)))
                {
                    //StatisticDictionaryKey dictionaryKey = new StatisticDictionaryKey(icdType,key);
                    //avgDict.Add(dictionaryKey,_icdStatistics[key].GetAvg(icdType));
                    StatisticDictionaryKey dictionaryKey = new StatisticDictionaryKey(icdType,key);
                    float currentAvg = _icdStatistics[key].GetAvg(icdType);
                    StatisticsDictionaryValue dictionaryValue = new StatisticsDictionaryValue(_severityEvaluator.EvaluateSeverity(key, (int)currentAvg), currentAvg);
                    avgDict.Add(dictionaryKey, dictionaryValue);
                }
            
            return avgDict;
        }
    }
}
