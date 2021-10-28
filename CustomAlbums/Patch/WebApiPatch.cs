using Assets.Scripts.PeroTools.Managers;
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
            harmony.Patch(sendToUrl, prefix:new HarmonyMethod(sendToUrlPrefix));
        }
        public static bool SendToUrlPrefix(ref string url, ref string method, ref Dictionary<string, object> datas,
            ref Action<JObject> callback,
            ref Action<string> faillCallback,
            ref Dictionary<string, string> headers,
            ref int failTime,
            ref bool isAutoSend,
            ref string appkey)
        {
            if ("/musedash/v1/music_tag" == url)
            {
                var oldCallback = callback;
                callback = delegate(JObject jObject){
                    var jArray = (JArray)jObject["music_tag_list"];
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
                    ModLogger.Debug("Injected");
                    oldCallback(jObject);
                };
                return true;
            }
            return true;
        }
    }
}
