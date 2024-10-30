using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device
{
    class TelemetryDeviceSettings
    {
        public int SimulatorDestPort { get; set; }
        public int TelemetryReadTimeout { get; set; }
        public string KafkaUrl { get; set; }

    }
}
