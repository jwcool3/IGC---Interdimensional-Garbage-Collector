using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class WasteItemGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Waste Items")]
    public static void ShowWindow()
    {
        GetWindow<WasteItemGenerator>("Waste Item Generator");
    }

    private TextAsset jsonFile;
    private string outputFolder = "Assets/Resources/ItemDatabase";
    private bool createByDimension = true; // Option to create items in dimension-specific folders

    private void OnGUI()
    {
        GUILayout.Label("Waste Item Generator", EditorStyles.boldLabel);

        jsonFile = (TextAsset)EditorGUILayout.ObjectField("Item Data JSON", jsonFile, typeof(TextAsset), false);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        createByDimension = EditorGUILayout.Toggle("Organize by Dimension", createByDimension);

        if (GUILayout.Button("Generate Items"))
        {
            GenerateItemsFromJson();
        }
    }

    [System.Serializable]
    private class ItemData
    {
        public string name;
        public string itemName;
        public string description;
        public int defaultRarity;
        public float baseStability;
        public float baseContamination;
        public float baseRecyclingPotential;
        public string dimensionalOrigin;
        public string spritePath;
    }

    [System.Serializable]
    private class ItemDataWrapper
    {
        public List<ItemData> items;
    }

    private void GenerateItemsFromJson()
    {
        if (jsonFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a JSON file", "OK");
            return;
        }

        // Ensure output directory exists
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        // Parse JSON
        List<ItemData> itemDataList;
        try
        {
            // The JSON is an array, so we need to deserialize it properly
            string jsonContent = jsonFile.text;
            itemDataList = JsonUtility.FromJson<ItemDataWrapper>("{\"items\":" + jsonContent + "}").items;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse JSON: {e.Message}");
            return;
        }

        int successCount = 0;
        int failCount = 0;

        // Keep track of created directories
        HashSet<string> createdDirectories = new HashSet<string>();

        foreach (var itemData in itemDataList)
        {
            try
            {
                // Create the ScriptableObject
                WasteItemData newItem = ScriptableObject.CreateInstance<WasteItemData>();

                // Set properties
                newItem.itemName = itemData.itemName;
                newItem.uniqueIdentifier = System.Guid.NewGuid().ToString();
                newItem.description = itemData.description;
                newItem.defaultRarity = (WasteRarity)itemData.defaultRarity;
                newItem.baseStability = itemData.baseStability;
                newItem.baseContamination = itemData.baseContamination;
                newItem.baseRecyclingPotential = itemData.baseRecyclingPotential;
                newItem.dimensionalOrigin = itemData.dimensionalOrigin;

                // Assign sprite if path is valid
                Sprite itemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(itemData.spritePath);
                if (itemSprite != null)
                {
                    newItem.itemSprites = new Sprite[] { itemSprite };
                }
                else
                {
                    Debug.LogWarning($"Could not find sprite at path: {itemData.spritePath} for item {itemData.name}");
                }

                // Determine asset path
                string assetPath;
                if (createByDimension)
                {
                    // Create dimension-specific folder
                    string dimensionFolder = itemData.dimensionalOrigin.Replace(" ", "");
                    string fullDimensionPath = $"{outputFolder}/{dimensionFolder}Items";

                    if (!createdDirectories.Contains(fullDimensionPath))
                    {
                        if (!Directory.Exists(fullDimensionPath))
                        {
                            Directory.CreateDirectory(fullDimensionPath);
                        }
                        createdDirectories.Add(fullDimensionPath);
                    }

                    assetPath = $"{fullDimensionPath}/{itemData.name}.asset";
                }
                else
                {
                    assetPath = $"{outputFolder}/{itemData.name}.asset";
                }

                // Save the asset
                AssetDatabase.CreateAsset(newItem, assetPath);
                successCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create item {itemData.name}: {e.Message}");
                failCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Generation Complete",
            $"Successfully created {successCount} items.\nFailed: {failCount}", "OK");
    }
}