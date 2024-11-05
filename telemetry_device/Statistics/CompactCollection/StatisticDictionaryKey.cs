using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using telemetry_device_main.icds;

namespace telemetry_device.compactCollection
{
    class StatisticDictionaryKey
    {
        private IcdTypes _icdType;
        private IcdStatisticType _icdStatisticType;
        private GlobalStatisticType ?_globalStatisticType;
        public StatisticDictionaryKey(IcdTypes icdType,IcdStatisticType icdStatisticType)
        {
            _icdType = icdType;
            _icdStatisticType = icdStatisticType;
            _globalStatisticType = null;
        }
        public StatisticDictionaryKey(GlobalStatisticType globalStatisticType)
        {
            _globalStatisticType = globalStatisticType;
        }
        public override string ToString()
        {
            if (_globalStatisticType == null)
                return _icdType.ToString() + " " + _icdStatisticType.ToString();
            else return _globalStatisticType.ToString();
        }
    }
}
