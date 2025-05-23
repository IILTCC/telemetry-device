﻿using System.Collections.Generic;
using telemetry_device_main.icds;

namespace telemetry_device_main.decodeor
{
    public class FlightBoxdecodeor<IcdType> : BasePacketGenerator<IcdType> where IcdType:IParameterIcd
    {
        public FlightBoxdecodeor(string json) : base(json) { }

        public override void GenerateParameters(List<IcdType> icdRows, ref Dictionary<string, (int paramValue, bool wasErrorFound)> icdParameters, byte[] packet)
        {
            foreach (IcdType icdType in icdRows)
            {
                if (icdType.GetLocation() == -1 || (icdType.GetCorrValue() != -1))
                    continue;

                int finalValue = GetdecodeedValue(icdType, packet);

                icdParameters[icdType.GetName()] = (finalValue, CheckIfInRange(finalValue, icdType));
            }
        }
        public override bool ValidateSync(byte[] packet)
        {
            for (int icdIndex = 0; icdIndex < Consts.FLIGHTBOX_SYNC_SIZE; icdIndex++)
                if (GetSingleValue(icdIndex, packet) != _icdRows[icdIndex].GetMin())
                    return false;
            return true;
        }
    }
}
