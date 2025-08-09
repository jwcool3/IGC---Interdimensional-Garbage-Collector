using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace ResourceSystem.UI
{
    /// <summary>
    /// Displays individual resource information in the resource inventory UI
    /// Shows icon, name, amount, storage, and handles visual effects for changes
    /// </summary>
    public class ResourceDisplayItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image background;
        [SerializeField] private Image resourceIcon;
        [SerializeField] private TextMeshProUGUI resourceNameText;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI maxStorageText;
        [SerializeField] private Slider storageBar;
        [SerializeField] private ParticleSystem changeEffect;
        
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color lowStorageColor = Color.red;
        [SerializeField] private float highlightDuration = 1f;
        [SerializeField] private float storageWarningThreshold = 0.9f;
        
        [Header("Animation Settings")]
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 0.3f, 1.1f);
        [SerializeField] private float animationDuration = 0.5f;
        
        // Current state
        private ResourceType currentResourceType;
        private int currentAmount;
        private int maxStorage;
        private ResourceConfig resourceConfig;
        private bool isHighlighted = false;
        
        // Animation
        private Coroutine highlightCoroutine;
        private Coroutine scaleAnimationCoroutine;
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the display with resource data
        /// </summary>
        public void Initialize(ResourceType resourceType, int amount, ResourceConfig config)
        {
            currentResourceType = resourceType;
            currentAmount = amount;
            resourceConfig = config;
            
            // Get max storage from ResourceManager
            if (ResourceManager.Instance != null)
            {
                maxStorage = ResourceManager.Instance.GetStorageLimit(resourceType);
            }
            else
            {
                maxStorage = 1000; // Default fallback
            }
            
            UpdateDisplay();
            SetupInteractions();
        }
        
        /// <summary>
        /// Update the displayed amount with visual feedback
        /// </summary>
        public void UpdateAmount(int newAmount)
        {
            int previousAmount = currentAmount;
            currentAmount = newAmount;
            
            UpdateDisplay();
            
            // Show change effects
            if (newAmount != previousAmount)
            {
                ShowChangeEffect(newAmount > previousAmount);
                PlayScaleAnimation();
            }
        }
        
        /// <summary>
        /// Highlight this resource display (useful for tutorials or notifications)
        /// </summary>
        public void Highlight(float duration = -1f)
        {
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
            }
            
            float actualDuration = duration > 0 ? duration : highlightDuration;
            highlightCoroutine = StartCoroutine(HighlightAnimation(actualDuration));
        }
        
        /// <summary>
        /// Remove highlight effect
        /// </summary>
        public void RemoveHighlight()
        {
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
                highlightCoroutine = null;
            }
            
            isHighlighted = false;
            UpdateBackgroundColor();
        }
        
        #endregion
        
        #region Private Methods
        
        private void UpdateDisplay()
        {
            // Update icon
            if (resourceIcon != null && resourceConfig?.icon != null)
            {
                resourceIcon.sprite = resourceConfig.icon;
                resourceIcon.color = Color.white;
            }
            else if (resourceIcon != null)
            {
                // Use default icon or show placeholder
                resourceIcon.color = Color.gray;
            }
            
            // Update name
            if (resourceNameText != null)
            {
                string displayName = resourceConfig?.displayName ?? currentResourceType.ToString();
                resourceNameText.text = displayName;
                
                // Color based on resource config
                if (resourceConfig != null)
                {
                    resourceNameText.color = resourceConfig.resourceColor;
                }
            }
            
            // Update amount text
            if (amountText != null)
            {
                amountText.text = FormatAmount(currentAmount);
            }
            
            // Update max storage text
            if (maxStorageText != null)
            {
                maxStorageText.text = $"/ {FormatAmount(maxStorage)}";
            }
            
            // Update storage bar
            if (storageBar != null)
            {
                float storageRatio = maxStorage > 0 ? (float)currentAmount / maxStorage : 0f;
                storageBar.value = storageRatio;
                
                // Change bar color based on storage level
                Image fillImage = storageBar.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (storageRatio >= storageWarningThreshold)
                    {
                        fillImage.color = lowStorageColor;
                    }
                    else if (storageRatio >= 0.7f)
                    {
                        fillImage.color = Color.yellow;
                    }
                    else
                    {
                        fillImage.color = Color.green;
                    }
                }
            }
            
            UpdateBackgroundColor();
        }
        
        private void UpdateBackgroundColor()
        {
            if (background == null) return;
            
            if (isHighlighted)
            {
                background.color = highlightColor;
            }
            else
            {
                // Check if storage is nearly full
                float storageRatio = maxStorage > 0 ? (float)currentAmount / maxStorage : 0f;
                if (storageRatio >= storageWarningThreshold)
                {
                    background.color = Color.Lerp(normalColor, lowStorageColor, 0.3f);
                }
                else
                {
                    background.color = normalColor;
                }
            }
        }
        
        private string FormatAmount(int amount)
        {
            // Format large numbers with suffixes (K, M, etc.)
            if (amount >= 1000000)
            {
                return $"{amount / 1000000f:F1}M";
            }
            else if (amount >= 1000)
            {
                return $"{amount / 1000f:F1}K";
            }
            else
            {
                return amount.ToString();
            }
        }
        
        private void SetupInteractions()
        {
            // Add button component if not present
            Button button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }
            
            // Set up click handler for resource details
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnResourceClicked);
        }
        
        private void OnResourceClicked()
        {
            Debug.Log($"Clicked on resource: {currentResourceType} (Amount: {currentAmount})");
            
            // You can add more functionality here, such as:
            // - Show detailed resource information panel
            // - Open crafting recipes that use this resource
            // - Show resource source information
            // - etc.
            
            // For now, just highlight the resource
            Highlight();
        }
        
        #endregion
        
        #region Visual Effects
        
        private void ShowChangeEffect(bool isIncrease)
        {
            if (changeEffect == null) return;
            
            // Configure particle effect based on increase/decrease
            var main = changeEffect.main;
            if (isIncrease)
            {
                main.startColor = Color.green;
            }
            else
            {
                main.startColor = Color.red;
            }
            
            // Play the effect
            changeEffect.gameObject.SetActive(true);
            changeEffect.Play();
            
            // Auto-disable after playing
            StartCoroutine(DisableEffectAfterPlay());
        }
        
        private IEnumerator DisableEffectAfterPlay()
        {
            yield return new WaitForSeconds(changeEffect.main.duration);
            if (changeEffect != null)
            {
                changeEffect.gameObject.SetActive(false);
            }
        }
        
        private void PlayScaleAnimation()
        {
            if (scaleAnimationCoroutine != null)
            {
                StopCoroutine(scaleAnimationCoroutine);
            }
            
            scaleAnimationCoroutine = StartCoroutine(ScaleAnimation());
        }
        
        private IEnumerator ScaleAnimation()
        {
            Vector3 originalScale = transform.localScale;
            float elapsedTime = 0f;
            
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                float scaleMultiplier = scaleCurve.Evaluate(progress);
                
                transform.localScale = originalScale * scaleMultiplier;
                yield return null;
            }
            
            transform.localScale = originalScale;
            scaleAnimationCoroutine = null;
        }
        
        private IEnumerator HighlightAnimation(float duration)
        {
            isHighlighted = true;
            UpdateBackgroundColor();
            
            yield return new WaitForSeconds(duration);
            
            isHighlighted = false;
            UpdateBackgroundColor();
            highlightCoroutine = null;
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void OnDestroy()
        {
            // Clean up coroutines
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
            }
            
            if (scaleAnimationCoroutine != null)
            {
                StopCoroutine(scaleAnimationCoroutine);
            }
        }
        
        #endregion
    }
}