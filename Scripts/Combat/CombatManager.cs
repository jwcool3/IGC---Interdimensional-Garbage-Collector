using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    // Singleton pattern
    public static CombatManager Instance { get; private set; }
    
    // Combat properties
    public float attackPower = 10f;
    public float defense = 5f;
    public float maxHP = 100f;
    public float currentHP = 100f;
    public float criticalChance = 0.05f;
    public float attackSpeed = 1f;
    
    // Enemy reference
    public EnemyShip currentEnemy;
    
    // Zone management
    public List<CombatZone> availableZones = new List<CombatZone>();
    public CombatZone currentZone;
    public int enemiesDefeatedInZone = 0;
    
    // Combat state
    public bool autoCombatEnabled = true;
    private float combatTimer = 0f;
    private float timeBetweenAttacks = 1f;
    private float uiUpdateTimer = 0f;
    private float uiUpdateInterval = 0.2f; // Update UI 5 times per second
    
    // Combat rewards
    public int shipParts = 0;
    public int alienTech = 0;
    public int combatData = 0;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCombat();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeCombat()
    {
        // Set up initial zones
        CreateZones();
        
        // Set current zone to first zone
        currentZone = availableZones[0];
        
        // Spawn first enemy
        SpawnEnemy();
    }
    
    private void Update()
    {
        if (autoCombatEnabled && currentEnemy != null)
        {
            // Increment combat timer
            combatTimer += Time.deltaTime * attackSpeed;
            
            // Check if it's time to attack
            if (combatTimer >= timeBetweenAttacks)
            {
                PerformAttack();
                combatTimer = 0f;
            }
            
            // Update UI periodically
            uiUpdateTimer += Time.deltaTime;
            if (uiUpdateTimer >= uiUpdateInterval)
            {
                CombatUI.Instance?.UpdateCombatDisplay();
                CombatUI.Instance?.UpdateStatsDisplay();
                uiUpdateTimer = 0f;
            }
        }
    }
    
    private void CreateZones()
    {
        // Create sample zones with increasing difficulty and sector assignments
        availableZones.Add(new CombatZone("Alpha Sector", 1, 1, 10));
        availableZones.Add(new CombatZone("Beta Sector", 2, 1, 10)); // Still sector 1
        availableZones.Add(new CombatZone("Gamma Sector", 3, 2, 15)); // Sector 2
        availableZones.Add(new CombatZone("Delta Sector", 4, 2, 15)); // Sector 2
        
        // Lock all zones except the first
        for (int i = 1; i < availableZones.Count; i++)
        {
            availableZones[i].isLocked = true;
        }
    }
    
    /// <summary>
    /// Get a random enemy type based on current zone probabilities
    /// </summary>
    private EnemyType GetRandomEnemyType()
    {
        // Use zone-specific enemy distribution if available
        if (currentZone != null)
        {
            return currentZone.GetRandomEnemyType();
        }
        
        // Fallback to default distribution
        float roll = Random.value;
        
        if (roll < 0.6f)
            return EnemyType.Scavenger;
        else if (roll < 0.85f)
            return EnemyType.Rival;
        else
            return EnemyType.Anomaly;
    }
    
    /// <summary>
    /// Spawn a new enemy based on current zone settings
    /// </summary>
    public void SpawnEnemy()
    {
        if (currentZone == null) return;
        
        // Calculate enemy level based on zone and progress
        float enemyLevel = currentZone.zoneLevel + (enemiesDefeatedInZone * 0.2f);
        
        // Check if this should be a boss
        bool isBoss = (enemiesDefeatedInZone + 1) % 10 == 0;
        
        string shipModelName = "";
        EnemyType enemyType = isBoss ? EnemyType.Boss : GetRandomEnemyType();
        
        // Try to get a ship model from the database
        if (ShipDatabase.Instance != null)
        {
            ShipModel shipModel = ShipDatabase.Instance.GetRandomShipForTypeAndSector(enemyType, currentZone.sectorNumber);
            
            if (shipModel != null)
            {
                shipModelName = shipModel.modelName;
                Debug.Log($"Using ship model: {shipModelName} for {enemyType} in sector {currentZone.sectorNumber}");
            }
            else
            {
                Debug.LogWarning($"No ship model found for {enemyType} in sector {currentZone.sectorNumber}");
                // Use a fallback name based on type
                shipModelName = $"Default{enemyType}Ship";
            }
        }
        else
        {
            Debug.LogWarning("ShipDatabase not found, using default ship model names");
            // Use a fallback name based on type
            shipModelName = $"Default{enemyType}Ship";
        }
        
        // Generate enemy with the selected ship model
        if (isBoss)
        {
            currentEnemy = new EnemyShip(
                "Zone Boss",
                shipModelName,
                enemyLevel * 1.5f,
                EnemyType.Boss,
                currentZone.sectorNumber
            );
            
            Debug.Log($"Spawned boss enemy: {currentEnemy.name} with model {shipModelName}");
        }
        else
        {
            currentEnemy = new EnemyShip(
                GetRandomEnemyName(),
                shipModelName,
                enemyLevel,
                enemyType,
                currentZone.sectorNumber
            );
            
            Debug.Log($"Spawned regular enemy: {currentEnemy.name} with model {shipModelName}");
        }
        
        // Update UI to show the new enemy
        CombatUI.Instance?.UpdateEnemyDisplay();
    }
    
    /// <summary>
    /// Perform an attack on the current enemy
    /// </summary>
    public void PerformAttack()
    {
        if (currentEnemy == null) return;
        
        // Calculate damage
        float damage = CalculateDamage();
        
        // Show player attack animation
        CombatUI.Instance?.ShowAttackEffect(true);
        
        // Apply damage to enemy
        bool enemyDefeated = currentEnemy.TakeDamage(damage);
        
        // Show damage effect on enemy
        CombatUI.Instance?.ShowDamageEffect(false);
        
        // Process enemy attack if still alive
        if (!enemyDefeated)
        {
            // Short delay before enemy attacks back
            StartCoroutine(EnemyAttackAfterDelay(0.5f));
        }
        else
        {
            // Process victory
            ProcessVictory();
        }
        
        // Update UI
        CombatUI.Instance?.UpdateCombatDisplay();
    }
    
    /// <summary>
    /// Handle enemy's counter-attack after a delay
    /// </summary>
    private IEnumerator EnemyAttackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        float enemyDamage = currentEnemy.CalculateDamage();
        
        // Show enemy attack animation
        CombatUI.Instance?.ShowAttackEffect(false);
        
        // Apply damage to player
        TakeDamage(enemyDamage);
        
        // Show damage effect on player
        CombatUI.Instance?.ShowDamageEffect(true);
        
        // Update UI after enemy attack - make sure to update everything
        CombatUI.Instance?.UpdateAllDisplays();
    }
    
    private float CalculateDamage()
    {
        // Base damage
        float damage = attackPower;
        
        // Critical hit chance
        if (UnityEngine.Random.value < criticalChance)
        {
            damage *= 2f;
            CombatUI.Instance?.ShowCriticalHit();
        }
        
        // Apply random variance (+-10%)
        damage *= UnityEngine.Random.Range(0.9f, 1.1f);
        
        return damage;
    }
    
    /// <summary>
    /// Apply damage to the player
    /// </summary>
    public bool TakeDamage(float damage)
    {
        // Apply defense reduction
        float reducedDamage = Mathf.Max(1, damage - defense * 0.5f);
        
        // Apply damage
        currentHP -= reducedDamage;
        
        Debug.Log($"Took {reducedDamage:F1} damage (reduced from {damage:F1} by defense)");
        
        // Update UI immediately after taking damage
        CombatUI.Instance?.UpdateCombatDisplay();
        CombatUI.Instance?.UpdateStatsDisplay();
        
        // Check if defeated
        if (currentHP <= 0)
        {
            currentHP = 0;
            ProcessDefeat();
            return true;
        }
        
        return false;
    }
    
    private void ProcessVictory()
    {
        // Award resources
        GiveRewards(currentEnemy);
        
        // Increment enemy counter
        enemiesDefeatedInZone++;
        
        // Check for zone completion
        if (enemiesDefeatedInZone >= currentZone.enemiesInZone)
        {
            CompleteZone();
        }
        else
        {
            // Spawn next enemy
            SpawnEnemy();
        }
    }
    
    private void CompleteZone()
    {
        // Give zone completion bonus
        int zoneBonus = currentZone.zoneLevel * 50;
        shipParts += zoneBonus;
        
        // Unlock next zone if available
        int nextZoneIndex = availableZones.IndexOf(currentZone) + 1;
        if (nextZoneIndex < availableZones.Count)
        {
            availableZones[nextZoneIndex].isLocked = false;
        }
        
        // Reset zone progress
        enemiesDefeatedInZone = 0;
        
        // Notify UI
        CombatUI.Instance?.ShowZoneComplete(currentZone.zoneName);
    }
    
    /// <summary>
    /// Process player defeat
    /// </summary>
    private void ProcessDefeat()
    {
        Debug.Log("Player defeated!");
        
        // Disable auto-combat
        autoCombatEnabled = false;
        CombatUI.Instance?.UpdateAutoButtonState(false);
        
        // Show defeat effects and UI
        CombatUI.Instance?.ShowDefeat();
        CombatUI.Instance?.UpdateAllDisplays();
        
        // Return to first zone
        ChangeZone(0);
    }
    
    /// <summary>
    /// Distribute rewards after defeating an enemy
    /// </summary>
    private void GiveRewards(EnemyShip enemy)
    {
        if (enemy == null) return;
        
        // Calculate base reward based on enemy level
        int baseReward = Mathf.RoundToInt(enemy.level * 5);
        
        // Apply boss multiplier
        if (enemy.type == EnemyType.Boss)
        {
            baseReward *= 3;
        }
        
        // Give resources through ResourceManager
        if (ResourceManager.Instance != null)
        {
            // Combat resources
            ResourceManager.Instance.AddShipParts(baseReward);
            ResourceManager.Instance.AddAlienTech(Mathf.RoundToInt(baseReward * 0.3f));
            ResourceManager.Instance.AddCombatData(Mathf.RoundToInt(baseReward * 0.5f));
            
            // Core resources
            ResourceManager.Instance.AddRecyclingPoints(baseReward * 2);
            ResourceManager.Instance.AddDimensionalPotential(baseReward * 0.5f);
            
            Debug.Log($"Gave rewards for defeating {enemy.name} (Level {enemy.level}): " +
                     $"SP: {baseReward}, " +
                     $"AT: {Mathf.RoundToInt(baseReward * 0.3f)}, " +
                     $"CD: {Mathf.RoundToInt(baseReward * 0.5f)}, " +
                     $"RP: {baseReward * 2}, " +
                     $"DP: {baseReward * 0.5f}");
        }
    }
    
    // Public methods for UI interaction
    /// <summary>
    /// Toggle auto-combat mode
    /// </summary>
    public void ToggleAutoCombat()
    {
        autoCombatEnabled = !autoCombatEnabled;
        
        // Reset timers when enabling auto-combat
        if (autoCombatEnabled)
        {
            combatTimer = 0f;
            uiUpdateTimer = 0f;
        }
        
        // Update UI state
        CombatUI.Instance?.UpdateAutoButtonState(autoCombatEnabled);
        CombatUI.Instance?.UpdateAllDisplays();
        
        Debug.Log($"Auto-combat {(autoCombatEnabled ? "enabled" : "disabled")}");
    }
    
    public void ManualAttack()
    {
        PerformAttack();
    }
    
    /// <summary>
    /// Change to a different zone
    /// </summary>
    public void ChangeZone(int zoneIndex)
    {
        if (zoneIndex < 0 || zoneIndex >= availableZones.Count)
        {
            Debug.LogError($"Invalid zone index: {zoneIndex}");
            return;
        }
        
        CombatZone newZone = availableZones[zoneIndex];
        
        if (newZone.isLocked)
        {
            Debug.LogWarning($"Cannot change to locked zone: {newZone.zoneName}");
            return;
        }
        
        // Reset zone progress
        currentZone = newZone;
        enemiesDefeatedInZone = 0;
        
        // Validate zone probabilities
        currentZone.ValidateProbabilities();
        
        // Spawn first enemy in new zone
        SpawnEnemy();
        
        // Update UI
        CombatUI.Instance?.UpdateZoneDisplay();
    }
    
    // Helper methods
    private string GetRandomEnemyName()
    {
        string[] names = {
            "Scavenger Drone", "Waste Hauler", "Dimensional Parasite",
            "Rogue Collector", "Space Junk", "Anomalous Entity"
        };
        
        return names[UnityEngine.Random.Range(0, names.Length)];
    }
}