using UnityEngine;
using UnityEngine.UI;

public class UILayoutManager : MonoBehaviour
{
    [Header("Layout References")]
    [SerializeField] private RectTransform mainCanvas;
    [SerializeField] private RectTransform headerPanel;
    [SerializeField] private RectTransform contentArea;
    [SerializeField] private float headerHeight = 80f;
    [SerializeField] private float minWidth = 800f;
    [SerializeField] private float minHeight = 600f;

    [Header("Grid Settings")]
    [SerializeField] private GridLayoutGroup artifactGrid;
    [SerializeField] private GridLayoutGroup upgradeGrid;
    [SerializeField] private float gridSpacing = 10f;
    [SerializeField] private Vector2 artifactCellSize = new Vector2(200f, 250f);
    [SerializeField] private Vector2 upgradeCellSize = new Vector2(300f, 150f);

    private void Start()
    {
        SetupLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        SetupLayout();
    }

    private void SetupLayout()
    {
        if (mainCanvas == null) return;

        // Get screen dimensions
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        float width = Mathf.Max(screenSize.x, minWidth);
        float height = Mathf.Max(screenSize.y, minHeight);

        // Set canvas size
        mainCanvas.sizeDelta = new Vector2(width, height);

        // Setup header
        if (headerPanel != null)
        {
            headerPanel.sizeDelta = new Vector2(width, headerHeight);
            headerPanel.anchoredPosition = new Vector2(0, height - headerHeight);
        }

        // Setup content area
        if (contentArea != null)
        {
            contentArea.sizeDelta = new Vector2(width, height - headerHeight);
            contentArea.anchoredPosition = Vector2.zero;
        }

        // Setup grids
        SetupGrid(artifactGrid, artifactCellSize);
        SetupGrid(upgradeGrid, upgradeCellSize);
    }

    private void SetupGrid(GridLayoutGroup grid, Vector2 cellSize)
    {
        if (grid == null) return;

        grid.spacing = Vector2.one * gridSpacing;
        grid.cellSize = cellSize;

        // Calculate columns based on available width
        float availableWidth = mainCanvas.rect.width - (grid.padding.left + grid.padding.right);
        int columns = Mathf.Max(1, Mathf.FloorToInt((availableWidth + grid.spacing.x) / (cellSize.x + grid.spacing.x)));
        
        grid.constraintCount = columns;
    }

    public void RefreshLayout()
    {
        SetupLayout();
        
        // Force update layout
        if (artifactGrid != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(artifactGrid.GetComponent<RectTransform>());
        
        if (upgradeGrid != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(upgradeGrid.GetComponent<RectTransform>());
    }

    // Helper method to get ideal size for new UI elements
    public Vector2 GetIdealSize(UIElementType elementType)
    {
        switch (elementType)
        {
            case UIElementType.ArtifactCard:
                return artifactCellSize;
            case UIElementType.UpgradeCard:
                return upgradeCellSize;
            case UIElementType.HeaderButton:
                return new Vector2(120f, headerHeight - 20f);
            default:
                return Vector2.zero;
        }
    }

    public enum UIElementType
    {
        ArtifactCard,
        UpgradeCard,
        HeaderButton,
        ResourceDisplay
    }
}

// Extension method for easy access to layout settings
public static class UILayoutExtensions
{
    public static void ApplyIdealSize(this RectTransform rect, UILayoutManager.UIElementType elementType)
    {
        if (UILayoutManager.Instance != null)
        {
            rect.sizeDelta = UILayoutManager.Instance.GetIdealSize(elementType);
        }
    }
} 