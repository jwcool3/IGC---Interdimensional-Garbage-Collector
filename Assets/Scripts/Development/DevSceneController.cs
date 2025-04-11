using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DevSceneController : MonoBehaviour
{
    [Header("Debug Controls")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private TMP_InputField debugInput;
    [SerializeField] private Button executeButton;
    [SerializeField] private TextMeshProUGUI debugLog;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.F1;

    [Header("Quick Actions")]
    [SerializeField] private Button addResourcesButton;
    [SerializeField] private Button generate10ArtifactsButton;
    [SerializeField] private Button maxStorageUpgradeButton;
    [SerializeField] private Button maxAllUpgradesButton;

    private void Start()
    {
        SetupDebugControls();
        SetupQuickActions();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleDebugKey))
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }
    }

    private void SetupDebugControls()
    {
        if (executeButton != null)
        {
            executeButton.onClick.AddListener(ExecuteDebugCommand);
        }
    }

    private void SetupQuickActions()
    {
        if (addResourcesButton != null)
            addResourcesButton.onClick.AddListener(() => {
                FacilityManager.Instance.AddDRP(5000f);
                FacilityManager.Instance.AddQuantumPotential(250f);
                LogDebug("Added resources: 5000 DRP, 250 QP");
            });

        if (generate10ArtifactsButton != null)
            generate10ArtifactsButton.onClick.AddListener(() => {
                for (int i = 0; i < 10; i++)
                    GameManager.Instance.CollectArtifact();
                LogDebug("Generated 10 artifacts");
            });

        if (maxStorageUpgradeButton != null)
            maxStorageUpgradeButton.onClick.AddListener(() => {
                MaxUpgrade("ArtifactStorage");
                LogDebug("Maxed Storage Upgrade");
            });

        if (maxAllUpgradesButton != null)
            maxAllUpgradesButton.onClick.AddListener(() => {
                foreach (var upgrade in FacilityManager.Instance.GetAllUpgrades())
                {
                    MaxUpgrade(upgrade.Key);
                }
                LogDebug("Maxed all upgrades");
            });
    }

    private void ExecuteDebugCommand()
    {
        if (string.IsNullOrEmpty(debugInput.text)) return;

        string[] cmd = debugInput.text.Split(' ');
        switch (cmd[0].ToLower())
        {
            case "help":
                ShowHelp();
                break;
            case "adddrp":
                if (cmd.Length > 1 && float.TryParse(cmd[1], out float drp))
                {
                    FacilityManager.Instance.AddDRP(drp);
                    LogDebug($"Added {drp} DRP");
                }
                break;
            case "addqp":
                if (cmd.Length > 1 && float.TryParse(cmd[1], out float qp))
                {
                    FacilityManager.Instance.AddQuantumPotential(qp);
                    LogDebug($"Added {qp} QP");
                }
                break;
            case "artifacts":
                if (cmd.Length > 1 && int.TryParse(cmd[1], out int count))
                {
                    for (int i = 0; i < count; i++)
                        GameManager.Instance.CollectArtifact();
                    LogDebug($"Generated {count} artifacts");
                }
                break;
            case "maxupgrade":
                if (cmd.Length > 1)
                {
                    MaxUpgrade(cmd[1]);
                    LogDebug($"Maxed upgrade: {cmd[1]}");
                }
                break;
            case "clear":
                debugLog.text = "";
                break;
            default:
                LogDebug($"Unknown command: {cmd[0]}");
                break;
        }

        debugInput.text = "";
    }

    private void MaxUpgrade(string upgradeName)
    {
        var upgrade = FacilityManager.Instance.GetUpgrade(upgradeName);
        if (upgrade == null) return;

        while (upgrade.CurrentLevel < upgrade.MaxLevel)
        {
            FacilityManager.Instance.AddDRP(upgrade.CurrentDRPCost);
            FacilityManager.Instance.AddQuantumPotential(upgrade.CurrentQuantumPotentialCost);
            FacilityManager.Instance.TryUpgradeFacility(upgradeName);
        }
    }

    private void ShowHelp()
    {
        string help = "Available Commands:\n" +
                     "help - Show this help\n" +
                     "adddrp <amount> - Add DRP\n" +
                     "addqp <amount> - Add Quantum Potential\n" +
                     "artifacts <count> - Generate artifacts\n" +
                     "maxupgrade <name> - Max specific upgrade\n" +
                     "clear - Clear debug log";
        LogDebug(help);
    }

    private void LogDebug(string message)
    {
        if (debugLog != null)
        {
            debugLog.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}\n" + debugLog.text;
        }
        Debug.Log($"[DevTools] {message}");
    }

    private void OnDestroy()
    {
        if (executeButton != null) executeButton.onClick.RemoveAllListeners();
        if (addResourcesButton != null) addResourcesButton.onClick.RemoveAllListeners();
        if (generate10ArtifactsButton != null) generate10ArtifactsButton.onClick.RemoveAllListeners();
        if (maxStorageUpgradeButton != null) maxStorageUpgradeButton.onClick.RemoveAllListeners();
        if (maxAllUpgradesButton != null) maxAllUpgradesButton.onClick.RemoveAllListeners();
    }
} 