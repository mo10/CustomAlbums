using Assets.Scripts.Common.XDSDK;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Interface;
using FormulaBase;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomAlbums.Patch
{
    public static class SavesPatch
    {
        public static void DoPatching(Harmony harmony)
        {
            // ConfigManager.Init
            var method = AccessTools.Method(typeof(DataManager), "Init");
            var methodPostfix = AccessTools.Method(typeof(SavesPatch), "ConfigManagerInitPostfix");
            harmony.Patch(method, postfix: new HarmonyMethod(methodPostfix));

            method = AccessTools.Method(typeof(StageBattleComponent), "Exit");
            var methodPrefix = AccessTools.Method(typeof(SavesPatch), "BattleExitPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
            // XDSDKManager.OnSaveSelectCallback
            method = AccessTools.Method(typeof(XDSDKManager), "OnSaveSelectCallback");
            methodPrefix = AccessTools.Method(typeof(SavesPatch), "OnSaveSelectCallbackPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
        }
        /// <summary>
        /// Clean custom data at ConfigManager Initialization phase.
        /// </summary>
        public static void ConfigManagerInitPostfix()
        {
            ModLogger.Debug(ToJsonDict(Singleton<DataManager>.instance.datas).JsonSerialize());
            SavesCleanUp(Singleton<DataManager>.instance.datas);
            // ModLogger.Debug(ToJsonDict(Singleton<DataManager>.instance.datas).JsonSerialize());
        }

        public static void BattleExitPrefix(ref string sceneName, ref Action calllback, ref bool withBack)
        {
            string result = Singleton<DataManager>.instance["Account"]["SelectedMusicUid"].GetResult<string>();

            if (result.StartsWith("999-"))
            {
                ModLogger.Debug($"Game/Finish sceneName:{sceneName} withBack:{withBack} SelectedMusicUid:{result}");
            }
        }
        /// <summary>
        /// Clean custom data before clound synchronization.
        /// </summary>
        /// <param name="isLocal"></param>
        /// <param name="datas"></param>
        /// <param name="auth"></param>
        /// <param name="jsonDatas"></param>
        /// <param name="callback"></param>
        public static void OnSaveSelectCallbackPrefix(ref bool isLocal, ref Dictionary<string, IData> datas, ref string auth, ref JToken jsonDatas, ref Action callback)
        {
            SavesCleanUp(datas);
            ModLogger.Debug(ToJsonDict(datas).JsonSerialize());

            //ModLogger.Debug($"isLocal:{isLocal} datas:{datas.JsonSerialize()}");
            //ModLogger.Debug($"jsonDatas:{jsonDatas.JsonSerialize()}");
        }

        public static void SavesCleanUp(Dictionary<string, IData> datas)
        {
            Dictionary<IVariable, Type> dataMapping = new Dictionary<IVariable, Type>()
            {
                {datas["Account"]["SelectedAlbumUid"], typeof(string)},
                {datas["Account"]["SelectedMusicUidFromInfoList"], typeof(string)},
                {datas["Account"]["SelectedAlbumTagIndex"], typeof(int)},

                {datas["Account"]["Collections"], typeof(List<string>)},
                {datas["Account"]["Hides"], typeof(List<string>)},
                {datas["Account"]["History"], typeof(List<string>)},

                {datas["Achievement"]["highest"], typeof(List<IData>)},
                {datas["Achievement"]["fail_count"], typeof(List<IData>)},
                {datas["Achievement"]["full_combo_music"], typeof(List<string>)},
                {datas["Achievement"]["achievements"], typeof(List<string>)},
                {datas["Achievement"]["easy_pass"], typeof(List<string>)},
                {datas["Achievement"]["hard_pass"], typeof(List<string>)},
                {datas["Achievement"]["master_pass"], typeof(List<string>)},
            };
            int index = 0;
            try
            {
                foreach (var mapping in dataMapping)
                {
                    var data = mapping.Key;
                    var type = mapping.Value;
                    if (type == typeof(int))
                    {
                        var value = (int)data.result;
                        if (value == 999)
                        {
                            data.SetResult(0);
                            ModLogger.Debug($"dataMapping[{index}]: Set {value} to {0}");
                        }
                    }
                    else if (type == typeof(string))
                    {
                        var value = data.GetResult<string>();
                        if (value.Contains("999"))
                        {
                            var newVal = value.Replace("999", "0");
                            data.SetResult(newVal);
                            ModLogger.Debug($"dataMapping[{index}]: Set {value} to {newVal}");
                        }
                    }
                    else if (type == typeof(List<string>))
                    {
                        var value = data.GetResult<List<string>>();
                        var count = value.RemoveAll(s => s.Contains("999"));
                        if (count > 0)
                        {
                            ModLogger.Debug($"dataMapping[{index}]: Deleted {count} record(s)");
                        }
                    }
                    else if (type == typeof(List<IData>))
                    {
                        var value = data.GetResult<List<IData>>();
                        //var items = value.Where((IData d) => d["uid"].GetResult<string>().Contains("999"));
                        var count = value.RemoveAll((IData d) => d["uid"].GetResult<string>().Contains("999"));
                        if (count > 0)
                        {
                            ModLogger.Debug($"dataMapping[{index}]: Deleted {count} record(s)");
                        }
                    }
                    else
                    {
                        ModLogger.Debug($"dataMapping[{index}]: Unknown data type");
                    }
                    index++;
                }
            }
            catch (Exception ex)
            {
                ModLogger.Debug($"Failed at {index} {ex}");
            }
        }

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
