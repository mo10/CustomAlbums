using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomAlbums
{
    class AssetBundleHelper
    {
        private AssetsManager assetsManager;
        private BundleFileInstance bundle;
        private AssetsFileInstance assets;

        private long nextPathId;
        private Dictionary<long, AssetTypeValueField> replaceFields;

        public AssetBundleHelper(string path, int index = 0)
        {
            replaceFields = new Dictionary<long, AssetTypeValueField>();
            assetsManager = new AssetsManager();
            bundle = assetsManager.LoadBundleFile(path);
            assets = assetsManager.LoadAssetsFileFromBundle(bundle, index);

            nextPathId = assets.table.assetFileInfo.Max(i => i.index) + 1;
        }
        public AssetTypeValueField GetReplaceAsset(long pathId)
        {
            AssetTypeValueField field;

            if (replaceFields.TryGetValue(pathId, out field))
            {
                return field;
            }
            return null;
        }
        public AssetTypeValueField GetReplaceAsset(string name)
        {
            foreach (var replField in replaceFields)
            {
                var replName = replField.Value.Get("m_Name").value.AsString();
                if (replName == name)
                {
                    return replField.Value;
                }
            }
            return null;
        }
        public long GetReplaceAssetPathId(string name)
        {
            foreach (var replField in replaceFields)
            {
                var replName = replField.Value.Get("m_Name").value.AsString();
                if (replName == name)
                {
                    return replField.Key;
                }
            }
            return -1;
        }
        public AssetTypeValueField GetAsset(long pathId)
        {
            var field = GetReplaceAsset(pathId);
            if (field != null)
                return field;

            foreach (var fileInfoEx in assets.table.assetFileInfo)
            {
                if (fileInfoEx.index == pathId)
                {
                    return assetsManager.GetTypeInstance(assets, fileInfoEx).GetBaseField();
                }
            }
            return null;
        }
        public AssetTypeValueField GetAsset(string name)
        {
            var field = GetReplaceAsset(name);
            if (field != null)
                return field;

            foreach (var replField in replaceFields)
            {
                var replName = replField.Value.Get("m_Name").value.AsString();
                if (replName == name)
                {
                    return replField.Value;
                }
            }

            var asset = assets.table.GetAssetInfo(name);
            field = assetsManager.GetTypeInstance(assets, asset).GetBaseField();

            return field;
        }
        public long ReplaceAsset(AssetTypeValueField field)
        {
            var name = field.Get("m_Name").value.AsString();
            var type = field.GetFieldType();
            var pathId = GetReplaceAssetPathId(name);


            if (pathId != -1)
            {
                // Update replace asset
                replaceFields[pathId] = field;
                return pathId;
            }

            var asset = assets.table.GetAssetInfo(name);
            if (asset == null)
            {
                // Add to replaceFields
                pathId = nextPathId;
                nextPathId++;
            }
            else
            {
                pathId = asset.index;
            }

            replaceFields.Add(pathId, field);
            return pathId;
        }
        public MemoryStream ApplyReplace()
        {
            var replacers = new List<AssetsReplacer>();
            foreach (var replace in replaceFields)
            {
                long pathId = replace.Key;
                int classId = AssetHelper.FindTypeTreeTypeByName(assets.file.typeTree, replace.Value.GetFieldType()).classId;
                byte[] buffer = replace.Value.WriteToByteArray();

                replacers.Add(new AssetsReplacerFromMemory(0, pathId, classId, 0xffff, buffer));
            }

            // Write changes to memory
            byte[] newAssetData;
            using (var stream = new MemoryStream())
            using (var writer = new AssetsFileWriter(stream))
            {
                assets.file.Write(writer, 0, replacers, 0);
                newAssetData = stream.ToArray();
            }

            var memoryStream = new MemoryStream();
            using (var writer = new AssetsFileWriter(memoryStream))
            {
                bundle.file.Write(writer, new List<BundleReplacer>() { new BundleReplacerFromMemory(assets.name, assets.name, true, newAssetData, -1) });
            }

            return memoryStream;
        }
        public AssetTypeValueField CreateAsset(string type)
        {
            var templateField = new AssetTypeTemplateField();
            var ttType = AssetHelper.FindTypeTreeTypeByName(assets.file.typeTree, type);
            templateField.From0D(ttType, 0);

            return ValueBuilder.DefaultValueFieldFromTemplate(templateField);
        }
        public void UpdateMetadata(long pathId, string path)
        {
            string fullyPath = $"Assets/Static Resources/{path}";
            var assetBundleInfo = GetAsset(1);
            var m_PreloadTable = assetBundleInfo["m_PreloadTable"]["Array"];
            var m_Container = assetBundleInfo["m_Container"]["Array"];

            // Check if already exist
            foreach (var pairData in m_Container.GetChildrenList())
            {
                if (pairData["second"]["asset"]["m_PathID"].value.AsInt64() == pathId)
                {
                    if (pairData["first"].value.AsString() != fullyPath)
                    {
                        pairData["first"].value.Set(fullyPath);
                        ReplaceAsset(assetBundleInfo);
                        return;
                    }
                }
            }

            var item = ValueBuilder.DefaultValueFieldFromArrayTemplate(m_PreloadTable);
            item["m_FileID"].value.Set(0);
            item["m_PathID"].value.Set(pathId);
            m_PreloadTable.AddChlidren(item);

            item = ValueBuilder.DefaultValueFieldFromArrayTemplate(m_Container);
            item["first"].value.Set($"Assets/Static Resources/{path}");
            item["second"]["preloadIndex"].value.Set(m_PreloadTable.GetChildrenCount() - 1);
            item["second"]["preloadSize"].value.Set(1);
            item["second"]["asset"]["m_FileID"].value.Set(0);
            item["second"]["asset"]["m_PathID"].value.Set(pathId);
            m_Container.AddChlidren(item);

            ReplaceAsset(assetBundleInfo);
        }

        public void Unload()
        {
            if (assetsManager != null)
            {
                assetsManager.UnloadAllAssetsFiles();
                assetsManager.UnloadAllBundleFiles();
            }
        }
    }
    public static class AssetBundleHelperExtension
    {
        public static T AsJson<T>(this AssetTypeValue value)
        {
            return value.AsString().JsonDeserialize<T>();
        }
        public static AssetTypeValueField AddChlidren(this AssetTypeValueField field, AssetTypeValueField chlidren)
        {
            var newArray = new List<AssetTypeValueField>(field.children);
            newArray.Add(chlidren);
            field.SetChildrenList(newArray.ToArray());

            return field;
        }
    }
}