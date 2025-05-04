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
            // Parse the JSON
            string jsonContent = inputJsonFile.text;

            // Parse the JSON array directly
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

            // Add proper indentation
            string[] lines = itemJson.Split('\n');
            for (int j = 0; j < lines.Length; j++)
            {
                if (j == 0)
                    sb.Append("  ");
                else
                    sb.Append("    ");

                sb.Append(lines[j]);

                if (j < lines.Length - 1)
                    sb.AppendLine();
            }

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
        // Map your existing dimensional origins to the ones used in your location system
        string lowerOrigin = originalOrigin.ToLower();

        // First handle the special cases that need to be mapped to Earth
        if (lowerOrigin.Contains("anomaly") ||
            lowerOrigin.Contains("temporal") ||
            lowerOrigin.Contains("material") ||
            lowerOrigin.Contains("erosion") ||
            lowerOrigin.Contains("stasis") ||
            lowerOrigin.Contains("echo") ||
            lowerOrigin.Contains("containment") ||
            lowerOrigin.Contains("residue") ||
            lowerOrigin == "dimensional anomaly")
        {
            return "Earth"; // Map various anomalies to Earth for now
        }

        switch (lowerOrigin)
        {
            case "technological waste":
                return "Technological Waste";

            case "biological remnants":
                return "Biological Remnants";

            case "quantum residue":
                return "Quantum Residue";

            case "philosophical byproducts":
                return "Philosophical Byproducts";

            case "cosmic debris":
                return "Cosmic Debris";

            case "ethereal plane":
                return "Ethereal Plane";

            case "archaeological waste":
                return "Archaeological Waste";

            case "earth":
                return "Earth";

            default:
                // Default to Earth for unrecognized origins
                Debug.LogWarning($"Unknown dimensional origin: {originalOrigin}, defaulting to Earth");
                return "Earth";
        }
    }
}