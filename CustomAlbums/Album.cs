using CustomAlbums.Data;
using Ionic.Zip;
using ModHelper;
using Newtonsoft.Json.Linq;
using RuntimeAudioClipLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomAlbums
{
    public class Album
    {
        public static readonly Dictionary<string, AudioFormat> AudioFormatMapping = new Dictionary<string, AudioFormat>()
            {
                {".aiff", AudioFormat.aiff},
                {".mp3", AudioFormat.mp3},
                {".ogg", AudioFormat.ogg},
                {".wav", AudioFormat.wav},
            };

        public AlbumInfo Info { get; private set; }
        public string BasePath { get; private set; }
        public bool IsPackaged { get; private set; }
        public int Index;

        public Texture2D CoverTex { get; private set; }
        public Sprite CoverSprite { get; private set; }
        public static AudioClip MusicAudio { get; private set; }
        public static Stream MusicStream { get; private set; }

        public Album(string path)
        {
            if (File.Exists($"{path}/info.json"))
            {
                // Load from folder
                this.Info = File.OpenRead($"{path}/info.json").JsonDeserialize<AlbumInfo>();
                this.BasePath = path;
                this.IsPackaged = false;
                return;
            }
            else
            {
                // Load from package
                using (ZipFile zip = ZipFile.Read(path))
                {
                    if (zip["info.json"] != null)
                    {
                        this.Info = zip["info.json"].OpenReader().JsonDeserialize<AlbumInfo>();
                        this.BasePath = path;
                        this.IsPackaged = true;
                        return;
                    }
                }
            }
            throw new FileNotFoundException($"info.json not found");
        }

        public bool IsPlayable()
        {
            return true;
        }
        public void GetRecord(int map)
        {

        }
        public void SetRecord(int map)
        {

        }

        public Sprite GetCover()
        {
            if (CoverSprite != null)
                return CoverSprite;
            try
            {
                using (Stream stream = Open("cover.png"))
                {
                    CoverTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    CoverTex.hideFlags = HideFlags.HideAndDontSave;
                    CoverTex.LoadImage(stream.ToArray());
                }
                CoverSprite = Sprite.Create(CoverTex,
                    new Rect(0, 0, CoverTex.width, CoverTex.height),
                    new Vector2(CoverTex.width / 2, CoverTex.height / 2));
            }
            catch (Exception ex)
            {
                ModLogger.Debug($"Error:{ex}");
                DestoryCover();
            }
            return CoverSprite;
        }

        public AudioClip GetMusic(string name = "music")
        {
            DestoryAudio(); // Destory old audio

            List<string> fileNames = new List<string>();
            foreach(var ext in AudioFormatMapping.Keys)
            {
                fileNames.Add(name + ext);
            }

            AudioFormat format = AudioFormat.unknown;
            MusicStream = OpenOneOf(fileNames, out string fileName);
            AudioFormatMapping.TryGetValue(Path.GetExtension(fileName), out format);
            MusicAudio = RuntimeAudioClipLoader.Manager.Load(MusicStream, format, name, false, true);
            return MusicAudio;
        }
        public AudioClip GetDemo()
        {
            return null;
        }

        public void DestoryAudio()
        {
            if(MusicAudio != null)
            {
                UnityEngine.Object.DestroyImmediate(MusicAudio);
                MusicAudio = null;
            }
            if(MusicStream != null)
            {
                MusicStream.Dispose();
                MusicStream = null;
            }
        }
        public void DestoryCover()
        {
            if (CoverSprite != null)
            {
                UnityEngine.Object.Destroy(CoverSprite);
                CoverSprite = null;
            }
            if (CoverTex != null)
            {
                UnityEngine.Object.Destroy(CoverTex);
                CoverTex = null;
            }
        }

        private Stream Open(string filePath)
        {
            if (IsPackaged)
            {
                // load from package
                using (ZipFile zip = ZipFile.Read(BasePath))
                {
                    if (!zip.ContainsEntry(filePath))
                        throw new FileNotFoundException($"No such as file:{filePath} in {BasePath}");
                    ModLogger.Debug($"Loaded:{BasePath}/{filePath}");
                    // CrcCalculatorStream not support set_position, Read all bytes then convert to MemoryStream
                    return zip[filePath].OpenReader().ToArray().ToStream();
                }
            }
            else
            {
                // Load from folder
                var fullPath = Path.Combine(BasePath, filePath);

                if (!File.Exists(fullPath))
                    throw new FileNotFoundException($"No such as file:{fullPath}");
                ModLogger.Debug($"Loaded:{BasePath}/{filePath}");
                return File.OpenRead(fullPath);
            }
        }
        private Stream OpenOneOf(IEnumerable<string> filePaths,out string openedFilePath)
        {
            if (IsPackaged)
            {
                // load from package
                using (ZipFile zip = ZipFile.Read(BasePath))
                {
                    foreach (var filePath in filePaths)
                    {
                        if (!zip.ContainsEntry(filePath))
                            continue;
                        openedFilePath = filePath;
                        ModLogger.Debug($"Loaded:{BasePath}/{filePath}");
                        // CrcCalculatorStream doesn't support set_position. We read all bytes then convert to MemoryStream
                        return zip[filePath].OpenReader().ToArray().ToStream();
                    }
                }
                throw new FileNotFoundException($"No such as file(s):{filePaths} in {BasePath}");
            }
            // Load from folder
            foreach (var filePath in filePaths)
            {
                var fullPath = Path.Combine(BasePath, filePath);
                if (!File.Exists(fullPath))
                    continue;
                openedFilePath = filePath;
                ModLogger.Debug($"Loaded:{BasePath}/{filePath}");
                return File.OpenRead(fullPath);
            }
            throw new FileNotFoundException($"No such as file(s):{filePaths} in {BasePath}");
        }
    }
}
