using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace media_files_rearrangement {
    class MediaFileService {
        public static void checkOldestDateTime(MediaProcessingContext context) {

            DateTime dateCreated = File.GetCreationTime(context.targetFile.FullName);
            DateTime? dateModified = File.GetLastWriteTime(context.targetFile.FullName);
            DateTime? dateFromFileName = getDateTimeFromFileName(context.targetFile.Name);
            DateTime? dateFromMetadata = getDateTimeFromMetadata(context);


            DateTime baseDateTime = new DateTime(1995, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime minDateTime = dateCreated;

            // 95 년 보다 큰 데이터 중에서 가장 작은
            List<DateTime> dates = new List<DateTime>() { dateCreated };

            if(dateModified != null && dateModified.HasValue && DateTime.Compare(baseDateTime, dateModified.Value) < 1) {
                dates.Add(dateModified.Value);
            }
            if(dateFromFileName != null && dateFromFileName.HasValue && DateTime.Compare(baseDateTime, dateFromFileName.Value) < 1) {
                dates.Add(dateFromFileName.Value);
            }
            if(dateFromMetadata != null && dateFromMetadata.HasValue && DateTime.Compare(baseDateTime, dateFromMetadata.Value) < 1) {
                dates.Add(dateFromMetadata.Value);
            }

            foreach(DateTime date in dates) {
                if(DateTime.Compare(minDateTime, date) > 0) {
                    minDateTime = date;
                }
            }

            context.oldestFileDate = minDateTime;

        }

        public static void createDestinationDir(MRArguments argument, MediaProcessingContext context) {
            String destinationParent = argument.dirForMove.FullName + Path.DirectorySeparatorChar + context.oldestFileDate.ToString("yyyyMMdd");
            if(!File.Exists(destinationParent)) {
                Directory.CreateDirectory(destinationParent);
            }
            context.destinationParent = destinationParent;
        }

        private static bool moveTo(String src, String to, int retrycount) {
            try {
                File.Move(src, to);
                return true;
            } catch(Exception e) {
                if(retrycount > 100) {
                    throw e;
                }
                return false;
            }
        }
        public static void moveToDestination(MediaProcessingContext context) {
            bool created;
            int tries = 0;
            String destination = context.destinationParent + Path.DirectorySeparatorChar + context.targetFile.Name;
            do {
                created = moveTo(context.targetFile.FullName, destination, tries++);
                if(!created) {
                    destination = context.destinationParent + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(context.targetFile.Name) + "_" + tries + Path.GetExtension(context.targetFile.Name);
                    Console.WriteLine("WARN : {0} move failed. retrying and renamed to {1}", context.targetFile.Name, Path.GetFileName(destination));
                } else {
                    context.movedFile = new FileInfo(destination);
                }
            } while(!created);
        }

        public static void editFileDates(MediaProcessingContext context) {
            ShellFile extendedFile = ShellFile.FromFilePath(context.movedFile.FullName);
            File.SetCreationTime(context.movedFile.FullName, context.oldestFileDate);
            File.SetLastWriteTime(context.movedFile.FullName, context.oldestFileDate);
            File.SetLastAccessTime(context.movedFile.FullName, context.oldestFileDate);

            ShellPropertyWriter writer = null;
            try {
                writer = extendedFile.Properties.GetPropertyWriter();
                if(context.mimeType.StartsWith("image")) {
                    writer.WriteProperty(SystemProperties.System.Photo.DateTaken, context.oldestFileDate);
                }
                if(context.mimeType.StartsWith("video")) {
                    writer.WriteProperty(SystemProperties.System.Media.DateEncoded, context.oldestFileDate);
                }
            } catch(Exception ignored) {
                Console.WriteLine("ERROR : {0}`s metadata writing failed", context.movedFile.Name);
            } finally {
                if(writer != null) {
                    writer.Close();
                }
            }
        }


        private static readonly String[] yearPrefix = new String[] { "19", "20" };//19xx~20xx
        private static readonly String[] epoch10Prefix = new String[] { "10", "11", "12", "13", "14", "15", "16", "17", "18" };//2001~2030
        private static readonly String[] epoch9Prefix = new String[] { "3", "4", "5", "6", "7", "8", "9" };//1979~2001
        private static readonly String[] isoDateFormats = new String[] { "yyyyMMddHHmmss", "yyyyMMddHHmm", "yyyyMMddHH", "yyyyMMdd" };

        private static DateTime? getDateTimeFromFileName(String fileName) {
            String fileNameWithoutExtention = Path.GetFileNameWithoutExtension(fileName).ToLower();
            fileNameWithoutExtention = fileNameWithoutExtention.Replace("b612", "");
            String digitOnlyFileName = new String(fileNameWithoutExtention.Where(Char.IsDigit).ToArray());
            DateTime? dateFromName = null;

            if(yearPrefix.Any(prefix => digitOnlyFileName.StartsWith(prefix)) && digitOnlyFileName.Length > 8 && digitOnlyFileName.Length < 20) {
                String isoDateFormatName = null;
                if(digitOnlyFileName.Length > 14) {
                    isoDateFormatName = digitOnlyFileName.Substring(0, 14);
                }

                String nameForParse = isoDateFormatName == null || isoDateFormatName.Length == 0 ? digitOnlyFileName : isoDateFormatName;
                DateTime convertedDateTime = new DateTime();
                foreach(String pattern in isoDateFormats) {
                    if(nameForParse.Length < pattern.Length) {
                        continue;
                    }
                    bool converted = DateTime.TryParseExact(nameForParse.Substring(0, pattern.Length), pattern, null, System.Globalization.DateTimeStyles.None, out convertedDateTime);
                    if(converted) {
                        dateFromName = convertedDateTime;
                        return dateFromName;
                    }
                }
            }

            if(epoch9Prefix.Any(prefix => digitOnlyFileName.StartsWith(prefix)) && digitOnlyFileName.Length > 8 && digitOnlyFileName.Length < 20) {
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                bool converted = long.TryParse(digitOnlyFileName.Substring(0, 9), out long convertedTimestamp);
                if(converted) {
                    dtDateTime.AddSeconds(convertedTimestamp);
                    return dtDateTime;
                }
            }

            if(epoch10Prefix.Any(prefix => digitOnlyFileName.StartsWith(prefix)) && digitOnlyFileName.Length > 9 && digitOnlyFileName.Length < 20) {
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                bool converted = long.TryParse(digitOnlyFileName.Substring(0, 10), out long convertedTimestamp);
                if(converted) {
                    dtDateTime.AddSeconds(convertedTimestamp);
                    return dtDateTime;
                }
            }

            return null;
        }

        private static DateTime? getDateTimeFromMetadata(MediaProcessingContext context) {
            if(context.mimeType.StartsWith("image")) {
                return context.extendedTargetFile.Properties.System.Photo.DateTaken.Value;
            }
            if(context.mimeType.StartsWith("video")) {
                return context.extendedTargetFile.Properties.System.Media.DateEncoded.Value;
            }
            return null;
        }
    }
}
