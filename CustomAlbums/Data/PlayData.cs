using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace CustomAlbums.Data
{
    [ProtoContract]
    public enum Side
    {
        Bottom = 1,
        Top
    }
    [ProtoContract]
    public class BeatRecord
    {
        [ProtoMember(1)]
        public string NoteUid { get; set; }
        [ProtoMember(2)]
        public int Offset { get; set; }
        [ProtoMember(3)]
        public int Score { get; set; }
        [ProtoMember(4)]
        public Side Side { get; set; }
    }

    [ProtoContract]
    public class PlayData
    {
        [ProtoMember(1)]
        public string BMSHash { get; set; }
        [ProtoMember(2)]
        public string SelectedMusicUid { get; set; }
        [ProtoMember(3)]
        public int SelectedDifficulty { get; set; }
        [ProtoMember(4)]
        public string SelectedCharacterUid { get; set; }
        [ProtoMember(5)]
        public string SelectedElfinUid { get; set; }
        [ProtoMember(6)]
        public int ComboCount { get; set; }
        [ProtoMember(7)]
        public int Hp { get; set; }
        [ProtoMember(8)]
        public int Score { get; set; }
        [ProtoMember(9)]
        public float Accuracy { get; set; }
        [ProtoMember(10)]
        public int MissCount { get; set; }
        [ProtoMember(11)]
        public string Judge { get; set; }
        [ProtoMember(12)]
        public List<BeatRecord> BeatRecords { get; set; }
    }
}
