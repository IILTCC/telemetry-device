using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device.compactCollection
{
    class IcdDictionaryItem
    {
        public IcdDictionaryItem(dynamic icdObject, string icdJson)
        {
            this.IcdObject = icdObject;
            this.IcdJson = icdJson;
        }
        public dynamic IcdObject { get; set; }
        public string IcdJson { get; set; }
    }
}
