using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public static Stream ReadEmbeddedFile(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name}.{file}");
        }
        public static T JsonDeserialize<T>(this Stream steamReader)
        {
            var buffer = new byte[steamReader.Length];
            steamReader.Read(buffer, 0, buffer.Length);
            return JsonConvert.DeserializeObject<T>(Encoding.Default.GetString(buffer));
        }
        public static T JsonDeserialize<T>(this string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
        public static string JsonSerialize(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
        public static byte[] ToBytes(this Stream steamReader)
        {
            var buffer = new byte[steamReader.Length];
            steamReader.Read(buffer, 0, buffer.Length);
            steamReader.Close();
            return buffer;
        }
        public static MemoryStream ToStream(this WaveStream waveStream)
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
    }
}
