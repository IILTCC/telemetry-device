namespace telemetry_device.compactCollection
{
    enum IcdStatisticType
    {
        CorruptedPacket, // precentage of packets containing at least one corrupted header
        DecodeTime, // in milliseconds
        KafkaUploadTime // in miliseconds
    }
}
