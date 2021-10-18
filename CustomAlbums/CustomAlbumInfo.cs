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
using RuntimeAudioClipLoader;
using System.ComponentModel;

namespace CustomAlbums
{
    public class CustomAlbumInfo
    {
        #region info.json define
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

        [DefaultValue("0")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string bpm;
        [DefaultValue("scene_01")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
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

        [DefaultValue("0")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string unlockLevel;
        #endregion

        [JsonIgnore]
        public string uid;
        [JsonIgnore]
        public string path { get; private set; }
        [JsonIgnore]
        public bool loadFromFolder { get; private set; }
        [JsonIgnore]
        private Sprite coverSprite;
        [JsonIgnore]
        private static UnityEngine.Object objectCache;
        [JsonIgnore]
        private StageInfo[] maps = new StageInfo[4];
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

        /// <summary>
        /// Load from zip file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CustomAlbumInfo LoadFromFile(string filePath)
        {
            using (ZipFile zip = ZipFile.Read(filePath))
            {
                if (zip["info.json"] == null)
                {
                    return null;
                }
                var albumInfo = zip["info.json"].OpenReader().JsonDeserialize<CustomAlbumInfo>();
                albumInfo.path = filePath;
                albumInfo.loadFromFolder = false;
                return albumInfo;
            }
        }
        /// <summary>
        /// Load from folder
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static CustomAlbumInfo LoadFromFolder(string folderPath)
        {
            if (!File.Exists($"{folderPath}/info.json"))
            {
                return null;
            }
            var albumInfo = File.OpenRead($"{folderPath}/info.json").JsonDeserialize<CustomAlbumInfo>();
            albumInfo.path = folderPath;
            albumInfo.loadFromFolder = true;
            return albumInfo;
        }
        public AudioClip GetAudioClip(string name)
        {
            string[] targetFiles = { $"{name}.aiff", $"{name}.mp3", $"{name}.ogg", $"{name}.wav" };

            Stream stream = null;
            AudioFormat format = AudioFormat.unknown;
            string fileExtension = null;

            AudioClip audio = null;
            //if (demoAudio != null)
            //{
            //    return demoAudio;
            //}

            if (loadFromFolder)
            {
                // Load from folder
                if (TryGetContainFile(path, targetFiles, out string filePath))
                {
                    fileExtension = Path.GetExtension(filePath);
                    stream = File.OpenRead(filePath);
                }
            }
            else
            {
                // load from .mdm
                using (ZipFile zip = ZipFile.Read(path))
                {
                    if (TryGetContainFile(zip, targetFiles, out string fileName))
                    {
                        fileExtension = Path.GetExtension(fileName);
                        // CrcCalculatorStream not support set_position, Read all bytes then convert to MemoryStream
                        byte[] data = zip[fileName].OpenReader().ToBytes();
                        stream = new MemoryStream(data);
                    }
                }
            }
            // Check audio format 
            switch (fileExtension)
            {
                case ".aiff":
                    format = AudioFormat.aiff;
                    break;
                case ".mp3":
                    format = AudioFormat.mp3;
                    break;
                case ".ogg":
                    format = AudioFormat.ogg;
                    break;
                case ".wav":
                    format = AudioFormat.wav;
                    break;
                default:
                    format = AudioFormat.unknown;
                    break;
            }
            if (stream != null)
            {
                audio = RuntimeAudioClipLoader.Manager.Load(stream, format, name, false, true);
                Cache(audio);
            }
            return audio;
        }
        //public AudioClip GetMusicAudioClip()
        //{
        //    if (musicAudio != null)
        //    {
        //        return musicAudio;
        //    }
        //    using (ZipFile zip = ZipFile.Read(path))
        //    {
        //        if (zip["music.mp3"] == null)
        //        {
        //            return null;
        //        }
        //        byte[] data = Utils.StreamToBytes(zip["music.mp3"].OpenReader());
        //        Stream stream = new MemoryStream(data);
        //        musicAudio = RuntimeAudioClipLoader.Manager.Load(stream, RuntimeAudioClipLoader.AudioFormat.mp3, "demo", false, true);

        //        return musicAudio;
        //    }
        //}
        public Sprite GetCoverSprite()
        {
            string[] targetFiles = { "cover.png" };
            // Load only once
            if (coverSprite != null)
            {
                return coverSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            if (loadFromFolder)
            {
                // Load from folder
                if (TryGetContainFile(path, targetFiles, out string filePath))
                {
                    ImageConversion.LoadImage(texture, File.ReadAllBytes(filePath));
                }
            }
            else
            {
                // Load from zip
                using (ZipFile zip = ZipFile.Read(path))
                {
                    if (TryGetContainFile(zip, targetFiles, out string file))
                    {
                        ImageConversion.LoadImage(texture, zip[file].OpenReader().ToBytes());
                    }
                }
            }
            coverSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
            return coverSprite;
        }
        public StageInfo GetMap(int index)
        {
            string[] targetFiles = { $"map{index}.bms" };

            if (loadFromFolder)
            {
                // Load from folder
                if (TryGetContainFile(path, targetFiles, out string filePath))
                {
                    return GetStageInfo(File.ReadAllBytes(filePath), index);
                }
            }
            else
            {
                // Load from zip
                using (ZipFile zip = ZipFile.Read(path))
                {
                    if (TryGetContainFile(zip, targetFiles, out string file))
                    {
                        return GetStageInfo(zip[file].OpenReader().ToBytes(), index);
                    }
                }
            }
            return null;
        }
        public static StageInfo GetStageInfo(byte[] bytes, int map_index)
        {
            /* 1.加载bms
             * 2.转换为MusicData
             * 3.创建StageInfo
             * */

            var bms = MyBMSCManager.instance.Load(bytes, $"map_{map_index}");

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
                delay = musicConfigReader.delay,
                mapName = (string)musicConfigReader.bms.info["TITLE"],
                music = ((string)musicConfigReader.bms.info["WAV10"]).BeginBefore('.'),
                scene = (string)musicConfigReader.bms.info["GENRE"],
                difficulty = map_index,
                bpm = musicConfigReader.bms.GetBpm(),
                md5 = musicConfigReader.bms.md5,
                sceneEvents = musicConfigReader.sceneEvents
            };
            ModLogger.Debug($"Delay: {musicConfigReader.delay}");
            return stageInfo;
        }
        public static bool TryGetContainFile(string path, string[] fileNames, out string filePath)
        {
            foreach (var fileName in fileNames)
            {
                if (File.Exists($"{path}/{fileName}"))
                {
                    filePath = $"{path}/{fileName}";
                    return true;
                }
            }
            filePath = null;
            return false;
        }
        public static bool TryGetContainFile(ZipFile zipEntries, string[] fileNames, out string file)
        {
            foreach (var fileName in fileNames)
            {
                if (zipEntries[fileName] != null)
                {
                    file = fileName;
                    return true;
                }
            }
            file = null;
            return false;
        }
        public static void Cache(UnityEngine.Object obj)
        {
            if (objectCache != null)
            {
                UnityEngine.Object.DestroyImmediate(objectCache, true);
            }
            objectCache = obj;
        }
        public override string ToString()
        {
            return $"Name:{GetName()} Author:{GetAuthor()}";
        }
    }
}
