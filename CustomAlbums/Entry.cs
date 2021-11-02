using Assets.Scripts.Common;
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

            AlbumManager.LoadAll();
            //StageUIPatch.DoPatching(harmony);
            //DataPatch.DoPathcing(harmony);
            //ExtraPatch.DoPatching(harmony);
            ILCodePatch.DoPatching(harmony);
            JsonPatch.DoPatching(harmony);
            AssetPatch.DoPatching(harmony);
            WebApiPatch.DoPatching(harmony);
            StagePatch.DoPatching(harmony);
            SavesPatch.DoPatching(harmony);
            //CustomAlbum.LoadCustomAlbums();

            // var entities = CustomDefines.entities.Keys;
            // var events = CustomDefines.events.Keys;
            // ModLogger.Debug("entities:" + entities.JsonSerialize());
            // ModLogger.Debug("events:" + events.JsonSerialize());
        }
    }
}
