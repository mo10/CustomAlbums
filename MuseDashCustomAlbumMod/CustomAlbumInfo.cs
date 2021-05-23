﻿using System.IO;
using Assets.Scripts.GameCore;
using Assets.Scripts.PeroTools.Commons;
using GameLogic;
using Ionic.Zip;
using MelonLoader;
using Newtonsoft.Json;
using RuntimeAudioClipLoader;
using UnityEngine;

namespace MuseDashCustomAlbumMod
{
    public class CustomAlbumInfo
    {
        [JsonIgnore] private static Object objectCache;

        [JsonProperty] public string author;

        [JsonProperty] public string author_en;

        [JsonProperty] public string author_ja;

        [JsonProperty] public string author_ko;

        [JsonProperty] public string author_zh_hans;

        [JsonProperty] public string author_zh_hant;

        [JsonProperty] public string bpm;

        [JsonIgnore] private Sprite coverSprite;

        [JsonProperty] public string difficulty1;

        [JsonProperty] public string difficulty2;

        [JsonProperty] public string difficulty3;

        [JsonProperty] public string difficulty4;

        [JsonProperty] public string levelDesigner;

        [JsonProperty] public string levelDesigner1;

        [JsonProperty] public string levelDesigner2;

        [JsonProperty] public string levelDesigner3;

        [JsonProperty] public string levelDesigner4;

        [JsonIgnore] private StageInfo[] maps = new StageInfo[4];

        [JsonProperty] public string name;

        [JsonProperty] public string name_en;

        [JsonProperty] public string name_ja;

        [JsonProperty] public string name_ko;

        [JsonProperty] public string name_zh_hans;

        [JsonProperty] public string name_zh_hant;

        [JsonProperty] public string scene;

        [JsonIgnore] public string uid;

        [JsonProperty] public string unlockLevel;

        [JsonIgnore] public string path { get; private set; }

        [JsonIgnore] public bool loadFromFolder { get; private set; }

        /// <summary>
        ///     Load from zip file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CustomAlbumInfo LoadFromFile(string filePath)
        {
            using (var zip = ZipFile.Read(filePath))
            {
                if (zip["info.json"] == null) return null;
                var albumInfo = Utils.StreamToJson<CustomAlbumInfo>(zip["info.json"].OpenReader());
                albumInfo.path = filePath;
                albumInfo.loadFromFolder = false;
                return albumInfo;
            }
        }

        /// <summary>
        ///     Load from folder
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static CustomAlbumInfo LoadFromFolder(string folderPath)
        {
            if (!File.Exists($"{folderPath}/info.json")) return null;
            var albumInfo = Utils.StreamToJson<CustomAlbumInfo>(File.OpenRead($"{folderPath}/info.json"));
            albumInfo.path = folderPath;
            albumInfo.loadFromFolder = true;
            return albumInfo;
        }

        public AudioClip GetAudioClip(string name)
        {
            string[] targetFiles = {$"{name}.aiff", $"{name}.mp3", $"{name}.ogg", $"{name}.wav"};

            Stream stream = null;
            var format = AudioFormat.unknown;
            string fileExtension = null;

            AudioClip audio = null;
            //if (demoAudio != null)
            //{
            //    return demoAudio;
            //}

            if (loadFromFolder)
            {
                // Load from folder
                if (TryGetContainFile(path, targetFiles, out var filePath))
                {
                    fileExtension = Path.GetExtension(filePath);
                    stream = File.OpenRead(filePath);
                }
            }
            else
            {
                // load from .mdm
                using (var zip = ZipFile.Read(path))
                {
                    if (TryGetContainFile(zip, targetFiles, out var fileName))
                    {
                        fileExtension = Path.GetExtension(fileName);
                        // CrcCalculatorStream not support set_position, Read all bytes then convert to MemoryStream
                        var data = Utils.StreamToBytes(zip[fileName].OpenReader());
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
                audio = Manager.Load(stream, format, name);
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
            string[] targetFiles = {"cover.png"};
            // Load only once
            if (coverSprite != null) return coverSprite;

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            if (loadFromFolder)
            {
                // Load from folder
                if (TryGetContainFile(path, targetFiles, out var filePath))
                    texture.LoadImage(File.ReadAllBytes(filePath));
            }
            else
            {
                // Load from zip
                using (var zip = ZipFile.Read(path))
                {
                    if (TryGetContainFile(zip, targetFiles, out var file))
                        texture.LoadImage(Utils.StreamToBytes(zip[file].OpenReader()));
                }
            }

            coverSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(texture.width / 2, texture.height / 2));
            return coverSprite;
        }

        public StageInfo GetMap(int index)
        {
            string[] targetFiles = {$"map{index}.bms"};

            if (loadFromFolder)
            {
                // Load from folder
                if (TryGetContainFile(path, targetFiles, out var filePath))
                    return GetStageInfo(File.ReadAllBytes(filePath), index);
            }
            else
            {
                // Load from zip
                using (var zip = ZipFile.Read(path))
                {
                    if (TryGetContainFile(zip, targetFiles, out var file))
                        return GetStageInfo(Utils.StreamToBytes(zip[file].OpenReader()), index);
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

            if (bms == null) return null;

            var musicConfigReader = MusicConfigReader.Instance;
            musicConfigReader.ClearData();
            musicConfigReader.bms = bms;
            musicConfigReader.Init("");


            var info = musicConfigReader.GetData().Cast<MusicData>();
            //var info = (from m in musicConfigReader.GetData().ToArray() select (MusicData)m).ToList();

            var stageInfo = new StageInfo
            {
                musicDatas = info,
                delay = musicConfigReader.delay,
                mapName = (string) musicConfigReader.bms.info["TITLE"],
                music = ((string) musicConfigReader.bms.info["WAV10"]).BeginBefore('.'),
                scene = (string) musicConfigReader.bms.info["GENRE"],
                difficulty = map_index,
                bpm = musicConfigReader.bms.GetBpm(),
                md5 = musicConfigReader.bms.md5,
                sceneEvents = musicConfigReader.sceneEvents
            };
            MelonLogger.Msg($"Delay: {musicConfigReader.delay}");
            return stageInfo;
        }

        public static bool TryGetContainFile(string path, string[] fileNames, out string filePath)
        {
            foreach (var fileName in fileNames)
                if (File.Exists($"{path}/{fileName}"))
                {
                    filePath = $"{path}/{fileName}";
                    return true;
                }

            filePath = null;
            return false;
        }

        public static bool TryGetContainFile(ZipFile zipEntries, string[] fileNames, out string file)
        {
            foreach (var fileName in fileNames)
                if (zipEntries[fileName] != null)
                {
                    file = fileName;
                    return true;
                }

            file = null;
            return false;
        }

        public static void Cache(Object obj)
        {
            if (objectCache != null) Object.DestroyImmediate(objectCache, true);
            objectCache = obj;
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