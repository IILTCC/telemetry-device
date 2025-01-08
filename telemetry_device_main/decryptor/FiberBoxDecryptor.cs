using System.Collections.Generic;
using System.Threading;
using telemetry_device_main.icds;

namespace telemetry_device_main.decodeor
{
    public class FiberBoxdecodeor<IcdType> : BasePacketGenerator<IcdType> where IcdType : IParameterIcd
    {
        public FiberBoxdecodeor(string json) : base(json) { }

        public override void GenerateParameters(List<IcdType> icdRows, ref Dictionary<string, (int paramValue, bool wasErrorFound)> icdParameters, byte[] packet)
        {
            int corValue = -1;
            foreach (IcdType icdType in icdRows)
            {
                if (icdType.GetLocation() == -1 || (icdType.GetCorrValue() != -1 && corValue != icdType.GetCorrValue()))
                    continue;

                int finalValue = GetdecodeedValue(icdType, packet);

                if (icdType.IsRowCorIdentifier())
                    corValue = finalValue;

                icdParameters[icdType.GetName()] = (finalValue, CheckIfInRange(finalValue, icdType));
            }
            Thread.Sleep(30);
        }
    }
}
