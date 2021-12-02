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
        public string accuracyString;
        [JsonProperty("Clear")]
        public float clear;
        [JsonProperty("FailCount")]
        public int failCount;
        [JsonProperty("Passed")]
        public bool isPassed;
    }

    public class CustomData
    {
        [JsonProperty]
        public string SelectedAlbum;
        [JsonProperty]
        public int SelectedDifficulty;
        [JsonProperty]
        public List<string> Collections = new List<string>();
        [JsonProperty]
        public List<string> Hides = new List<string>();
        [JsonProperty]
        public List<string> History = new List<string>();
        [JsonProperty]
        public Dictionary<string, Dictionary<int, CustomScore>> Highest = new Dictionary<string, Dictionary<int, CustomScore>>();
        [JsonProperty]
        public Dictionary<string, List<int>> FullCombo = new Dictionary<string, List<int>>();
    }
}
