using System;
using System.Linq;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CustomAlbums.Addressable
{
    public class ResourceProviderEmpty : Il2CppSystem.Object
    {
        private readonly Logger Log = new Logger("ResourceProviderEmpty");

        public string ProviderId => "CustomAlbumAssetResourceProvider";
        public ProviderBehaviourFlags BehaviourFlags => ProviderBehaviourFlags.None;

        public ResourceProviderEmpty(IntPtr handle) : base(handle) { }
        [HideFromIl2Cpp]
        public ResourceProviderEmpty() : base(ClassInjector.DerivedConstructorPointer<ResourceProviderEmpty>()) => ClassInjector.DerivedConstructorBody(this);

        public bool CanProvide(Il2CppSystem.Type type, IResourceLocation location)
        {
            Log.Debug("CanProvide");
            return false;
        }

        public Il2CppSystem.Type GetDefaultType(IResourceLocation location)
        {
            Log.Debug("GetDefaultType");
            return null;
        }

        public void Provide(ProvideHandle provideHandle)
        {
            Log.Debug("Provide");
        }

        public void Release(IResourceLocation location, Il2CppSystem.Object asset)
        {
            Log.Debug("Release");
        }
    }
    public class ResourceProvider : Il2CppSystem.Object
    {
        private static readonly Logger Log = new Logger("ResourceProvider");

        public string ProviderId => "CustomAlbumAssetResourceProvider";
        public ProviderBehaviourFlags BehaviourFlags => ProviderBehaviourFlags.None;

        public static ProvideHandle handle;

        public ResourceProvider(IntPtr handle) : base(handle) { }
        public ResourceProvider() : base(ClassInjector.DerivedConstructorPointer<ResourceProvider>()) => ClassInjector.DerivedConstructorBody(this);

        public bool CanProvide(Il2CppSystem.Type type, IResourceLocation location)
        {
            if (location.GetType() == typeof(ResourceLocation))
            {
                var customLocation = location.Cast<ResourceLocation>();
                return true;
            }
            else
            {
                Log.Debug("False");
            }
            return false;
        }

        public Il2CppSystem.Type GetDefaultType(IResourceLocation location)
        {
            return null;
        }

        public void Provide(ProvideHandle provideHandle)
        {
            var assetKey = provideHandle.Location.InternalId;
            var assetType = provideHandle.Location.ResourceType;
            Log.Debug($"Provide asset: {assetKey}");

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
                Log.Debug($"Suffix not found: {assetKey}");
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

        public void Release(IResourceLocation location, Il2CppSystem.Object asset)
        {
            Log.Debug($"Release asset: {asset} {asset.GetType()}");
            try
            {
                Album.DestoryAudio();
            }
            catch
            {
                Log.Debug("GGG");
            }
        }
    }
}