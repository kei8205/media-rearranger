
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using NGeoNames;
using NGeoNames.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace dir_rename_by_exif_gps_data {
    class Program {
        static void Main(string[] args) {
            MRArguments argument = UsageChecker.checkAndBuildArgument(args);

            var r = new ReverseGeoCode<ExtendedGeoName>(GeoFileReader.ReadExtendedGeoNames(@"cities1000.txt"));
            Parallel.ForEach(argument.targetDirs, (dir) => {
                FileInfo[] jpgs = dir.GetFiles();
                if(jpgs != null && jpgs.Length > 0) {
                    Console.WriteLine("scanning {0} -> has {1} file(s)", dir.Name, jpgs.Length);

                    foreach(FileInfo file in jpgs) {

                        try {
                            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file.FullName);
                            GpsDirectory gpsDirectory = directories.OfType<GpsDirectory>().First();
                            if(gpsDirectory != null && gpsDirectory.GetGeoLocation() != null) {
                                double lat = gpsDirectory.GetGeoLocation().Latitude;
                                double lon = gpsDirectory.GetGeoLocation().Longitude;
                                ExtendedGeoName result = r.NearestNeighbourSearch(r.CreateFromLatLong(lat, lon), 1).First();

                                Console.WriteLine("{0}, lat.length:{1}, lon.length:{2} ==> {3}@{4}", file.Name, lat, lon, result.Name, result.CountryCode);
                            }
                        } catch(Exception) { }
                    }
                }
            });
        }
    }
}
