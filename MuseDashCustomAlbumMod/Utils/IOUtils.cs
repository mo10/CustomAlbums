using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ModHelper;

namespace MuseDashCustomAlbumMod.Utils
{
    public static class IOUtils
    {
        public static string GetFileContent(string path)
        {
            if (path.IsNullOrEmpty() || !File.Exists(path))
            {
                ModLogger.Debug("File does not exist!");
                return String.Empty;
            }

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(fs,StringUtils.GetEncoding(bytes));
                return sr.ReadToEnd();
            }
        }

        public static string GetFileContent(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                StreamReader sr = new StreamReader(ms, StringUtils.GetEncoding(bytes));
                return sr.ReadToEnd();
            }
        }

    }
}
