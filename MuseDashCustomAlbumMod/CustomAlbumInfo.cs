using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.Compression;
using Ionic.Zip;

namespace MuseDashCustomAlbumMod
{
    public class CustomAlbumInfo
    {
        [JsonProperty]
        public string name;
        [JsonProperty]
        public string name_en;
        [JsonProperty]
        public string name_ko;
        [JsonProperty]
        public string name_ja;
        [JsonProperty]
        public string name_zh_hans;
        [JsonProperty]
        public string name_zh_hant;

        [JsonProperty]
        public string author;
        [JsonProperty]
        public string author_en;
        [JsonProperty]
        public string author_ko;
        [JsonProperty]
        public string author_ja;
        [JsonProperty]
        public string author_zh_hans;
        [JsonProperty]
        public string author_zh_hant;

        [JsonProperty]
        public string bpm;
        [JsonProperty]
        public string scene;

        [JsonProperty]
        public string levelDesigner;
        [JsonProperty]
        public string levelDesigner1;
        [JsonProperty]
        public string levelDesigner2;
        [JsonProperty]
        public string levelDesigner3;
        [JsonProperty]
        public string levelDesigner4;

        [JsonProperty]
        public string difficulty1;
        [JsonProperty]
        public string difficulty2;
        [JsonProperty]
        public string difficulty3;
        [JsonProperty]
        public string difficulty4;

        [JsonProperty]
        public string unlockLevel;
        [JsonIgnore]
        public string Uid;
        public static CustomAlbumInfo Load(ZipEntry zipEntry)
        {
            var stream = zipEntry.OpenReader();
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            var albumInfo = Load(Encoding.Default.GetString(buffer));

            return albumInfo;
        }
        public static CustomAlbumInfo Load(string rawJson)
        {
            var albumInfo = JsonConvert.DeserializeObject<CustomAlbumInfo>(rawJson);
            return albumInfo;
        }
        public override string ToString()
        {
            return
                $"name:{name} " +
                $"name_en:{name_en} " +
                $"name_ko:{name_ko} " +
                $"name_ja:{name_ja} " +
                $"name_zh_hans:{name_zh_hans} " +
                $"name_zh_hant:{name_zh_hant} " +
                $"author:{author} " +
                $"author_en:{author_en} " +
                $"author_ko:{author_ko} " +
                $"author_ja:{author_ja} " +
                $"author_zh_hans:{author_zh_hans} " +
                $"author_zh_hant:{author_zh_hant} " +
                $"bpm:{bpm} " +
                $"scene:{scene} " +
                $"levelDesigner:{levelDesigner} " +
                $"levelDesigner1:{levelDesigner1} " +
                $"levelDesigner2:{levelDesigner2} " +
                $"levelDesigner3:{levelDesigner3} " +
                $"levelDesigner4:{levelDesigner4} " +
                $"difficulty1:{difficulty1} " +
                $"difficulty2:{difficulty2} " +
                $"difficulty3:{difficulty3} " +
                $"difficulty4:{difficulty4} " +
                $"unlockLevel:{unlockLevel}";
        }
    }
}
