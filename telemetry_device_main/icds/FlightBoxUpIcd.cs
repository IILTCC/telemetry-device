﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device_main.icds
{
    public class FlightBoxUpIcd : IBaseIcd
    {
        public int Location { get; set; }
        public string Mask { get; set; }
        public int Bit { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string StartBit { get; set; }

        public int GetLocation() { return this.Location; }
        public string GetMask() { return this.Mask; }
        public int GetSize() { return this.Bit; }
        public int GetMin() { return this.Min; }
        public int GetMax() { return this.Max; }

        public string GetName() { return this.Name; }

        public int GetCorrValue() { return -1; }
        public string GetError() { return string.Empty; }
        public bool IsRowCorIdentifier() { return false; }

        public int GetRowId() { return this.Id; }

    }
}
