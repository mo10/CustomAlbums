using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Nice.Datas;
using NiceData = Assets.Scripts.PeroTools.Nice.Datas.Data;
using Assets.Scripts.PeroTools.Nice.Interface;
using CustomAlbums.Data;
using ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace CustomAlbums
{
    /// <summary>
    /// The legacy of NoPollution
    /// </summary>
    /// <returns></returns>
    static class NoPollutionHelper
    {
        public static readonly string FileName = "NoPollutionLost.json";

        public static void Upgrade()
        {
            var fields = Singleton<DataManager>.instance["Account"].fields;

            if (!fields.ContainsKey("CustomTracks"))
                return; // NoPollution data not found

            if (AlbumManager.LoadedAlbums?.Count == 0)
            {
                // No albums available
                ModLogger.Debug("No albums available");
                return;
            }
            // Generate all albums mapping
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (var album in AlbumManager.LoadedAlbums)
            {
                var albumKey = album.Key;
                var hash = GetCustomAlbumHashcode(album.Value);

                if (map.ContainsKey(hash))
                {
                    ModLogger.Debug($"Already exist mapping, skipped. key: {hash} value: {albumKey}, exist value: {map[hash]}");
                    continue;
                }
                map.Add(hash, albumKey);
            }
            // Upgrading
            ModLogger.Debug("Data upgrade started");
            DataObject oldRecord = Singleton<DataManager>.instance["Account"]["CustomTracks"].result as DataObject;
            if (oldRecord != null)
            {
                foreach (var data in oldRecord.fields)
                {
                    var hash = data.Key;
                    var uid = data.Value.GetResult<string>();

                    if (!map.ContainsKey(data.Key))
                    {
                        ModLogger.Debug($"Album not found: Hash: {data.Key}");
                        continue;
                    }
                    UpgradeDataToSaveManager(map[data.Key], uid);

                    ModLogger.Debug($"Upgraded: {map[data.Key]}");
                }
            }
            // Remove old record
            fields.Remove("CustomTracks");
            ModLogger.Debug($"Data upgrade completed");
        }

        public static void UpgradeDataToSaveManager(string albumKey, string uid)
        {
            var albumIndex = int.Parse(uid.RemoveFromStart($"{AlbumManager.Uid}-"));

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
            if (GameSelectedMusicPack == AlbumManager.MusicPackge
                && uid == $"{AlbumManager.MusicPackge}-{GameSelectedIndex}")
            {
                SaveManager.CustomData.SelectedAlbum = albumKey;
                SaveManager.CustomData.SelectedDifficulty = GameSelectedDifficulty;
            }
            // The following records must be packaged.
            if (albumKey.StartsWith("fs_")) return;
            // Account.Collections
            if (GameCollections.Exists(s => s == uid)
                && !SaveManager.CustomData.Collections.Contains(albumKey))
            {
                SaveManager.CustomData.Collections.RemoveAll(s => s == albumKey);
                SaveManager.CustomData.Collections.Add(albumKey);
            }
            // Account.Hide
            if (GameHides.Exists(s => s == uid)
                && !SaveManager.CustomData.Hides.Contains(albumKey))
            {
                SaveManager.CustomData.Hides.RemoveAll(s => s == albumKey);
                SaveManager.CustomData.Hides.Add(albumKey);
            }
            // Account.History
            if (GameHistory.Exists(s => s == uid)
                && !SaveManager.CustomData.History.Contains(albumKey))
            {
                SaveManager.CustomData.History.RemoveAll(s => s == albumKey);
                SaveManager.CustomData.History.Add(albumKey);
            }
            // Achievement.highest
            foreach (var highest in GameHighest.Where(d => d["uid"].GetResult<string>().StartsWith(uid)))
            {
                var dUid = highest["uid"].GetResult<string>();
                var difficulty = int.Parse(highest["uid"].GetResult<string>().Split('_')[1]);

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
                if (!SaveManager.CustomData.Highest.ContainsKey(albumKey))
                    SaveManager.CustomData.Highest.Add(albumKey, new Dictionary<int, CustomScore>());
                // Overwrite
                if (SaveManager.CustomData.Highest[albumKey].ContainsKey(difficulty))
                    SaveManager.CustomData.Highest[albumKey][difficulty] = score;
                // Append
                else
                    SaveManager.CustomData.Highest[albumKey].Add(difficulty, score);
            }
            // Achievement.full_combo_music
            foreach (var fullComboMusic in GameFullComboMusic.Where(s => s.StartsWith(uid)))
            {
                var difficulty = int.Parse(fullComboMusic.Split('_')[1]);

                // New
                if (!SaveManager.CustomData.FullCombo.ContainsKey(albumKey))
                    SaveManager.CustomData.FullCombo.Add(albumKey, new List<int>());
                // Append
                if (!SaveManager.CustomData.FullCombo[albumKey].Contains(difficulty))
                    SaveManager.CustomData.FullCombo[albumKey].Add(difficulty);
            }
        }

        public static string GetCustomAlbumHashcode(Album album)
        {
            return GetCustomAlbumHashcode(album.Info);
        }
        public static string GetCustomAlbumHashcode(AlbumInfo info)
        {
            uint nameHash = (uint)info.name.GetHashCode();
            uint authHash = (uint)info.author.GetHashCode();
            uint desiHash = (uint)info.levelDesigner.GetHashCode();
            return $"{nameHash}-{authHash}-{desiHash}";
        }
    }
}
