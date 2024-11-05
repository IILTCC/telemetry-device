namespace telemetry_device
{
    class TelemetryDeviceSettings
    {
        public int SimulatorDestPort { get; set; }
        public int TelemetryReadTimeout { get; set; }
        public string KafkaUrl { get; set; }
    }
}
