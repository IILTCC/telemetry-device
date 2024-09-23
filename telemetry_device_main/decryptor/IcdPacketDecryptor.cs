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

        // takes a icd row the entire packet and returnes accurate byte array of the required row
        private byte[] GetAccurateValue(IcdType row, byte[] packet)
        {
            byte[] retValue = new byte[row.GetSize() / 8 + (row.GetSize() % 8 != 0 ? 1 : 0)];
            for (int i = 0; i < retValue.Length; i++)
                retValue[i] = packet[row.GetLocation() + i];

            return retValue;
        }
        // masks the bit of required row
        private void ProcessMask(string mask,ref byte rowValue)
        {
            if (mask == string.Empty)
                return;
            byte maskByte = Convert.ToByte(mask, 2);

            // leave only required bits
            rowValue = (byte)(rowValue &maskByte);

            // push the bits to the start
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

        // determines if current value in packet is a negative value
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
            foreach (IcdType row in icdRows)
            {
                // get the exact byte array of the row
                byte[] rowValue = GetAccurateValue(row, packet);

                // mask the byte array
                ProcessMask(row.GetMask(), ref rowValue[0]);

                // add it to the dictionary
                icdParameters[row.GetName()] = (ConvertByteArrayToInt(rowValue, IsNegative(row,rowValue)),false);
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
            catch (Exception)
            {
                return null;
            }
            GenerateParameters(icdRows, ref icdParameters, packet);
            return icdParameters;
        }
    }
}
