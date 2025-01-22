using System.Collections.Generic;

namespace telemetry_device_main.decodeor
{
    public interface IDecodePacket
    {
        public Dictionary<string, (int, bool)> DecodePacket(byte[] packet);
    }
}
