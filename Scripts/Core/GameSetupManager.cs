using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the initialization sequence for the game systems
/// </summary>
public class GameSetupManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject wasteItemDatabasePrefab;
    [SerializeField] private GameObject resourceManagerPrefab;
    [SerializeField] private GameObject facilityManagerPrefab;
    [SerializeField] private GameObject wasteInventoryManagerPrefab;
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject uiManagerPrefab;

    [Header("Configuration")]
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private float delayBetweenSystems = 0.1f;

    // Singleton reference
    public static GameSetupManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialization
        if (initializeOnStart)
        {
            StartCoroutine(InitializeGameSystems());
        }
    }

    /// <summary>
    /// Initializes all game systems in the correct order
    /// </summary>
    public IEnumerator InitializeGameSystems()
    {
        Debug.Log("Starting game system initialization sequence...");

        // Step 1: Create WasteItemDatabase
        yield return CreateSystem("WasteItemDatabase", wasteItemDatabasePrefab, () => WasteItemDatabase.Instance == null);

        // Step 2: Create ResourceManager
        yield return CreateSystem("ResourceManager", resourceManagerPrefab, () => ResourceManager.Instance == null);

        // Step 3: Create FacilityManager
        yield return CreateSystem("FacilityManager", facilityManagerPrefab, () => FacilityManager.Instance == null);

        // Step 4: Create WasteInventoryManager
        yield return CreateSystem("WasteInventoryManager", wasteInventoryManagerPrefab, () => WasteInventoryManager.Instance == null);

        // Step 5: Create GameManager
        yield return CreateSystem("GameManager", gameManagerPrefab, () => GameManager.Instance == null);

        // Step 6: Create UIManager
        yield return CreateSystem("UIManager", uiManagerPrefab, () => UIManager.Instance == null);

        Debug.Log("Game system initialization complete!");
    }

    /// <summary>
    /// Creates a game system if needed
    /// </summary>
    private IEnumerator CreateSystem(string systemName, GameObject prefab, System.Func<bool> createCondition)
    {
        if (createCondition())
        {
            Debug.Log($"Creating {systemName}...");

            GameObject systemObject;

            if (prefab != null)
            {
                systemObject = Instantiate(prefab);
                systemObject.name = systemName;
            }
            else
            {
                systemObject = new GameObject(systemName);
                // Add the component based on the system name
                switch (systemName)
                {
                    case "WasteItemDatabase":
                        systemObject.AddComponent<WasteItemDatabase>();
                        break;
                    case "ResourceManager":
                        systemObject.AddComponent<ResourceManager>();
                        break;
                    case "FacilityManager":
                        systemObject.AddComponent<FacilityManager>();
                        break;
                    case "WasteInventoryManager":
                        systemObject.AddComponent<WasteInventoryManager>();
                        break;
                    case "GameManager":
                        systemObject.AddComponent<GameManager>();
                        break;
                    case "UIManager":
                        systemObject.AddComponent<UIManager>();
                        break;
                }
            }

            DontDestroyOnLoad(systemObject);

            yield return new WaitForSeconds(delayBetweenSystems);
        }
        else
        {
            Debug.Log($"{systemName} already exists, skipping creation.");
        }
    }

    /// <summary>
    /// Manually trigger initialization
    /// </summary>
    public void StartInitialization()
    {
        if (!initializeOnStart)
        {
            StartCoroutine(InitializeGameSystems());
        }
    }
}