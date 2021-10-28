using Assets.Scripts.PeroTools.AssetBundles;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.UI.Panels;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace CustomAlbums.Patch
{
    public class ILCodePatch
    {
        public static void DoPatching(Harmony harmony)
        {
            // PnlRank.Refresh
            var refresh = AccessTools.Method(typeof(PnlRank), "Refresh");
            var refreshTranspiler = AccessTools.Method(typeof(ILCodePatch), "RefreshTranspiler");
            harmony.Patch(refresh, transpiler: new HarmonyMethod(refreshTranspiler));

            // WebUtils.SendToUrl
            var sendToUrl = AccessTools.Method(typeof(WebUtils), "SendToUrl");
            var sendToUrlTranspiler = AccessTools.Method(typeof(ILCodePatch), "SendToUrlTranspiler");
            harmony.Patch(sendToUrl, transpiler: new HarmonyMethod(sendToUrlTranspiler));

            // WebUtils.SendToUrl callback
            var sendToUrlCallback = AccessTools.Method(typeof(WebUtils).GetNestedNonPublicType("<SendToUrl>c__AnonStorey0"), "<>m__1");
            harmony.Patch(sendToUrlCallback, transpiler: new HarmonyMethod(sendToUrlTranspiler));
        }

        /// <summary>
        /// Remove useless code:
        ///     int num = int.Parse(result.Substring(0, 1)) * 100 + int.Parse(result.Substring(2, result.Length - 2));
        /// Prevent int.Parse exceptions when uid is greater than two digits.
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IEnumerable<CodeInstruction> RefreshTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var ilcodes = new List<CodeInstruction>(instructions);

            if (ilcodes.Count >= 70 && ilcodes.Count >= 86 &&
                ilcodes[70].opcode == OpCodes.Ldloc_1 && ilcodes[86].opcode == OpCodes.Stloc_3)
            {
                // Delete `int num = int.Parse(result.Substring(0, 1)) * 100 + int.Parse(result.Substring(2, result.Length - 2));`
                ilcodes.RemoveRange(70, (86 - 70) + 1); // Remove ilcode from range 70 to 86
                ModLogger.Debug($"Fixed: PnlRank.Refresh");
            }
            return ilcodes.AsEnumerable();
        }
        /// <summary>
        /// Make the WebUtils.SendToUrl quiet
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IEnumerable<CodeInstruction> SendToUrlTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var ilcodes = new List<CodeInstruction>(instructions);

            if (ilcodes.Count >= 12 && ilcodes[12].opcode == OpCodes.Ldstr)
            {
                // "==============Succuessfully recieve from url: {0} on method: {1}==============, with data: \n{2} with response code : {3}"
                ilcodes[12].operand = "[SendToUrl] Response received: {1} {0} Status:{3}";
                ModLogger.Debug($"Fixed: response received message");
            }
            if (ilcodes.Count >= 55 && ilcodes[55].opcode == OpCodes.Ldstr)
            {
                // "==============Error recieve from url: {0} on method: {1}==============, with data: \n{2} with response code : {3}"
                ilcodes[55].operand = "[SendToUrl] Error received: {1} {0} Status:{3}";
                ModLogger.Debug($"Fixed: error received message");
            }
            if (ilcodes.Count >= 78 && ilcodes[78].opcode == OpCodes.Ldstr)
            {
                // "==============Send to url: {0} on method: {1}==============, with data: \n{2}"
                ilcodes[78].operand = "[SendToUrl] Request sent: {1} {0}";
                ModLogger.Debug($"Fixed: request sent message");
            }
            if (ilcodes.Count >= 114 && ilcodes[114].opcode == OpCodes.Ldstr)
            {
                // "==============With header: \n{0}=============="
                ilcodes[114].operand = "";
                ModLogger.Debug($"Fixed: hidden request header");
            }

            return ilcodes.AsEnumerable();
        }
    }
}
