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
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Linq;
using System.Text.Json.Nodes;
#if MELON
using MelonLoader;
#endif
namespace CustomAlbums.Patch
{
    class WebApiPatch
    {
        private static readonly Logger Log = new Logger("WebApiPatch");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr SendToUrlDelegate(
            IntPtr hiddenStructReturn,
            IntPtr thisPtr,
            IntPtr url,
            IntPtr method,
            IntPtr datas,
            IntPtr succeedCallback,
            IntPtr failCallback,
            IntPtr startCallback,
            IntPtr completeCallback,
            IntPtr headers,
            IntPtr nativeMethodInfo
            );
        private static SendToUrlDelegate OriginalSendToUrl;

        unsafe public static void DoPatching()
        {
            var methodPtr = Utils.NativeMethod(typeof(GameAccountSystem), nameof(GameAccountSystem.SendToUrl));
            var methodPatchPtr = AccessTools.Method(typeof(WebApiPatch), nameof(WebApiPatch.SendToUrlPatch)).MethodHandle.GetFunctionPointer();
#if BEPINEX
            Log.Debug("Not implemented yet");
#elif MELON
            MelonUtils.NativeHookAttach((IntPtr)(&methodPtr), methodPatchPtr);
            OriginalSendToUrl = Marshal.GetDelegateForFunctionPointer<SendToUrlDelegate>(methodPtr);
#endif
        }
        /// <summary>
        /// Hook GameAccountSystem request.
        /// </summary>
        public static IntPtr SendToUrlPatch(
            IntPtr hiddenStructReturn,
            IntPtr thisPtr,
            IntPtr url,
            IntPtr method,
            IntPtr datas,
            IntPtr succeedCallback,
            IntPtr failCallback,
            IntPtr startCallback,
            IntPtr completeCallback,
            IntPtr headers,
            IntPtr nativeMethodInfo
            )
        {
            bool flag = true;

            var _url = IL2CPP.Il2CppStringToManaged(url);
            var _method = IL2CPP.Il2CppStringToManaged(method);
            Dictionary<string, Il2CppSystem.Object> _datas = null;
            Il2CppSystem.Action<JObject> _succeedCallback = null;
            Il2CppSystem.Action<JObject> _failCallback = null;

            if (datas != IntPtr.Zero)
                _datas = new Dictionary<string, Il2CppSystem.Object>(datas);

            if (succeedCallback != IntPtr.Zero)
                _succeedCallback = new Il2CppSystem.Action<JObject>(succeedCallback);

            if (failCallback != IntPtr.Zero)
                _failCallback = new Il2CppSystem.Action<JObject>(failCallback);


            var originalSucceedCallback = _succeedCallback;
            var originalFailCallback = _failCallback;
            
            Log.Debug($"[SendToUrlPatch] url:{_url} method:{_method}");

            switch (_url)
            {
                // Add custom tag.
                case "musedash/v1/music_tag":
                    _succeedCallback = new Action<JObject>(jObject =>
                    {
                        var JObj = JsonSerializer.Deserialize<JsonObject>(jObject.JsonSerialize());
                        var jArray = (JsonArray)JObj["music_tag_list"];

                        var music_tag = jArray.First(o => o["sort_key"].GetValue<int>() == 8);
                        foreach(var kv in AlbumManager.Langs)
                            music_tag["tag_name"][kv.Key] = kv.Value;
                        music_tag["tag_picture"] = "https://mdmc.moe/cdn/melon.png";
                        music_tag["icon_name"] = "";
                        music_tag["music_list"] = AlbumManager.GetAllUid();


                        //jArray.RemoveAt
                        originalSucceedCallback?.Invoke(jObject);
                    });
                    break;
            }

            if (flag)
                return OriginalSendToUrl(hiddenStructReturn, thisPtr, url, method, datas, succeedCallback, failCallback, startCallback, completeCallback, headers, nativeMethodInfo);

            return IntPtr.Zero;
        }
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
            Log.Debug($"request: {url}");
            return true;
        }
        /// <summary>
        /// Hook all request.
        /// </summary>
        /// <returns></returns>
        //[HarmonyPatch(typeof(WebUtils), nameof(WebUtils.SendToUrl), new Type[] { typeof(PeroWebRequest) })]
        //[HarmonyPostfix]
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
