using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomAlbums.Data
{
    public class CustomScore
    {
        [JsonProperty("Evaluate")]
        public int evaluate;
        [JsonProperty("Score")]
        public int score;
        [JsonProperty("Combo")]
        public int combo;
        [JsonProperty("Accuracy")]
        public float accuracy;
        [JsonProperty("AccuracyStr")]
        public string accuracyStr;
        [JsonProperty("Clear")]
        public float clear;
        [JsonProperty("FailCount")]
        public int fail_count;
        [JsonProperty("Passed")]
        public bool pass;
    }

    public class CustomData
    {
        [JsonProperty]
        public string SelectedAlbum;
        [JsonProperty]
        public int SelectedDifficulty;
        [JsonProperty]
        public List<string> Collections;
        [JsonProperty]
        public List<string> Hides;
        [JsonProperty]
        public List<string> History;
        [JsonProperty("Highest")]
        public Dictionary<string, Dictionary<int, CustomScore>> highest;
        [JsonProperty("FullCombo")]
        public Dictionary<string, List<int>> full_combo_music;
    }
}
