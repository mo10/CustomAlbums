using Assets.Scripts.Common.XDSDK;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Interface;
using Assets.Scripts.PeroTools.Platforms;
using Assets.Scripts.PeroTools.Platforms.Steam;
using Assets.Scripts.UI.Panels;
using FormulaBase;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustomAlbums.Patch
{
    public static class SavesPatch
    {
        public static void DoPatching(Harmony harmony)
        {
            MethodInfo method;
            MethodInfo methodPrefix;
            MethodInfo methodPostfix;

            // StageBattleComponent.Exit
            method = AccessTools.Method(typeof(StageBattleComponent), "Exit");
            methodPrefix = AccessTools.Method(typeof(SavesPatch), "BattleExitPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
            // XDSDKManager.OnSaveSelectCallback
            method = AccessTools.Method(typeof(XDSDKManager), "OnSaveSelectCallback");
            methodPrefix = AccessTools.Method(typeof(SavesPatch), "OnSaveSelectCallbackPrefix");
            methodPostfix = AccessTools.Method(typeof(SavesPatch), "OnSaveSelectCallbackPostfix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix), postfix: new HarmonyMethod(methodPostfix));
            // XDSDKManager.RefreshDatas
            method = AccessTools.Method(typeof(XDSDKManager), "RefreshDatas");
            methodPrefix = AccessTools.Method(typeof(SavesPatch), "RefreshDatasPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
            // ISync implement
            foreach (var ISyncImpl in typeof(ISync).GetAllImplement())
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
            SaveManager.SplitCustomData();
            SaveManager.Save();
            SaveManager.CleanCustomData();
        }
        /// <summary>
        /// Restore custom data after saving.
        /// </summary>
        public static void ISyncSaveLocalPostfix()
        {
            SaveManager.RestoreCustomData();
        }
        /// <summary>
        /// Restore custom data after save file loaded.
        /// </summary>
        public static void ISyncLoadLocalPostfix()
        {
            // Backup
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"saves_backup");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var filePath = Path.Combine(path, $"{DateTime.Now.ToString("yyyy_MM_dd_H_mm_ss")}.sav");
            File.WriteAllBytes(filePath, Singleton<DataManager>.instance.ToBytes());
            ModLogger.Debug($"Save backup: {filePath}");

            NoPollutionHelper.Upgrade();
            SaveManager.CleanCustomData();
            SaveManager.RestoreCustomData();
        }
        /// <summary>
        /// Upgrade NoPollution data and restore custom data after cloud synchronization
        /// </summary>
        public static void RefreshDatasPrefix()
        {
            // Backup
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"saves_backup");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var filePath = Path.Combine(path, $"{DateTime.Now.ToString("yyyy_MM_dd_H_mm_ss")}_cloudsync.sav");
            File.WriteAllBytes(filePath, Singleton<DataManager>.instance.ToBytes());
            ModLogger.Debug($"Save backup: {filePath}");

            NoPollutionHelper.Upgrade();
            SaveManager.CleanCustomData();
            SaveManager.RestoreCustomData();
        }

        /// <summary>
        /// On battle end.
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="calllback"></param>
        /// <param name="withBack"></param>
        public static void BattleExitPrefix(ref string sceneName, ref Action calllback, ref bool withBack)
        {
            string result = Singleton<DataManager>.instance["Account"]["SelectedMusicUid"].GetResult<string>();

            if (result.StartsWith("999-"))
            {
                ModLogger.Debug($"Game/Finish sceneName:{sceneName} withBack:{withBack} SelectedMusicUid:{result}");
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

        public static JObject Clean(JObject datas)
        {
            Dictionary<string, Type> keyMapping = new Dictionary<string, Type>()
            {
                // Account
                {"SelectedAlbumUid", typeof(string)},
                {"SelectedMusicUidFromInfoList", typeof(string)},
                {"SelectedAlbumTagIndex", typeof(int)},
                {"Collections", typeof(List<string>)},
                {"Hides", typeof(List<string>)},
                {"History", typeof(List<string>)},
                // Achievement
                {"highest", typeof(List<JObject>)},
                {"fail_count", typeof(List<JObject>)},
                {"full_combo_music", typeof(List<string>)},
                {"achievements", typeof(List<string>)},
                {"easy_pass", typeof(List<string>)},
                {"hard_pass", typeof(List<string>)},
                {"master_pass", typeof(List<string>)},

            };
            foreach (var mapping in keyMapping)
            {
                var key = mapping.Key;
                var type = mapping.Value;

                if (datas[key] == null)
                    continue;

                //if (type == typeof(int))
                //{
                //    var value = datas[key]?.Value<int>() ?? 0;
                //    if (value == 999)
                //    {
                //        datas[key] = 0;
                //        ModLogger.Debug($"{key}: Set {value} to 0");
                //    }
                //}
                //else if (type == typeof(string))
                //{
                //    var value = datas[key]?.Value<string>() ?? "";
                //    if (value.Contains("999"))
                //    {
                //        var newVal = value.Replace("999", "0");
                //        datas[key] = newVal;
                //        ModLogger.Debug($"{key}: Set {value} to {newVal}");
                //    }
                //}
                else if (type == typeof(List<string>))
                {

                    var value = ((JArray)datas[key]).ToObject<List<string>>();
                    var count = value.RemoveAll(s => s.Contains("999"));
                    if (count > 0)
                    {
                        datas[key] = JArray.FromObject(value);
                        ModLogger.Debug($"{key}: Deleted {count} record(s)");
                    }
                }
                else if (type == typeof(List<JObject>))
                {
                    var value = ((JArray)datas[key]).ToObject<List<JObject>>();
                    var count = value.RemoveAll((JObject d) => d["uid"].Value<string>().Contains("999"));
                    if (count > 0)
                    {
                        datas[key] = JArray.FromObject(value);
                        ModLogger.Debug($"{key}: Deleted {count} record(s)");
                    }
                }
                else
                {
                    ModLogger.Debug($"{key}: Unknown data type: {type}");
                }
            }
            return datas;
        }
        /// <summary>
        /// IData dict to JObject dict
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static Dictionary<string, JObject> ToJsonDict(Dictionary<string, IData> datas)
        {
            Dictionary<string, JObject> dictionary = new Dictionary<string, JObject>();
            foreach (KeyValuePair<string, IData> keyValuePair in Singleton<DataManager>.instance.datas)
            {
                SingletonDataObject singletonDataObject = keyValuePair.Value as SingletonDataObject;
                if (singletonDataObject)
                {
                    dictionary.Add(keyValuePair.Key, singletonDataObject.ToJson().JsonDeserialize<JObject>());
                }
            }
            return dictionary;
        }
    }
}
