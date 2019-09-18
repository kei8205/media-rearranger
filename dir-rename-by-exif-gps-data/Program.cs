
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
                if(dir.Name.Contains("@")) {
                    Console.WriteLine("skipping {0}.already renamed. maybe.. it`s name contains @ character", dir.Name);
                    return;
                }
                FileInfo[] files = dir.GetFiles();
                if(files != null && files.Length > 0) {
                    Dictionary<String, int> cCodeCounter = new Dictionary<String, int>();
                    Dictionary<String, int> cityCounter = new Dictionary<String, int>();

                    foreach(FileInfo file in files) {
                        try {
                            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file.FullName);
                            GpsDirectory gpsDirectory = directories.OfType<GpsDirectory>().First();
                            if(gpsDirectory != null && gpsDirectory.GetGeoLocation() != null) {
                                double lat = gpsDirectory.GetGeoLocation().Latitude;
                                double lon = gpsDirectory.GetGeoLocation().Longitude;
                                ExtendedGeoName result = r.NearestNeighbourSearch(r.CreateFromLatLong(lat, lon), 1).First();
                                String city = result.Name;
                                if(cityCounter.TryGetValue(city, out int tvalue)) {
                                    cityCounter[city] = tvalue + 1;
                                } else {
                                    cityCounter.Add(city, 1);
                                }
                                String ccode = result.CountryCode;
                                if(cCodeCounter.TryGetValue(ccode, out int cvalue)) {
                                    cCodeCounter[ccode] = cvalue + 1;
                                } else {
                                    cCodeCounter.Add(ccode, 1);
                                }
                            }
                        } catch(Exception) { }
                    }
                    
                    if(cCodeCounter.Count > 0) {
                        int total = 0;
                        String countryName = "";
                        int maxCountryCount = 0;
                        foreach(String key in cCodeCounter.Keys) {
                            int tempCCount = cCodeCounter[key];
                            total += tempCCount;
                            if(tempCCount > maxCountryCount) {
                                countryName = key;
                            }
                        }
                        String cityName = "";
                        int maxCityCuont = 0;
                        foreach(String key in cityCounter.Keys) {
                            int tempCCount = cityCounter[key];
                            if(tempCCount > maxCityCuont) {
                                cityName = key;
                            }
                        }
                        String destination = dir.Name + " " + cityName + "@" + countryName;
                        
                        bool moved = false;
                        try {
                            System.IO.Directory.Move(dir.FullName, dir.Parent.FullName + Path.DirectorySeparatorChar + destination);
                            moved = true;
                        } catch(Exception) { }

                        Console.WriteLine("{0}`s total {1} file  has {2} gps data and most file located {3}@{4} then renaming it to {5}  {6}", dir.Name, files.Length, total, cityName, countryName, destination, moved ? "success" : "failed");
                    } else {
                        Console.WriteLine("{0}`s total {1} file has no gps data", dir.Name, files.Length);
                    }

                    
                }
            });
        }
    }
}
