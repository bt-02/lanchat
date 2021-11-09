using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lanchat.Core.Json;
using Mono.Unix;

namespace Lanchat.ClientCore
{
    /// <summary>
    ///     File system utilities.
    /// </summary>
    public static class Storage
    {
        static Storage()
        {
            SetPaths();
        }

        /// <summary>
        ///     Path of main Lanchat data folder.
        /// </summary>
        public static string DataPath { get; set; }
        
        /// <summary>
        ///     Path of nodes database.
        /// </summary>
        public static string DatabasePath { get; set; }

        /// <summary>
        ///     Path of RSA pem files directory.
        /// </summary>
        public static string RsaDatabasePath { get; set; }

        
        /// <summary>
        ///     Path of Lanchat config file.
        /// </summary>
        public static string ConfigPath => DataPath + "/config.json";

        /// <summary>
        ///     Path of saved files directory.
        /// </summary>
        public static string DownloadsPath { get; set; }

        private static JsonSerializerOptions JsonSerializerOptions => new()
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(),
                new IpAddressConverter()
            }
        };

        /// <summary>
        ///     Load config from file or create new if it's non present or corrupted.
        /// </summary>
        /// <returns>
        ///     <see cref="Config" />
        /// </returns>
        public static Config LoadConfig()
        {
            Config config;

            try
            {
                var json = File.ReadAllText(ConfigPath);
                config = JsonSerializer.Deserialize<Config>(json, JsonSerializerOptions);
            }
            catch (JsonException)
            {
                config = new Config { Fresh = true };
            }
            catch (Exception e)
            {
                CatchFileSystemExceptions(e);
                config = new Config { Fresh = true };
            }

            SaveConfig(config);
            SubscribeEvents(config);
            return config;
        }
        
        internal static void SaveConfig(Config config)
        {
            try
            {
                CreateStorageDirectoryIfNotExists();
                CreateAndSetPermissions(ConfigPath);
                File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, JsonSerializerOptions));
            }
            catch (Exception e)
            {
                CatchFileSystemExceptions(e);
            }
        }
        
        internal static void CreateStorageDirectoryIfNotExists()
        {
            try
            {
                if (!Directory.Exists(DataPath))
                {
                    Directory.CreateDirectory(DataPath);
                }

                if (!Directory.Exists(DownloadsPath))
                {
                    Directory.CreateDirectory(DownloadsPath);
                }

                if (!Directory.Exists($"{DatabasePath}"))
                {
                    Directory.CreateDirectory($"{DatabasePath}");
                }
                
                if (!Directory.Exists($"{RsaDatabasePath}"))
                {
                    Directory.CreateDirectory($"{RsaDatabasePath}");
                }
            }
            catch (Exception e)
            {
                CatchFileSystemExceptions(e);
            }
        }

        internal static void CatchFileSystemExceptions(Exception e)
        {
            if (e is not (
                DirectoryNotFoundException or
                FileNotFoundException or
                IOException or
                UnauthorizedAccessException))
            {
                throw e;
            }

            Trace.WriteLine("Cannot access file system");
        }

        internal static void CreateAndSetPermissions(string filePath)
        {
            File.Create(filePath).Dispose();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            var fileInfo = new UnixFileInfo(filePath)
            {
                FileAccessPermissions = FileAccessPermissions.UserRead | FileAccessPermissions.UserWrite
            };

            fileInfo.Refresh();
        }
        
        private static void SetPaths()
        {
            var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            if (xdgDataHome != null)
            {
                DataPath = xdgDataHome;
                DownloadsPath = xdgDataHome;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                DataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Lanchat2";
                DownloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                DataPath = $"{home}/.Lanchat2";
                DownloadsPath = $"{home}/Downloads";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                DataPath = $"{home}/Library/Preferences/.Lanchat2";
                DownloadsPath = $"{home}/Downloads";
            }

            DatabasePath = $"{DataPath}/Nodes";
            RsaDatabasePath = $"{DataPath}/RSA";
        }

        private static void SubscribeEvents(Config config)
        {
            config.PropertyChanged += (_, _) => { SaveConfig(config); };
            config.BlockedAddresses.CollectionChanged += (_, _) => { SaveConfig(config); };
            config.SavedAddresses.CollectionChanged += (_, _) => { SaveConfig(config); };
        }
    }
}