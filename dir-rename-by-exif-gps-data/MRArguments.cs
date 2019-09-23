using System.Collections.Generic;
using System.IO;

namespace dir_rename_by_exif_gps_data {
    class MRArguments {

        public DirectoryInfo dirForScan;

        public FileInfo cityDbFileInfo;

        public List<DirectoryInfo> targetDirs;

        public int statusCode;

    }
}
