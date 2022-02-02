using Il2CppSystem.Collections.Generic;
using System;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace CustomAlbums.Addressable
{
    public class ResourceLocatorEmpty : Il2CppSystem.Object
    {
        public string LocatorId => "CustomAlbumAssetLocator";

        public IEnumerable<Il2CppSystem.Object> Keys => AlbumManager.AssetKeys.Cast<IEnumerable<Il2CppSystem.Object>>();

        public ResourceLocatorEmpty(IntPtr handle) : base(handle) { }
        [HideFromIl2Cpp]
        public ResourceLocatorEmpty() : base(ClassInjector.DerivedConstructorPointer<ResourceLocatorEmpty>()) => ClassInjector.DerivedConstructorBody(this);
        
        public bool Locate(Il2CppSystem.Object key, Il2CppSystem.Type type, out IList<IResourceLocation> locations)
        {
            locations = new List<IResourceLocation>().Cast<IList<IResourceLocation>>();
            return false;
        }
    }
    public class ResourceLocator : Il2CppSystem.Object
    {
        public string LocatorId => "CustomAlbumAssetLocator";

        public IEnumerable<Il2CppSystem.Object> Keys => AlbumManager.AssetKeys.Cast<IEnumerable<Il2CppSystem.Object>>();

        public ResourceLocator(IntPtr handle) : base(handle) { }
        public ResourceLocator() : base(ClassInjector.DerivedConstructorPointer<ResourceLocator>()) => ClassInjector.DerivedConstructorBody(this);
        public bool Locate(Il2CppSystem.Object key, Il2CppSystem.Type type, out IList<IResourceLocation> locations)
        {
            var assetKey = key.Cast<Il2CppSystem.String>();

            locations = null;
            if (string.IsNullOrEmpty(assetKey) || !assetKey.StartsWith("fs_") && !assetKey.StartsWith("pkg_"))
                return false;

            var locats = new List<IResourceLocation>();
            locats.Add(new ResourceLocation()
            {
                AssetType = type,
                AssetKey = assetKey
            }.Cast< IResourceLocation>());
            locations = locats.Cast<IList<IResourceLocation>>();

            return true;
        }
    }
}