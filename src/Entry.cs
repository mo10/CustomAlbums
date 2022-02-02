using CustomAlbums.Patch;
using UnityEngine;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.GameCore.Managers;
using UnhollowerRuntimeLib;
using CustomAlbums.Addressable;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.AddressableAssets;
#if BEPINEX
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
#elif MELON
using MelonLoader;
[assembly: MelonInfo(typeof(CustomAlbums.ModEntry), "CustomAlbums", "3.0.0", "Mo10")]
[assembly: MelonGame("PeroPeroGames", "MuseDash")]
#endif

namespace CustomAlbums
{
    public static class Entry
    {
        public static void DoPatching(HarmonyLib.Harmony harmony)
        {
            Application.runInBackground = true;

            AlbumManager.LoadAll();
            harmony.PatchAll(typeof(SteamPatch));
            harmony.PatchAll(typeof(AssetPatch));
            WebApiPatch.DoPatching();
            harmony.PatchAll(typeof(WebApiPatch));
            harmony.PatchAll(typeof(ResourcePatch));
            //ResourcePatch.DoPatching(harmony);
        }
    }

#if BEPINEX
    [BepInPlugin("com.github.mo10.customalbums", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("MuseDash.exe")]
    public class ModEntry : BasePlugin
    {
        public override void Load()
        {
            Log.LogInfo($"CustomAlbums is loaded!");
            Harmony harmony = new Harmony("com.github.mo10.customalbums");

            Entry.DoPatching(harmony);
        }
    }

#elif MELON
    public class ModEntry : MelonMod
    {
        public override void OnApplicationStart()
        {
            LoggerInstance.Msg($"CustomAlbums is loaded!");
            LoggerInstance.Msg($"Singleton<iBMSCManager>.instance {Singleton<iBMSCManager>.instance.bmsFile}");

            //ClassInjector.RegisterTypeInIl2CppWithInterfaces<ResourceProviderEmpty>(true, typeof(IResourceProvider));
            //ClassInjector.RegisterTypeInIl2CppWithInterfaces<ResourceLocatorEmpty>(true, typeof(IResourceLocator));
            //ClassInjector.RegisterTypeInIl2CppWithInterfaces<ResourceLocationEmpty>(true, typeof(IResourceLocation));

            //var providerPtr = new ResourceProviderEmpty().Pointer;
            //var locatorPtr = new ResourceLocatorEmpty().Pointer;
            //var locationPtr = new ResourceLocationEmpty().Pointer;

            //var provider = new IResourceProvider(providerPtr);
            //var locator = new IResourceLocator(locatorPtr);
            //var location = new IResourceLocation(locationPtr);

            //Addressables.ResourceManager.m_ResourceProviders.Add(provider);
            //Addressables.m_Addressables.m_ResourceLocators.Add(new AddressablesImpl.ResourceLocatorInfo(locator, "CustomLocator", location));

            //LoggerInstance.Msg($"m_ResourceLocators:{Addressables.m_Addressables.m_ResourceLocators.Count}");

            Entry.DoPatching(HarmonyInstance);
        }
    }
#endif
}
