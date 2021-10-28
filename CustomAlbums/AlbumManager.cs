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
        public static readonly int Uid = 999;
        public static readonly string JsonName = $"ALBUM{Uid + 1}";
        public static readonly string MusicPackge = $"music_package_{Uid}";
        public static readonly Dictionary<string, string> Langs = new Dictionary<string, string>()
        {
            { "ChineseT", "自定義" },
            { "ChineseS", "自定义" },
            { "English", "Custom" },
            { "Korean", "Custom" },
            { "Japanese", "Custom" },
        };

        public static readonly string SearchPath = "Custom_Albums";
        public static readonly string SearchExtension = "mdm";
        // Loaded albums
        public static Dictionary<string, Album> LoadedAlbums = new Dictionary<string, Album>();
        public static Dictionary<string, string> CorruptedAlbums = new Dictionary<string, string>();

        public static void LoadAll()
        {
            LoadedAlbums.Clear();
            CorruptedAlbums.Clear();

            if (!Directory.Exists(SearchPath))
            {
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

                        LoadedAlbums.Add($"pkg_{fileName}", album);
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

                        LoadedAlbums.Add($"fs_{folderName}", album);
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
