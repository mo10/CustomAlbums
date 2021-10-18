using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CustomAlbums.Data
{
    public class AlbumInfo
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
        public string levelDesigner;
        [JsonProperty]
        public string levelDesigner1;
        [JsonProperty]
        public string levelDesigner2;
        [JsonProperty]
        public string levelDesigner3;
        [JsonProperty]
        public string levelDesigner4;

        [DefaultValue("0")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string bpm;
        [DefaultValue("scene_01")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string scene;

        [JsonProperty]
        public string difficulty1;
        [JsonProperty]
        public string difficulty2;
        [JsonProperty]
        public string difficulty3;
        [JsonProperty]
        public string difficulty4;

        [DefaultValue("0")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string unlockLevel;

        public string GetName(string lang = null)
        {
            // If "name_<lang>" not avaliable will return "name"
            // If "name" not avaliable will return "Unknown" 
            string result;
            switch (lang)
            {
                case "ChineseT":
                    result = name_zh_hant;
                    if (string.IsNullOrEmpty(result)) goto default;
                    break;
                case "ChineseS":
                    result = name_zh_hans;
                    if (string.IsNullOrEmpty(result)) goto default;
                    break;
                case "English":
                    result = name_en;
                    if (string.IsNullOrEmpty(result)) goto default;
                    break;
                case "Korean":
                    result = name_ko;
                    if (string.IsNullOrEmpty(result)) goto default;
                    break;
                case "Japanese":
                    result = name_ja;
                    if (string.IsNullOrEmpty(result)) goto default;
                    break;
                default:
                    result = name;
                    if (string.IsNullOrEmpty(result)) result = "Unknown";
                    break;
            }
            return result;
        }
        public string GetAuthor(string lang = null)
        {
            // If "author_<lang>" not avaliable will return "author"
            // If "author" not avaliable will return "Unknown" 
            string result;
            switch (lang)
            {
                case "ChineseT":
                    result = author_zh_hant;
                    if (string.IsNullOrEmpty(result)) goto default;
                    break;
                case "ChineseS":
                    result = author_zh_hans;
                    if (string.IsNullOrEmpty(result)) goto default;
                    break;
                case "English":
                    result = author_en;
                    if (string.IsNullOrEmpty(result)) goto default;
                    break;
                case "Korean":
                    result = author_ko;
                    if (string.IsNullOrEmpty(result)) goto default;
                    break;
                case "Japanese":
                    result = author_ja;
                    if (string.IsNullOrEmpty(result)) goto default;
                    break;
                default:
                    result = author;
                    if (string.IsNullOrEmpty(result)) result = "Unknown";
                    break;
            }
            return result;
        }
    }
}
