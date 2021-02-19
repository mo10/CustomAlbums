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
using NAudio.Wave;
using ModHelper;

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
        public string filePath { get; private set; }
        [JsonIgnore]
        private Sprite coverSprite;
        [JsonIgnore]
        private AudioClip demoAudio;
        [JsonIgnore]
        private AudioClip musicAudio;
        [JsonIgnore]
        private StageInfo[] maps = new StageInfo[4];

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
        public AudioClip GetDemoAudioClip()
        {
            if (demoAudio != null)
            {
                return demoAudio;
            }
            using (ZipFile zip = ZipFile.Read(filePath))
            {
                if (zip["demo.mp3"] == null)
                {
                    return null;
                }
                byte[] data = Utils.StreamToBytes(zip["demo.mp3"].OpenReader());
                Stream stream = new MemoryStream(data);
                demoAudio = RuntimeAudioClipLoader.Manager.Load(stream, RuntimeAudioClipLoader.AudioFormat.mp3, "demo", true, false);

                return demoAudio;
            }
        }
        public AudioClip GetMusicAudioClip()
        {
            if (musicAudio != null)
            {
                return musicAudio;
            }
            using (ZipFile zip = ZipFile.Read(filePath))
            {
                if (zip["music.mp3"] == null)
                {
                    return null;
                }
                byte[] data = Utils.StreamToBytes(zip["music.mp3"].OpenReader());
                Stream stream = new MemoryStream(data);
                musicAudio = RuntimeAudioClipLoader.Manager.Load(stream, RuntimeAudioClipLoader.AudioFormat.mp3, "demo", true, false);

                return musicAudio;
            }
        }
        public Sprite GetCoverSprite()
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
        public StageInfo GetMap(int index)
        {
            string target = $"map{index}.bms";
            if (maps[index] != null)
            {
                return maps[index];
            }
            using (ZipFile zip = ZipFile.Read(filePath))
            {
                if (zip[target] == null)
                {
                    return null;
                }
                maps[index] = GetStageInfo(Utils.StreamToBytes(zip[target].OpenReader()), target);
                return maps[index];
            }
        }
        public static StageInfo GetStageInfo(byte[] bytes, string name)
        {
            /* 1.加载bms
             * 2.转换为MusicData
             * 3.创建StageInfo
             * */

            var bms = MyBMSCManager.instance.Load(bytes, name);

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
            
            StageInfo stageInfo = new StageInfo
            {
                musicDatas = info,
                delay = 0.45M,
                mapName = (string)musicConfigReader.bms.info["TITLE"],
                music = ((string)musicConfigReader.bms.info["WAV10"]).BeginBefore('.'),
                scene = (string)musicConfigReader.bms.info["GENRE"],
                difficulty = int.Parse((string)musicConfigReader.bms.info["RANK"]),
                bpm = musicConfigReader.bms.GetBpm(),
                md5 = musicConfigReader.bms.md5,
                sceneEvents = musicConfigReader.sceneEvents
            };
            ModLogger.Debug($"Delay: {musicConfigReader.delay}");
            return stageInfo;
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
