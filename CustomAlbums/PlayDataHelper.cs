using CustomAlbums.Data;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomAlbums
{
    public static class PlayDataHelper
    {
        private static readonly byte[] ORBytes = new byte[] { 0x23, 0x32, 0x23, 0x22, 0x33, 0x33, 0x23 };

        public static PlayData Load(Dictionary<string, object> datas)
        {
            var text = datas["payload"] as string;

            var buffer = LoadFromBytes(Convert.FromBase64String(text));

            PlayData playData;
            using (var stream = new MemoryStream(buffer))
            {
                playData = Serializer.Deserialize<PlayData>(stream);
            }

            return playData;
        }
        public static byte[] LoadFromBytes(byte[] array)
        {
            var buffer = array.ToArray(); // Clone a new array

            for (int i = 0; i < buffer.Length; i++)
            {
                var orIdx = i % ORBytes.Length;

                byte or = ORBytes[orIdx];
                byte value = buffer[i];

                buffer[i] = (byte)(value ^ or);

            }
            return buffer;
        }
    }
}
