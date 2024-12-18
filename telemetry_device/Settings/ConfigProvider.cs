using HealthCheck;
using Microsoft.Extensions.Configuration;
using System.IO;
using telemetry_device.Statistics.Sevirity;
using telemetry_device_main;

namespace telemetry_device.Settings
{
    class ConfigProvider
    {
        private static ConfigProvider _instance;
        private static IConfigurationRoot _configFile;
        private TelemetryDeviceSettings _telemetryDeviceSettings;
        private StatisticsSeveritySettings _statisicsSeveritySettings;
        private HealthCheckSettings _healthCheckSettings;
        public static ConfigProvider Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConfigProvider();
                return _instance;
            }
        }
        public ConfigProvider()
        {
            _configFile = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(Consts.APPSETTINGS_PATH, optional: false, reloadOnChange: true)
            .Build();
            _telemetryDeviceSettings = _configFile.GetRequiredSection(nameof(TelemetryDeviceSettings)).Get<TelemetryDeviceSettings>();
            _statisicsSeveritySettings = _configFile.GetRequiredSection(nameof(StatisticsSeveritySettings)).Get<StatisticsSeveritySettings>();
            _healthCheckSettings = _configFile.GetRequiredSection(nameof(HealthCheckSettings)).Get<HealthCheckSettings>();
        }
        public TelemetryDeviceSettings ProvideTelemetrySettings()
        {
            return _telemetryDeviceSettings;
        }
        public StatisticsSeveritySettings ProvideStatisticSeverity()
        {
            return _statisicsSeveritySettings;
        }
        public HealthCheckSettings ProvideHealthCheckSettings()
        {
            return _healthCheckSettings;
        }
    }
}
