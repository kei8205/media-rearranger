using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.IO;

namespace media_files_rearrangement {
    class MediaProcessingContext {
        public MediaProcessingContext(FileInfo targetFileInfo) {
            this.targetFile = targetFileInfo;
            this.extendedTargetFile = ShellFile.FromFilePath(targetFileInfo.FullName);
            this.mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(targetFile.Name));
            if(this.mimeType.StartsWith("image/") || this.mimeType.StartsWith("video/")) {
                isSupportedMimeType = true;
            } else {
                isSupportedMimeType = false;
            }
        }
        public String mimeType { get; }

        public bool isSupportedMimeType { get; }

        public FileInfo targetFile { get; }
        public ShellFile extendedTargetFile { get; }

        public DateTime oldestFileDate;

        public String destinationParent;

        public FileInfo movedFile;
    }
}
