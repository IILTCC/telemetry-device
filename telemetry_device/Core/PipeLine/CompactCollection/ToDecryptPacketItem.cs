using telemetry_device_main.icds;

namespace telemetry_device.compactCollection
{
    class ToDecodePacketItem
    {
        public IcdTypes PacketType { get; set; }
        public int PacketPort { get; set; }
        public byte[] PacketData { get; set; }
        public ToDecodePacketItem(IcdTypes packetType, byte[] packetData, int packetPort)
        {
            this.PacketPort = packetPort;
            this.PacketType = packetType;
            this.PacketData = packetData;
        }
    }
}
