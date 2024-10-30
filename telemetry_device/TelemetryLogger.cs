using NLog;
namespace telemetry_device
{
    public class TelemetryLogger
    {
        private static TelemetryLogger _instance;

        private Logger _logger;

        private TelemetryLogger()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }
        public static TelemetryLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TelemetryLogger();
                }
                return _instance;
            }
        }
        public void LogInfo(string log)
        {
            _logger.Info(log);
        }
        public void LogWarn(string log)
        {
            _logger.Warn(log);
        }
        public void LogFatal(string log)
        {
            _logger.Fatal(log);
        }
        public void LogError(string log)
        {
            _logger.Error(log);
        }
    }
}
