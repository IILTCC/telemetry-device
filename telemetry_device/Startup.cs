using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device
{
    class Startup
    {
        static async Task Main(string[] args)
        {
            TelemetryDevice telemetryDevice = new TelemetryDevice();
            await telemetryDevice.RunAsync();
        }

    }
}
