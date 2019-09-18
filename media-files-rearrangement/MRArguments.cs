using System.Collections.Generic;
using System.IO;

namespace media_files_rearrangement
{
    class MRArguments
    {

        public DirectoryInfo dirForScan { get; set; }
        public DirectoryInfo dirForMove { get; set; }
        public List<FileInfo> targetFiles { get; set; }

        public int statusCode { get; set; }

    }
}
