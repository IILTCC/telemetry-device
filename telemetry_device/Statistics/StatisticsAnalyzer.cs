using System;
using System.Collections.Generic;
using telemetry_device.compactCollection;
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

        public void UpdateStatistic(GlobalStatisticType statType,int value)
        {
            _globalStatistics[statType].AddCounter(value);
        }

        public void UpdateStatistic(IcdStatisticType statType,IcdTypes icdType,int value)
        {
            _icdStatistics[statType].AddCounter(icdType, value);
        }

        public Dictionary<StatisticDictionaryKey, float> GetDataDictionary()
        {
            Dictionary<StatisticDictionaryKey, float> avgDict = new Dictionary<StatisticDictionaryKey, float>();
            foreach(GlobalStatisticType key in _globalStatistics.Keys)
            {
                StatisticDictionaryKey dictionaryKey = new StatisticDictionaryKey(key);
                avgDict.Add(dictionaryKey, _globalStatistics[key].GetAvg());
            }

            foreach(IcdStatisticType key in _icdStatistics.Keys)
                foreach(IcdTypes icdType in Enum.GetValues(typeof(IcdTypes)))
                {
                    StatisticDictionaryKey dictionaryKey = new StatisticDictionaryKey(icdType,key);
                    avgDict.Add(dictionaryKey,_icdStatistics[key].GetAvg(icdType));
                }
            
            return avgDict;
        }
    }
}
