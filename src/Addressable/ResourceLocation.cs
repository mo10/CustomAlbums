using Il2CppSystem.Collections.Generic;
using System;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace CustomAlbums.Addressable
{
    public class ResourceLocationEmpty : Il2CppSystem.Object
    {
        public string InternalId => "InternalId";
        public string ProviderId => "CustomAlbumAssetResourceProvider";
        public IList<IResourceLocation> Dependencies => new List<IResourceLocation>().Cast<IList<IResourceLocation>>();
        public int DependencyHashCode => 0;
        public bool HasDependencies => false;
        public Il2CppSystem.Object Data => null;
        public string PrimaryKey => "PrimaryKey";
        public Il2CppSystem.Type ResourceType => null;

        public ResourceLocationEmpty(IntPtr handle) : base(handle) { }

        [HideFromIl2Cpp]
        public ResourceLocationEmpty() : base(ClassInjector.DerivedConstructorPointer<ResourceLocationEmpty>()) => ClassInjector.DerivedConstructorBody(this);
        public int Hash(Il2CppSystem.Type resultType)
        {
            return 0;
        }
    }
    public class ResourceLocation : Il2CppSystem.Object
    {
        public string InternalId => AssetKey;
        public string ProviderId => "CustomAlbumAssetResourceProvider";
        public IList<IResourceLocation> Dependencies => new List<IResourceLocation>().Cast<IList<IResourceLocation>>();
        public int DependencyHashCode => 0;
        public bool HasDependencies => false;
        public Il2CppSystem.Object Data => null;
        public string PrimaryKey => AssetKey;
        public Il2CppSystem.Type ResourceType => AssetType;
        /// Custom property
        public Il2CppSystem.Type AssetType;
        public string AssetKey;

        public ResourceLocation(IntPtr handle) : base(handle) { }
        public ResourceLocation() : base(ClassInjector.DerivedConstructorPointer<ResourceLocation>()) => ClassInjector.DerivedConstructorBody(this);
        public int Hash(Il2CppSystem.Type resultType)
        {
            return AssetKey.GetHashCode() * 31 + ((resultType == null) ? 0 : resultType.GetHashCode());
        }
    }
}