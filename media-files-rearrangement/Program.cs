using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace media_files_rearrangement {
    class Program {

        static int Main(string[] args) {

            MRArguments argument = UsageChecker.checkAndBuildArgument(args);
            if(MRStatusCode.CODE_SUCCESS != argument.statusCode) {
                return argument.statusCode;
            }

            int processedCounter = 0;
            int totalCount = argument.targetFiles.Count;

            Parallel.ForEach(argument.targetFiles, (file) => {
                int currentProcessCount = Interlocked.Increment(ref processedCounter);
                MediaProcessingContext processingContext = new MediaProcessingContext(file);

                if(!processingContext.isSupportedMimeType) {
                    Console.WriteLine("{2}/{3}  - {0} is not supported file type [{1}]", processingContext.targetFile.Name, processingContext.mimeType, currentProcessCount, totalCount);
                    return;
                }

                MediaFileService.checkOldestDateTime(processingContext);
                MediaFileService.createDestinationDir(argument, processingContext);
                MediaFileService.moveToDestination(processingContext);
                if(!processingContext.targetFile.FullName.Equals(processingContext.movedFile.FullName)) {
                    MediaFileService.editFileDates(processingContext);
                    Console.WriteLine("{3}/{4}  - {0} -> {1} and set its all meta dates to [{2}]", processingContext.targetFile.Name, processingContext.movedFile.Name, processingContext.oldestFileDate, currentProcessCount, totalCount);
                    return;
                }
                Console.WriteLine("{1}/{2}  - {0} rearrangement failed.", processingContext.targetFile.Name, processingContext.movedFile.Name, processingContext.oldestFileDate, currentProcessCount, totalCount);
            });
            return MRStatusCode.CODE_SUCCESS;
        }
    }
}
