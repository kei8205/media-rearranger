using System;
using System.Collections.Generic;
using System.IO;

namespace dir_rename_by_exif_gps_data {
    class UsageChecker
    {
        public static MRArguments checkAndBuildArgument(string[] args)
        {
            MRArguments baseResponse = new MRArguments
            {
                statusCode = MRStatusCode.CODE_SUCCESS
            };

            if (args == null || args.Length < 1)
            {
                Console.WriteLine("Usage :media-files-rearrangement.exe {scan directory} {optional: city db path}");
                baseResponse.statusCode = MRStatusCode.CODE_INVALID_ARG_COUNT;
                return baseResponse;
            }

            string scanDirectoryPath = args[0];
            if (File.Exists(scanDirectoryPath) || !Directory.Exists(scanDirectoryPath))
            {
                Console.WriteLine("src {0} is not exist or not directory", scanDirectoryPath);
                baseResponse.statusCode = MRStatusCode.CODE_INVALID_SCAN_DIR;
                return baseResponse;
            }

            DirectoryInfo scanDirectory = new DirectoryInfo(scanDirectoryPath);
            Console.WriteLine("scan from {0}", scanDirectory.FullName);

            DirectoryInfo[] targetDirectories = scanDirectory.GetDirectories();

            Console.WriteLine("target directories count : {0}", targetDirectories == null ? 0 : targetDirectories.Length);
            List<DirectoryInfo> directories = new List<DirectoryInfo>(targetDirectories);
            baseResponse.dirForScan = scanDirectory;            
            baseResponse.targetDirs = directories;
            return baseResponse;
        }

    }
}
