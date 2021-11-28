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

namespace CustomAlbums
{
    /// <summary>
    /// The legacy of NoPollution
    /// </summary>
    /// <returns></returns>
    static class NoPollutionHelper
    {

        public static void UpgradeAndRestore()
        {
            var fields = Singleton<DataManager>.instance["Account"].fields;

            if (AlbumManager.LoadedAlbums?.Count == 0)
            {
                // No albums available
                return;
            }
            
            if (fields.ContainsKey("CustomTracks"))
            {
                DataObject oldRecord = Singleton<DataManager>.instance["Account"]["CustomTracks"].result as DataObject;
                var newRecord = Singleton<DataManager>.instance["Account"]["CustomAlbums"].GetResult<NiceData>() ?? new NiceData();

                foreach (var data in oldRecord.fields)
                {
                    ModLogger.Debug($"NoPollution data {data.Key} {data.Value.GetResult<string>()}");
                    newRecord[data.Key].SetResult(data.Value.GetResult<string>());
                }
                // Remove old record, save new record
                fields.Remove("CustomTracks");
                Singleton<DataManager>.instance["Account"]["CustomAlbums"].SetResult(newRecord);
                Singleton<DataManager>.instance.Save();
                ModLogger.Debug($"NoPollution upgraded");
            }

            if (fields.ContainsKey("CustomAlbums"))
            {
                ModLogger.Debug($"Has CustomAlbums");
            }
        }

        public static string CreateCustomAlbumHashcode(Album album)
        {
            return CreateCustomAlbumHashcode(album.Info);
        }
        public static string CreateCustomAlbumHashcode(AlbumInfo info)
        {
            uint nameHash = (uint)info.name.GetHashCode();
            uint authHash = (uint)info.author.GetHashCode();
            uint desiHash = (uint)info.levelDesigner.GetHashCode();
            return $"{nameHash}-{authHash}-{desiHash}";
        }
    }
}
