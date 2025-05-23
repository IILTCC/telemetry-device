﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using telemetry_device_main.Enums;
using telemetry_device_main.icds;

namespace telemetry_device_main.decodeor
{
    public abstract class BasePacketGenerator<IcdType> : IDecodePacket where IcdType : IParameterIcd
    {
        protected List<IcdType> _icdRows;
        protected TelemetryLogger _logger;
        public BasePacketGenerator(string json)
        {
            _logger = TelemetryLogger.Instance;
            try
            {
                _icdRows = JsonConvert.DeserializeObject<List<IcdType>>(json);
            }
            catch (Exception ex)
            {
                _logger.LogFatal("Tried to deseralize icd -" + ex.Message,LogId.FatalDeseralize);
                return;
            }
            _logger.LogInfo("Succesfuly deserialized icd", LogId.Deseralized);
        }

        // takes a icd row the entire packet and returnes accurate byte array of correct length
        protected byte[] GetAccurateValue(IcdType row, byte[] packet)
        {
            int retValueSize = row.GetSize() / Consts.BYTE_LENGTH + (row.GetSize() % Consts.BYTE_LENGTH != 0 ? 1 : 0);
            byte[] retValue = new byte[retValueSize];
            for (int i = 0; i < retValue.Length; i++)
                retValue[i] = packet[row.GetLocation() + i];

            return retValue;
        }

        protected void CreateMask(string mask, ref byte rowValue)
        {
            if (mask == string.Empty)
                return;
            byte maskByte = Convert.ToByte(mask, Consts.MASK_BASE);

            rowValue = (byte)(rowValue & maskByte);

            while ((maskByte & 0b00000001) == 0)
            {
                rowValue = (byte)(rowValue >> 1);
                maskByte = (byte)(maskByte >> 1);
            }
        }

        protected int ConvertByteArrayToInt(byte[] byteArray, bool isNegative)
        {
            byte[] retvalue = new byte[Consts.INT32_SIZE];

            // if the integer is negative needs to be initialized with 1
            if (isNegative)
                retvalue = new byte[] { 255, 255, 255, 255 };
            int index = 0;
            foreach (byte oneByte in byteArray)
                retvalue[index++] = oneByte;
            return BitConverter.ToInt32(retvalue, 0);
        }

        protected bool IsNegative(IcdType row, byte[] rowValue)
        {
            if (row.GetMax() < 0 || row.GetMin() < 0)
                if ((rowValue[0] & 0b10000000) > 0) // checks msb
                    return true;
            return false;
        }

        protected bool CheckIfInRange(int value, IcdType row)
        {
            if (value <= row.GetMax() && value >= row.GetMin())
                return false;
            return true;
        }

        public abstract void GenerateParameters(List<IcdType> icdRows, ref Dictionary<string, (int paramValue, bool wasErrorFound)> icdParameters, byte[] packet);
        public int GetSingleValue(int rowNumber, byte[] packet)
        {
            if (_icdRows[rowNumber].GetLocation() == -1)
                return 0;

            int finalValue = GetdecodeedValue(_icdRows[rowNumber], packet);
            return finalValue;
        }
        public abstract bool ValidateSync(byte[] packet);
        public bool ValidateCheckSum(byte[] packet)
        {
            int sum = 0;
            for (int checkIndex = 0; checkIndex < Consts.CHECKSUM_TOTAL; checkIndex++)
                sum+=GetSingleValue(_icdRows.Count - checkIndex - 2,packet);
            
            if (GetSingleValue(_icdRows.Count - 1, packet) != sum)
                return false;
            return true;
        }
        public int GetdecodeedValue(IcdType icdType, byte[] packet)
        {
            byte[] rowValue = GetAccurateValue(icdType, packet);
            CreateMask(icdType.GetMask(), ref rowValue[Consts.MASK_BYTE_POSITION]);

            return ConvertByteArrayToInt(rowValue, IsNegative(icdType, rowValue));
        }
        public Dictionary<string, (int, bool)> DecodePacket(byte[] packet)
        {
            Dictionary<string, (int paramValue, bool wasErrorFound)> icdParameters = new Dictionary<string, (int, bool)>();

            GenerateParameters(_icdRows, ref icdParameters, packet);
            return icdParameters;
        }
    }
}
