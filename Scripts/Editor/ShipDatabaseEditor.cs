#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ShipDatabaseEditor : EditorWindow
{
    private Vector2 scrollPosition;
    private ShipDatabase targetDatabase;
    private SerializedObject serializedObject;
    private SerializedProperty shipModelsProperty;
    private bool showOnlyEmpty = false;
    private string searchFilter = "";
    private int typeFilterIndex = 0; // 0 = All Types
    private Sprite manualSprite;
    private EnemyType manualType = EnemyType.Scavenger;
    private int manualSector = 1;

    [MenuItem("Game Tools/Ship Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<ShipDatabaseEditor>("Ship Database Editor");
    }

    private void OnEnable()
    {
        FindShipDatabase();
    }

    private void FindShipDatabase()
    {
        // Try to find ShipDatabase in scene
        targetDatabase = FindObjectOfType<ShipDatabase>();

        if (targetDatabase != null)
        {
            serializedObject = new SerializedObject(targetDatabase);
            shipModelsProperty = serializedObject.FindProperty("shipModels");
        }
    }

    private void OnGUI()
    {
        try
        {
            // Always wrap the entire OnGUI in a try-catch to prevent layout errors
            DrawShipDatabaseEditor();
        }
        catch (System.Exception e)
        {
            EditorGUILayout.HelpBox("Error drawing UI: " + e.Message, MessageType.Error);
            Debug.LogException(e);
        }
    }

    private void DrawShipDatabaseEditor()
    {
        if (serializedObject == null || targetDatabase == null)
        {
            EditorGUILayout.HelpBox("No ShipDatabase found in scene!", MessageType.Error);
            if (GUILayout.Button("Find Ship Database"))
            {
                FindShipDatabase();
            }
            return;
        }

        serializedObject.Update();

        // Database actions
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Auto-Populate Database"))
            {
                targetDatabase.AutoPopulateDatabase();
                serializedObject.Update();
            }

            if (GUILayout.Button("Clear Database"))
            {
                if (EditorUtility.DisplayDialog("Clear Database",
                    "Are you sure you want to clear the ship database? This cannot be undone.",
                    "Yes, Clear Database", "Cancel"))
                {
                    shipModelsProperty.ClearArray();
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Test resource loading button
        if (GUILayout.Button("Test Resource Loading"))
        {
            TestResourceLoading();
        }

        // Database stats
        int size = shipModelsProperty != null ? shipModelsProperty.arraySize : 0;

        // Count entries with and without descriptions
        int withDescriptions = 0;
        for (int i = 0; i < size; i++)
        {
            SerializedProperty shipProperty = shipModelsProperty.GetArrayElementAtIndex(i);
            if (shipProperty != null)
            {
                SerializedProperty descriptionProperty = shipProperty.FindPropertyRelative("description");
                if (descriptionProperty != null && !string.IsNullOrEmpty(descriptionProperty.stringValue))
                {
                    withDescriptions++;
                }
            }
        }

        EditorGUILayout.LabelField($"Ship Models: {size} total, {withDescriptions} with descriptions, {size - withDescriptions} without", EditorStyles.boldLabel);

        // Filter options
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            {
                showOnlyEmpty = EditorGUILayout.Toggle("Show only empty descriptions", showOnlyEmpty);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                searchFilter = EditorGUILayout.TextField("Search by name:", searchFilter);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                // Create type filter dropdown
                var typeNames = new List<string> { "All Types" };
                typeNames.AddRange(System.Enum.GetNames(typeof(EnemyType)));

                typeFilterIndex = EditorGUILayout.Popup("Filter by type:", typeFilterIndex, typeNames.ToArray());
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clear Filters"))
            {
                searchFilter = "";
                typeFilterIndex = 0;
                showOnlyEmpty = false;
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Manual creation section
        DrawManualCreation();

        EditorGUILayout.Space();

        // Draw ship models list if we have any
        if (size > 0)
        {
            // Ship models list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                DrawShipModelsList(size);
            }
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("No ship models in database. Click 'Auto-Populate Database' to add ships from your Resources folder.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawManualCreation()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Manual Ship Creation", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Sprite:", GUILayout.Width(100));
                manualSprite = (Sprite)EditorGUILayout.ObjectField(manualSprite, typeof(Sprite), false);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Type:", GUILayout.Width(100));
                manualType = (EnemyType)EditorGUILayout.EnumPopup(manualType);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Sector:", GUILayout.Width(100));
                manualSector = EditorGUILayout.IntField(manualSector);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Add Ship Model") && manualSprite != null)
            {
                AddManualShipModel(manualSprite, manualType, manualSector);
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void AddManualShipModel(Sprite sprite, EnemyType type, int sector)
    {
        serializedObject.Update();

        // Add a new element to the array
        shipModelsProperty.arraySize++;

        // Get the new element
        SerializedProperty newShip = shipModelsProperty.GetArrayElementAtIndex(shipModelsProperty.arraySize - 1);

        // Clean the sprite name
        string cleanName = (targetDatabase != null)
            ? targetDatabase.CleanSpriteName(sprite.name)
            : sprite.name.EndsWith("_0") ? sprite.name.Substring(0, sprite.name.Length - 2) : sprite.name;

        // Set properties
        newShip.FindPropertyRelative("modelName").stringValue = cleanName;
        newShip.FindPropertyRelative("shipType").enumValueIndex = (int)type;
        newShip.FindPropertyRelative("minSectorLevel").intValue = sector;
        newShip.FindPropertyRelative("shipIcon").objectReferenceValue = sprite;
        newShip.FindPropertyRelative("description").stringValue = "";

        serializedObject.ApplyModifiedProperties();

        Debug.Log($"Manually added ship model: {cleanName}");
    }

    private void DrawShipModelsList(int size)
    {
        for (int i = 0; i < size; i++)
        {
            // Get ship properties safely
            SerializedProperty shipProperty = shipModelsProperty.GetArrayElementAtIndex(i);
            if (shipProperty == null) continue;

            SerializedProperty nameProperty = shipProperty.FindPropertyRelative("modelName");
            SerializedProperty typeProperty = shipProperty.FindPropertyRelative("shipType");
            SerializedProperty sectorProperty = shipProperty.FindPropertyRelative("minSectorLevel");
            SerializedProperty descriptionProperty = shipProperty.FindPropertyRelative("description");
            SerializedProperty iconProperty = shipProperty.FindPropertyRelative("shipIcon");

            // Skip if any required property is null
            if (nameProperty == null || typeProperty == null || descriptionProperty == null) continue;

            // Apply filters
            bool hasDescription = !string.IsNullOrEmpty(descriptionProperty.stringValue);
            if (showOnlyEmpty && hasDescription) continue;

            bool matchesSearch = string.IsNullOrEmpty(searchFilter) ||
                                (nameProperty.stringValue != null &&
                                 nameProperty.stringValue.ToLower().Contains(searchFilter.ToLower()));
            if (!matchesSearch) continue;

            bool matchesType = typeFilterIndex == 0 || // "All Types"
                              (typeProperty.enumValueIndex == typeFilterIndex - 1); // -1 because "All Types" is at index 0
            if (!matchesType) continue;

            // Create a background style
            GUIStyle itemStyle = new GUIStyle(EditorStyles.helpBox);

            // Draw ship item
            EditorGUILayout.BeginVertical(itemStyle);
            {
                // Get type name safely
                string typeName = "Unknown";
                if (typeProperty.enumValueIndex >= 0 &&
                    typeProperty.enumValueIndex < System.Enum.GetNames(typeof(EnemyType)).Length)
                {
                    typeName = System.Enum.GetNames(typeof(EnemyType))[typeProperty.enumValueIndex];
                }

                EditorGUILayout.BeginHorizontal();
                {
                    // Show ship icon
                    if (iconProperty != null && iconProperty.objectReferenceValue != null)
                    {
                        Texture2D texture = AssetPreview.GetAssetPreview(iconProperty.objectReferenceValue);
                        if (texture != null)
                        {
                            GUILayout.Label(texture, GUILayout.Width(64), GUILayout.Height(64));
                        }
                        else
                        {
                            GUILayout.Label("No Preview", GUILayout.Width(64), GUILayout.Height(64));
                        }
                    }
                    else
                    {
                        GUILayout.Label("No Icon", GUILayout.Width(64), GUILayout.Height(64));
                    }

                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField(
                            nameProperty.stringValue != null ? nameProperty.stringValue : "No Name",
                            EditorStyles.boldLabel);

                        EditorGUILayout.LabelField(
                            $"Type: {typeName} | Sector: {(sectorProperty != null ? sectorProperty.intValue.ToString() : "?")}");

                        EditorGUILayout.Space();

                        // Description label with color
                        GUIStyle descLabelStyle = new GUIStyle(EditorStyles.boldLabel);
                        if (!hasDescription)
                        {
                            descLabelStyle.normal.textColor = Color.red;
                        }

                        EditorGUILayout.LabelField("Description:", descLabelStyle);

                        // Show description field
                        if (descriptionProperty != null)
                        {
                            descriptionProperty.stringValue = EditorGUILayout.TextArea(
                                descriptionProperty.stringValue ?? "",
                                GUILayout.Height(40));
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
        }
    }

    // Test resource loading with more detailed diagnostics
    private void TestResourceLoading()
    {
        string basePath = targetDatabase.resourceBasePath; // Use the current setting
        Debug.Log($"Testing resource loading from base path: Resources/{basePath}");

        // First, test if the Resources folder itself exists
        Debug.Log("All sprite resources in project:");
        var allSprites = Resources.LoadAll<Sprite>("");
        foreach (var sprite in allSprites)
        {
            Debug.Log($"- Found sprite: {sprite.name} at path: {AssetDatabase.GetAssetPath(sprite)}");
        }

        // Test Default folder
        string defaultPath = $"{basePath}/Default";
        TestPathDetailed(defaultPath);

        // Test sector folders
        for (int sector = 1; sector <= 3; sector++)
        {
            string sectorPath = $"{basePath}/Sector{sector}";
            TestPathDetailed(sectorPath);

            // Test type subfolders in sector folders
            foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType)))
            {
                string typePath = $"{sectorPath}/{type}";
                TestPathDetailed(typePath);
            }
        }

        Debug.Log("Resource testing complete. Check console for results.");
    }

    private void TestPathDetailed(string path)
    {
        // Check if path exists by looking for ANY assets
        var allAssets = Resources.LoadAll(path);
        bool pathExists = allAssets != null && allAssets.Length > 0;

        // Specifically look for sprites
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);

        // Also try using correct path format with forward slashes
        string normalizedPath = path.Replace('\\', '/');
        if (sprites == null || sprites.Length == 0)
        {
            sprites = Resources.LoadAll<Sprite>(normalizedPath);
        }

        // Check what's in the folder even if no sprites were found
        if (sprites != null && sprites.Length > 0)
        {
            Debug.Log($"✅ Found {sprites.Length} sprites in Resources/{path}:");
            foreach (var sprite in sprites)
            {
                Debug.Log($"  - {sprite.name} ({AssetDatabase.GetAssetPath(sprite)})");
            }
        }
        else
        {
            Debug.Log($"❌ No sprites found in Resources/{path}");

            // Check if the directory exists but has other assets
            if (pathExists)
            {
                Debug.Log($"  But directory exists with {allAssets.Length} other assets:");
                foreach (var asset in allAssets)
                {
                    Debug.Log($"  - {asset.name} ({asset.GetType().Name})");
                }
            }
            else
            {
                Debug.Log($"  Directory does not exist or is empty");

                // Check parent directory
                string parentPath = path.Substring(0, path.LastIndexOf('/'));
                if (!string.IsNullOrEmpty(parentPath))
                {
                    var parentAssets = Resources.LoadAll(parentPath);
                    if (parentAssets != null && parentAssets.Length > 0)
                    {
                        Debug.Log($"  But parent directory {parentPath} has {parentAssets.Length} assets");
                    }
                }
            }
        }
    }
}
#endif