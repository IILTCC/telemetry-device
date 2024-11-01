using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device.compactCollection
{
    class Statistic
    {
        private int _counter;
        private int _sum;
        public Statistic()
        {
            _counter = 0;
            _sum = 0;
        }
        public void AddCounter(int increment)
        {
            _counter++;
            _sum += increment;
        }   
        public float GetAvg()
        {
            if (_counter == 0)
                return 0;
            return (float)_sum / _counter;
        }
    }
}
