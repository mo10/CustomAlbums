using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using FormulaBase;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CustomAlbums.Patch
{
    public static class StagePatch
    {
        public static void DoPatching(Harmony harmony)
        {
            MethodInfo method;
            MethodInfo methodPrefix;
            MethodInfo methodPostfix;

            // AssetBundle.LoadAsset
            method = AccessTools.Method(typeof(StageBattleComponent), "OnLoadComplete");
            methodPostfix = AccessTools.Method(typeof(StagePatch), "OnLoadCompletePostfix");
            harmony.Patch(method, postfix: new HarmonyMethod(methodPostfix));
        }
        /// <summary>
        /// Fix music playing too early. 
        /// Author: thegamemaster1234
        /// </summary>
        public static void OnLoadCompletePostfix()
        {
            Singleton<AudioManager>.instance.bgm.Stop();
            Singleton<AudioManager>.instance.bgm.mute = true;
        }
    }
}
