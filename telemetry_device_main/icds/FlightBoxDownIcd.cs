namespace telemetry_device_main.icds
{
    public class FlightBoxDownIcd : BaseBox
    {
        public int Location { get; set; }
        public string Mask { get; set; }
        public int Bit { get; set; }
        public string Name { get; set; }
        public string StartBit { get; set; }

        public override int GetSyncSize()
        {
            return Consts.FLIGHTBOX_SYNC_SIZE;
        }

        public override int GetLocation() { return this.Location; }
        public override string GetMask() { return this.Mask; }
        public override int GetSize() { return this.Bit; }
        public override string GetName(){ return this.Name; }
        public override int GetCorrValue(){return -1;}
        public override string GetError() {return string.Empty;}
        public override bool IsRowCorIdentifier() { return false; }
    }
}
