using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace simulator_main.icd
{
    public interface IBaseIcd
    {
        public int GetRowId();
        public int GetLocation();
        public string GetMask();
        public int GetSize();
        public int GetMin();
        public int GetMax();
        public string GetName();
        public int GetCorrValue();
        public string GetError();
        public bool IsRowCorIdentifier();
        
    }
}
