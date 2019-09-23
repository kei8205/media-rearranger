using System.Collections.Generic;
using System.IO;

namespace media_files_rearrangement {
    class MRArguments {

        public DirectoryInfo dirForScan;
        public DirectoryInfo dirForMove;
        public List<FileInfo> targetFiles;

        public int statusCode;

    }
}
