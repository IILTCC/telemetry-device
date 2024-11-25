namespace telemetry_device.Statistics.Sevirity
{
    class StatisticsSeveritySettings
    {
        public MinMaxSeverity PacketDropRate { get; set; }
        public MinMaxSeverity KafkaUploadTime { get; set; }
        public MinMaxSeverity SniffingTime { get; set; }
        public MinMaxSeverity DecryptTime { get; set; }
        public MinMaxSeverity CorruptedPacket { get; set; }
    }
}
