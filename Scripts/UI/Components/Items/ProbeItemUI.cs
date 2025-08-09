using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProbeItemUI : MonoBehaviour
{
    // Match your prefab structure
    [SerializeField] private TextMeshProUGUI probeName;      // ProbeItemPrefab/ProbeInfo/ProbeName
    [SerializeField] private TextMeshProUGUI locationText;   // ProbeItemPrefab/ProbeInfo/LocationText
    [SerializeField] private TextMeshProUGUI levelText;      // ProbeItemPrefab/ProbeInfo/LevelText
    [SerializeField] private TextMeshProUGUI rateText;       // ProbeItemPrefab/ProbeInfo/RateText
    [SerializeField] private Button upgradeButton;           // ProbeItemPrefab/ButtonsContainer/UpgradeButton
    [SerializeField] private Button recallButton;            // ProbeItemPrefab/ButtonsContainer/RecallButton

    private Probe probeData;

    private void Awake()
    {
        // Find components if not assigned
        if (probeName == null)
            probeName = transform.Find("ProbeInfo/ProbeName")?.GetComponent<TextMeshProUGUI>();

        if (locationText == null)
            locationText = transform.Find("ProbeInfo/LocationText")?.GetComponent<TextMeshProUGUI>();

        if (levelText == null)
            levelText = transform.Find("ProbeInfo/LevelText")?.GetComponent<TextMeshProUGUI>();

        if (rateText == null)
            rateText = transform.Find("ProbeInfo/RateText")?.GetComponent<TextMeshProUGUI>();

        if (upgradeButton == null)
            upgradeButton = transform.Find("ButtonsContainer/UpgradeButton")?.GetComponent<Button>();

        if (recallButton == null)
            recallButton = transform.Find("ButtonsContainer/RecallButton")?.GetComponent<Button>();
    }

    public void Initialize(Probe probe)
    {
        probeData = probe;

        if (probeName != null)
            probeName.text = $"Probe #{probe.probeId.Substring(0, 4)}";

        if (locationText != null)
            locationText.text = probe.location.displayName;

        if (levelText != null)
            levelText.text = $"Level {probe.level}";

        if (rateText != null)
            rateText.text = $"Rate: {probe.collectionRate:F1}/min";

        // Set up button listeners
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeClicked);

        if (recallButton != null)
            recallButton.onClick.AddListener(OnRecallClicked);
    }

    private void OnUpgradeClicked()
    {
        // Check if we can afford the upgrade
        bool canUpgrade = ResourceManager.Instance.RecyclingPoints >= GetUpgradeCost();

        if (canUpgrade)
        {
            // Pay the cost
            ResourceManager.Instance.SpendRecyclingPoints(GetUpgradeCost());

            // Upgrade the probe
            ProbeManager.Instance.UpgradeProbe(probeData.probeId);

            // Update UI
            levelText.text = $"Level {probeData.level}";
            rateText.text = $"Rate: {probeData.collectionRate:F1}/min";
        }
    }

    private float GetUpgradeCost()
    {
        // Simple formula: 100 * probe level
        return 100f * probeData.level;
    }

    private void OnRecallClicked()
    {
        ProbeManager.Instance.RecallProbe(probeData.probeId);
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);

        if (recallButton != null)
            recallButton.onClick.RemoveListener(OnRecallClicked);
    }
}