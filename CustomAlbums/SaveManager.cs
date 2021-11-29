using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Nice.Datas;
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

        public static void SplitCustomData()
        {

            CustomData.SelectedAlbum = null;
            if (AlbumManager.MusicPackge == Singleton<DataManager>.instance["Account"]["SelectedAlbumUid"].GetResult<string>())
            {
                var index = Singleton<DataManager>.instance["Account"]["SelectedMusicIndex"].GetResult<int>();
                CustomData.SelectedAlbum = AlbumManager.GetAlbumKeyByIndex(index);
                CustomData.SelectedDifficulty = Singleton<DataManager>.instance["Account"]["SelectedDifficulty"].GetResult<int>();
            }

            CustomData.Collections = new List<string>();
            foreach (var uid in Singleton<DataManager>.instance["Account"]["Collections"].GetResult<List<string>>().Where(s => s.StartsWith($"{AlbumManager.Uid}-")))
            {
                var index = int.Parse(uid.RemoveFromStart($"{AlbumManager.Uid}-"));
                var key = AlbumManager.GetAlbumKeyByIndex(index);

                if (string.IsNullOrEmpty(key))
                {
                    ModLogger.Debug($"Not found album index:{index}");
                    continue;
                }
                if (key.StartsWith("fs_"))
                {
                    ModLogger.Debug($"Ignore folder album:{key}");
                    continue;
                }
                CustomData.Collections.Add(key);
            }

            CustomData.Hides = new List<string>();
            foreach (var uid in Singleton<DataManager>.instance["Account"]["Hides"].GetResult<List<string>>().Where(s => s.StartsWith($"{AlbumManager.Uid}-")))
            {
                var index = int.Parse(uid.RemoveFromStart($"{AlbumManager.Uid}-"));
                var key = AlbumManager.GetAlbumKeyByIndex(index);

                if (string.IsNullOrEmpty(key))
                {
                    ModLogger.Debug($"Not found album index:{index}");
                    continue;
                }
                if (key.StartsWith("fs_"))
                {
                    ModLogger.Debug($"Ignore folder album:{key}");
                    continue;
                }
                CustomData.Hides.Add(key);
            }

            CustomData.History = new List<string>();
            foreach (var uid in Singleton<DataManager>.instance["Account"]["History"].GetResult<List<string>>().Where(s => s.StartsWith($"{AlbumManager.Uid}-")))
            {
                var index = int.Parse(uid.RemoveFromStart($"{AlbumManager.Uid}-"));
                var key = AlbumManager.GetAlbumKeyByIndex(index);

                if (string.IsNullOrEmpty(key))
                {
                    ModLogger.Debug($"Not found album index:{index}");
                    continue;
                }
                if (key.StartsWith("fs_"))
                {
                    ModLogger.Debug($"Ignore folder album:{key}");
                    continue;
                }
                CustomData.History.Add(key);
            }

            CustomData.highest = new Dictionary<string, Dictionary<int, CustomScore>>();
            foreach (var data in Singleton<DataManager>.instance["Achievement"]["highest"].GetResult<List<IData>>().Where(d => d["uid"].GetResult<string>().StartsWith($"{AlbumManager.Uid}-")))
            {
                var str = data["uid"].GetResult<string>();
                var index = int.Parse(str.RemoveFromStart($"{AlbumManager.Uid}-").Split('_')[0]);
                var difficulty = int.Parse(str.RemoveFromStart($"{AlbumManager.Uid}-").Split('_')[1]);
                var key = AlbumManager.GetAlbumKeyByIndex(index);

                if (string.IsNullOrEmpty(key))
                {
                    ModLogger.Debug($"Not found album index:{index}");
                    continue;
                }
                if (key.StartsWith("fs_"))
                {
                    ModLogger.Debug($"Ignore folder album:{key}");
                    continue;
                }
                if (!CustomData.highest.ContainsKey(key))
                {
                    // New List
                    CustomData.highest.Add(key, new Dictionary<int, CustomScore>());
                }
                if (!CustomData.highest[key].ContainsKey(difficulty))
                {
                    // Append
                    var fail_data = Singleton<DataManager>.instance["Achievement"]["fail_count"].GetResult<List<IData>>().FirstOrDefault(d => d["uid"].GetResult<string>() == str);
                    var fail_count = fail_data?["count"].GetResult<int>() ?? 0;
                    var passed = false;
                    switch (difficulty)
                    {
                        case 1:
                            passed = Singleton<DataManager>.instance["Achievement"]["easy_pass"].GetResult<List<string>>().Contains(str);
                            break;
                        case 2:
                            passed = Singleton<DataManager>.instance["Achievement"]["hard_pass"].GetResult<List<string>>().Contains(str);
                            break;
                        case 3:
                        case 4:
                            passed = Singleton<DataManager>.instance["Achievement"]["master_pass"].GetResult<List<string>>().Contains(str);
                            break;
                    }
                    var score = new CustomScore()
                    {
                        evaluate = data["evaluate"].GetResult<int>(),
                        score = data["score"].GetResult<int>(),
                        combo = data["combo"].GetResult<int>(),
                        accuracy = data["accuracy"].GetResult<float>(),
                        accuracyStr = data["accuracyStr"].GetResult<string>(),
                        clear = data["clear"].GetResult<float>(),
                        fail_count = fail_count,
                        pass = passed
                    };
                    CustomData.highest[key].Add(difficulty, score);
                }
            }

            CustomData.full_combo_music = new Dictionary<string, List<int>>();
            foreach (var str in Singleton<DataManager>.instance["Achievement"]["full_combo_music"].GetResult<List<string>>().Where(s => s.StartsWith($"{AlbumManager.Uid}-")))
            {
                var index = int.Parse(str.RemoveFromStart($"{AlbumManager.Uid}-").Split('_')[0]);
                var difficulty = int.Parse(str.RemoveFromStart($"{AlbumManager.Uid}-").Split('_')[1]);
                var key = AlbumManager.GetAlbumKeyByIndex(index);

                if (string.IsNullOrEmpty(key))
                {
                    ModLogger.Debug($"Not found album index:{index}");
                    continue;
                }
                if (key.StartsWith("fs_"))
                {
                    ModLogger.Debug($"Ignore folder album:{key}");
                    continue;
                }

                if (!CustomData.full_combo_music.ContainsKey(key))
                {
                    // New List
                    CustomData.full_combo_music.Add(key, new List<int>());
                }
                if (!CustomData.full_combo_music[key].Contains(difficulty))
                {
                    // Append
                    CustomData.full_combo_music[key].Add(difficulty);
                }
            }

        }
        public static void CleanCustomData()
        {

        }
        public static void RestoreCustomData()
        {

        }

        public static void Save()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FileName);
            File.WriteAllText(path, CustomData.JsonSerialize());
        }
    }
}
