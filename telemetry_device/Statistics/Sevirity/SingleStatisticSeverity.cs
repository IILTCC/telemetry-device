﻿using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using telemetry_device_main;
using telemetry_device_main.Enums;

namespace telemetry_device.Statistics.Sevirity
{
    class SingleStatisticSeverity
    {
        private Normal _normal;
        private List<double> _statisticValues;
        public SingleStatisticSeverity()
        {
            _normal = null;
            _statisticValues = new List<double>();
        }
        public void SetValues(List<double> values)
        {
            _statisticValues = values;
            _normal = new Normal(GetAvg(), GetStandardDev());
        }
        public double GetAvg()
        {
            double sum = 0;
            foreach (double val in _statisticValues)
                sum += val;
            if (_statisticValues.Count == 0)
                return 0;
            return sum / _statisticValues.Count;
        }
        public double GetStandardDev()
        {
            double avg = GetAvg();
            double topSum = 0;
            foreach (double val in _statisticValues)
                topSum += Math.Pow((val - avg), 2);
            return Math.Sqrt(topSum / (_statisticValues.Count - 1));
        }
        public bool CheckIfEqual() 
        {
            for(int valueIndex = 1; valueIndex<_statisticValues.Count; valueIndex++)
                if (_statisticValues[valueIndex - 1] != _statisticValues[valueIndex])
                    return false;
            
            return true;
        }
        public StatisticsSeverity EvalSevirity(double value)
        {
            if (_normal == null)
                return StatisticsSeverity.Bad;
            if (CheckIfEqual())
                return StatisticsSeverity.Good;
            if (_normal.CumulativeDistribution(value) < Consts.STATISTICS_LOWER_BOUND_NORMAL)
                return StatisticsSeverity.Good;
            else if (_normal.CumulativeDistribution(value) < Consts.STATISTICS_UPPER_BOUND_NORMAL)
                return StatisticsSeverity.Warn;
            return StatisticsSeverity.Bad;
        }
    }
}
