using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.Compression;
using Ionic.Zip;
using Assets.Scripts.GameCore;
using Assets.Scripts.GameCore.Managers;
using GameLogic;
using Assets.Scripts.PeroTools.Commons;
using UnityEngine;

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
        [JsonIgnore]
        private Sprite coverSprite;
      
        private static byte[] ReadBuffer(ZipEntry zipEntry)
        {
            var stream = zipEntry.OpenReader();
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

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
        public Sprite GetCover()
        {
            if (coverSprite != null)
            {
                return coverSprite;
            }
            // Load only once
            using (ZipFile zip = ZipFile.Read(filePath))
            {
                if (zip["cover.png"] == null)
                {
                    return null;
                }
                Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                ImageConversion.LoadImage(texture, Utils.StreamToBytes(zip["cover.png"].OpenReader()));
                coverSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
                
                return coverSprite;
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
        public static StageInfo LoadAsStageInfo(ZipEntry zipEntry, string name)
        {
            /* 1.加载bms
             * 2.转换为MusicData
             * 3.创建StageInfo
             * */

            var bms = MyBMSCManager.instance.Load(ReadBuffer(zipEntry), name);

            if (bms == null)
            {
                return null;
            }

            MusicConfigReader musicConfigReader = GameLogic.MusicConfigReader.Instance;
            musicConfigReader.ClearData();
            musicConfigReader.bms = bms;
            musicConfigReader.Init("");


            var info = musicConfigReader.GetData().Cast<MusicData>();
            //var info = (from m in musicConfigReader.GetData().ToArray() select (MusicData)m).ToList();

            StageInfo stgInfo = new StageInfo
            {
                musicDatas = info,
                delay = musicConfigReader.delay,
                mapName = (string)musicConfigReader.bms.info["TITLE"],
                music = ((string)musicConfigReader.bms.info["WAV10"]).BeginBefore('.'),
                scene = (string)musicConfigReader.bms.info["GENRE"],
                difficulty = int.Parse((string)musicConfigReader.bms.info["RANK"]),
                bpm = musicConfigReader.bms.GetBpm(),
                md5 = musicConfigReader.bms.md5,
                sceneEvents = musicConfigReader.sceneEvents
            };


            return stgInfo;
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
