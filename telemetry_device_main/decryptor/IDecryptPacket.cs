using System.Collections.Generic;

namespace telemetry_device_main.decryptor
{
    public interface IDecryptPacket
    {
        public Dictionary<string, (int, bool)> DecryptPacket(byte[] packet);
    }
}
