using PeroTools2.Commons;
using Newtonsoft.Json.Linq;
using System;
using Account;
using UnityEngine.Networking;
using static PeroTools2.Commons.WebUtils;
using Il2CppSystem.Collections.Generic;
using UnhollowerRuntimeLib;
using UnhollowerBaseLib;
using HarmonyLib;

namespace CustomAlbums.Patch
{
    class WebApiPatch
    {
        private static Logger Log = new Logger("WebApiPatch");

        /// <summary>
        /// Hook GameAccountSystem request.
        /// </summary>
        /// <returns></returns>
        [HarmonyPatch(typeof(GameAccountSystem), nameof(GameAccountSystem.SendToUrl))]
        [HarmonyPrefix]
        public static bool SendToUrlPrefix(
            ref string url,
            ref string method, 
            ref Dictionary<string, Il2CppSystem.Object> datas, 
            ref Il2CppSystem.Action<JObject> succeedCallback , 
            ref Il2CppSystem.Action<long, string> faillCallback,
            ref Il2CppSystem.Action startCallback,
            ref Il2CppSystem.Action completeCallback,
            ref Dictionary<string, string> headers
            )
        {
            Log.Debug($"request:{url}");
            return true;
        }
        /// <summary>
        /// Hook all request.
        /// </summary>
        /// <returns></returns>
        [HarmonyPatch(typeof(WebUtils), nameof(WebUtils.SendToUrl), new Type[] { typeof(PeroWebRequest) })]
        [HarmonyPostfix]
        public static void SendToUrlPrefix2(PeroWebRequest webRequest)
        {
            Log.Debug($"Incoming request:{webRequest.method} {webRequest.url}");

            Log.Debug($"Request:{webRequest.method} {webRequest.url} headers:{webRequest.headers?.JsonSerialize()} datas:{webRequest.datas?.JsonSerialize()}");
            
            var originSuccessCallback = webRequest.succeedCallback;
            var originFailCallback = webRequest.faillCallback;

            webRequest.succeedCallback = new Action<DownloadHandler>(handler =>
            {
                Log.Debug($"Response:{webRequest.method} {webRequest.url} body:{handler.text}");
                originSuccessCallback?.Invoke(handler);
            });
            webRequest.faillCallback = new Action<long, string>((code, error) =>
            {
                Log.Debug($"Failed:{webRequest.method} {webRequest.url} code:{code} error:{error}");
                originFailCallback?.Invoke(code, error);
            });
        }
    }
}
