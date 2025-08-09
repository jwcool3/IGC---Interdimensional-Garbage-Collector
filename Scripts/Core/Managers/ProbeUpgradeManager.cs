using UnityEngine;
using System.Collections.Generic;

public class ProbeUpgradeManager : MonoBehaviour
{
    public static ProbeUpgradeManager Instance { get; private set; }

    [System.Serializable]
    public class ProbeUpgradeOption
    {
        public string upgradeName;
        public string description;
        public float recyclingPointCost;
        public float dimensionalPotentialCost;
        public int maxLevel;
        public Sprite icon;
    }

    [Header("Probe Count Upgrade")]
    [SerializeField] private ProbeUpgradeOption probeCountUpgrade;
    [SerializeField] private float probeCountCostMultiplier = 1.5f;

    [Header("Probe Efficiency Upgrade")]
    [SerializeField] private ProbeUpgradeOption probeEfficiencyUpgrade;
    [SerializeField] private float efficiencyCostMultiplier = 1.4f;

    [Header("Probe Speed Upgrade")]
    [SerializeField] private ProbeUpgradeOption probeSpeedUpgrade;
    [SerializeField] private float speedCostMultiplier = 1.3f;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = true;

    // Track upgrade levels
    private Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Fix DontDestroyOnLoad warning
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            if (logDebugMessages)
                Debug.Log("ProbeUpgradeManager: Initialized singleton instance");

            // Initialize upgrades right away in Awake
            InitializeUpgrades();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (logDebugMessages)
        {
            Debug.Log("ProbeUpgradeManager: Start method called");

            // Log upgrade levels for debugging
            foreach (var upgrade in upgradeLevels)
            {
                Debug.Log($"ProbeUpgradeManager: Upgrade '{upgrade.Key}' is at level {upgrade.Value}");
            }
        }

        // Set default values if not set in the Inspector
        if (probeCountUpgrade == null)
        {
            probeCountUpgrade = new ProbeUpgradeOption
            {
                upgradeName = "More Probes",
                description = "Increase your maximum probe count",
                recyclingPointCost = 100f,
                dimensionalPotentialCost = 10f,
                maxLevel = 5
            };

            Debug.LogWarning("ProbeUpgradeManager: probeCountUpgrade was null, created default");
        }

        if (probeEfficiencyUpgrade == null)
        {
            probeEfficiencyUpgrade = new ProbeUpgradeOption
            {
                upgradeName = "Better Efficiency",
                description = "Increase the value of collected waste",
                recyclingPointCost = 150f,
                dimensionalPotentialCost = 15f,
                maxLevel = 5
            };

            Debug.LogWarning("ProbeUpgradeManager: probeEfficiencyUpgrade was null, created default");
        }

        if (probeSpeedUpgrade == null)
        {
            probeSpeedUpgrade = new ProbeUpgradeOption
            {
                upgradeName = "Faster Collection",
                description = "Decrease the collection interval",
                recyclingPointCost = 200f,
                dimensionalPotentialCost = 20f,
                maxLevel = 5
            };

            Debug.LogWarning("ProbeUpgradeManager: probeSpeedUpgrade was null, created default");
        }
    }

    private void InitializeUpgrades()
    {
        // Clear and Initialize upgrade levels
        upgradeLevels.Clear();

        // Ensure all keys are properly initialized
        upgradeLevels["ProbeCount"] = 1;      // Start with 1 probe
        upgradeLevels["Efficiency"] = 1;      // Base efficiency
        upgradeLevels["Speed"] = 1;           // Base speed

        if (logDebugMessages)
            Debug.Log("ProbeUpgradeManager: Upgrades initialized");
    }

    public bool CanAffordUpgrade(string upgradeType)
    {
        if (string.IsNullOrEmpty(upgradeType))
        {
            Debug.LogError("ProbeUpgradeManager: CanAffordUpgrade called with null or empty upgradeType");
            return false;
        }

        ProbeUpgradeOption upgrade = GetUpgradeByType(upgradeType);

        if (upgrade == null)
        {
            Debug.LogError($"ProbeUpgradeManager: No upgrade found for type '{upgradeType}'");
            return false;
        }

        int currentLevel = GetUpgradeLevel(upgradeType);

        if (currentLevel >= upgrade.maxLevel)
            return false;

        float currentRPCost = CalculateCurrentCost(upgradeType, upgrade.recyclingPointCost);
        float currentDPCost = CalculateCurrentCost(upgradeType, upgrade.dimensionalPotentialCost);

        if (ResourceManager.Instance == null)
        {
            Debug.LogError("ProbeUpgradeManager: ResourceManager.Instance is null");
            return false;
        }

        return ResourceManager.Instance.RecyclingPoints >= currentRPCost &&
               ResourceManager.Instance.DimensionalPotential >= currentDPCost;
    }

    public bool PurchaseUpgrade(string upgradeType)
    {
        if (string.IsNullOrEmpty(upgradeType))
        {
            Debug.LogError("ProbeUpgradeManager: PurchaseUpgrade called with null or empty upgradeType");
            return false;
        }

        if (!CanAffordUpgrade(upgradeType))
            return false;

        ProbeUpgradeOption upgrade = GetUpgradeByType(upgradeType);

        if (upgrade == null)
        {
            Debug.LogError($"ProbeUpgradeManager: No upgrade found for type '{upgradeType}'");
            return false;
        }

        int currentLevel = GetUpgradeLevel(upgradeType);

        float currentRPCost = CalculateCurrentCost(upgradeType, upgrade.recyclingPointCost);
        float currentDPCost = CalculateCurrentCost(upgradeType, upgrade.dimensionalPotentialCost);

        // Pay the cost
        ResourceManager.Instance.SpendRecyclingPoints(currentRPCost);
        ResourceManager.Instance.SpendDimensionalPotential(currentDPCost);

        // Increase level
        upgradeLevels[upgradeType] = currentLevel + 1;

        // Apply upgrade effects
        ApplyUpgradeEffects(upgradeType);

        if (logDebugMessages)
            Debug.Log($"ProbeUpgradeManager: Purchased {upgradeType} upgrade to level {upgradeLevels[upgradeType]}");

        return true;
    }

    private float CalculateCurrentCost(string upgradeType, float baseCost)
    {
        int currentLevel = GetUpgradeLevel(upgradeType);
        float multiplier = 1f;

        switch (upgradeType)
        {
            case "ProbeCount":
                multiplier = Mathf.Pow(probeCountCostMultiplier, currentLevel - 1);
                break;
            case "Efficiency":
                multiplier = Mathf.Pow(efficiencyCostMultiplier, currentLevel - 1);
                break;
            case "Speed":
                multiplier = Mathf.Pow(speedCostMultiplier, currentLevel - 1);
                break;
            default:
                Debug.LogWarning($"ProbeUpgradeManager: Unknown upgrade type '{upgradeType}' in CalculateCurrentCost");
                break;
        }

        return baseCost * multiplier;
    }

    private void ApplyUpgradeEffects(string upgradeType)
    {
        if (ProbeManager.Instance == null)
        {
            Debug.LogError("ProbeUpgradeManager: Cannot apply upgrade effects - ProbeManager.Instance is null");
            return;
        }

        switch (upgradeType)
        {
            case "ProbeCount":
                // Increase max probe count
                ProbeManager.Instance.IncreaseMaxProbes(1);
                if (logDebugMessages)
                    Debug.Log("ProbeUpgradeManager: Applied ProbeCount upgrade effect");
                break;

            case "Efficiency":
                // Improve probe efficiency - update all active probes
                var probes = ProbeManager.Instance.GetActiveProbes();
                foreach (var probe in probes)
                {
                    probe.efficiencyMultiplier = 1f + (GetUpgradeLevel("Efficiency") - 1) * 0.2f; // +20% per level
                }
                if (logDebugMessages)
                    Debug.Log("ProbeUpgradeManager: Applied Efficiency upgrade effect");
                break;

            case "Speed":
                // Improve collection speed by reducing interval
                // This will be handled automatically by ProbeManager's UpdateCollectionInterval method
                // Update the rate for all probes
                var activeProbes = ProbeManager.Instance.GetActiveProbes();
                foreach (var probe in activeProbes)
                {
                    probe.collectionRate *= 1.2f; // 20% faster per level
                }
                ProbeManager.Instance.UpdateCollectionInterval();
                if (logDebugMessages)
                    Debug.Log("ProbeUpgradeManager: Applied Speed upgrade effect");
                break;

            default:
                Debug.LogWarning($"ProbeUpgradeManager: Unknown upgrade type '{upgradeType}' in ApplyUpgradeEffects");
                break;
        }
    }

    private ProbeUpgradeOption GetUpgradeByType(string upgradeType)
    {
        if (string.IsNullOrEmpty(upgradeType))
        {
            Debug.LogError("ProbeUpgradeManager: GetUpgradeByType called with null or empty upgradeType");
            return null;
        }

        switch (upgradeType)
        {
            case "ProbeCount":
                return probeCountUpgrade;
            case "Efficiency":
                return probeEfficiencyUpgrade;
            case "Speed":
                return probeSpeedUpgrade;
            default:
                Debug.LogWarning($"ProbeUpgradeManager: Unknown upgrade type '{upgradeType}'");
                return null;
        }
    }

    public int GetUpgradeLevel(string upgradeType)
    {
        if (string.IsNullOrEmpty(upgradeType))
        {
            Debug.LogError("ProbeUpgradeManager: GetUpgradeLevel called with null or empty upgradeType");
            return 0;
        }

        // Make sure the dictionary contains the key
        if (!upgradeLevels.ContainsKey(upgradeType))
        {
            Debug.LogWarning($"ProbeUpgradeManager: The upgrade type '{upgradeType}' is not in upgradeLevels dictionary. Adding it with default level 1.");
            upgradeLevels[upgradeType] = 1;
        }

        return upgradeLevels[upgradeType];
    }

    public float GetUpgradeCost(string upgradeType, bool isDimensionalPotential = false)
    {
        if (string.IsNullOrEmpty(upgradeType))
        {
            Debug.LogError("ProbeUpgradeManager: GetUpgradeCost called with null or empty upgradeType");
            return 0f;
        }

        ProbeUpgradeOption upgrade = GetUpgradeByType(upgradeType);
        if (upgrade == null)
        {
            Debug.LogError($"ProbeUpgradeManager: No upgrade found for type '{upgradeType}'");
            return 0f;
        }

        float baseCost = isDimensionalPotential ?
            upgrade.dimensionalPotentialCost : upgrade.recyclingPointCost;

        return CalculateCurrentCost(upgradeType, baseCost);
    }

    public int GetMaxLevel(string upgradeType)
    {
        if (string.IsNullOrEmpty(upgradeType))
        {
            Debug.LogError("ProbeUpgradeManager: GetMaxLevel called with null or empty upgradeType");
            return 0;
        }

        ProbeUpgradeOption upgrade = GetUpgradeByType(upgradeType);
        return upgrade?.maxLevel ?? 0;
    }
}