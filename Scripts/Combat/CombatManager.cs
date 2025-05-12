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
    private float timeBetweenAttacks = 3f;
    
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
        }
    }
    
    private void CreateZones()
    {
        // Create sample zones with increasing difficulty
        availableZones.Add(new CombatZone("Alpha Sector", 1, 10));
        availableZones.Add(new CombatZone("Beta Sector", 2, 10));
        availableZones.Add(new CombatZone("Gamma Sector", 3, 15));
        availableZones.Add(new CombatZone("Delta Sector", 4, 15));
        
        // Lock all zones except the first
        for (int i = 1; i < availableZones.Count; i++)
        {
            availableZones[i].isLocked = true;
        }
    }
    
    public void SpawnEnemy()
    {
        // Create a new enemy based on current zone
        float enemyLevel = currentZone.zoneLevel + (enemiesDefeatedInZone * 0.2f);
        
        // Boss enemy every 10 enemies
        bool isBoss = (enemiesDefeatedInZone + 1) % 10 == 0;
        
        if (isBoss)
        {
            currentEnemy = new EnemyShip("Zone Boss", enemyLevel * 1.5f, EnemyType.Boss);
        }
        else
        {
            currentEnemy = new EnemyShip(GetRandomEnemyName(), enemyLevel, GetRandomEnemyType());
        }
        
        // Notify UI of new enemy
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
        
        // Update UI after enemy attack
        CombatUI.Instance?.UpdateCombatDisplay();
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
    
    public bool TakeDamage(float damage)
    {
        // Apply defense reduction
        float reducedDamage = Mathf.Max(1, damage - defense * 0.5f);
        
        // Apply damage
        currentHP -= reducedDamage;
        
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
    
    private void ProcessDefeat()
    {
        // Stop auto combat
        autoCombatEnabled = false;
        
        // Restore some HP
        currentHP = maxHP * 0.5f;
        
        // Notify UI
        CombatUI.Instance?.ShowDefeat();
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
    public void ToggleAutoCombat()
    {
        autoCombatEnabled = !autoCombatEnabled;
        CombatUI.Instance?.UpdateAutoButtonState(autoCombatEnabled);
    }
    
    public void ManualAttack()
    {
        PerformAttack();
    }
    
    public void ChangeZone(int zoneIndex)
    {
        if (zoneIndex < 0 || zoneIndex >= availableZones.Count)
            return;
            
        if (availableZones[zoneIndex].isLocked)
            return;
            
        currentZone = availableZones[zoneIndex];
        enemiesDefeatedInZone = 0;
        SpawnEnemy();
        
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
    
    private EnemyType GetRandomEnemyType()
    {
        // Weighted random type
        float roll = UnityEngine.Random.value;
        
        if (roll < 0.6f)
            return EnemyType.Scavenger;
        else if (roll < 0.85f)
            return EnemyType.Rival;
        else
            return EnemyType.Anomaly;
    }
}