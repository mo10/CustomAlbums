using Assets.Scripts.GameCore;
using Assets.Scripts.PeroTools.Managers;
using HarmonyLib;
using ModHelper;
using PeroTools2.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CustomAlbums.Patch
{
    public class CustomAlbumAssetLocator : IResourceLocator
    {
        public string LocatorId => "CustomAlbumAssetLocator";
        public IEnumerable<object> Keys => AlbumManager.AssertKeys;

        public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
        {
            var assetKey = key as string;
            locations = null;

            if (string.IsNullOrEmpty(assetKey) || !assetKey.StartsWith("fs_") && !assetKey.StartsWith("pkg_"))
                return false;

            //ModLogger.Debug($"Key:{assetKey} type:{type.Name}");
            locations = new List<IResourceLocation>();
            locations.Add(new CustomAlbumAssetResourceLocation() {
                AssetType = type,
                AssetKey = assetKey
            });

            return true;
        }
    }
    public class CustomAlbumAssetResourceLocation : IResourceLocation
    {
        public string InternalId => AssetKey;
        public string ProviderId => "CustomAlbumAssetResourceProvider";
        public IList<IResourceLocation> Dependencies => new List<IResourceLocation>();
        public int DependencyHashCode => 0;
        public bool HasDependencies => false;
        public object Data => null;
        public string PrimaryKey => AssetKey;
        public Type ResourceType => AssetType;
        /// Custom property
        public Type AssetType;
        public string AssetKey;

        public int Hash(Type resultType)
        {
            return AssetKey.GetHashCode() * 31 + ((resultType == null) ? 0 : resultType.GetHashCode());
        }
    }

    public class CustomAlbumAssetResourceProvider : IResourceProvider
    {
        public string ProviderId => "CustomAlbumAssetResourceProvider";
        public ProviderBehaviourFlags BehaviourFlags => ProviderBehaviourFlags.None;
        public static ProvideHandle handle;
        public bool CanProvide(Type type, IResourceLocation location)
        {
            if (location.GetType() == typeof(CustomAlbumAssetResourceLocation))
            {
                var customLocation = location as CustomAlbumAssetResourceLocation;
                return true;
            }
            return false;
        }

        public Type GetDefaultType(IResourceLocation location)
        {
            return typeof(object);
        }

        public void Provide(ProvideHandle provideHandle)
        {
            var assetKey = provideHandle.Location.InternalId;
            var assetType = provideHandle.Location.ResourceType;
            ModLogger.Debug($"Provide asset: {assetKey}");

            string[] suffixes = new string[] {
                "_demo",
                "_music",
                "_cover",
                "_map1",
                "_map2",
                "_map3",
                "_map4"
            };
            var suffix = suffixes.FirstOrDefault(s => assetKey.EndsWith(s));

            if (string.IsNullOrEmpty(suffix))
            {
                ModLogger.Debug($"Suffix not found: {assetKey}");
            }
            var albumKey = assetKey.RemoveFromEnd(suffixes);

            //if (Album.MusicAudio != null)
            //{
            //    Addressables.Release(Album.MusicAudio);
            //}
            if (AlbumManager.LoadedAlbums.TryGetValue(albumKey, out Album album))
            {
                switch (suffix)
                {
                    case "_demo":
                        provideHandle.Complete(album.GetMusic("demo"), true, null);
                        break;
                    case "_music":
                        provideHandle.Complete(album.GetMusic(), true, null);
                        break;
                    case "_cover":
                        provideHandle.Complete(album.GetCover(), true, null);
                        break;
                    case "_map1":
                        provideHandle.Complete(album.GetMap(1), true, null);
                        break;
                    case "_map2":
                        provideHandle.Complete(album.GetMap(2), true, null);
                        break;
                    case "_map3":
                        provideHandle.Complete(album.GetMap(3), true, null);
                        break;
                    case "_map4":
                        provideHandle.Complete(album.GetMap(4), true, null);
                        break;
                    default:
                        provideHandle.Complete(assetType, false, null);
                        break;
                }
            }
        }

        public void Release(IResourceLocation location, object asset)
        {
            ModLogger.Debug($"Release asset: {asset} {asset.GetType()}");
            try
            {
                Album.DestoryAudio();
            }catch(Exception ex)
            {
                ModLogger.Debug(ex);
            }
        }
    }

    public static class AssetPatch
    {
        public static Album beforeAlbum;

        public static void DoPatching(Harmony harmony)
        {
            MethodInfo method;
            MethodInfo methodPrefix;
            MethodInfo methodPostfix;

            // AssetBundle.LoadAsset
            //method = AccessTools.Method(typeof(ResourceManager), "ProvideResource", new Type[] { typeof(IResourceLocation), typeof(Type),typeof(bool) });
            //methodPrefix = AccessTools.Method(typeof(AssetPatch), "ProvideResourcePrefix");
            //harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
            
        }

    }
}