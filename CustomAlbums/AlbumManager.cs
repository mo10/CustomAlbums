using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CustomAlbums
{
    public static class AlbumManager
    {
        /// <summary>
        /// Music package uid.
        /// </summary>
        public static readonly int Uid = 999;
        /// <summary>
        /// Album file name.
        /// </summary>
        public static readonly string JsonName = $"ALBUM{Uid + 1}";
        /// <summary>
        /// Music package uid in albums.json.
        /// </summary>
        public static readonly string MusicPackge = $"music_package_{Uid}";
        /// <summary>
        /// Localized string.
        /// </summary>
        public static readonly Dictionary<string, string> Langs = new Dictionary<string, string>()
        {
            { "ChineseT", "自定義" },
            { "ChineseS", "自定义" },
            { "English", "Custom Albums" },
            { "Korean", "Custom Albums" },
            { "Japanese", "Custom Albums" },
        };
        /// <summary>
        /// Search custom album in this folder.
        /// </summary>
        public static readonly string SearchPath = "Custom_Albums";
        /// <summary>
        /// Packaged custom album extension name.
        /// </summary>
        public static readonly string SearchExtension = "mdm";
        /// <summary>
        /// Loaded custom album. 
        /// </summary>
        public static Dictionary<string, Album> LoadedAlbums = new Dictionary<string, Album>();
        /// <summary>
        /// Failed to load custom album.
        /// </summary>
        public static Dictionary<string, string> CorruptedAlbums = new Dictionary<string, string>();
        /// <summary>
        /// Clear all loaded custom albums and reload.
        /// </summary>
        public static void LoadAll()
        {
            LoadedAlbums.Clear();
            CorruptedAlbums.Clear();

            if (!Directory.Exists(SearchPath))
            {
                // Target folder not exist, create it.
                Directory.CreateDirectory(SearchPath);
                return;
            }

            int nextIndex = 0;
            // Load albums package
            foreach (var file in Directory.GetFiles(SearchPath, $"*.{SearchExtension}"))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                try
                {
                    var album = new Album(file);
                    if (album.Info != null)
                    {
                        album.Index = nextIndex;
                        nextIndex++;

                        LoadedAlbums.Add($"pkg_{fileName}".Replace("/", "_").Replace("\\", "_").Replace(".","_"), album);
                        ModLogger.Debug($"Album \"pkg_{fileName}\" loaded.");
                    }
                }
                catch (Exception ex)
                {
                    ModLogger.Debug($"Load album failed: pkg_{fileName}, reason: {ex}");
                    CorruptedAlbums.Add(file, ex.Message);
                }
            }
            // Load albums folder
            foreach (var path in Directory.GetDirectories(SearchPath))
            {
                string folderName = Path.GetFileNameWithoutExtension(path);

                try
                {
                    var album = new Album(path);
                    if (album.Info != null)
                    {
                        album.Index = nextIndex;
                        nextIndex++;

                        LoadedAlbums.Add($"fs_{folderName}".Replace("/", "_").Replace("\\", "_").Replace(".", "_"), album);
                        ModLogger.Debug($"Album \"fs_{folderName}\" loaded.");
                    }
                }
                catch (Exception ex)
                {
                    ModLogger.Debug($"Load album failed: fs_{folderName}, reason: {ex}");
                    CorruptedAlbums.Add(path, ex.Message);
                }
            }
        }
        /// <summary>
        /// Get all loaded album uid.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetAllUid()
        {
            List<string> uids = new List<string>();

            foreach(var album in LoadedAlbums)
            {
                uids.Add($"{Uid}-{album.Value.Index}");
            }

            return uids;
        }
    }
}
