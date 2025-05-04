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

    // Helper class to handle JSON arrays
    public static class WasteItemJsonHelper
    {
        public static List<T> FromJson<T>(string json)
        {
            // Wrap the array in an object
            string wrappedJson = "{\"items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
            return wrapper.items;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public List<T> items;
        }
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
            // Parse the JSON array using our helper
            itemDataList = WasteItemJsonHelper.FromJson<ItemData>(jsonFile.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse JSON: {e.Message}");
            return;
        }

        // Create subdirectories for each dimension
        Dictionary<string, string> dimensionFolders = new Dictionary<string, string>();
        int successCount = 0;
        int failCount = 0;

        foreach (var itemData in itemDataList)
        {
            try
            {
                // Normalize the dimensional origin
                string normalizedOrigin = NormalizeDimensionalOrigin(itemData.dimensionalOrigin);
                itemData.dimensionalOrigin = normalizedOrigin;

                // Create dimension subfolder if it doesn't exist
                if (!dimensionFolders.ContainsKey(normalizedOrigin))
                {
                    string dimensionFolder = Path.Combine(outputFolder, normalizedOrigin.Replace(" ", ""));
                    Directory.CreateDirectory(dimensionFolder);
                    dimensionFolders[normalizedOrigin] = dimensionFolder;
                }

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
                newItem.dimensionalOrigin = normalizedOrigin;

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

                // Save the asset in its dimension subfolder
                string assetPath = Path.Combine(dimensionFolders[normalizedOrigin], $"{itemData.name}.asset");
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
            $"Successfully created {successCount} items\nFailed to create {failCount} items\n" +
            $"Items organized in {dimensionFolders.Count} dimension folders",
            "OK");
    }

    private string NormalizeDimensionalOrigin(string origin)
    {
        if (string.IsNullOrEmpty(origin))
        {
            Debug.LogWarning("Empty dimensional origin, defaulting to Earth");
            return "Earth";
        }

        // Normalize the input
        string normalizedOrigin = origin.Trim();

        // Direct matches (case-insensitive)
        switch (normalizedOrigin.ToLower())
        {
            case "earth":
                return "Earth";

            case "technological waste":
            case "tech waste":
            case "technology":
                return "Technological Waste";

            case "biological remnants":
            case "bio waste":
            case "biological":
                return "Biological Remnants";

            case "quantum residue":
            case "quantum waste":
            case "quantum":
                return "Quantum Residue";

            case "philosophical byproducts":
            case "philosophical waste":
            case "philosophy":
                return "Philosophical Byproducts";

            case "cosmic debris":
            case "cosmic waste":
            case "cosmic":
                return "Cosmic Debris";

            case "ethereal plane":
            case "ethereal waste":
            case "ethereal":
                return "Ethereal Plane";

            case "archaeological waste":
            case "archaeological":
            case "artifacts":
                return "Archaeological Waste";
        }

        // Special cases that should NOT be mapped to Earth
        if (normalizedOrigin.ToLower().Contains("temporal"))
            return "Temporal Anomaly";

        if (normalizedOrigin.ToLower().Contains("quantum"))
            return "Quantum Residue";

        if (normalizedOrigin.ToLower().Contains("tech"))
            return "Technological Waste";

        if (normalizedOrigin.ToLower().Contains("bio"))
            return "Biological Remnants";

        if (normalizedOrigin.ToLower().Contains("cosmic"))
            return "Cosmic Debris";

        if (normalizedOrigin.ToLower().Contains("ethereal"))
            return "Ethereal Plane";

        if (normalizedOrigin.ToLower().Contains("archaeological") ||
            normalizedOrigin.ToLower().Contains("artifact"))
            return "Archaeological Waste";

        // Only map to Earth if it's explicitly Earth-related
        if (normalizedOrigin.ToLower().Contains("earth") ||
            normalizedOrigin.ToLower().Contains("terrestrial") ||
            normalizedOrigin.ToLower().Contains("mundane"))
            return "Earth";

        // Log warning for unknown types
        Debug.LogWarning($"Unknown dimensional origin: {origin}, creating new dimension type");
        return origin; // Keep original if unknown
    }
}