using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuseDashCustomAlbumMod.Utils
{
    public static class StringUtils
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static Encoding GetEncoding(byte[] bytes)
        {
            Encoding reVal;

            if (bytes[0] >= 0xEF)
            {
                if (bytes[0] == 0xEF && bytes[1] == 0xBB)
                {
                    reVal = Encoding.UTF8;
                }
                else if (bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
                {
                    reVal = Encoding.UTF32;
                }
                else if (bytes[0] == 0xFE && bytes[1] == 0xFF && bytes[2] == 0x00)
                {
                    reVal = Encoding.BigEndianUnicode;
                }
                else if (bytes[0] == 0xFF && bytes[1] == 0xFE)
                {
                    reVal = Encoding.Unicode;
                }
                else
                {
                    reVal = Encoding.Default;
                }
            }
            else
            {
                reVal = Encoding.GetEncoding(936);//GBK2312
            }
            return reVal;
        }


    }
}
