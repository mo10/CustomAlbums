using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.GeneralLocalization;
using Assets.Scripts.PeroTools.GeneralLocalization.Modles;
using Assets.Scripts.PeroTools.Nice.Variables;
using Assets.Scripts.UI.Controls;
using Assets.Scripts.UI.Panels;
using ModHelper;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

using UnityEngine;
using HarmonyLib;

namespace MuseDashCustomAlbumMod
{
    public static class StageUIPatch
    {
        public static readonly string AlbumTagUid = "custom";
        public static readonly string AlbumTagGameObjectName = "AlbumTagCell_Custom";

        public static void DoPatching(Harmony harmony)
        {
            // PnlStage.PreWarm
            var preWarm = AccessTools.Method(typeof(PnlStage), "PreWarm", new Type[] { typeof(int) });
            var preWarmPrefix = AccessTools.Method(typeof(StageUIPatch), "PreWarmPrefix");
            harmony.Patch(preWarm, new HarmonyMethod(preWarmPrefix));
            // PnlStage.RangeStageList
            var rangeStageList = AccessTools.Method(typeof(PnlStage), "RangeStageList");
            var rangeStageListPostfix = AccessTools.Method(typeof(StageUIPatch), "RangeStageListPostfix");
            harmony.Patch(rangeStageList, null, new HarmonyMethod(rangeStageListPostfix));
            // AlbumTagName.GetAlbumTagLocaliztion
            var getAlbumTagLocaliztion = AccessTools.Method(typeof(AlbumTagName), "GetAlbumTagLocaliztion");
            var getAlbumTagLocaliztionPostfix = AccessTools.Method(typeof(StageUIPatch), "GetAlbumTagLocaliztionPostfix");
            harmony.Patch(getAlbumTagLocaliztion, null, new HarmonyMethod(getAlbumTagLocaliztionPostfix));
        }
        // PnlStage.PreWarm
        public static void PreWarmPrefix(int slice, ref List<PnlStage.albumInfo> ___m_AllAlbumTagData, ref Transform ___albumFancyScrollViewContent, ref List<GameObject> ___m_AlbumFSVCells)
        {
            if (slice == 0)
            {
                AddAlbumTagCell(ref ___albumFancyScrollViewContent);
                AddAlbumTagData(ref ___m_AllAlbumTagData);
            }
        }
        // PnlStage.RangeStageList
        public static void RangeStageListPostfix(ref List<string> ___m_AllOtherAlbumUid, ref List<string> ___m_AllOtherAlbumName_Re, ref List<string> ___m_AllOtherAlbumUid_Re, ref List<string> ___m_AllOtherAlbumName, ref List<PnlStage.albumInfo> ___m_AllAlbumTagData)
        {
            // Rebind data
            ___m_AllOtherAlbumUid.Remove(CustomAlbum.JsonName);
            ___m_AllOtherAlbumName.Remove(CustomAlbum.MusicPackge);
            ___m_AllOtherAlbumUid_Re.Remove(CustomAlbum.JsonName);
            ___m_AllOtherAlbumName_Re.Remove(CustomAlbum.MusicPackge);

            ___m_AllAlbumTagData[7].list = ___m_AllOtherAlbumUid;
            ___m_AllAlbumTagData[7].nameList = ___m_AllOtherAlbumName;
        }
        // AlbumTagName.GetAlbumTagLocaliztion
        public static void GetAlbumTagLocaliztionPostfix(string albumUid, ref string __result)
        {
            string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");
            if (albumUid == AlbumTagUid)
            {
                __result = CustomAlbum.Languages[activeOption];
            }
        }


        private static void AddAlbumTagCell(ref Transform albumFancyScrollViewContent)
        {
            if (albumFancyScrollViewContent.Find(AlbumTagGameObjectName) == null)
            {
                string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");

                // Clone gameobject
                var sourceGameObject = albumFancyScrollViewContent.GetChild(albumFancyScrollViewContent.childCount - 2).gameObject;
                var cloneGameObject = GameObject.Instantiate(sourceGameObject, sourceGameObject.transform.parent);
                // Initialization
                cloneGameObject.name = AlbumTagGameObjectName;
                cloneGameObject.transform.SetSiblingIndex(albumFancyScrollViewContent.childCount - 1);
                // Localization
                var txtTagGameObject = cloneGameObject.transform.Find("TxtTagName");
                if (txtTagGameObject != null)
                {
                    txtTagGameObject.GetComponent<UnityEngine.UI.Text>().text = CustomAlbum.Languages[activeOption];
                    var l10n = txtTagGameObject.GetComponent<Localization>();
                    foreach (var opt in l10n.optionPairs)
                    {
                        ((TextOption)opt.option).value = CustomAlbum.Languages[opt.optionEntry.name];
                        ModLogger.Debug($" opt:{opt.optionEntry.name}  val:{((TextOption)opt.option).value}");
                    }
                }
                // Icon GameObject
                var iconGameObject = txtTagGameObject.transform.Find("ImgCollab");
                var image = iconGameObject.GetComponent<Image>();
                try
                {
                    if (iconGameObject != null)
                    {
                        Sprite newSprite = new Sprite();
                        Texture2D newTex = new Texture2D(1, 1, TextureFormat.RGBA32, false); ;

                        iconGameObject.name = "ImgCustom";
                        // Load default image from embedded resources.
                        ImageConversion.LoadImage(newTex, Utils.ReadEmbeddedFile("Resources.AlbumIcon.png"));
                        newTex.filterMode = FilterMode.Point;
                        newSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0, 0), 100);
                        newSprite.name = "ImgCustom";

                        image.sprite = newSprite;
                    }
                }
                catch (Exception ex)
                {
                    ModLogger.Debug(ex);
                }
                // Bind DataIndex
                var variable = cloneGameObject.GetComponent<VariableBehaviour>();
                variable.result = albumFancyScrollViewContent.childCount - 1;

            }
        }
        private static void AddAlbumTagData(ref List<PnlStage.albumInfo> m_AllAlbumTagData)
        {
            m_AllAlbumTagData.Add(new PnlStage.albumInfo
            {
                uid = AlbumTagUid,
                name = CustomAlbum.Languages["ChineseS"],
                list = new List<string>() { CustomAlbum.JsonName },
                nameList = new List<string>() { CustomAlbum.MusicPackge },
                isWeekFree = false,
                isNew = false
            });
        }
    }
}
