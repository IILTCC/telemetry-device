using System.Collections.Generic;
using telemetry_device_main.icds;

namespace telemetry_device_main.decryptor
{
    public class FlightBoxDecryptor<IcdType> : BasePacketGenerator<IcdType> where IcdType:IParameterIcd
    {
        public FlightBoxDecryptor(string json) : base(json) { }

        public override void GenerateParameters(List<IcdType> icdRows, ref Dictionary<string, (int paramValue, bool wasErrorFound)> icdParameters, byte[] packet)
        {
            foreach (IcdType icdType in icdRows)
            {
                if (icdType.GetLocation() == -1 || (icdType.GetCorrValue() != -1))
                    continue;

                int finalValue = GetFinalValue(icdType, packet);

                icdParameters[icdType.GetName()] = (finalValue, CheckIfInRange(finalValue, icdType));
            }
        }
    }
}
