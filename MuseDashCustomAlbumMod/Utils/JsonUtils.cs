using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MuseDashCustomAlbumMod.Utils
{
    public static class JsonUtils
    {
        public static T ToObject<T>(string json)
        {
            if (json.IsNullOrEmpty())
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
