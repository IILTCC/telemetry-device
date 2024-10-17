using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device
{
    class Startup
    {
        private static IConfigurationRoot _configFile;
        private static TelemetryDeviceSettings _telemetryDeviceSettings;
        static async Task Main(string[] args)
        {
            _configFile = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile(ConfigPaths.AppSettingsName, optional: false, reloadOnChange: true)
                   .Build();

            _telemetryDeviceSettings = _configFile.GetRequiredSection(ConfigPaths.TopLevelSettingsName).Get<TelemetryDeviceSettings>();

            TelemetryDevice telemetryDevice = new TelemetryDevice(_telemetryDeviceSettings);
            await telemetryDevice.RunAsync();
        }

    }
}
