using System;
using System.Collections.Generic;
using System.IO;

namespace media_files_rearrangement {
    class UsageChecker {
        public static MRArguments checkAndBuildArgument(string[] args) {
            MRArguments baseResponse = new MRArguments {
                statusCode = MRStatusCode.CODE_SUCCESS
            };

            if(args == null || args.Length < 2) {
                Console.WriteLine("Usage :media-files-rearrangement.exe {scan directory} {target directory}");
                baseResponse.statusCode = MRStatusCode.CODE_INVALID_ARG_COUNT;
                return baseResponse;
            }

            string scanDirectoryPath = args[0];
            string targetDirectoryPath = args[1];

            if(File.Exists(scanDirectoryPath) || !Directory.Exists(scanDirectoryPath)) {
                Console.WriteLine("src {0} is not exist or not directory", scanDirectoryPath);
                baseResponse.statusCode = MRStatusCode.CODE_INVALID_SCAN_DIR;
                return baseResponse;
            }

            if(File.Exists(targetDirectoryPath) || !Directory.Exists(targetDirectoryPath)) {
                Console.WriteLine("target {0} is not exist or not directory", targetDirectoryPath);
                baseResponse.statusCode = MRStatusCode.CODE_INVALID_TARGET_DIR;
                return baseResponse;
            }

            DirectoryInfo scanDirectory = new DirectoryInfo(scanDirectoryPath);
            Console.WriteLine("scan from {0}", scanDirectory.FullName);

            DirectoryInfo targetDirectory = new DirectoryInfo(targetDirectoryPath);
            Console.WriteLine("move to {0}", targetDirectory.FullName);

            FileInfo[] targetFiles = scanDirectory.GetFiles();
            Console.WriteLine("target file count : {0}", targetFiles == null ? 0 : targetFiles.Length);
            List<FileInfo> files = new List<FileInfo>(targetFiles);
            baseResponse.dirForScan = scanDirectory;
            baseResponse.dirForMove = targetDirectory;
            baseResponse.targetFiles = files;


            return baseResponse;
        }

    }
}
