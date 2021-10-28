using CustomAlbums.Patch;
using HarmonyLib;
using ModHelper;
using System.Reflection;

namespace CustomAlbums
{
    class Entry : IMod
    {
        public string Name => "CustomAlbums";

        public string Description => "Adds custom charts to Muse Dash.";

        public string Author => "Mo10";

        public string HomePage => "https://github.com/mo10/CustomAlbums";

        public void DoPatching()
        {
            var harmony = new Harmony("com.github.mo10.customalbums");
            var assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);

            AlbumManager.LoadAll();

            //StageUIPatch.DoPatching(harmony);
            //DataPatch.DoPathcing(harmony);
            //ExtraPatch.DoPatching(harmony);
            ILCodePatch.DoPatching(harmony);
            JsonPatch.DoPatching(harmony);
            AssetPatch.DoPatching(harmony);
            WebApiPatch.DoPatching(harmony);
            //CustomAlbum.LoadCustomAlbums();
        }
    }
}
