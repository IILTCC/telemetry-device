using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using telemetry_device.Statistics.Sevirity;

namespace telemetry_device
{
    class Startup
    {
        private static IConfigurationRoot _configFile;
        private static TelemetryDeviceSettings _telemetryDeviceSettings;
        private static StatisticsSeveritySettings _severitySettings;
        static async Task Main(string[] args)
        {
            _configFile = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile(ConfigPaths.AppSettingsName, optional: false, reloadOnChange: true)
                   .Build();

            _telemetryDeviceSettings = _configFile.GetRequiredSection(ConfigPaths.TopLevelSettingsName).Get<TelemetryDeviceSettings>();
            _severitySettings = _configFile.GetRequiredSection(nameof(StatisticsSeveritySettings)).Get<StatisticsSeveritySettings>();
            System.Console.WriteLine(_severitySettings);
            TelemetryDevice telemetryDevice = new TelemetryDevice(_telemetryDeviceSettings,_severitySettings);
            await telemetryDevice.RunAsync();
        }

    }
}
