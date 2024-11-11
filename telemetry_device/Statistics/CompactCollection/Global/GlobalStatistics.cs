namespace telemetry_device.compactCollection
{
    class GlobalStatistics
    {
        private int _counter;
        private int _sum;
        public GlobalStatistics()
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
