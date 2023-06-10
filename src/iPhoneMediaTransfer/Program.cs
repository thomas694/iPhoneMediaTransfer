//--------------------------------------------------------------------------
// iPhoneMediaTransfer - transfer photos and videos from iOS device to PC
// 
// Transfer photos and videos from iOS device to PC. Keep one folder (DCIM) 
// for reference and replicate the albums in another folder (Albums) with 
// optional hardlinks or file copies.
//
// Version 1.0
// Copyright (c) Feb 2023  thomas694 (@GH 0CFD61744DA1A21C)
//     initial version
//
// iPhoneMediaTransfer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Microsoft.Data.Sqlite;
using MK.MobileDevice.Lite;

namespace iPhoneMediaTransfer
{
    class Program
    {
        static iOSDeviceMK _device;
        static bool _transferDatabase;
        static bool _transferMedia;
        static bool _createAlbumHardLinks;
        static bool _adjustDateTimes;
        static string _mediaLibraryPath = "";

        static void Main(string[] args)
        {
            EvaluateArguments(args);

            _device = new iOSDeviceMK();
            Console.WriteLine(@"iPhoneMediaTransfer (c) 2022 thomas694 (@GH)");
            Console.WriteLine("Waiting for iOS Device to be connected...");
            _device.Connect += Iphone_Connect;
            _device.Disconnect += Iphone_Disconnect;
            while (true) { }

            DoWork();
        }

        private static void Iphone_Disconnect(object sender, ITMDConnectEventArgs args)
        {
            Console.WriteLine("iOS Device Disconnected.");
        }

        private static void Iphone_Connect(object sender, ITMDConnectEventArgs args)
        {
            Console.WriteLine("iOS Device Connected in USB Multiplexing Mode.");
            Console.WriteLine("Device is named {0}, an {1} running iOS {2}", _device.DeviceName, _device.DeviceProductType, _device.DeviceVersion);
            bool isPhone = _device.DeviceProductType.StartsWith("iPhone");
            if (isPhone)
            {
                Console.WriteLine("Phone Number {0}", _device.DevicePhoneNumber);
            }
            DoWork();
        }

        private static void EvaluateArguments(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("At least one argument must be specified!");
                Environment.Exit(-1);
            }

            foreach (var arg in args)
            {
                var argument = arg.ToLower().TrimStart('-', '/');

                if (new string[] { "h", "help", "?" }.Contains(argument))
                {
                    var version = Assembly.GetEntryAssembly().GetName().Version.ToString(2);
                    var compiled = new DateTime(CompileTimeHelper.CompileTime, DateTimeKind.Utc).ToShortDateString();
                    Console.WriteLine("iPhoneMediaTransfer v{0} compiled {1} (c) thomas694 (@GH)", version, compiled);
                    Console.WriteLine("Transferring photos and videos from iOS device to PC.");
                    Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY. This is free software, and you");
                    Console.WriteLine("are welcome to redistribute it under certain conditions; view GNU GPLv3 for more." + Environment.NewLine);
                    Console.WriteLine("Usage: iPhoneMediaTransfer [options] <folder>");
                    Console.WriteLine("Options:");
                    Console.WriteLine(" -h[elp]|?    Shows this help screen");
                    Console.WriteLine(" -database    Copies the Photos.sqlite from the device to <folder>\\Photos_yyyyMMddHHmmss.sqlite.");
                    Console.WriteLine("              If not specified, the newest DB in the folder will be used.");
                    Console.WriteLine(" -media       Transfer photos and videos from iOS device to PC.");
                    Console.WriteLine(" -hardlinks   Create hardlinks. Works on NTFS file systems only and needs admin rights.");
                    Console.WriteLine(" -adjust      Adjust the modified date of the media files with values from the DB.");
                    Console.WriteLine(" <folder>     Path to your target folder.");
                    Environment.Exit(0);
                }
                else if (argument == "database")
                {
                    _transferDatabase = true;
                }
                else if (argument == "media")
                {
                    _transferMedia = true;
                }
                else if (argument == "hardlinks")
                {
                    if (!IsElevated())
                    {
                        Console.WriteLine("Admin rights required to create hardlinks!");
                        Environment.Exit(-2);
                    }
                    _createAlbumHardLinks = true;
                }
                else if (argument == "adjust")
                {
                    _adjustDateTimes = true;
                }
                else if (Directory.Exists(arg))
                {
                    _mediaLibraryPath = arg;
                }
                else if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    Console.WriteLine($"Unrecognized argument: {arg}");
                    Environment.Exit(-3);
                }
                else
                {
                    Console.WriteLine($"{_mediaLibraryPath} is no existing directory!");
                    Environment.Exit(-4);
                }
            }
            if (!_transferDatabase && !_transferMedia && !_adjustDateTimes)
            {
                Console.WriteLine("At least one option must be specified!");
                Environment.Exit(-5);
            }
            if (_mediaLibraryPath == "")
            {
                Console.WriteLine("Using current folder as Library Path.");
                _mediaLibraryPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            }
        }

        private static bool IsElevated()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private static void DoWork()
        {
            //_transferDatabase = false;
            //_transferMedia = true;
            //_createAlbumHardLinks = true;

            string filenameDB = _transferDatabase ? CopyPhotosDatabaseFromIOSDevice() : GetFilenameOfNewestLocalDatabase();

            if (_adjustDateTimes)
            {
                AdjustDateTimes(filenameDB);
            }
            if (_transferMedia)
            {
                TransferMedia(filenameDB);
            }
            
            Console.WriteLine("Finished.");
        }

        private static string CopyPhotosDatabaseFromIOSDevice()
        {
            var filename = Path.Combine(_mediaLibraryPath, $"Photos_{DateTime.Now:yyyyMMddHHmmss}.sqlite");
            CopyFileFromDevice("PhotoData", "Photos.sqlite", filename);
            return filename;
        }

        private static string GetFilenameOfNewestLocalDatabase()
        {
            var files = Directory.GetFiles(_mediaLibraryPath, "Photos_*.sqlite", SearchOption.TopDirectoryOnly);
            var filename = files.OrderByDescending(x => x).FirstOrDefault();
            if (filename is null)
            {
                Console.WriteLine("No Photos_*.sqlite found!");
                Environment.Exit(-6);
            }
            return filename;
        }

        private static void TransferMedia(string filenameDB)
        {
            using (var connection = new SqliteConnection("Data Source=" + filenameDB))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"select zAsset.ZDIRECTORY, zAsset.ZFILENAME, zAsset.ZDATECREATED, zGenAlbum.ZTITLE, zAsset.ZTRASHEDSTATE " +
                                        "from ZASSET zAsset " +
                                        "left join Z_28ASSETS zAssets on zAsset.Z_PK = zAssets.Z_3ASSETS " +
                                        "left join ZGENERICALBUM zGenAlbum on zAssets.Z_28ALBUMS = zGenAlbum.Z_PK " +
                                        "order by 2";

                var assets = new List<Tuple<string, string, double, string, bool>>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var folder = reader.GetString(0);
                        var filename = reader.GetString(1);
                        var timestamp = reader.GetDouble(2);
                        var albumTitle = reader.IsDBNull(3) ? null : reader.GetString(3);
                        var deleted = reader.GetBoolean(4);
                        assets.Add(Tuple.Create(folder, filename, timestamp, albumTitle, deleted));
                    }
                }

                var i = 0;
                while (i <= assets.Count-1)
                {
                    var (folder, filename, timestamp, albumTitle, isDeleted) = assets[i];

                    Console.Write($"{folder}/{filename} ");

                    var albumNames = new List<string>();
                    if (!string.IsNullOrEmpty(albumTitle)) albumNames.Add(albumTitle);

                    while (i + 1 <= assets.Count - 1 && assets[i + 1].Item2 == filename)
                    {
                        i++;
                        if (!string.IsNullOrEmpty(assets[i].Item4)) albumNames.Add(assets[i].Item4);
                    }

                    DownloadOrMoveFile(folder, filename, timestamp, isDeleted);

                    CreateAlbumEntries(folder, filename, albumNames);

                    i++;
                }
            }
        }

        private static List<string> SearchCurrentAlbumNames(string filename)
        {
            var albumNames = new List<string>();
            albumNames.AddRange(Directory.GetFiles(Path.Combine(_mediaLibraryPath, "Albums"), filename, SearchOption.AllDirectories).Select(s => Path.GetFileName(Path.GetDirectoryName(s))));
            return albumNames;
        }

        private static void DownloadOrMoveFile(string folder, string filename, double timestamp, bool isDeleted)
        {
            var filePath = Path.Combine(_mediaLibraryPath, folder.Replace("/", "\\"), filename);
            var filePathDeleted = Path.Combine(_mediaLibraryPath, folder.Replace("/", "\\").Replace("DCIM", "DCIM_deleted"), filename);
            var action = "skipped";
            if (!File.Exists(filePath))
            {
                if (!File.Exists(filePathDeleted))
                {
                    CopyFileFromDevice(folder, filename, isDeleted ? filePathDeleted : filePath);
                    AdjustFileDateTime(filePath, timestamp);
                    action = "copied";
                }
                else if (!isDeleted)
                {
                    File.Move(filePathDeleted, filePath);
                    action = "moved to undeleted";
                }
            }
            else if (isDeleted)
            {
                File.Move(filePath, filePathDeleted);
                action = "moved to deleted";
            }
            Console.WriteLine(action);
        }

        private static void CreateAlbumEntries(string folder, string filename, List<string> albumNames)
        {
            var filePath = Path.Combine(_mediaLibraryPath, folder.Replace("/", "\\"), filename);

            var oldAlbumNames = SearchCurrentAlbumNames(filename);

            foreach (var name in albumNames)
            {
                var newPath = Path.Combine(_mediaLibraryPath, "Albums", name, filename);
                if (!File.Exists(newPath))
                {
                    if (_createAlbumHardLinks)
                    {
                        NativeMethods.CreateHardLink(newPath, filePath, IntPtr.Zero);
                    }
                    else
                    {
                        File.Copy(filePath, newPath);
                    }
                }
                oldAlbumNames.Remove(name);
            }

            foreach (var name in oldAlbumNames)
            {
                File.Delete(Path.Combine(_mediaLibraryPath, "Albums", name, filename));
            }
        }

        private static void CopyFileFromDevice(string folder, string filename, string localFilePath)
        {
            byte[] bytes;
            using (var file = iPhoneFile.OpenRead(_device, $"{folder}/{filename}"))
            {
                bytes = file.ReadAll();
            }

            Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));

            File.WriteAllBytes(localFilePath, bytes);
        }

        private static void AdjustFileDateTime(string filePath, double timestamp)
        {
            DateTime datetime = new DateTime(2001, 01, 01, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
            var fi = new FileInfo(filePath);
            if (fi.IsReadOnly) { return; }
            if (fi.LastWriteTimeUtc != datetime)
                fi.LastWriteTimeUtc = datetime;
        }

        private static void AdjustDateTimes(string filenameDB)
        {
            using (var connection = new SqliteConnection("Data Source=" + filenameDB))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"select zdirectory, zfilename, zdatecreated from zasset";

                using (var reader = command.ExecuteReader())
                {
                    var albumsPath = Path.Combine(_mediaLibraryPath, "Albums");
                    var dcimPath = Path.Combine(_mediaLibraryPath, "DCIM");
                    var dcimDeletedPath = Path.Combine(_mediaLibraryPath, "DCIM_Deleted");
                    while (reader.Read())
                    {
                        //var directory = reader.GetString(0);
                        var filename = reader.GetString(1);
                        var timestamp = reader.GetDouble(2);

                        var basename = Path.GetFileNameWithoutExtension(filename);

                        var files = Directory.GetFiles(albumsPath, $"{basename}.*", SearchOption.AllDirectories);
                        foreach (var fn in files)
                        {
                            AdjustFileDateTime(fn, timestamp);
                        }

                        files = Directory.GetFiles(dcimPath, $"{basename}.*", SearchOption.AllDirectories);
                        foreach (var fn in files)
                        {
                            AdjustFileDateTime(fn, timestamp);
                        }

                        files = Directory.GetFiles(dcimDeletedPath, $"{basename}.*", SearchOption.AllDirectories);
                        foreach (var fn in files)
                        {
                            AdjustFileDateTime(fn, timestamp);
                        }
                    }
                }
            }
        }
    }
}
