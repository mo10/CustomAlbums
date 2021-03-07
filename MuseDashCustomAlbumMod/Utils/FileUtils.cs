using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;

namespace MuseDashCustomAlbumMod.Utils
{
    public static class FileUtils
    {
        public static bool FileExists(string path, string name, out string filePath, params string[] ext)
        {
            filePath = String.Empty;
            foreach (var fileExt in ext)
            {
                string tempFilePath = $"{path}/{name}{fileExt}";
                if (File.Exists(tempFilePath))
                {
                    filePath = tempFilePath;
                    return true;
                }
            }
            return false;
        }
        public static bool ZipFileExists(ZipFile zipEntries, string name, out string filePath, params string[] ext)
        {
            filePath = String.Empty;
            foreach (var fileExt in ext)
            {
                string tempFileName = name + fileExt;
                if (zipEntries[tempFileName] != null)
                {
                    filePath = tempFileName;
                    return true;
                }
            }
            return false;
        }
    }
}
