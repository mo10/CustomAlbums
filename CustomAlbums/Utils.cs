using ModHelper;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace CustomAlbums
{
    public static class Utils
    {
        /// <summary>
        /// Read embedded file from this dll
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] ReadEmbeddedFile(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            byte[] buffer;
            using (var stream = assembly.GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name}.{file}"))
            {
                buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }
        /// <summary>
        /// Print all child GameObject and Component
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="layer"></param>
        public static void GameObjectTracker(GameObject gameObject, int layer = 0)
        {
            foreach (var component in gameObject.GetComponents(typeof(object)))
            {
                ModLogger.Debug($"Layer:{layer} Name:{gameObject.name} Component:{component.GetType()}");
            }
            if (gameObject.transform.childCount > 0)
            {
                ++layer;
                for (var i = 0; i < gameObject.transform.childCount; i++)
                {
                    GameObjectTracker(gameObject.transform.GetChild(i).gameObject, layer);
                }
            }
        }

        public static T StreamToJson<T>(Stream steamReader)
        {
            var buffer = new byte[steamReader.Length];
            steamReader.Read(buffer, 0, buffer.Length);
            return JsonConvert.DeserializeObject<T>(Encoding.Default.GetString(buffer));
        }
        public static byte[] StreamToBytes(Stream steamReader)
        {
            var buffer = new byte[steamReader.Length];
            steamReader.Read(buffer, 0, buffer.Length);
            return buffer;
        }
        public static MemoryStream AudioMemStream(WaveStream waveStream)
        {
            MemoryStream outputStream = new MemoryStream();
            using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
            {
                byte[] bytes = new byte[waveStream.Length];
                waveStream.Position = 0;
                waveStream.Read(bytes, 0, Convert.ToInt32(waveStream.Length));
                waveFileWriter.Write(bytes, 0, bytes.Length);
                waveFileWriter.Flush();
            }
            return outputStream;
        }

        public static string BytesToMD5(byte[] bytes)
        {
            byte[] hash = MD5.Create().ComputeHash(bytes);
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < hash.Length; ++index)
                stringBuilder.Append(hash[index].ToString("x2"));
            return stringBuilder.ToString();
        }
    }
}
