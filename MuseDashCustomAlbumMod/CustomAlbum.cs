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
        public static byte[] newAssetBundle;
        public static void DoPatching()
        {
            
            var harmony = new Harmony("com.github.mo10.customalbum");
            AlbumManager.Init();
            //DataPatch.DoPathcing(harmony);
            //ExtraPatch.DoPatching(harmony);
            //RankPatch.DoPatching(harmony);

            //LoadCustomAlbums();


            AssetBundleHelper helper = new AssetBundleHelper("MuseDash_Data/StreamingAssets/AssetBundles/datas/configs/others");
            // albums.json
            var albums = helper.GetAsset("albums");
            var albumJson = albums["m_Script"].value.AsJson<JArray>();
            albumJson.Add(AlbumManager.MusicPackage);
            albums["m_Script"].value.Set(albumJson.JsonSerialize());
            helper.SaveAsset(albums);
            // new ALBUM.json
            var newAsset = helper.CreateAsset("TextAsset");
            newAsset["m_Name"].value.Set(AlbumManager.JsonName);
            newAsset["m_Script"].value.Set(AlbumManager.AlbumsPackage.JsonSerialize());
            var newAssetPathId = helper.SaveAsset(newAsset, "TextAsset");
            // Update metadata
            helper.AddMetadata(newAssetPathId,);
            var stream = helper.Apply();
            newAssetBundle = stream.ToArray();
            stream.Close();

            // For debug
            File.WriteAllBytes("other.cache", newAssetBundle); 
            StageUIPatch.DoPatching(harmony);
        }

    }
}