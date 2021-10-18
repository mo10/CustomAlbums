using Assets.Scripts.GameCore;
using Assets.Scripts.GameCore.Managers;
using Assets.Scripts.PeroTools.AssetBundles;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.GeneralLocalization;
using Assets.Scripts.PeroTools.GeneralLocalization.Modles;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.PeroTools.Nice.Components;
using Assets.Scripts.PeroTools.Nice.Variables;
using Assets.Scripts.UI.Controls;
using Assets.Scripts.UI.Panels;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Assets.Scripts.PeroTools.Nice.Datas;
using System.Reflection.Emit;
using System.IO;
using Ionic.Zip;
using AssetsTools.NET.Extra;

namespace CustomAlbums
{
    public static class CustomAlbum
    {
        public static Dictionary<string, byte[]> abCache = new Dictionary<string, byte[]>();
        public static Dictionary<string, string> abDirectory = new Dictionary<string, string>();
        public static Dictionary<string, string> abName = new Dictionary<string, string>();

        public static void DoPatching()
        {

            var harmony = new Harmony("com.github.mo10.customalbum");
            AlbumManager.Init();
            //DataPatch.DoPathcing(harmony);
            //ExtraPatch.DoPatching(harmony);
            //RankPatch.DoPatching(harmony);

            //LoadCustomAlbums();

            abOthers();
            abLanguage();


            StageUIPatch.DoPatching(harmony);
        }
        public static void abOthers()
        {
            AssetBundleHelper helper = new AssetBundleHelper("MuseDash_Data/StreamingAssets/AssetBundles/datas/configs/others");
            // albums.json
            var albums = helper.GetAsset("albums");
            var albumJson = albums["m_Script"].value.AsJson<JArray>();
            albumJson.Add(AlbumManager.MusicPackage);
            albums["m_Script"].value.Set(albumJson.JsonSerialize());
            helper.ReplaceAsset(albums);
            // add ALBUM.json 
            var newAsset = helper.CreateAsset("TextAsset");
            newAsset["m_Name"].value.Set(AlbumManager.JsonName);
            newAsset["m_Script"].value.Set(AlbumManager.AlbumsPackage.JsonSerialize());
            var pathId = helper.ReplaceAsset(newAsset);
            helper.UpdateMetadata(pathId, $"data/configs/others/{AlbumManager.JsonName}.json".ToLower());

            using (var stream = helper.ApplyReplace())
            {
                abCache.Add("datas/configs/others", stream.ToArray());
                abDirectory.Add("datas/configs/others", "Data/Configs/others");
                abName.Add("datas/configs/others",AlbumManager.JsonName);
            }
        }
        public static void abLanguage()
        {
            foreach (var lang in AlbumManager.Langs)
            {
                AssetBundleHelper helper = new AssetBundleHelper($"MuseDash_Data/StreamingAssets/AssetBundles/datas/configs/{lang.Key.ToLower()}");
                // albums_<lang>.json
                var albums = helper.GetAsset($"albums_{lang.Key}");
                var albumJson = albums["m_Script"].value.AsJson<JArray>();
                albumJson.Add(AlbumManager.MusicPackageLang[lang.Key]);
                albums["m_Script"].value.Set(albumJson.JsonSerialize());
                helper.ReplaceAsset(albums);
                // ALBUMxx_<lang>.json
                var newAsset = helper.CreateAsset("TextAsset");
                newAsset["m_Name"].value.Set($"{AlbumManager.JsonName}_{lang}");
                newAsset["m_Script"].value.Set(AlbumManager.AlbumsPackageLang[lang.Key].JsonSerialize());
                var pathId = helper.ReplaceAsset(newAsset);
                helper.UpdateMetadata(pathId, $"data/configs/{lang.Key.ToLower()}/{AlbumManager.JsonName}_{lang}.json".ToLower());

                using (var stream = helper.ApplyReplace())
                {
                    abCache.Add($"datas/configs/{lang.Key.ToLower()}", stream.ToArray());
                    abDirectory.Add($"datas/configs/{lang.Key.ToLower()}", $"Data/Configs/{lang.Key.ToLower()}");
                    abName.Add($"datas/configs/{lang.Key.ToLower()}", $"{AlbumManager.JsonName}_{lang}");
                }
            }
        }
    }
}