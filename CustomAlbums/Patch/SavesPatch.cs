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

            // ConfigManager.Init
            method = AccessTools.Method(typeof(DataManager), "Init");
            methodPostfix = AccessTools.Method(typeof(SavesPatch), "DataManagerInitPostfix");
            harmony.Patch(method, postfix: new HarmonyMethod(methodPostfix));
            // StageBattleComponent.Exit
            method = AccessTools.Method(typeof(StageBattleComponent), "Exit");
            methodPrefix = AccessTools.Method(typeof(SavesPatch), "BattleExitPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
            // XDSDKManager.OnSaveSelectCallback
            method = AccessTools.Method(typeof(XDSDKManager), "OnSaveSelectCallback");
            methodPrefix = AccessTools.Method(typeof(SavesPatch), "OnSaveSelectCallbackPrefix");
            methodPrefix = AccessTools.Method(typeof(SavesPatch), "OnSaveSelectCallbackPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
            // PnlStage.PreWarm
            method = AccessTools.Method(typeof(PnlStage), "PreWarm");
            methodPrefix = AccessTools.Method(typeof(SavesPatch), "PreWarmPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
        }
        public static void PreWarmPrefix()
        {
            //NoPollutionHelper.UpgradeAndRestore();
        }
        /// <summary>
        /// 
        /// Todo: Restore custom score at ConfigManager initialization phase.
        /// 
        /// Note: NoPollution data save in Singleton<DataManager>.instance["Account"]["CustomTracks"]
        /// </summary>
        public static void DataManagerInitPostfix()
        {

            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"saves_backup");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var filePath = Path.Combine(path, $"{DateTime.Now.ToString("yyyy_MM_dd_H_mm_ss")}.json");
            File.WriteAllText(filePath, ToJsonDict(Singleton<DataManager>.instance.datas).JsonSerialize());
            ModLogger.Debug($"Saves backup:{filePath}");
            SaveManager.SplitCustomData();
            SaveManager.Save();
            //ModLogger.Debug(ToJsonDict(Singleton<DataManager>.instance.datas).JsonSerialize());
            // SavesCleanUp(Singleton<DataManager>.instance.datas);
            //foreach (var album in AlbumManager.LoadedAlbums.Values)
            //{
            //    ModLogger.Debug(album.availableMaps.JsonSerialize());
            //}
            // ModLogger.Debug(ToJsonDict(Singleton<DataManager>.instance.datas).JsonSerialize());
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
        /// <summary>
        /// Clean custom data before cloud synchronization.
        /// </summary>
        /// <param name="isLocal"></param>
        /// <param name="datas"></param>
        /// <param name="auth"></param>
        /// <param name="jsonDatas"></param>
        /// <param name="callback"></param>
        public static void OnSaveSelectCallbackPrefix(ref bool isLocal, ref Dictionary<string, IData> datas, ref string auth, ref JToken jsonDatas, ref Action callback)
        {
            //SavesCleanUp(datas);
            //ModLogger.Debug(ToJsonDict(datas).JsonSerialize());

            //ModLogger.Debug($"isLocal:{isLocal} datas:{datas.JsonSerialize()}");
            //ModLogger.Debug($"jsonDatas:{jsonDatas.JsonSerialize()}");
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
