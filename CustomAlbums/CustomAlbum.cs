using HarmonyLib;
using ModHelper;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustomAlbums
{
    public static class CustomAlbum
    {
        public static readonly Dictionary<string, string> Languages = new Dictionary<string, string>()
        {
            { "ChineseT", "自定義" },
            { "ChineseS", "自定义" },
            { "English", "Custom Albums" },
            { "Korean", "Custom Albums" },
            { "Japanese", "Custom Albums" },
        };
        public static readonly int MusicPackgeUid = 999;
        public static readonly string JsonName = $"ALBUM{MusicPackgeUid + 1}";
        public static readonly string MusicPackge = $"music_package_{MusicPackgeUid}";

        public static readonly string AlbumPackPath = "Custom_Albums";
        public static readonly string AlbumPackExt = "mdm";

        public static Dictionary<string, CustomAlbumInfo> Albums = new Dictionary<string, CustomAlbumInfo>();
        public static void DoPatching()
        {
            var harmony = new Harmony("com.github.mo10.customalbum");

            StageUIPatch.DoPatching(harmony);
            DataPatch.DoPathcing(harmony);
            ExtraPatch.DoPatching(harmony);
            RankPatch.DoPatching(harmony);

            LoadCustomAlbums();
        }

        public static void LoadCustomAlbums()
        {

            if (!Directory.Exists(AlbumPackPath))
            {
                // Create custom album path
                Directory.CreateDirectory(AlbumPackPath);
            }
            // Load *.mbm
            foreach (var file in Directory.GetFiles(AlbumPackPath, $"*.{AlbumPackExt}"))
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    var albumInfo = CustomAlbumInfo.LoadFromFile(file);
                    if (albumInfo != null)
                    {
                        ModLogger.Debug($"Loaded archive:{albumInfo}");
                        Albums.Add($"archive_{fileName}", albumInfo);
                    }
                }catch(Exception ex)
                {
                    ModLogger.Debug($"Load archive failed:{file},reason:{ex}");
                }

            }
            // Load from folder
            foreach (var folder in Directory.GetDirectories(AlbumPackPath))
            {
                try
                {
                    var albumInfo = CustomAlbumInfo.LoadFromFolder(folder);
                    if (albumInfo != null)
                    {
                        ModLogger.Debug($"Loaded folder:{albumInfo} {folder}");
                        Albums.Add($"folder_{folder.Remove(0, AlbumPackPath.Length+1)}", albumInfo);
                    }
                }
                catch (Exception ex)
                {
                    ModLogger.Debug($"Load folder failed:{folder},reason:{ex}");
                }
            }
        }
    }
}