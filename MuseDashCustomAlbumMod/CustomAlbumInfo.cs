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

        private static byte[] ReadBuffer(ZipEntry zipEntry)
        {
            var stream = zipEntry.OpenReader();
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public static CustomAlbumInfo Load(ZipEntry zipEntry)
        {
            var buffer = ReadBuffer(zipEntry);
            var albumInfo = Load(Encoding.Default.GetString(buffer));

            return albumInfo;
        }

        public static UnityEngine.AudioClip LoadAsAudioClip(ZipEntry zipEntry)
        {
            var buffer = ReadBuffer(zipEntry);
            // TODO: mp3/more format support
            return AudioUtility.WavUtility.ToAudioClip(buffer);
        }

        public static UnityEngine.Sprite LoadAsSprite(ZipEntry zipEntry)
        {
            // TODO:
            throw new NotImplementedException();
        }

        public static UnityEngine.Sprite LoadAsSprite(ZipEntry zipEntry, int width, int height)
        {
            var tex = new UnityEngine.Texture2D(width, height);

            byte[] binary = ReadBuffer(zipEntry);

            UnityEngine.ImageConversion.LoadImage(tex, binary);
            return UnityEngine.Sprite.Create(tex, new UnityEngine.Rect(0, 0, tex.width, tex.height), new UnityEngine.Vector2(0.0f, 0.0f));
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
