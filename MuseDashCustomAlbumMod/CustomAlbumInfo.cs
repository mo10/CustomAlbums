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

        [JsonIgnore]
        public string filePath;
        public static CustomAlbumInfo LoadFromFile(string filePath)
        {
            using (ZipFile zip = ZipFile.Read(filePath))
            {
                if (zip["info.json"] == null)
                {
                    return null;
                }
                var albumInfo = Utils.StreamToJson<CustomAlbumInfo>(zip["info.json"].OpenReader());
                albumInfo.filePath = filePath;
                return albumInfo;
            }
        }
        public byte[] GetDemo()
        {
            using (ZipFile zip = ZipFile.Read(filePath))
            {
                if (zip["demo.wav"] == null)
                {
                    return null;
                }
                return Utils.StreamToBytes(zip["demo.wav"].OpenReader());
            }
        }
        public byte[] GetMusic()
        {
            using (ZipFile zip = ZipFile.Read(filePath))
            {
                if (zip["music.wav"] == null)
                {
                    return null;
                }
                return Utils.StreamToBytes(zip["music.wav"].OpenReader());
            }
        }
        public byte[] GetCover()
        {
            using (ZipFile zip = ZipFile.Read(filePath))
            {
                if (zip["cover.png"] == null)
                {
                    return null;
                }
                return Utils.StreamToBytes(zip["cover.png"].OpenReader());
            }
        }
        public byte[] GetMap1()
        {
            return null;
        }
        public byte[] GetMap2()
        {
            return null;
        }
        public byte[] GetMap3()
        {
            return null;
        }
        public byte[] GetMap4()
        {
            return null;
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
