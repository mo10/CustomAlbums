using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Nice.Datas;
using NiceData = Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Interface;
using CustomAlbums.Data;
using ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CustomAlbums
{
    public static class SaveManager
    {
        public static readonly string FileName = "CustomAlbums.json";

        public static CustomData CustomData = new CustomData();

        public static void Save()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FileName);
            File.WriteAllText(path, CustomData.JsonSerialize());
        }

        public static void Load()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FileName);
            if (File.Exists(path))
            {
                CustomData = File.ReadAllText(path).JsonDeserialize<CustomData>();
            }
        }

        public static void SplitCustomData()
        {
            var GameAccount = Singleton<DataManager>.instance["Account"];
            var GameAchievement = Singleton<DataManager>.instance["Achievement"];
            var GameSelectedIndex = GameAccount["SelectedMusicIndex"].GetResult<int>();
            var GameSelectedDifficulty = GameAccount["SelectedDifficulty"].GetResult<int>();
            var GameSelectedMusicPack = GameAccount["SelectedAlbumUid"].GetResult<string>();
            var GameCollections = GameAccount["Collections"].GetResult<List<string>>();
            var GameHides = GameAccount["Hides"].GetResult<List<string>>();
            var GameHistory = GameAccount["History"].GetResult<List<string>>();
            var GameHighest = GameAchievement["highest"].GetResult<List<IData>>();
            var GameFailCount = GameAchievement["fail_count"].GetResult<List<IData>>();
            var GameEasyPass = GameAchievement["easy_pass"].GetResult<List<string>>();
            var GameHardPass = GameAchievement["hard_pass"].GetResult<List<string>>();
            var GameMasterPass = GameAchievement["master_pass"].GetResult<List<string>>();
            var GameFullComboMusic = GameAchievement["full_combo_music"].GetResult<List<string>>();

            // Account.SelectedAlbumUid, Account.SelectedMusicIndex, Account.SelectedDifficulty
            CustomData.SelectedAlbum = null;
            CustomData.SelectedDifficulty = 2;
            if (AlbumManager.MusicPackge == GameSelectedMusicPack)
            {
                CustomData.SelectedAlbum = AlbumManager.GetAlbumKeyByIndex(GameSelectedIndex);
                CustomData.SelectedDifficulty = GameSelectedDifficulty;
            }
            // Account.Collections
            CustomData.Collections.Clear();
            foreach (var uid in GameCollections.Where(s => s.StartsWith($"{AlbumManager.Uid}-")))
            {
                var albumIndex = int.Parse(uid.RemoveFromStart($"{AlbumManager.Uid}-"));
                var albumKey = AlbumManager.GetAlbumKeyByIndex(albumIndex);

                if (AssertAlbumKeyIndex(albumKey, albumIndex))
                    CustomData.Collections.Add(albumKey);
            }
            // Account.Hide
            CustomData.Hides.Clear();
            foreach (var uid in GameHides.Where(s => s.StartsWith($"{AlbumManager.Uid}-")))
            {
                var albumIndex = int.Parse(uid.RemoveFromStart($"{AlbumManager.Uid}-"));
                var albumKey = AlbumManager.GetAlbumKeyByIndex(albumIndex);

                if (AssertAlbumKeyIndex(albumKey, albumIndex))
                    CustomData.Hides.Add(albumKey);
            }
            // Account.History
            CustomData.History.Clear();
            foreach (var uid in GameHistory.Where(s => s.StartsWith($"{AlbumManager.Uid}-")))
            {
                var albumIndex = int.Parse(uid.RemoveFromStart($"{AlbumManager.Uid}-"));
                var albumKey = AlbumManager.GetAlbumKeyByIndex(albumIndex);

                if (AssertAlbumKeyIndex(albumKey, albumIndex))
                    CustomData.History.Add(albumKey);
            }
            // Achievement.highest
            foreach (var highest in GameHighest.Where(d => d["uid"].GetResult<string>().StartsWith($"{AlbumManager.Uid}-")))
            {
                var dUid = highest["uid"].GetResult<string>();
                var albumIndex = int.Parse(dUid.RemoveFromStart($"{AlbumManager.Uid}-").Split('_')[0]);
                var difficulty = int.Parse(dUid.RemoveFromStart($"{AlbumManager.Uid}-").Split('_')[1]);
                var albumKey = AlbumManager.GetAlbumKeyByIndex(albumIndex);

                if (!AssertAlbumKeyIndex(albumKey, albumIndex))
                    continue;

                var failData = GameFailCount.FirstOrDefault(d => d["uid"].GetResult<string>() == dUid);
                var failCount = failData?["count"].GetResult<int>() ?? 0;
                var passed = false;
                switch (difficulty)
                {
                    case 1:
                        passed = GameEasyPass.Contains(dUid);
                        break;
                    case 2:
                        passed = GameHardPass.Contains(dUid);
                        break;
                    case 3:
                    case 4:
                        passed = GameMasterPass.Contains(dUid);
                        break;
                }
                var score = new CustomScore()
                {
                    evaluate = highest["evaluate"].GetResult<int>(),
                    score = highest["score"].GetResult<int>(),
                    combo = highest["combo"].GetResult<int>(),
                    accuracy = highest["accuracy"].GetResult<float>(),
                    accuracyString = highest["accuracyStr"].GetResult<string>(),
                    clear = highest["clear"].GetResult<float>(),
                    failCount = failCount,
                    isPassed = passed
                };

                // New
                if (!CustomData.Highest.ContainsKey(albumKey))
                    CustomData.Highest.Add(albumKey, new Dictionary<int, CustomScore>());
                // Overwrite
                if (CustomData.Highest[albumKey].ContainsKey(difficulty))
                    CustomData.Highest[albumKey][difficulty] = score;
                // Append
                else
                    CustomData.Highest[albumKey].Add(difficulty, score);
            }
            // Achievement.full_combo_music
            foreach (var dUid in GameFullComboMusic.Where(s => s.StartsWith($"{AlbumManager.Uid}-")))
            {
                var albumIndex = int.Parse(dUid.RemoveFromStart($"{AlbumManager.Uid}-").Split('_')[0]);
                var difficulty = int.Parse(dUid.RemoveFromStart($"{AlbumManager.Uid}-").Split('_')[1]);
                var albumKey = AlbumManager.GetAlbumKeyByIndex(albumIndex);

                if (!AssertAlbumKeyIndex(albumKey, albumIndex))
                    continue;

                // New
                if (!CustomData.FullCombo.ContainsKey(albumKey))
                    CustomData.FullCombo.Add(albumKey, new List<int>());
                // Append
                if (!CustomData.FullCombo[albumKey].Contains(difficulty))
                    CustomData.FullCombo[albumKey].Add(difficulty);
            }
        }

        public static void CleanCustomData()
        {
            var GameAccount = Singleton<DataManager>.instance["Account"];
            var GameAchievement = Singleton<DataManager>.instance["Achievement"];
            var GameSelectedIndex = GameAccount["SelectedMusicIndex"];
            var GameSelectedDifficulty = GameAccount["SelectedDifficulty"];
            var GameSelectedMusicPack = GameAccount["SelectedAlbumUid"];
            var GameSelectedMusicUidFromInfoList = GameAccount["SelectedMusicUidFromInfoList"];

            var GameCollections = GameAccount["Collections"].GetResult<List<string>>();
            var GameHides = GameAccount["Hides"].GetResult<List<string>>();
            var GameHistory = GameAccount["History"].GetResult<List<string>>();
            var GameHighest = GameAchievement["highest"].GetResult<List<IData>>();
            var GameFailCount = GameAchievement["fail_count"].GetResult<List<IData>>();
            var GameEasyPass = GameAchievement["easy_pass"].GetResult<List<string>>();
            var GameHardPass = GameAchievement["hard_pass"].GetResult<List<string>>();
            var GameMasterPass = GameAchievement["master_pass"].GetResult<List<string>>();
            var GameFullComboMusic = GameAchievement["full_combo_music"].GetResult<List<string>>();

            if (AlbumManager.MusicPackge == GameSelectedMusicPack.GetResult<string>())
            {
                GameSelectedMusicPack.SetResult("music_package_0");
                GameSelectedIndex.SetResult(0);
                GameSelectedDifficulty.SetResult(2);
            }
            if (GameSelectedMusicUidFromInfoList.GetResult<string>().StartsWith($"{AlbumManager.Uid}-"))
            {
                GameSelectedMusicUidFromInfoList.SetResult("0-0");
            }
            GameCollections.RemoveAll(s => s.StartsWith($"{AlbumManager.Uid}-"));
            GameHides.RemoveAll(s => s.StartsWith($"{AlbumManager.Uid}-"));
            GameHistory.RemoveAll(s => s.StartsWith($"{AlbumManager.Uid}-"));
            GameHighest.RemoveAll(d => d["uid"].GetResult<string>().StartsWith($"{AlbumManager.Uid}-"));
            GameFailCount.RemoveAll(d => d["uid"].GetResult<string>().StartsWith($"{AlbumManager.Uid}-"));
            GameEasyPass.RemoveAll(s => s.StartsWith($"{AlbumManager.Uid}-"));
            GameHardPass.RemoveAll(s => s.StartsWith($"{AlbumManager.Uid}-"));
            GameMasterPass.RemoveAll(s => s.StartsWith($"{AlbumManager.Uid}-"));
            GameFullComboMusic.RemoveAll(s => s.StartsWith($"{AlbumManager.Uid}-"));
        }

        public static void RestoreCustomData()
        {
            var GameAccount = Singleton<DataManager>.instance["Account"];
            var GameAchievement = Singleton<DataManager>.instance["Achievement"];
            var GameSelectedIndex = GameAccount["SelectedMusicIndex"];
            var GameSelectedDifficulty = GameAccount["SelectedDifficulty"];
            var GameSelectedMusicPack = GameAccount["SelectedAlbumUid"];
            var GameSelectedMusicUidFromInfoList = GameAccount["SelectedMusicUidFromInfoList"];

            var GameCollections = GameAccount["Collections"].GetResult<List<string>>();
            var GameHides = GameAccount["Hides"].GetResult<List<string>>();
            var GameHistory = GameAccount["History"].GetResult<List<string>>();
            var GameHighest = GameAchievement["highest"].GetResult<List<IData>>();
            var GameFailCount = GameAchievement["fail_count"].GetResult<List<IData>>();
            var GameEasyPass = GameAchievement["easy_pass"].GetResult<List<string>>();
            var GameHardPass = GameAchievement["hard_pass"].GetResult<List<string>>();
            var GameMasterPass = GameAchievement["master_pass"].GetResult<List<string>>();
            var GameFullComboMusic = GameAchievement["full_combo_music"].GetResult<List<string>>();

            // Account.SelectedAlbumUid, Account.SelectedMusicIndex, Account.SelectedDifficulty
            if (!string.IsNullOrEmpty(CustomData.SelectedAlbum) && AlbumManager.LoadedAlbums.ContainsKey(CustomData.SelectedAlbum))
            {
                var albumIndex = AlbumManager.LoadedAlbums[CustomData.SelectedAlbum].Index;

                GameSelectedMusicPack.SetResult(AlbumManager.MusicPackge);
                GameSelectedIndex.SetResult(albumIndex);
                GameSelectedDifficulty.SetResult(CustomData.SelectedDifficulty);
                GameSelectedMusicUidFromInfoList.SetResult($"{AlbumManager.Uid}-{albumIndex}");
            }
            // Account.Collections
            foreach (var albumKey in CustomData.Collections)
            {
                if (!AlbumManager.LoadedAlbums.ContainsKey(albumKey))
                {
                    ModLogger.Debug($"[Collections] Album \"{albumKey}\" not found, cannot restore.");
                    continue;
                }
                var albumIndex = AlbumManager.LoadedAlbums[albumKey].Index;

                var value = $"{AlbumManager.Uid}-{albumIndex}";
                if (!GameCollections.Contains(value))
                    GameCollections.Add(value);
            }
            // Account.Hide
            foreach (var albumKey in CustomData.Hides)
            {
                if (!AlbumManager.LoadedAlbums.ContainsKey(albumKey))
                {
                    ModLogger.Debug($"[Hides] Album \"{albumKey}\" not found, cannot restore.");
                    continue;
                }
                var albumIndex = AlbumManager.LoadedAlbums[albumKey].Index;

                var value = $"{AlbumManager.Uid}-{albumIndex}";
                if (!GameHides.Contains(value))
                    GameHides.Add(value);
            }
            // Account.History
            foreach (var albumKey in CustomData.History)
            {
                if (!AlbumManager.LoadedAlbums.ContainsKey(albumKey))
                {
                    ModLogger.Debug($"[History] Album \"{albumKey}\" not found, cannot restore.");
                    continue;
                }
                var albumIndex = AlbumManager.LoadedAlbums[albumKey].Index;

                var value = $"{AlbumManager.Uid}-{albumIndex}";
                if (!GameHistory.Contains(value))
                    GameHistory.Add(value);
            }
            // Achievement.highest, Achievement.fail_count, Achievement.easy_pass, Achievement.hard_pass, Achievement.master_pass
            foreach (var highest in CustomData.Highest)
            {
                var albumKey = highest.Key;
                if (!AlbumManager.LoadedAlbums.ContainsKey(albumKey))
                {
                    ModLogger.Debug($"[Highest] Album \"{albumKey}\" not found, cannot restore.");
                    continue;
                }
                var albumIndex = AlbumManager.LoadedAlbums[albumKey].Index;

                foreach (var record in highest.Value)
                {
                    var difficulty = record.Key;
                    var score = record.Value;
                    var dUid = $"{AlbumManager.Uid}-{albumIndex}_{difficulty}";
                    // Achievement.highest
                    var data = new NiceData.Data();
                    data["uid"].SetResult(dUid);
                    data["evaluate"].SetResult(score.evaluate);
                    data["score"].SetResult(score.score);
                    data["combo"].SetResult(score.combo);
                    data["accuracy"].SetResult(score.accuracy);
                    data["accuracyStr"].SetResult(score.accuracyString);
                    data["clear"].SetResult(score.clear);
                    if (GameHighest.Exists(d => d["uid"].GetResult<string>() == dUid))
                        GameHighest.RemoveAll(d => d["uid"].GetResult<string>() == dUid);
                    GameHighest.Add(data);

                    // Achievement.fail_count
                    data = new NiceData.Data();
                    data["uid"].SetResult(dUid);
                    data["count"].SetResult(score.failCount);
                    if (GameFailCount.Exists(d => d["uid"].GetResult<string>() == dUid))
                        GameFailCount.RemoveAll(d => d["uid"].GetResult<string>() == dUid);
                    GameFailCount.Add(data);

                    // Achievement.easy_pass, Achievement.hard_pass, Achievement.master_pass
                    switch (difficulty)
                    {
                        case 1:
                            if(!GameEasyPass.Contains(dUid))
                                GameEasyPass.Add(dUid);
                            break;
                        case 2:
                            if (!GameHardPass.Contains(dUid))
                                GameHardPass.Add(dUid);
                            break;
                        case 3:
                        case 4:
                            if (!GameMasterPass.Contains(dUid))
                                GameMasterPass.Add(dUid);
                            break;
                    }
                }
            }
            // Achievement.full_combo_music
            foreach (var fullComboMusic in CustomData.FullCombo)
            {
                var albumKey = fullComboMusic.Key;
                if (!AlbumManager.LoadedAlbums.ContainsKey(albumKey))
                {
                    ModLogger.Debug($"[full_combo_music] Album \"{albumKey}\" not found, cannot restore.");
                    continue;
                }
                var albumIndex = AlbumManager.LoadedAlbums[albumKey].Index;

                foreach (var difficulty in fullComboMusic.Value)
                {
                    var dUid = $"{AlbumManager.Uid}-{albumIndex}_{difficulty}";

                    if (!GameFullComboMusic.Contains(dUid))
                        GameFullComboMusic.Add(dUid);
                }
            }
        }

        public static bool AssertAlbumKeyIndex(string albumKey, int albumIndex)
        {
            if (string.IsNullOrEmpty(albumKey))
            {
                ModLogger.Debug($"Not found album index: {albumIndex}");
                return false;
            }
            if (albumKey.StartsWith("fs_"))
            {
                ModLogger.Debug($"Ignore folder album: {albumKey}");
                return false;
            }
            return true;
        }
    }
}
