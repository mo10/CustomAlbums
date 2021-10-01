using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustomAlbums
{
    static class AlbumManager
    {
        
        public static readonly int MusicPackgeUid = 999;
        public static readonly string JsonName = $"ALBUM{MusicPackgeUid + 1}";
        public static readonly string MusicPackge = $"music_package_{MusicPackgeUid}";
        public static readonly Dictionary<string, string> Langs = new Dictionary<string, string>()
        {
            { "ChineseT", "自定義" },
            { "ChineseS", "自定义" },
            { "English", "Custom" },
            { "Korean", "Custom" },
            { "Japanese", "Custom" },
        };
        // Search path
        public static readonly string AlbumPath = "Custom_Albums";
        public static readonly string AlbumExt = "mdm";
        // Loaded albums
        public static Dictionary<string, Album> Albums = new Dictionary<string, Album>();
        // Inject to json
        public static JObject MusicPackage;
        public static Dictionary<string, JObject> MusicPackageLang;
        public static JArray AlbumsPackage;
        public static Dictionary<string, JArray> AlbumsPackageLang;
        private static void LoadAllAlbum()
        {
            if (!Directory.Exists(AlbumPath))
            {
                Directory.CreateDirectory(AlbumPath);
            }
            Albums.Clear();
            // Load albums package
            foreach (var file in Directory.GetFiles(AlbumPath, $"*.{AlbumExt}"))
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    var album = new Album(file);
                    if (album.Info != null)
                    {
                        ModLogger.Debug($"Album {fileName} {album.Info.name} loaded.");
                        Albums.Add($"pkg_{fileName}", album);
                    }
                }
                catch (Exception ex)
                {
                    ModLogger.Debug($"Load album failed: {file}, reason: {ex}");
                }
            }
            // Load albums folder
            foreach (var folder in Directory.GetDirectories(AlbumPath))
            {
                try
                {
                    var album = new Album(folder);
                    if (album.Info != null)
                    {
                        ModLogger.Debug($"Album {album.Info.name} loaded.");
                        Albums.Add($"fs_{folder.Remove(0, AlbumPath.Length + 1)}", album);
                    }
                }
                catch (Exception ex)
                {
                    ModLogger.Debug($"Load album failed: {folder}, reason: {ex}");
                }
            }
        }
        
        public static void Init()
        {
            LoadAllAlbum();
            // album.json
            MusicPackage = new JObject();
            MusicPackage.Add("uid", MusicPackge);
            MusicPackage.Add("title", "Custom Albums");
            MusicPackage.Add("prefabsName", $"AlbumDisco{MusicPackgeUid}");
            MusicPackage.Add("price", "¥25.00");
            MusicPackage.Add("jsonName", JsonName);
            MusicPackage.Add("needPurchase", false);
            MusicPackage.Add("free", true);
            // album_<lang>.json
            MusicPackageLang = new Dictionary<string, JObject>();
            foreach(var lang in Langs)
            {
                var obj = new JObject();
                obj.Add("title", lang.Value);
                MusicPackageLang.Add(lang.Key, obj);
            }
            // ALBUM<index>.json  ALBUM<index>_<lang>.json
            AlbumsPackage = new JArray();
            AlbumsPackageLang = new Dictionary<string, JArray>();
            int count = 0;
            foreach(var album in Albums)
            {
                var info = album.Value.Info;
                var albumInfo = new JObject();
                albumInfo.Add("uid", $"{MusicPackgeUid}-{count++}");
                albumInfo.Add("name", info.GetName());
                albumInfo.Add("author",info.GetAuthor());
                
                albumInfo.Add("bpm", info.bpm);
                albumInfo.Add("music", $"{album.Key}_music");
                albumInfo.Add("demo", $"{album.Key}_demo");
                albumInfo.Add("cover", $"{album.Key}_cover");
                albumInfo.Add("noteJson", $"{album.Key}_map");
                albumInfo.Add("scene", info.scene);
                albumInfo.Add("unlockLevel", info.unlockLevel);

                if (!string.IsNullOrEmpty(info.levelDesigner))
                    albumInfo.Add("levelDesigner", info.levelDesigner);
                if (!string.IsNullOrEmpty(info.levelDesigner1))
                    albumInfo.Add("levelDesigner1", info.levelDesigner1);
                if (!string.IsNullOrEmpty(info.levelDesigner2))
                    albumInfo.Add("levelDesigner2", info.levelDesigner2);
                if (!string.IsNullOrEmpty(info.levelDesigner3))
                    albumInfo.Add("levelDesigner3", info.levelDesigner3);
                if (!string.IsNullOrEmpty(info.levelDesigner4))
                    albumInfo.Add("levelDesigner4", info.levelDesigner4);

                if (!string.IsNullOrEmpty(info.difficulty1))
                    albumInfo.Add("difficulty1", info.difficulty1);
                if (!string.IsNullOrEmpty(info.difficulty2))
                    albumInfo.Add("difficulty2", info.difficulty2);
                if (!string.IsNullOrEmpty(info.difficulty3))
                    albumInfo.Add("difficulty3", info.difficulty3);
                if (!string.IsNullOrEmpty(info.difficulty4))
                    albumInfo.Add("difficulty4", info.difficulty4);

                AlbumsPackage.Add(albumInfo);
                // Add lang
                foreach(var lang in Langs)
                {
                    var albumLang = new JObject();
                    albumLang.Add("name", info.GetName(lang.Key));
                    albumLang.Add("author", info.GetAuthor(lang.Key));

                    if (!AlbumsPackageLang.ContainsKey(lang.Key))
                        AlbumsPackageLang.Add(lang.Key, new JArray());

                    AlbumsPackageLang[lang.Key].Add(albumLang);
                }
            }
        }
    }
}
