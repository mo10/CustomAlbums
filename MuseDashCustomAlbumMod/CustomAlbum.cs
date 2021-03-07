using HarmonyLib;
using ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using MuseDashCustomAlbumMod.Managers;

namespace MuseDashCustomAlbumMod
{
    public static class CustomAlbum
    {

        public static void DoPatching()
        {
            var harmony = new Harmony("com.github.mo10.customalbum");

            StageUIPatch.DoPatching(harmony);
            DataPatch.DoPathcing(harmony);
            ExtraPatch.DoPatching(harmony);
            ScorePatch.DoPathcing(harmony);

            CustomInfoManager.LoadCustom();

            //LoadCustomAlbums();
        }

        //public static void LoadCustomAlbums()
        //{

        //    if (!Directory.Exists(CustomInfoManager.ALBUM_PACK_PATH))
        //    {
        //        // Create custom album path
        //        Directory.CreateDirectory(CustomInfoManager.ALBUM_PACK_PATH);
        //    }
           
        //}
    }
}