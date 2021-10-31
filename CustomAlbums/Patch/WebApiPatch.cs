using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Interface;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomAlbums.Patch
{
    class WebApiPatch
    {
        public static void DoPatching(Harmony harmony)
        {
            // ServerManager.SendToUrl
            var sendToUrl = AccessTools.Method(typeof(ServerManager), "SendToUrl");
            var sendToUrlPrefix = AccessTools.Method(typeof(WebApiPatch), "SendToUrlPrefix");
            harmony.Patch(sendToUrl, prefix: new HarmonyMethod(sendToUrlPrefix));
        }
        /// <summary>
        /// Hook any web request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="datas"></param>
        /// <param name="callback"></param>
        /// <param name="faillCallback"></param>
        /// <param name="headers"></param>
        /// <param name="failTime"></param>
        /// <param name="isAutoSend"></param>
        /// <param name="appkey"></param>
        /// <returns></returns>
        public static bool SendToUrlPrefix(ref string url, ref string method, ref Dictionary<string, object> datas,
            ref Action<JObject> callback,
            ref Action<string> faillCallback,
            ref Dictionary<string, string> headers,
            ref int failTime,
            ref bool isAutoSend,
            ref string appkey)
        {
            var originSuccessCallback = callback;
            var originFailCallback = faillCallback;

            ModLogger.Debug($"Incoming request:{method} {url}");
            switch (url)
            {
                case "/musedash/v1/music_tag":
                    callback = delegate (JObject jObject)
                    {
                        var jArray = (JArray)jObject["music_tag_list"];

                        // Fix 45-* in 'music_tag_list'
                        foreach (JToken obj in jArray)
                        {
                            if ((string)obj["object_id"] != "61557604a75ed5015c2e439a")
                                continue;
                            ModLogger.Debug("Fucked");
                            JArray new_music_list = new JArray();
                            foreach (var uid in obj["music_list"])
                            {
                                string text = (string)uid;
                                if (text == "45-3" ||
                                    text == "45-4" ||
                                    text == "45-5")
                                    continue;

                                if (text.StartsWith("45-"))
                                    new_music_list.Add("44-" + text.RemoveFromStart("45-"));
                                else
                                    new_music_list.Add(uid);
                            }
                            obj["music_list"] = new_music_list;
                        }

                        // Add new music tag
                        jArray.Add(JObject.FromObject(new
                        {
                            object_id = "3d2be24f837b2ec1e5e119bb",
                            created_at = "2021-10-24T00:00:00.000Z",
                            updated_at = "2021-10-24T00:00:00.000Z",
                            tag_name = JObject.FromObject(AlbumManager.Langs),
                            tag_picture = "https://mdmc.moe/cdn/melon.png",
                            pic_name = "",
                            music_list = AlbumManager.GetAllUid(),
                            anchor_pattern = false,
                            sort_key = jArray.Count + 1,
                        }));
                        ModLogger.Debug("Music tag injected.");
                        originSuccessCallback(jObject);
                    };
                    return true;
                case "musedash/v2/pcleaderboard/high-score":
                case "musedash/v2/exhileaderboard/high-score":
                    var uida = Singleton<DataManager>.instance["Account"]["SelectedMusicUid"].GetResult<string>();
                    if (uida.StartsWith("999-"))
                    {
                        ModLogger.Debug($"Blocked high score upload:{uida}");
                        return false; // block this request
                    }
                    ModLogger.Debug($"High score Upload:{uida}");
                    return true;
                
                default:
                    var innerMethod = method;
                    var innerUrl = url;
                    var innerHeaders = headers;
                    

                    callback = delegate (JObject jObject)
                    {
                        // ModLogger.Debug($"Response:{innerMethod} {innerUrl} headers:{innerHeaders.JsonSerialize()} result:{jObject.JsonSerialize()}");
                        originSuccessCallback?.Invoke(jObject);
                    };
                    faillCallback = delegate (string str)
                    {
                        originFailCallback?.Invoke(str);
                    };
                    break;
            }
            return true;
        }
    }
}
