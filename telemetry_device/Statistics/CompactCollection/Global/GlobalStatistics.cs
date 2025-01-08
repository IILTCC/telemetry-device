using System.Collections.Generic;
using System.Threading.Tasks;
using telemetry_device.Statistics.Sevirity;
using telemetry_device_main;
using telemetry_device_main.Enums;

namespace telemetry_device.compactCollection
{
    class GlobalStatistics
    {
        private double _sum;
        private double _counter;
        private List<double> _statisticValues;
        private SingleStatisticSeverity _sevirity;
        public GlobalStatistics()
        {
            _statisticValues = new List<double>();
            _sevirity = new SingleStatisticSeverity();
            _sum = 0;
            _counter = 0;
            Task.Run(() => Loop());
            Task.Run(()=>PullSaved());
        }
        public void AddValue(double val)
        {
            _sum += val;
            _counter++;
        }   
        public double GetLast()
        {
            if (_statisticValues.Count == 0)
                return 0;
            return _statisticValues[_statisticValues.Count-1];
        }
        public async Task PullSaved()
        {
            while (true)
            {
                await Task.Delay(Consts.STATISTICS_UPDATE_DELAY);
                if (_counter == 0)
                    _statisticValues.Add(Consts.STATISTICS_NO_ITEM_SAVED);
                else _statisticValues.Add(_sum / _counter);
            }
        }
        public async Task Loop()
        {
            while (true)
            {
                await Task.Delay(Consts.STATISTICS_RESET_DELAY);
                RestartLoop();
            }
        }
        public void RestartLoop()
        {
            _sevirity.SetValues(_statisticValues);
            _statisticValues = new List<double>();
            _sum = 0;
            _counter = 0;
        }
        public StatisticsSeverity EvalSevirity(double value)
        {
            return _sevirity.EvalSevirity(value);
        }
    }
}
