using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MelonLoader;

namespace MuseDashCustomAlbumMod
{
    public static class CustomAlbum
    {
        public static readonly Dictionary<string, string> Languages = new Dictionary<string, string>
        {
            {"ChineseT", "自定義谱面"},
            {"ChineseS", "自定义谱面"},
            {"English", "Custom Albums"},
            {"Korean", "Custom Albums"},
            {"Japanese", "Custom Albums"}
        };

        public static readonly int MusicPackgeUid = 999;
        public static readonly string JsonName = $"ALBUM{MusicPackgeUid + 1}";
        public static readonly string MusicPackge = $"music_package_{MusicPackgeUid}";

        public static readonly string AlbumPackPath = "Custom_Albums";
        public static readonly string AlbumPackExt = "mdm";

        public static Dictionary<string, CustomAlbumInfo> Albums = new Dictionary<string, CustomAlbumInfo>();

        public static void DoPatching(HarmonyLib.Harmony harmony)
        {
            StageUIPatch.DoPatching(harmony);
            DataPatch.DoPatching(harmony);
            ExtraPatch.DoPatching(harmony);
            ScorePatch.DoPatching(harmony);

            LoadCustomAlbums();
        }

        public static void LoadCustomAlbums()
        {
            if (!Directory.Exists(AlbumPackPath))
                // Create custom album path
                Directory.CreateDirectory(AlbumPackPath);
            // Load *.mbm
            foreach (var file in Directory.GetFiles(AlbumPackPath, $"*.{AlbumPackExt}"))
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var albumInfo = CustomAlbumInfo.LoadFromFile(file);
                    if (albumInfo != null)
                    {
                        MelonLogger.Msg($"Loaded archive:{albumInfo}");
                        Albums.Add($"archive_{fileName}", albumInfo);
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Msg($"Load archive failed:{file},reason:{ex}");
                }

            // Load from folder
            foreach (var folder in Directory.GetDirectories(AlbumPackPath))
                try
                {
                    var albumInfo = CustomAlbumInfo.LoadFromFolder(folder);
                    if (albumInfo != null)
                    {
                        MelonLogger.Msg($"Loaded folder:{albumInfo} {folder}");
                        Albums.Add($"folder_{folder.Remove(0, AlbumPackPath.Length + 1)}", albumInfo);
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Msg($"Load folder failed:{folder},reason:{ex}");
                }
        }

        public static void LoadDependencies()
        {
            try
            {
                Assembly.Load(Utils.ReadEmbeddedFile("Depends.I18N.dll"));
                Assembly.Load(Utils.ReadEmbeddedFile("Depends.I18N.West.dll"));
                Assembly.Load(Utils.ReadEmbeddedFile("Depends.I18N.CJK.dll"));
                Assembly.Load(Utils.ReadEmbeddedFile("Depends.I18N.MidEast.dll"));
                Assembly.Load(Utils.ReadEmbeddedFile("Depends.I18N.Other.dll"));
                Assembly.Load(Utils.ReadEmbeddedFile("Depends.I18N.Rare.dll"));
            }
            catch(Exception e)
            {
                MelonLogger.Warning($"Couldn't load Dependencies. Already Loaded by another mod? {e.Message}");
            }
        }
    }
}