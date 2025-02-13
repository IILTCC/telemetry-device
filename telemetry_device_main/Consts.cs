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
        public const double GOOD_PACKET_PRECENTAGE = 0;
        public const double BAD_PACKET_PRECENTAGE = 100;
        public const string NETWORK_DEVICE_NAME = @"\Device\NPF_{026FB3A3-275B-4791-91BE-BB5CC388A4D7}";
        public const int BYTE_LENGTH = 8;
        public const int INT32_SIZE = 4;
        public const int MASK_BYTE_POSITION = 0;
        public const int MASK_BASE = 2;
        public const string APPSETTINGS_PATH = "Settings/appsettings.json";
        public const string LOGGER_IP = "127.0.0.1";
        public const string LOGGER_VARIABLE_NAME = "ProjectName";
        public const string PROJECT_NAME = "TelemetryDevice";
        public const int STATISTICS_UPDATE_DELAY = 100;
        public const int STATISTICS_RESET_DELAY = 5000;
        public const int STATISTICS_NO_ITEM_SAVED = 0;
        public const double STATISTICS_LOWER_BOUND_NORMAL = 0.75;
        public const double STATISTICS_UPPER_BOUND_NORMAL = 0.99;
        public const string KAFKA_TIMESTAMP_NAME = "timestamp";
        public const string KAFKA_TIMESTAMP_FORMAT = "o";
        public const string KAFKA_PACKET_TIME_FORMAT = "yyyy - MM - dd HH: mm:ss.fff";
        public const string KAFKA_PACKET_SPLIT = "$split$";
    }
}
