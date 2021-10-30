using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using FormulaBase;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomAlbums.Patch
{
    public static class StagePatch
    {
        public static void DoPatching(Harmony harmony)
        {
            // AssetBundle.LoadAsset
            var originalMethod = AccessTools.Method(typeof(StageBattleComponent), "OnLoadComplete");
            var postfixMethod = AccessTools.Method(typeof(StagePatch), "OnLoadCompletePostfix");
            harmony.Patch(originalMethod, postfix: new HarmonyMethod(postfixMethod));
        }
        public static void OnLoadCompletePostfix()
        {
            // Fix play music too early. Author: thegamemaster1234
            Singleton<AudioManager>.instance.bgm.Stop();
            Singleton<AudioManager>.instance.bgm.mute = true;
        }
    }
}
