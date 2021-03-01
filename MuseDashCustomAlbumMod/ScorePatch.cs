using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Interface;
using Assets.Scripts.UI.Panels;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace MuseDashCustomAlbumMod
{
    public static class ScorePatch
    {
        public static void DoPathcing(Harmony harmony)
        {
            var getRanks = AccessTools.Method(typeof(ServerManager), "GetRanks");
            var getRanksPrefix = AccessTools.Method(typeof(ScorePatch), "GetRanksPrefix");
            harmony.Patch(getRanks, new HarmonyMethod(getRanksPrefix));

            var refresh = AccessTools.Method(typeof(PnlRank), "Refresh");
            var refreshTranspiler = AccessTools.Method(typeof(ScorePatch), "RefreshTranspiler");
            harmony.Patch(refresh,transpiler: new HarmonyMethod(refreshTranspiler));
        }
        public static bool GetRanksPrefix(string musicUid, int difficulty, Action<JToken, JToken, int> callback, Action<string> failCallback)
        {
			//string text = Singleton<DataManager>.instance["GameConfig"]["Auth"].GetResult<string>();
			//text = ((!string.IsNullOrEmpty(text)) ? text : string.Empty);
			//string text2 = "musedash/v1/pcleaderboard/top";
			//string url = text2;
			//string method = "GET";

            if (musicUid.StartsWith(CustomAlbum.MusicPackgeUid.ToString()))
            {
                ModLogger.Debug($"Disable rank, MusicUid:{musicUid} difficulty:{difficulty}");
                failCallback("Custom");
                return false;
            }
            return true;
        }
        public static IEnumerable<CodeInstruction> RefreshTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // 移除PnlRank遗留代码
            // int num = int.Parse(result.Substring(0, 1)) * 100 + int.Parse(result.Substring(2, result.Length - 2));
            var startIndex = -1;
            var endIndex = -1;

            bool foundIL = false;

            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (!foundIL)
                {
                    if (codes[i].opcode == OpCodes.Ldloc_1)
                    {
                        startIndex = i;
                        foundIL = true;
                        continue;
                    }
                }
                else
                {
                    if (codes[i].opcode == OpCodes.Stloc_3)
                    {
                        endIndex = i;
                        break;
                    }
                }
            }
            if (startIndex > -1 && endIndex > -1)
            {
                codes[startIndex].opcode = OpCodes.Nop;
                codes.RemoveRange(startIndex + 1, endIndex - startIndex);
                ModLogger.Debug("Fixed!");
            }

            return codes.AsEnumerable();
        }
    }
}
