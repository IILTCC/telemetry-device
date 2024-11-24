namespace telemetry_device_main.icds
{
    public abstract class BaseBox : IParameterIcd
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int Id { get; set; }

        public abstract int GetLocation();
        public abstract string GetMask();
        public abstract int GetSize();
        public int GetRowId() { return this.Id; }
        public int GetMin() { return this.Min; }
        public int GetMax() { return this.Max; }
        public abstract string GetName();
        public abstract int GetCorrValue();
        public abstract string GetError();
        public abstract bool IsRowCorIdentifier();
    }
}
