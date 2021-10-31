using Assets.Scripts.PeroTools.AssetBundles;
using Assets.Scripts.PeroTools.Managers;
using HarmonyLib;
using ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustomAlbums.Patch
{
    public static class AssetPatch
    {
        public static LoadedAssetBundle ABundle = new LoadedAssetBundle(AssetBundle.LoadFromMemory(Utils.ReadEmbeddedFile("Resources.EmptyAssetBundle")));
        public static Album beforeAlbum;

        public static void DoPatching(Harmony harmony)
        {
            // AssetBundle.LoadAsset
            var loadAsset = AccessTools.Method(typeof(AssetBundle), "LoadAsset", new Type[] { typeof(string), typeof(Type) });
            var loadAssetPostfix = AccessTools.Method(typeof(AssetPatch), "LoadAssetPostfix");
            harmony.Patch(loadAsset, null, new HarmonyMethod(loadAssetPostfix));

            var loadAssetBundle = AccessTools.Method(typeof(AssetBundleManager), "LoadAssetBundle");
            var loadAssetBundlePrefix = AccessTools.Method(typeof(AssetPatch), "LoadAssetBundlePrefix");
            harmony.Patch(loadAssetBundle, new HarmonyMethod(loadAssetBundlePrefix));
        }
        /// <summary>
        /// Load resource from custom album
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="__result"></param>
        public static void LoadAssetPostfix(string name, Type type, ref UnityEngine.Object __result)
        {
            string[] suffixes = new string[] {
                "_demo.mp3",
                "_music.mp3",
                "_cover.png",
                "_map1.bms",
                "_map2.bms",
                "_map3.bms",
                "_map4.bms"
            };
            if (__result == null)
            {
                foreach (var suffix in suffixes){
                    if (!name.EndsWith(suffix))
                        continue;

                    var albumKey = name.RemoveFromEnd(suffix).RemoveFromStart("Assets/Static Resources/");
                    if (AlbumManager.LoadedAlbums.TryGetValue(albumKey, out Album album))
                    {
                        switch (suffix)
                        {
                            case "_demo.mp3":
                                __result = album.GetMusic("demo");
                                break;
                            case "_music.mp3":
                                __result = album.GetMusic();
                                break;
                            case "_cover.png":
                                __result = album.GetCover();
                                break;
                            case "_map1.bms":
                                __result = album.GetMap(1);
                                break;
                            case "_map2.bms":
                                __result = album.GetMap(2);
                                break;
                            case "_map3.bms":
                                __result = album.GetMap(3);
                                break;
                            case "_map4.bms":
                                __result = album.GetMap(4);
                                break;
                        }

                        //if(beforeAlbum != null)
                        //    beforeAlbum.DestoryAudio();
                        //beforeAlbum = album;
                    }
                    break;
                }

                //if (customAssets.TryGetValue(name, out CustomAlbumInfo albumInfo))
                //{
                //    // Load cover image 
                //    if (type == typeof(UnityEngine.Sprite))
                //    {
                //        __result = albumInfo.GetCoverSprite();
                //        return;
                //    }
                //    // Load demo audio
                //    if (type == typeof(UnityEngine.AudioClip) && name.EndsWith("_demo.mp3"))
                //    // Load full music audio
                //    if (type == typeof(UnityEngine.AudioClip) && name.EndsWith("_music.mp3"))
                //    {
                //        __result = albumInfo.GetAudioClip("music");
                //        return;
                //    }
                //    // Load map
                //    if (type == typeof(StageInfo))
                //    {
                //        if (name.EndsWith("_map1.bms"))
                //        {
                //            __result = albumInfo.GetMap(1);
                //            return;
                //        }
                //        if (name.EndsWith("_map2.bms"))
                //        {
                //            __result = albumInfo.GetMap(2);
                //            return;
                //        }
                //        if (name.EndsWith("_map3.bms"))
                //        {
                //            __result = albumInfo.GetMap(3);
                //            return;
                //        }
                //        if (name.EndsWith("_map4.bms"))
                //        {
                //            __result = albumInfo.GetMap(4);
                //            return;
                //        }
                //    }
                //}
            }
        }
        /// <summary>
        /// Dymanic load empty asset bundle for custom albums.
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="async"></param>
        /// <param name="___m_LoadedAssetBundles"></param>
        public static void LoadAssetBundlePrefix(string assetBundleName, bool async, ref Dictionary<string, LoadedAssetBundle> ___m_LoadedAssetBundles)
        {
            if (!___m_LoadedAssetBundles.ContainsKey(assetBundleName) && JsonPatch.assetMapping.TryGetValue(assetBundleName, out string albumKey))
            {
                // ModLogger.Debug($"Load Asset Bundle:{assetBundleName}");
                ___m_LoadedAssetBundles.Add(assetBundleName, ABundle);
            }
        }
    }
}