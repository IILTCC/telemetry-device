using System.Collections.Generic;
using System.Threading.Tasks;
using telemetry_device.Statistics.Sevirity;
using telemetry_device_main.Enums;

namespace telemetry_device.compactCollection
{
    class GlobalStatistics
    {
        private List<double> _statisticValues;
        private SingleStatisticSeverity _sevirity;
        public GlobalStatistics()
        {
            _statisticValues = new List<double>();
            _sevirity = new SingleStatisticSeverity();
            Task.Run(() => Loop());

        }
        public void AddValue(double val)
        {
            _statisticValues.Add(val);
        }   
        //public double GetAvg()
        //{
        //    if (_statisticValues.Count == 0)
        //        return 0;
        //    double sum = 0;
        //    foreach (double val in _statisticValues)
        //        sum += val;
        //    return sum / _statisticValues.Count;
        //}
        public double GetLast()
        {
            return _statisticValues[_statisticValues.Count-1];
        }
        public async Task Loop()
        {
            while (true)
            {
                await Task.Delay(1000);
                RestartLoop();
            }
        }
        public void RestartLoop()
        {
            _sevirity.SetValues(_statisticValues);
            _statisticValues = new List<double>();
        }
        public StatisticsSeverity EvalSevirity(double value)
        {
            return _sevirity.EvalSevirity(value);
        }
    }
}
