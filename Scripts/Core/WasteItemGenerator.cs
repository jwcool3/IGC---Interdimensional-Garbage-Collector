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

    private void OnGUI()
    {
        GUILayout.Label("Waste Item Generator", EditorStyles.boldLabel);

        jsonFile = (TextAsset)EditorGUILayout.ObjectField("Item Data JSON", jsonFile, typeof(TextAsset), false);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

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
    private class ItemDataList
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
            // If your JSON is an array
            itemDataList = JsonUtility.FromJson<ItemDataList>("{\"items\":" + jsonFile.text + "}").items;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse JSON: {e.Message}");
            return;
        }

        int successCount = 0;
        int failCount = 0;

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
                    // Assuming you implemented Approach 3 to simplify sprite references
                    newItem.itemSprites = new Sprite[] { itemSprite };
                }
                else
                {
                    Debug.LogWarning($"Could not find sprite at path: {itemData.spritePath} for item {itemData.name}");
                }

                // Save the asset
                string assetPath = $"{outputFolder}/{itemData.name}.asset";
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