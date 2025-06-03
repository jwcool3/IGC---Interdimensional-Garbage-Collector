using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TabTransition : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float fadeSpeed = 0.3f;
    [SerializeField] private float slideDistance = 50f;
    
    private void Awake()
    {
        // Get components if not assigned
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }
    
    public void ShowTab()
    {
        // Stop any running animations
        StopAllCoroutines();
        
        // Start fade in animation
        StartCoroutine(FadeIn());
    }
    
    public void HideTab()
    {
        // Stop any running animations
        StopAllCoroutines();
        
        // Start fade out animation
        StartCoroutine(FadeOut());
    }
    
    private IEnumerator FadeIn()
    {
        // Reset initial state
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = new Vector2(slideDistance, 0);
        gameObject.SetActive(true);
        
        // Animate fade in and slide
        float elapsedTime = 0f;
        while (elapsedTime < fadeSpeed)
        {
            float t = elapsedTime / fadeSpeed;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(slideDistance, 0),
                Vector2.zero,
                t
            );
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final state
        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    private IEnumerator FadeOut()
    {
        // Start from current state
        float elapsedTime = 0f;
        Vector2 startPosition = rectTransform.anchoredPosition;
        float startAlpha = canvasGroup.alpha;
        
        // Animate fade out and slide
        while (elapsedTime < fadeSpeed)
        {
            float t = elapsedTime / fadeSpeed;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            rectTransform.anchoredPosition = Vector2.Lerp(
                startPosition,
                new Vector2(-slideDistance, 0),
                t
            );
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final state
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}