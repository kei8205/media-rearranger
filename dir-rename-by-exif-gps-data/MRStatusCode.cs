namespace dir_rename_by_exif_gps_data {
    class MRStatusCode
    {
        public static int CODE_SUCCESS { get; } = 0;
        public static int CODE_INVALID_ARG_COUNT { get; } = 1;
        public static int CODE_INVALID_SCAN_DIR { get; } = 2;
        public static int CODE_INVALID_TARGET_DIR { get; } = 3;
        public static int CODE_INVALID_GEO_DB { get; } = 4;
    }
}
