using NLog;
using System;

namespace telemetry_device_main
{
    public class DecryptorLogger
    {
        private static DecryptorLogger _instance;

        private Logger _logger;

        private DecryptorLogger()
        {
            //LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(@"../../../../telemetry_device_main/bin\Debug\net5.0/nlog.config", true);
            _logger = LogManager.GetCurrentClassLogger();
        }
        public static DecryptorLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DecryptorLogger();
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
