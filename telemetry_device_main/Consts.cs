namespace telemetry_device_main
{
    public static class Consts
    {
        public const string FILE_TYPE = ".json";
        public const string REPO_PATH = "../../../icd_repo/";
        public const int HEADER_SIZE = 25;
        public const string TIMESTAMP_FORMAT = "dd,MM,yyyy,HH,mm,ss,ffff";
        public const string STATISTIC_TOPIC = "TelemetryStatistics";
        public const int TIMEOUT = 5;
        public const int TYPE_SIZE = 1;
        public const int TYPE_PLACE = 0;
        public const int TIMESTAMP_SIZE = 24;
        public const int GOOD_PACKET_PRECENTAGE = 0;
        public const int BAD_PACKET_PRECENTAGE = 100;
        public const string NETWORK_DEVICE_NAME = @"\Device\NPF_{026FB3A3-275B-4791-91BE-BB5CC388A4D7}";
        public const int BYTE_LENGTH = 8;
        public const int INT32_SIZE = 4;
        public const int MASK_BYTE_POSITION = 0;
        public const int MASK_BASE = 2;
        public const string APPSETTINGS_PATH = "Settings/appsettings.json";
    }
}
