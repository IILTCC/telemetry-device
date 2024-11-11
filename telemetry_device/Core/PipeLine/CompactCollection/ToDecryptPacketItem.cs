using telemetry_device_main.icds;

namespace telemetry_device.compactCollection
{
    class ToDecryptPacketItem
    {
        public IcdTypes PacketType { get; set; }
        public byte[] PacketData { get; set; }
        public ToDecryptPacketItem(IcdTypes packetType, byte[] packetData)
        {
            this.PacketType = packetType;
            this.PacketData = packetData;
        }
    }
}
