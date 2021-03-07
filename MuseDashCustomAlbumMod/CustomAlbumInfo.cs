using Assets.Scripts.GameCore;
using Assets.Scripts.PeroTools.Commons;
using GameLogic;
using Ionic.Zip;
using ModHelper;
using MuseDashCustomAlbumMod.Managers;
using RuntimeAudioClipLoader;
using System.IO;
using System.Text;
using UnityEngine;
using FileUtils = MuseDashCustomAlbumMod.Utils.FileUtils;

namespace MuseDashCustomAlbumMod
{
    public class CustomAlbumInfo
    {
        public string name;
        public string name_en;
        public string name_ko;
        public string name_ja;
        public string name_zh_hans;
        public string name_zh_hant;
        
        public string author;
        public string author_en;
        public string author_ko;
        public string author_ja;
        public string author_zh_hans;
        public string author_zh_hant;
        
        public string bpm;
        public string scene;
        
        public string levelDesigner;
        public string levelDesigner1;
        public string levelDesigner2;
        public string levelDesigner3;
        public string levelDesigner4;
        
        public string difficulty1;
        public string difficulty2;
        public string difficulty3;
        public string difficulty4;
        
        public string unlockLevel;
        
        public string uid;



        public string path { get; private set; }
        public bool loadFromFolder { get; private set; }

        private Sprite coverSprite;
        private static UnityEngine.Object objectCache;
        private StageInfo[] maps = new StageInfo[4];

        public void SetLoadFromFolder(bool isFolder)
        {
            loadFromFolder = isFolder;
        }

        public void SetPath(string filePath)
        {
            path = filePath;
        }


        public AudioClip GetAudioClip(string name)
        {
            //string[] targetFiles = { $"{name}.aiff", $"{name}.mp3", $"{name}.ogg", $"{name}.wav" };

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
                
                if (FileUtils.FileExists(path, name, out string filePath, CustomInfoManager.musicExt))
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
                    if (FileUtils.ZipFileExists(zip, name, out string fileName, CustomInfoManager.musicExt))
                    {
                        fileExtension = Path.GetExtension(fileName);
                        // CrcCalculatorStream not support set_position, Read all bytes then convert to MemoryStream
                        var tempStream = zip[fileName].OpenReader();
                        byte[] data = new byte[tempStream.Length];
                        tempStream.Read(data, 0, data.Length);

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
            if(stream != null)
            {
                audio = Manager.Load(stream, format, name);
                Cache(audio);
            }
            return audio;
        }

        public Sprite GetCoverSprite()
        {
            //string[] targetFiles = { "cover.png" };

            // Load only once
            if (coverSprite != null)
            {
                return coverSprite;
            }

            string name = "cover";

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            if (loadFromFolder)
            {
                // Load from folder
                if(FileUtils.FileExists(path, name , out string filePath, ".png"))
                {
                    ImageConversion.LoadImage(texture, File.ReadAllBytes(filePath));
                }
            }
            else
            {
                // Load from zip
                using (ZipFile zip = ZipFile.Read(path))
                {
                    if(FileUtils.ZipFileExists(zip, name, out string file, ".png"))
                    {
                        var tempStream = zip[file].OpenReader();
                        byte[] data = new byte[tempStream.Length];
                        tempStream.Read(data, 0, data.Length);
                        ImageConversion.LoadImage(texture, data);
                    }
                }
            }
            coverSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
            return coverSprite;
        }
        public StageInfo GetMap(int index)
        {
            string name = $"map{index}";
            if (loadFromFolder)
            {
                // Load from folder
                if (FileUtils.FileExists(path, name, out string filePath, ".bms"))
                {
                    return GetStageInfo(File.ReadAllBytes(filePath), index);
                }
            }
            else
            {
                // Load from zip
                using (ZipFile zip = ZipFile.Read(path))
                {
                    if (FileUtils.ZipFileExists(zip, name, out string file, ".bms"))
                    {
                        var tempStream = zip[file].OpenReader();
                        byte[] data = new byte[tempStream.Length];
                        tempStream.Read(data, 0, data.Length);
                        return GetStageInfo(data, index);
                    }
                }
            }
            return null;
        }
        private static StageInfo GetStageInfo(byte[] bytes, int map_index)
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
            StringBuilder sb = new StringBuilder();

            sb.Append($"name:{name} ");
            sb.Append($"name_en:{name_en} ");
            sb.Append($"name_ko:{name_ko} ");
            sb.Append($"name_ja:{name_ja} ");
            sb.Append($"name_zh_hans:{name_zh_hans} ");
            sb.Append($"name_zh_hant:{name_zh_hant} ");
            sb.Append($"author:{author} ");
            sb.Append($"author_en:{author_en} ");
            sb.Append($"author_ko:{author_ko} ");
            sb.Append($"author_ja:{author_ja} ");
            sb.Append($"author_zh_hans:{author_zh_hans} ");
            sb.Append($"author_zh_hant:{author_zh_hant} ");
            sb.Append($"bpm:{bpm} ");
            sb.Append($"scene:{scene} ");
            sb.Append($"levelDesigner:{levelDesigner} ");
            sb.Append($"levelDesigner1:{levelDesigner1} ");
            sb.Append($"levelDesigner2:{levelDesigner2} ");
            sb.Append($"levelDesigner3:{levelDesigner3} ");
            sb.Append($"levelDesigner4:{levelDesigner4} ");
            sb.Append($"difficulty1:{difficulty1} ");
            sb.Append($"difficulty2:{difficulty2} ");
            sb.Append($"difficulty3:{difficulty3} ");
            sb.Append($"difficulty4:{difficulty4} ");
            sb.Append($"unlockLevel:{unlockLevel}");
            return sb.ToString();
        }
    }
}
