using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class WasteItemJsonConverter : EditorWindow
{
    [MenuItem("Tools/Convert Waste Item JSON")]
    public static void ShowWindow()
    {
        GetWindow<WasteItemJsonConverter>("Waste Item JSON Converter");
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

    private TextAsset inputJsonFile;
    private string outputFolder = "Assets/Resources/ConvertedJSON";

    private void OnGUI()
    {
        GUILayout.Label("Waste Item JSON Converter", EditorStyles.boldLabel);

        inputJsonFile = (TextAsset)EditorGUILayout.ObjectField("Input JSON File", inputJsonFile, typeof(TextAsset), false);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        if (GUILayout.Button("Convert JSON"))
        {
            if (inputJsonFile != null)
            {
                ConvertJson();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select an input JSON file", "OK");
            }
        }

        GUILayout.Space(20);
        GUILayout.Label("Dimension Mapping", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This will convert your existing dimensional origins to match the location system", MessageType.Info);
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

    private void ConvertJson()
    {
        try
        {
            // Parse the JSON using our helper
            string jsonContent = inputJsonFile.text;
            List<ItemData> itemList = WasteItemJsonHelper.FromJson<ItemData>(jsonContent);

            // Create dictionaries for converted items by dimension
            Dictionary<string, List<ItemData>> itemsByDimension = new Dictionary<string, List<ItemData>>();

            // Convert dimensional origins to match your location system
            foreach (var item in itemList)
            {
                string convertedDimension = ConvertDimensionalOrigin(item.dimensionalOrigin);

                if (!itemsByDimension.ContainsKey(convertedDimension))
                {
                    itemsByDimension[convertedDimension] = new List<ItemData>();
                }

                item.dimensionalOrigin = convertedDimension;
                itemsByDimension[convertedDimension].Add(item);
            }

            // Ensure output directory exists
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // Save separate JSON files for each dimension
            foreach (var kvp in itemsByDimension)
            {
                string dimensionName = kvp.Key.Replace(" ", "");
                string outputPath = Path.Combine(outputFolder, $"{dimensionName}Items.json");

                // Format as JSON array without wrapper
                string outputJson = FormatJsonArray(kvp.Value);

                File.WriteAllText(outputPath, outputJson);
                Debug.Log($"Saved {kvp.Value.Count} items to {outputPath}");
            }

            // Also save a combined file with corrected dimensions
            string combinedPath = Path.Combine(outputFolder, "AllItemsCorrected.json");
            string combinedJson = FormatJsonArray(itemList);
            File.WriteAllText(combinedPath, combinedJson);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success",
                $"Converted JSON files saved to {outputFolder}\n" +
                $"Total items: {itemList.Count}\n" +
                $"Dimensions: {string.Join(", ", itemsByDimension.Keys)}", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to convert JSON: {e.Message}", "OK");
            Debug.LogError(e);
        }
    }

    private string FormatJsonArray(List<ItemData> items)
    {
        if (items == null || items.Count == 0)
            return "[]";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("[");

        for (int i = 0; i < items.Count; i++)
        {
            string itemJson = JsonUtility.ToJson(items[i], true);
            sb.Append("  ");
            sb.Append(itemJson);

            if (i < items.Count - 1)
                sb.AppendLine(",");
            else
                sb.AppendLine();
        }

        sb.Append("]");
        return sb.ToString();
    }

    private string ConvertDimensionalOrigin(string originalOrigin)
    {
        if (string.IsNullOrEmpty(originalOrigin))
        {
            Debug.LogWarning("Empty dimensional origin, defaulting to Earth");
            return "Earth";
        }

        // Normalize the input
        string normalizedOrigin = originalOrigin.Trim();

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
        Debug.LogWarning($"Unknown dimensional origin: {originalOrigin}, creating new dimension type");
        return originalOrigin; // Keep original if unknown
    }
}