using Ionic.Zip;
using ModHelper;
using MuseDashCustomAlbumMod.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MuseDashCustomAlbumMod.Managers
{
    public static class CustomInfoManager
    {

        private static Dictionary<string, CustomAlbumInfo> albums = new Dictionary<string, CustomAlbumInfo>();

        public static string GetTitltTextFromLanguages(string lang)
        {
            switch (lang)
            {
                case "ChineseT":
                    return "自定義谱面";
                case "ChineseS":
                    return "自定义谱面";
                //case "English":
                //case "Korean":
                //case "Japanese":
                default:
                    return "Custom Albums";
            }
        }

        public static Dictionary<string, CustomAlbumInfo> GetAlbumInfoDic()
        {
            return albums;
        }

        /// <summary>
        /// ALBUM_1000
        /// </summary>
        public static string JsonName
        {
            get;
            private set;
        } = $"ALBUM{MUSIC_PACKGE_UID + 1}";

        /// <summary>
        /// music_package_999
        /// </summary>
        public static string MusicPackge
        {
            get;
            private set;
        } = $"music_package_{MUSIC_PACKGE_UID}";


        #region Define

        public const int MUSIC_PACKGE_UID = 999;

        public const string ALBUM_PACK_PATH = "Custom_Albums";

        public const string ALBUM_PACK_EXT = "mdm";

        public static readonly string[] musicExt = new[]
        {
            ".aiff",
            ".mp3",
            ".ogg",
            ".wav"
        };
        #endregion

        private static bool loaded;



        public static void LoadCustom()
        {
            if (loaded)
                return;

            if (!Directory.Exists(CustomInfoManager.ALBUM_PACK_PATH))
            {
                // Create custom album path
                Directory.CreateDirectory(CustomInfoManager.ALBUM_PACK_PATH);
                return;
            }
            // Load *.mbm
            foreach (var file in Directory.GetFiles(ALBUM_PACK_PATH, $"*.{ALBUM_PACK_EXT}"))
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    var albumInfo = LoadFromZipFile(file);
                    if (albumInfo != null)
                    {
                        ModLogger.Debug($"Loaded archive:{albumInfo}");
                        albums.Add($"archive_{fileName}", albumInfo);
                    }
                }
                catch (Exception ex)
                {
                    ModLogger.Debug($"Load archive failed:{file},reason:{ex}");
                }

            }

            // Load from folder
            foreach (var folder in Directory.GetDirectories(ALBUM_PACK_PATH))
            {
                try
                {
                    var albumInfo = LoadFromFolder(folder);
                    if (albumInfo != null)
                    {
                        ModLogger.Debug($"Loaded folder:{albumInfo} {folder}");
                        albums.Add($"folder_{folder.Remove(0, ALBUM_PACK_PATH.Length + 1)}", albumInfo);
                    }
                }
                catch (Exception ex)
                {
                    ModLogger.Debug($"Load folder failed:{folder},reason:{ex}");
                }
            }

            loaded = true;

        }
        
        /// <summary>
        /// Load from folder
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        private static CustomAlbumInfo LoadFromFolder(string folderPath)
        {
            if (!File.Exists($"{folderPath}/info.json"))
            {
                return null;
            }
            string jsonText = IOUtils.GetFileContent($"{folderPath}/info.json");
            var albumInfo = JsonConvert.DeserializeObject<CustomAlbumInfo>(jsonText);
            albumInfo.SetPath(folderPath);
            albumInfo.SetLoadFromFolder(true);
            return albumInfo;
        }

        /// <summary>
        /// Load from zip file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static CustomAlbumInfo LoadFromZipFile(string filePath)
        {
            using (ZipFile zip = ZipFile.Read(filePath))
            {
                if (zip["info.json"] == null)
                {
                    return null;
                }
                string jsonText = IOUtils.GetFileContent(zip["info.json"].OpenReader());
                CustomAlbumInfo albumInfo = JsonConvert.DeserializeObject<CustomAlbumInfo>(jsonText);
                albumInfo.SetPath(filePath);
                albumInfo.SetLoadFromFolder(false);
                return albumInfo;
            }
        }
    }
}
