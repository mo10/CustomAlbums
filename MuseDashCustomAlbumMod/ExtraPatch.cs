using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Interface;
using Assets.Scripts.PeroTools.Nice.Variables;
using Assets.Scripts.UI.Panels;
using HarmonyLib;

namespace MuseDashCustomAlbumMod
{
    class ExtraPatch
    {
        public static void DoPatching(Harmony harmony)
        {
            MethodInfo methodIsCanPreparationOut = AccessTools.Method(typeof(Assets.Scripts.UI.Panels.PnlStage), "IsCanPreparationOut");
            MethodInfo methodICPOPrefix = AccessTools.Method(typeof(ExtraPatch), "IsCanPreparationOutPrefix");
            harmony.Patch(methodIsCanPreparationOut, new HarmonyMethod(methodICPOPrefix), null, null);

            MethodInfo methodSetBgLockAction = AccessTools.Method(typeof(Assets.Scripts.UI.Panels.PnlStage), "SetBgLockAction");
            MethodInfo methodSBLAPrefix = AccessTools.Method(typeof(ExtraPatch), "SetBgLockActionPrefix");
            harmony.Patch(methodSetBgLockAction, new HarmonyMethod(methodSBLAPrefix), null, null);

            MethodInfo methodOnBattleEnd = AccessTools.Method(typeof(Assets.Scripts.GameCore.Managers.StatisticsManager), "OnBattleEnd");
            MethodInfo methodOBEPrefix = AccessTools.Method(typeof(ExtraPatch), "OnBattleEndPrefix");
            harmony.Patch(methodOnBattleEnd, new HarmonyMethod(methodOBEPrefix), null, null);

            MethodInfo methodChangeMusic = AccessTools.Method(typeof(Assets.Scripts.UI.Panels.PnlStage), "ChangeMusic");
            MethodInfo methodCMPostfix = AccessTools.Method(typeof(ExtraPatch), "ChangeMusicPostfix");
            harmony.Patch(methodChangeMusic, null, new HarmonyMethod(methodCMPostfix), null);
        }

        public static bool IsCanPreparationOutPrefix(PnlStage __instance, ref bool __result)
        {
            // 解除自定义谱面的上锁状态
            if (__instance.GetSelectedMusicAlbumJsonName() == CustomAlbum.JsonName)
            {
                __result = true;
                return false;
            }

            return true;
        }

        public static bool SetBgLockActionPrefix(PnlStage __instance)
        {
            // 解除自定义谱面的上锁背景
            ModHelper.ModLogger.Debug("Try Set BGLockAction");
            ModHelper.ModLogger.Debug(__instance.GetSelectedMusicAlbumJsonName());
            if (__instance.GetSelectedMusicAlbumJsonName() == CustomAlbum.JsonName)
            {
                ModHelper.ModLogger.Debug("Set BGLockAction");
                __instance.bgAlbumLock.SetActive(false);
                __instance.bgAlbumFree.SetActive(false);

                __instance.txtBudgetIsBurning15.SetActive(false);
                __instance.txtBudgetIsBurning30.SetActive(false);
                __instance.txtNotPurchase.SetActive(false);

                return false;
            }

            return true;
        }

        public static bool OnBattleEndPrefix()
        {
            // 禁用自定义谱面的成绩上传
            if (Singleton<DataManager>.instance["Account"]["SelectedMusicUid"].GetResult<string>()
                .StartsWith($"{CustomAlbum.MusicPackgeUid}-")) return false;

            return true;
        }

        public static void ChangeMusicPostfix(PnlStage __instance)
        {
            // 禁用掉收藏按钮
            if (__instance.GetSelectedMusicAlbumJsonName() == CustomAlbum.JsonName)
            {
                //__instance.difficulty3Lock.SetActive(false);
                //__instance.difficulty3Master.SetActive(__instance.difficulty3.text != "0");

                __instance.tglLike.gameObject.SetActive(false);
                SetBgLockActionPrefix(__instance);
                return;
            }

            __instance.tglLike.gameObject.SetActive(true);
        }
    }
}
