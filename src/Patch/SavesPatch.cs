using Account;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Interface;
using Assets.Scripts.PeroTools.Platforms;
using Assets.Scripts.PeroTools.Platforms.Steam;
using Assets.Scripts.UI.Panels;
using FormulaBase;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using Il2CppSystem.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustomAlbums.Patch
{
    public static class SavesPatch
    {
        private static readonly Logger Log = new Logger("SavesPatch");
        public static readonly string BackupPath = "Mods/saves_backup";

        public static readonly System.Collections.Generic.List<Type> ISyncTypes = new System.Collections.Generic.List<Type> {
            typeof(SteamSync)
        };

        public static void DoPatching(HarmonyLib.Harmony harmony)
        {
            MethodInfo method;
            MethodInfo methodPrefix;
            MethodInfo methodPostfix;
            
            // StageBattleComponent.Exit
            method = AccessTools.Method(typeof(StageBattleComponent), "Exit");
            methodPrefix = AccessTools.Method(typeof(SavesPatch), "BattleExitPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
            // XDSDKManager.OnSaveSelectCallback
            method = AccessTools.Method(typeof(GameAccountSystem), "OnSaveSelectCallback");
            methodPrefix = AccessTools.Method(typeof(SavesPatch), "OnSaveSelectCallbackPrefix");
            methodPostfix = AccessTools.Method(typeof(SavesPatch), "OnSaveSelectCallbackPostfix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix), postfix: new HarmonyMethod(methodPostfix));
            // XDSDKManager.RefreshDatas
            method = AccessTools.Method(typeof(GameAccountSystem), "RefreshDatas");
            methodPrefix = AccessTools.Method(typeof(SavesPatch), "RefreshDatasPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
            // ISync implement
            foreach (var ISyncImpl in ISyncTypes)
            {
                // ISync.SaveLocal
                method = AccessTools.Method(ISyncImpl, "SaveLocal");    
                methodPrefix = AccessTools.Method(typeof(SavesPatch), "ISyncSaveLocalPrefix");
                methodPostfix = AccessTools.Method(typeof(SavesPatch), "ISyncSaveLocalPostfix");
                harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix), postfix: new HarmonyMethod(methodPostfix));
                // ISync.LoadLocal
                method = AccessTools.Method(ISyncImpl, "LoadLocal");
                // methodPrefix = AccessTools.Method(typeof(SavesPatch), "ISyncLoadLocalPrefix");
                methodPostfix = AccessTools.Method(typeof(SavesPatch), "ISyncLoadLocalPostfix");
                harmony.Patch(method, postfix: new HarmonyMethod(methodPostfix));
            }
        }
        /// <summary>
        /// Remove custom data before saving.
        /// </summary>
        public static void ISyncSaveLocalPrefix()
        {
            try {
                SaveManager.SplitCustomData();
                SaveManager.Save();
                SaveManager.CleanCustomData();
            } catch(Exception e) {
                Log.Error(e);
            }
        }
        /// <summary>
        /// Restore custom data after saving.
        /// </summary>
        public static void ISyncSaveLocalPostfix()
        {
            try {
                SaveManager.RestoreCustomData();
            } catch(Exception e) {
                Log.Error(e);
            }
        }
        /// <summary>
        /// Restore custom data after save file loaded.
        /// </summary>
        public static void ISyncLoadLocalPostfix()
        {
            try {
                Backup();

                SaveManager.CleanCustomData();
                SaveManager.RestoreCustomData();
            } catch(Exception e) {
                Log.Error(e);
            }
        }
        /// <summary>
        /// Upgrade NoPollution data and restore custom data after cloud synchronization
        /// </summary>
        public static void RefreshDatasPrefix()
        {
            try {
                Backup();

                SaveManager.SplitCustomData();
                SaveManager.Save();

                SaveManager.CleanCustomData();
                SaveManager.RestoreCustomData();
            } catch(Exception e) {
                Log.Error(e);
            }
        }

        /// <summary>
        /// On battle end.
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="calllback"></param>
        /// <param name="withBack"></param>
        public static void BattleExitPrefix(ref string sceneName, ref Il2CppSystem.Action calllback, ref bool backToPnlStage)
        {
            string result = Singleton<DataManager>.instance["Account"]["SelectedMusicUid"].GetResult<string>();

            if (result.StartsWith("999-"))
            {
                Log.Debug($"Game/Finish sceneName:{sceneName} withBack:{backToPnlStage} SelectedMusicUid:{result}");
            }
        }


        public static void OnSaveSelectCallbackPrefix(ref bool isLocal)
        {
            if (isLocal)
            {
                SaveManager.SplitCustomData();
                SaveManager.Save();
                SaveManager.CleanCustomData();
            }
        }
        public static void OnSaveSelectCallbackPostfix(ref bool isLocal)
        {
            if (isLocal)
            {
                SaveManager.RestoreCustomData();
            }
        }
        public static void Backup()
        {
            // Backup
            var path = Path.Combine(Directory.GetCurrentDirectory(), BackupPath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var now = DateTime.Now.ToString("yyyy_MM_dd_H_mm_ss");
            var filePath = Path.Combine(path, $"{now}.sav");
            File.WriteAllBytes(filePath, Singleton<DataManager>.instance.ToBytes());
            Log.Debug($"Save backup: {filePath}");
            // Backup as json
            filePath = Path.Combine(path, $"{now}_debug.json");
            File.WriteAllText(filePath, ToJsonDict(Singleton<DataManager>.instance.datas).JsonSerialize());
            Log.Debug($"Save backup: {filePath}");
        }
        /// <summary>
        /// IData dict to JObject dict
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static System.Collections.Generic.Dictionary<string, JObject> ToJsonDict(Dictionary<string, IData> datas)
        {
            var dictionary = new System.Collections.Generic.Dictionary<string, JObject>();
            foreach (KeyValuePair<string, IData> keyValuePair in datas)
            {
                SingletonDataObject singletonDataObject = keyValuePair.Value?.TryCast<SingletonDataObject>();
                if (singletonDataObject != null)
                {
                    dictionary.Add(keyValuePair.Key, singletonDataObject.ToJson().JsonDeserialize<JObject>());
                }
            }
            return dictionary;
        }
    }
}