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
        private long maxPathId;
        private Dictionary<string, AssetsReplacer> replacers;
        public AssetBundleHelper(string path, int index = 0)
        {
            replacers = new Dictionary<string, AssetsReplacer>();
            assetsManager = new AssetsManager();
            bundle = assetsManager.LoadBundleFile(path);
            assets = assetsManager.LoadAssetsFileFromBundle(bundle, index);
            maxPathId = assets.table.assetFileInfo.Max(i => i.index);
        }
        public AssetTypeValueField GetAssetByPathId(long pathId)
        {
            foreach (var fileInfoEx in assets.table.assetFileInfo)
            {
                if (fileInfoEx.index == pathId)
                {
                    var baseField = assetsManager.GetTypeInstance(assets, fileInfoEx).GetBaseField();
                    return baseField;
                }
            }
            return null;
        }
        public AssetTypeValueField GetAsset(string name)
        {
            var asset = assets.table.GetAssetInfo(name);
            var baseField = assetsManager.GetTypeInstance(assets, asset).GetBaseField();
            return baseField;
        }
        public long SaveAsset(AssetTypeValueField baseField, string type = null)
        {
            var name = baseField.Get("m_Name").value.AsString();
            var asset = assets.table.GetAssetInfo(name);

            long pathId;
            int classId;
            byte[] buffer;

            if (asset == null)
            {
                // New Asset
                var ttType = AssetHelper.FindTypeTreeTypeByName(assets.file.typeTree, type);
                classId = ttType.classId;
                pathId = ++maxPathId;
            }
            else
            {
                // Exist Asset
                classId = (int)asset.curFileType;
                pathId = asset.index;
            }
            buffer = baseField.WriteToByteArray();

            var repl = new AssetsReplacerFromMemory(0, pathId, classId, 0xffff, buffer);
            // Add to replacer list, use Apply() to apply
            if (replacers.ContainsKey(name))
            {
                replacers[name] = repl;
            }
            else
            {
                replacers.Add(name, repl);
            }
            return pathId;
        }
        public AssetTypeValueField CreateAsset(string type)
        {
            var templateField = new AssetTypeTemplateField();
            var ttType = AssetHelper.FindTypeTreeTypeByName(assets.file.typeTree, type);
            templateField.From0D(ttType, 0);

            return ValueBuilder.DefaultValueFieldFromTemplate(templateField);
        }
        public MemoryStream Apply()
        {
            List<AssetsReplacer> repls = new List<AssetsReplacer>();
            foreach (var item in replacers)
            {
                repls.Add(item.Value);
            }
            //write changes to memory
            byte[] newAssetData;
            using (var stream = new MemoryStream())
            using (var writer = new AssetsFileWriter(stream))
            {
                assets.file.Write(writer, 0, repls, 0);
                newAssetData = stream.ToArray();
            }

            //rename this asset name from boring to cool when saving
            var bunRepl = new BundleReplacerFromMemory(assets.name, assets.name, true, newAssetData, -1);

            var memoryStream = new MemoryStream();
            using (var writer = new AssetsFileWriter(memoryStream))
            {
                bundle.file.Write(writer, new List<BundleReplacer>() { bunRepl });
            }
            return memoryStream;
        }
        public void UpdateMetadata(long pathId, string path)
        {
            string fullyPath = $"Assets/Static Resources/{path}";
            var assetBundleInfo = this.GetAssetByPathId(1);
            var m_PreloadTable = assetBundleInfo["m_PreloadTable"]["Array"];
            var m_Container = assetBundleInfo["m_Container"]["Array"];

            // Check if already exist
            foreach (var pairData in m_Container.GetChildrenList())
            {
                if (pairData["second"]["asset"]["m_PathID"].value.AsInt64() == pathId)
                {
                    if(pairData["first"].value.AsString() != fullyPath)
                    {
                        pairData["first"].value.Set(fullyPath);
                        SaveAsset(assetBundleInfo);
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

            SaveAsset(assetBundleInfo);
        }
        public int WriteFile(string path, string name, object data)
        {
            var newAsset = this.CreateAsset("TextAsset");
            newAsset["m_Name"].value.Set(name);
            newAsset["m_Script"].value.Set(data);
            var newAssetPathId = SaveAsset(newAsset, "TextAsset");
            // Update metadata
            UpdateMetadata(newAssetPathId,);
        }
        ~AssetBundleHelper()
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