using Assets.Scripts.UI.Panels;
using HarmonyLib;
using ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace CustomAlbums
{
    public static class RankPatch
    {
        public static readonly string AlbumTagUid = "custom";
        public static readonly string AlbumTagGameObjectName = "AlbumTagCell_Custom";

        public static void DoPatching(Harmony harmony)
        {
            // PnlRank.Refresh
            var refresh = AccessTools.Method(typeof(PnlRank), "Refresh");
            var refreshTranspiler = AccessTools.Method(typeof(RankPatch), "RefreshTranspiler");
            harmony.Patch(refresh, transpiler: new HarmonyMethod(refreshTranspiler));
        }
        public static IEnumerable<CodeInstruction> RefreshTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var ilcodes = new List<CodeInstruction>(instructions);

            if (ilcodes[70].opcode == OpCodes.Ldloc_1 && ilcodes[86].opcode == OpCodes.Stloc_3)
            {
                // Delete `int num = int.Parse(result.Substring(0, 1)) * 100 + int.Parse(result.Substring(2, result.Length - 2));`
                ModLogger.Debug($"Garbage opcode deleted");
                ilcodes.RemoveRange(70, (86 - 70) + 1);
            }
            return ilcodes.AsEnumerable();
        }
    }
}
