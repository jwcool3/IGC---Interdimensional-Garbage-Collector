using UnityEngine;

/// <summary>
/// Test script for the scanning system - attach to any GameObject for testing
/// </summary>
public class ScannerTester : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private KeyCode commonScanKey = KeyCode.Q;
    [SerializeField] private KeyCode rareScanKey = KeyCode.E;
    [SerializeField] private KeyCode fightKey = KeyCode.F;
    [SerializeField] private KeyCode tradeKey = KeyCode.T;
    [SerializeField] private KeyCode clearShipKey = KeyCode.C;
    
    private void Update()
    {
        // Test scanner system with keyboard input
        TestScannerInput();
        TestInteractionInput();
        
        // Debug display
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowScannerInfo();
        }
    }
    
    private void TestScannerInput()
    {
        if (ShipScanner.Instance == null) return;
        
        // Common scan
        if (Input.GetKeyDown(commonScanKey))
        {
            Debug.Log("Testing common scan...");
            bool success = ShipScanner.Instance.TryScanForCommonShip();
            Debug.Log($"Common scan started: {success}");
        }
        
        // Rare scan
        if (Input.GetKeyDown(rareScanKey))
        {
            Debug.Log("Testing rare scan...");
            bool success = ShipScanner.Instance.TryScanForRareShip();
            Debug.Log($"Rare scan started: {success}");
        }
        
        // Clear ship
        if (Input.GetKeyDown(clearShipKey))
        {
            Debug.Log("Clearing current ship...");
            ShipScanner.Instance.ClearCurrentShip();
        }
    }
    
    private void TestInteractionInput()
    {
        if (ShipScanner.Instance?.HasDiscoveredShip != true) return;
        
        var ship = ShipScanner.Instance.CurrentShip;
        if (ship == null) return;
        
        // Test fight
        if (Input.GetKeyDown(fightKey))
        {
            Debug.Log($"Testing fight with {ship.ShipName}...");
            
            if (ShipInteractionManager.Instance != null)
            {
                bool success = ShipInteractionManager.Instance.StartCombatWithShip(ship);
                Debug.Log($"Combat started: {success}");
            }
            else
            {
                Debug.LogError("ShipInteractionManager not found!");
            }
        }
        
        // Test trade
        if (Input.GetKeyDown(tradeKey))
        {
            Debug.Log($"Testing trade with {ship.ShipName}...");
            
            if (ShipInteractionManager.Instance != null)
            {
                bool success = ShipInteractionManager.Instance.TryTradeWithShip(ship);
                Debug.Log($"Trade completed: {success}");
                
                if (!success)
                {
                    Debug.Log("Trade info: " + ShipInteractionManager.Instance.GetTradeInfoText(ship));
                }
            }
            else
            {
                Debug.LogError("ShipInteractionManager not found!");
            }
        }
    }
    
    private void ShowScannerInfo()
    {
        if (ShipScanner.Instance == null)
        {
            Debug.Log("ShipScanner instance not found!");
            return;
        }
        
        Debug.Log("=== SCANNER INFO ===");
        Debug.Log($"Energy: {ShipScanner.Instance.CurrentEnergy:F1}/{ShipScanner.Instance.MaxEnergy:F1}");
        Debug.Log($"Is Scanning: {ShipScanner.Instance.IsScanning}");
        Debug.Log($"Has Ship: {ShipScanner.Instance.HasDiscoveredShip}");
        
        if (ShipScanner.Instance.HasDiscoveredShip)
        {
            var ship = ShipScanner.Instance.CurrentShip;
            Debug.Log($"Current Ship: {ship.ShipName} ({ship.GetRarityDisplayText()})");
            Debug.Log($"Actions: {ship.GetAvailableActionsText()}");
            
            if (ShipInteractionManager.Instance != null)
            {
                Debug.Log("Combat Info: " + ShipInteractionManager.Instance.GetCombatInfoText(ship));
                Debug.Log("Trade Info: " + ShipInteractionManager.Instance.GetTradeInfoText(ship));
            }
        }
        
        Debug.Log($"Common Scan Cost: {ShipScanner.Instance.GetScanCost(true)}");
        Debug.Log($"Rare Scan Cost: {ShipScanner.Instance.GetScanCost(false)}");
        Debug.Log($"Common Success Rate: {ShipScanner.Instance.GetSuccessRate(true):P1}");
        Debug.Log($"Rare Success Rate: {ShipScanner.Instance.GetSuccessRate(false):P1}");
        
        // Show combat manager info
        if (CombatManager.Instance != null)
        {
            Debug.Log($"Player HP: {CombatManager.Instance.currentHP:F0}/{CombatManager.Instance.maxHP:F0}");
            Debug.Log($"Current Enemy: {(CombatManager.Instance.currentEnemy?.name ?? "None")}");
            Debug.Log($"Auto Combat: {CombatManager.Instance.autoCombatEnabled}");
        }
    }
    
    private void Start()
    {
        Debug.Log("Scanner Tester initialized!");
        Debug.Log($"Controls:");
        Debug.Log($"  {commonScanKey} - Common Scan");
        Debug.Log($"  {rareScanKey} - Rare Scan");
        Debug.Log($"  {fightKey} - Fight Current Ship");
        Debug.Log($"  {tradeKey} - Trade with Current Ship");
        Debug.Log($"  {clearShipKey} - Clear Current Ship");
        Debug.Log($"  I - Show Scanner Info");
    }
}