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
        /// Read embedded file from this assembly.
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
        /// Load json from stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="steamReader"></param>
        /// <returns></returns>
        public static T JsonDeserialize<T>(this Stream steamReader)
        {
            var buffer = new byte[steamReader.Length];
            steamReader.Read(buffer, 0, buffer.Length);
            return JsonConvert.DeserializeObject<T>(Encoding.Default.GetString(buffer));
        }
        /// <summary>
        /// Load json from string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <returns></returns>
        public static T JsonDeserialize<T>(this string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
        /// <summary>
        /// Convert a object to json string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string JsonSerialize(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
        /// <summary>
        /// Get the specified non-public type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Type GetNestedNonPublicType(this Type type, string name)
        {
            return type.GetNestedType(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }
        public static string RemoveFromEnd(this string str, IEnumerable<string> suffixes)
        {
            foreach(var suffix in suffixes)
            {
                if (str.EndsWith(suffix))
                {
                    return str.Substring(0, str.Length - suffix.Length);
                }
            }
            return str;
        }
        public static string RemoveFromEnd(this string str, string suffix)
        {
            if (str.EndsWith(suffix))
            {
                return str.Substring(0, str.Length - suffix.Length);
            }
            return str;
        }
        public static string RemoveFromStart(this string str, IEnumerable<string> suffixes)
        {
            foreach (var suffix in suffixes)
            {
                if (str.StartsWith(suffix))
                {
                    return str.Substring(suffix.Length);
                }
            }
            return str;
        }
        public static string RemoveFromStart(this string str, string suffix)
        {
            if (str.StartsWith(suffix))
            {
                return str.Substring(suffix.Length);
            }
            return str;
        }
        /// <summary>
        /// Read all bytes from a Stream.
        /// </summary>
        /// <param name="steamReader"></param>
        /// <returns></returns>
        public static byte[] ToArray(this Stream steamReader)
        {
            var buffer = new byte[steamReader.Length];
            steamReader.Read(buffer, 0, buffer.Length);
            return buffer;
        }
        /// <summary>
        /// Put all bytes into Stream.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static MemoryStream ToStream(this byte[] bytes)
        {
            return new MemoryStream(bytes);
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
        public static string ToString(this IEnumerable<byte> bytes, string format)
        {
            string result = string.Empty;
            foreach(var _byte in bytes)
            {
                result += _byte.ToString(format);
            }
            return result;
        }
        public static byte[] GetMD5(this IEnumerable<byte> bytes)
        {
            return MD5.Create().ComputeHash(bytes.ToArray());
        }
        /// <summary>
        /// Search all implementation classes of interfce
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllImplement(this Type type)    
        {
            var classes = Assembly.GetAssembly(type).GetTypes().Where(t =>
                t.GetInterfaces().Contains(type) && t.GetConstructor(Type.EmptyTypes) != null);

            return classes;
        }
        public static IEnumerable<Type> GetAllSublass(this Type type)
        {
            var classes = Assembly.GetAssembly(type).GetTypes().Where(
                t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type));

            return classes;
        }
    }
}
