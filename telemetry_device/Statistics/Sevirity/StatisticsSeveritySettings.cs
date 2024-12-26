namespace telemetry_device.Statistics.Sevirity
{
    class StatisticsSeveritySettings
    {
        public SingleStatisticSeverity PacketDropRate { get; set; }
        public SingleStatisticSeverity KafkaUploadTime { get; set; }
        public SingleStatisticSeverity SniffingTime { get; set; }
        public SingleStatisticSeverity DecodeTime { get; set; }
        public SingleStatisticSeverity CorruptedPacket { get; set; }
    }
}
