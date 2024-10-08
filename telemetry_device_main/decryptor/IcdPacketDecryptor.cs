using Newtonsoft.Json;
using simulator_main.icd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device_main.decryptor
{
    public class IcdPacketDecryptor<IcdType> where IcdType : IBaseIcd
    {
        public IcdPacketDecryptor() { }
        const int BYTE_LENGTH = 8;

        // takes a icd row the entire packet and returnes accurate byte array of correct length
        private byte[] GetAccurateValue(IcdType row, byte[] packet)
        {
            int retValueSize = row.GetSize() / BYTE_LENGTH + (row.GetSize() % BYTE_LENGTH != 0 ? 1 : 0);
            byte[] retValue = new byte[retValueSize];
            for (int i = 0; i < retValue.Length; i++)
                retValue[i] = packet[row.GetLocation() + i];

            return retValue;
        }
        private void CreateMask(string mask,ref byte rowValue)
        {
            if (mask == string.Empty)
                return;
            byte maskByte = Convert.ToByte(mask, 2);

            rowValue = (byte)(rowValue &maskByte);

            while((maskByte & 0b00000001) == 0 )
            {
                rowValue = (byte)(rowValue >> 1);
                maskByte = (byte)(maskByte >> 1);
            }
        }
        private int ConvertByteArrayToInt(byte[] byteArray,bool isNegative)
        {
            // base for int 32 requires 4 byte array
            byte[] retvalue = new byte[4];

            // if the integer is negative needs to be initialized with 1
            if(isNegative)
                retvalue = new byte[]{ 255,255,255,255};
            int index = 0;
            foreach (byte oneByte in byteArray)
                retvalue[index++] = oneByte;
            return BitConverter.ToInt32(retvalue,0);
        }

        private bool IsNegative(IcdType row,byte[] rowValue)
        {
            // cheks if icd is sigend or unsigned
            if (row.GetMax() < 0 || row.GetMin() < 0)
                if ((rowValue[0] & 0b10000000) >0) // checks msb
                    return true;
            return false;
        }
        // generates the entire dictionary
        private void GenerateParameters(List<IcdType> icdRows, ref Dictionary<string, (int, bool)> icdParameters, byte[] packet)
        {
            int corValue = -1;
            foreach (IcdType icdType in icdRows)
            {
                if (icdType.GetLocation() == -1 || (icdType.GetCorrValue() != -1 && corValue != icdType.GetCorrValue()))
                    continue;

                byte[] rowValue = GetAccurateValue(icdType, packet);
                CreateMask(icdType.GetMask(), ref rowValue[0]);

                if (icdType.IsRowCorIdentifier())
                    corValue = ConvertByteArrayToInt(rowValue, IsNegative(icdType, rowValue));
                
                icdParameters[icdType.GetName()] = (ConvertByteArrayToInt(rowValue, IsNegative(icdType, rowValue)), false);
            }
        }

        public Dictionary<string,(int,bool)> DecryptPacket(byte[] packet, string json)
        {
            // bool in dictionary is for error detection
            Dictionary<string, (int,bool)> icdParameters = new Dictionary<string, (int,bool)>();
            List<IcdType> icdRows;
            try
            {
                icdRows = JsonConvert.DeserializeObject<List<IcdType>>(json);
            }
            catch (Exception ex)
            {
                return null;
            }

            GenerateParameters(icdRows, ref icdParameters, packet);

            return icdParameters;
        }
    }
}
