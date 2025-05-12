using UnityEngine;
[System.Serializable]
public class CombatZone
{
    public string zoneName;
    public int zoneLevel;
    public int enemiesInZone;
    public bool isLocked;
    
    public CombatZone(string name, int level, int enemies)
    {
        zoneName = name;
        zoneLevel = level;
        enemiesInZone = enemies;
        isLocked = false;
    }
}