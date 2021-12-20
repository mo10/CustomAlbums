using Assets.Scripts.Common;
using Assets.Scripts.PeroTools.Platforms;
using CustomAlbums.Patch;
using HarmonyLib;
using ModHelper;
using System;
using System.Reflection;
using UnityEngine;

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
            // Fix game pauses when loses focus
            Application.runInBackground = true;

            var harmony = new Harmony("com.github.mo10.customalbums");

            AlbumManager.LoadAll();
            SaveManager.Load();

            //ILCodePatch.DoPatching(harmony);
            JsonPatch.DoPatching(harmony);
            AssetPatch.DoPatching(harmony);
            WebApiPatch.DoPatching(harmony);
            StagePatch.DoPatching(harmony);
            SavesPatch.DoPatching(harmony);
        }
    }
}
