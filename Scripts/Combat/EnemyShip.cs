public enum EnemyType
{
    Scavenger,
    Rival,
    Anomaly,
    Boss
}

public class EnemyShip
{
    public string name;           // Display name (for UI)
    public string shipModelName;  // Specific ship model name (for icon lookup)
    public float level;
    public EnemyType type;
    public int iconVariation; // Track which icon variation this enemy uses
    public int sectorNumber; // Add sector number
    
    // Combat stats
    public float maxHP;
    public float currentHP;
    public float attackPower;
    public float defense;
    
    /// <summary>
    /// Constructor with ship model name
    /// </summary>
    public EnemyShip(string displayName, string shipModelName, float level, EnemyType type, int sector)
    {
        this.name = displayName;
        this.shipModelName = shipModelName;
        this.level = level;
        this.type = type;
        this.sectorNumber = sector;
        
        // Calculate stats based on level and type
        CalculateStats();
    }
    
    /// <summary>
    /// Constructor for backward compatibility
    /// </summary>
    public EnemyShip(string displayName, float level, EnemyType type)
    {
        this.name = displayName;
        this.level = level;
        this.type = type;
        this.sectorNumber = Mathf.RoundToInt(level);
        
        // Use display name as ship model name by default
        this.shipModelName = displayName.Replace(" ", "");
        
        // Calculate stats based on level and type
        CalculateStats();
    }
    
    private void CalculateStats()
    {
        // Base stats from level
        maxHP = level * 20f;
        attackPower = level * 5f;
        defense = level * 2f;
        
        // Modify based on enemy type
        switch (type)
        {
            case EnemyType.Scavenger:
                // Low defense, medium HP, medium attack
                defense *= 0.8f;
                break;
                
            case EnemyType.Rival:
                // Balanced stats
                attackPower *= 1.2f;
                break;
                
            case EnemyType.Anomaly:
                // High attack, low HP
                attackPower *= 1.5f;
                maxHP *= 0.8f;
                break;
                
            case EnemyType.Boss:
                // High everything
                maxHP *= 2.5f;
                attackPower *= 1.8f;
                defense *= 1.5f;
                break;
        }
        
        // Set current HP to max
        currentHP = maxHP;
    }
    
    public float CalculateDamage()
    {
        // Base damage
        float damage = attackPower;
        
        // Apply random variance (+-10%)
        damage *= UnityEngine.Random.Range(0.9f, 1.1f);
        
        return damage;
    }
    
    public bool TakeDamage(float damage)
    {
        // Apply defense reduction
        float reducedDamage = Mathf.Max(1, damage - defense * 0.3f);
        
        // Apply damage
        currentHP -= reducedDamage;
        
        // Check if defeated
        if (currentHP <= 0)
        {
            currentHP = 0;
            return true;
        }
        
        return false;
    }
}