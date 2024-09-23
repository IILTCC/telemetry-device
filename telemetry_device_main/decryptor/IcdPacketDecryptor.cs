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
        public IcdPacketDecryptor()
        {

        }

        public void printByteArray(byte[] array)
        {
            foreach (var item in array)
            {
                Console.Write(Convert.ToString(item, 2).PadLeft(8, '0')+" ");
            }
            Console.WriteLine();
        }

        public byte[] StringToByteArray(string packet)
        {
            byte[] retValue = new byte[packet.Length / 8];
            string temp = "";
            int counter = 0;
            foreach (var item in packet)
            {
                temp += item;
                if (temp.Length == 8)
                {
                    retValue[counter++] = Convert.ToByte(temp, 2);
                    temp = "";
                }
            }
            return retValue;
        }
        private byte[] GetAccurateValue(IcdType row, byte[] packet)
        {
           
            byte[] retValue = new byte[row.GetSize() / 8 + (row.GetSize() % 8 != 0 ? 1 : 0)];
            for (int i = 0; i < retValue.Length; i++)
                retValue[i] = packet[row.GetLocation() + i];

            return retValue;
        }
        private void ProcessMask(string mask,ref byte rowValue)
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
        private int ConvertByteArrayToInt(byte[] byteArray,bool isSigned)
        {
            byte[] retvalue = new byte[4];
            if(isSigned)
                retvalue = new byte[]{ 255,255,255,255};
            int index = 0;
            foreach (byte oneByte in byteArray)
                retvalue[index++] = oneByte;
            return BitConverter.ToInt32(retvalue,0);

        }

        private void GenerateParameters(List<IcdType> icdRows, ref Dictionary<string, (int, bool)> icdParameters, byte[] packet)
        {
            foreach (IcdType row in icdRows)
            {
                Console.WriteLine("--------------------");
                byte[] rowValue = GetAccurateValue(row, packet);
                Console.WriteLine("id " + row.GetRowId());
                Console.WriteLine("name "+row.GetName());
                Console.WriteLine("size " + row.GetSize());
                Console.WriteLine("min " + row.GetMin());
                Console.WriteLine("max " + row.GetMax());
                Console.Write("mask ");
                printByteArray(StringToByteArray(row.GetMask()));
                printByteArray(rowValue);
                ProcessMask(row.GetMask(), ref rowValue[0]);
                Console.Write("after mask ");
                printByteArray(rowValue);
                Console.WriteLine(ConvertByteArrayToInt(rowValue, row.GetMin() < 0|| row.GetMax()<0));
                //Console.WriteLine("final"+ BitConverter.ToInt32(rowValue,0));
                icdParameters[row.GetName()] = (ConvertByteArrayToInt(rowValue, row.GetMin() < 0 || row.GetMax() < 0),false);

            }
        }
        public void PrintDictionary(Dictionary<string,(int,bool)> dict)
        {
            foreach(var item in dict.Keys)
            {
                Console.WriteLine("key "+item +" equals "+dict[item].Item1+" "+dict[item].Item2);
            }
        }
            

        public Dictionary<string,(int,bool)> DecryptPacket(byte[] packet, string json)
        {
            Console.WriteLine("packet size " + packet.Length);
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
            PrintDictionary(icdParameters);
            return icdParameters;
        }
    }
}
