using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IconTester : MonoBehaviour
{
    public Image testImage;
    public TMP_Dropdown enemyTypeDropdown;
    public TMP_Dropdown sectorDropdown;
    
    private void Start()
    {
        // Populate enemy type dropdown
        enemyTypeDropdown.ClearOptions();
        foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType)))
        {
            enemyTypeDropdown.options.Add(new TMP_Dropdown.OptionData(type.ToString()));
        }
        
        // Populate sector dropdown
        sectorDropdown.ClearOptions();
        for (int i = 1; i <= 3; i++)
        {
            sectorDropdown.options.Add(new TMP_Dropdown.OptionData($"Sector {i}"));
        }
        
        // Set initial values
        enemyTypeDropdown.value = 0;
        sectorDropdown.value = 0;
        enemyTypeDropdown.RefreshShownValue();
        sectorDropdown.RefreshShownValue();
    }
    
    public void TestIcon()
    {
        EnemyType selectedType = (EnemyType)enemyTypeDropdown.value;
        int selectedSector = sectorDropdown.value + 1; // +1 because sectors start at 1
        
        if (EnemyIconManager.Instance != null)
        {
            Sprite icon = EnemyIconManager.Instance.GetIconForEnemy(selectedType, selectedSector, 0);
            
            if (icon != null)
            {
                testImage.sprite = icon;
                testImage.color = Color.white;
                Debug.Log($"Found icon for {selectedType} in Sector {selectedSector}");
            }
            else
            {
                testImage.color = EnemyIconManager.Instance.GetColorForEnemyType(selectedType);
                Debug.Log($"No icon found for {selectedType} in Sector {selectedSector}, using color fallback");
            }
        }
        else
        {
            Debug.LogError("EnemyIconManager instance not found!");
        }
    }
    
    public void TestShipModel()
    {
        if (ShipDatabase.Instance == null)
        {
            Debug.LogError("ShipDatabase instance not found!");
            return;
        }
        
        EnemyType selectedType = (EnemyType)enemyTypeDropdown.value;
        int selectedSector = sectorDropdown.value + 1;
        
        ShipModel model = ShipDatabase.Instance.GetRandomShipForTypeAndSector(selectedType, selectedSector);
        
        if (model != null && model.shipIcon != null)
        {
            testImage.sprite = model.shipIcon;
            testImage.color = Color.white;
            Debug.Log($"Found ship model: {model.modelName} for {selectedType} in Sector {selectedSector}");
        }
        else
        {
            Debug.Log($"No ship model icon found for {selectedType} in Sector {selectedSector}");
            
            // Try fallback to EnemyIconManager
            if (EnemyIconManager.Instance != null)
            {
                testImage.color = EnemyIconManager.Instance.GetColorForEnemyType(selectedType);
                Debug.Log("Using color fallback from EnemyIconManager");
            }
            else
            {
                testImage.color = Color.gray;
                Debug.Log("No fallback available, using gray");
            }
        }
    }
    
    public void OnEnemyTypeChanged(int value)
    {
        // Automatically test the new selection
        TestShipModel();
    }
    
    public void OnSectorChanged(int value)
    {
        // Automatically test the new selection
        TestShipModel();
    }
} 