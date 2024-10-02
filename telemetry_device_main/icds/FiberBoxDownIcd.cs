using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using telemetry_device_main.icds;

namespace simulator_main.icd
{
    public class FiberBoxDownIcd : IBaseIcd
    {
        public int Id { get; set; }
        public string Error { get; set; }
        public string Location { get; set; }
        public string CorrValue { get; set; }
        public string Mask { get; set; }
        public string Identifier { get; set; }
        public string Type { get; set; }
        public string Units { get; set; }
        public string PhysicalLimitMin { get; set; }
        public string PhysicalLimitMax { get; set; }
        public int PhysicalLimitDef { get; set; }
        public string InterfaceType { get; set; }
        public int Size { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int Length { get; set; }
        public string Enum { get; set; }

        public int GetRowId() { return this.Id; }

        public int GetLocation()
        {
            if (this.Location == string.Empty)
                return -1;
            return Int32.Parse(this.Location); 
        }
        public string GetMask() 
        {
            // the use of temp is to no change the original value so you can
            // call multiple times this function

            string retValue = this.Mask;
            // remove '' before and after the mask
            if (retValue != string.Empty)
                retValue = retValue.Substring(1, this.Mask.Length - 2);
            return retValue;
        }
        public int GetSize() { return this.Size; }
        public int GetMin() { return this.Min; }
        public int GetMax() { return this.Max; }
        public string GetName() { return this.Identifier; }
        public int GetCorrValue() 
        {
            if (this.CorrValue == string.Empty)
                return -1;

            // the use of temp is to no change the original value so you can
            // call multiple times this function
            string retValue = this.CorrValue;

            // remove ' before and after the corr value
            if (retValue != string.Empty)
                retValue = this.CorrValue.Substring(1, this.CorrValue.Length - 2);

            // remove leading zeros
            retValue = retValue.TrimStart(new char[] { '0' });
            if (retValue == string.Empty)
                return 0;

            return Int32.Parse(retValue); 
        }
        public string GetError()
        {
            return this.Error;
        }
        public bool IsRowCorIdentifier()
        {
            return this.Identifier == "correlator";
        }
    }
}
