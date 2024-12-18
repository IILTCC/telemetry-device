﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using telemetry_device_main.icds;

namespace telemetry_device_main.decryptor
{
    public class FiberBoxDecryptor<IcdType> : BasePacketGenerator<IcdType> where IcdType : IParameterIcd
    {
        public FiberBoxDecryptor(string json) : base(json) { }

        public override void GenerateParameters(List<IcdType> icdRows, ref Dictionary<string, (int paramValue, bool wasErrorFound)> icdParameters, byte[] packet)
        {
            int corValue = -1;
            foreach (IcdType icdType in icdRows)
            {
                if (icdType.GetLocation() == -1 || (icdType.GetCorrValue() != -1 && corValue != icdType.GetCorrValue()))
                    continue;

                int finalValue = GetDecryptedValue(icdType, packet);

                if (icdType.IsRowCorIdentifier())
                    corValue = finalValue;

                icdParameters[icdType.GetName()] = (finalValue, CheckIfInRange(finalValue, icdType));
            }
        }
    }
}
