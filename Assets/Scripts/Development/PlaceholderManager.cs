using UnityEngine;
using UnityEngine.UI;

public class PlaceholderManager : MonoBehaviour
{
    [Header("Placeholder Colors")]
    [SerializeField] private Color[] dimensionColors;
    [SerializeField] private Color commonColor = Color.gray;
    [SerializeField] private Color uncommonColor = Color.green;
    [SerializeField] private Color rareColor = Color.blue;
    [SerializeField] private Color legendaryColor = Color.yellow;

    [Header("Placeholder Shapes")]
    [SerializeField] private Sprite[] artifactShapes;
    [SerializeField] private Sprite[] upgradeIcons;

    private static PlaceholderManager instance;
    public static PlaceholderManager Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializePlaceholders();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePlaceholders()
    {
        if (dimensionColors == null || dimensionColors.Length == 0)
        {
            dimensionColors = new Color[]
            {
                new Color(0.8f, 0.2f, 0.2f), // Technological
                new Color(0.2f, 0.2f, 0.8f), // Magical
                new Color(0.2f, 0.8f, 0.2f), // Biological
                new Color(0.8f, 0.8f, 0.2f), // Philosophical
                new Color(0.8f, 0.2f, 0.8f)  // Quantum
            };
        }

        // Create basic shapes if none assigned
        if (artifactShapes == null || artifactShapes.Length == 0)
        {
            CreateBasicShapes();
        }
    }

    private void CreateBasicShapes()
    {
        // Create a basic set of procedural sprites
        artifactShapes = new Sprite[5];
        upgradeIcons = new Sprite[4];

        // Create basic shapes (you'll replace these with actual sprites)
        for (int i = 0; i < artifactShapes.Length; i++)
        {
            artifactShapes[i] = CreateProceduralSprite($"ArtifactShape_{i}", 64, i);
        }

        for (int i = 0; i < upgradeIcons.Length; i++)
        {
            upgradeIcons[i] = CreateProceduralSprite($"UpgradeIcon_{i}", 32, i + artifactShapes.Length);
        }
    }

    private Sprite CreateProceduralSprite(string name, int size, int shapeIndex)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];

        // Fill with different patterns based on shapeIndex
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float centerX = size * 0.5f;
                float centerY = size * 0.5f;
                float dx = x - centerX;
                float dy = y - centerY;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);

                Color color = Color.white;
                switch (shapeIndex % 5)
                {
                    case 0: // Circle
                        color = distance < size * 0.4f ? Color.white : Color.clear;
                        break;
                    case 1: // Diamond
                        color = (Mathf.Abs(dx) + Mathf.Abs(dy)) < size * 0.4f ? Color.white : Color.clear;
                        break;
                    case 2: // Star
                        float starPoint = Mathf.Abs(Mathf.Sin(angle * 5f));
                        color = distance < size * (0.2f + starPoint * 0.2f) ? Color.white : Color.clear;
                        break;
                    case 3: // Square
                        color = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) < size * 0.4f ? Color.white : Color.clear;
                        break;
                    case 4: // Hexagon
                        float hexAngle = Mathf.Abs(Mathf.Sin(angle * 6f));
                        color = distance < size * (0.3f + hexAngle * 0.1f) ? Color.white : Color.clear;
                        break;
                }

                colors[y * size + x] = color;
            }
        }

        tex.SetPixels(colors);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public Color GetDimensionColor(string dimensionType)
    {
        if (string.IsNullOrEmpty(dimensionType)) return Color.white;

        switch (dimensionType.ToLower())
        {
            case "technological": return dimensionColors[0];
            case "magical": return dimensionColors[1];
            case "biological": return dimensionColors[2];
            case "philosophical": return dimensionColors[3];
            case "quantum divergent": return dimensionColors[4];
            default: return Color.white;
        }
    }

    public Color GetRarityColor(float rarityScore)
    {
        if (rarityScore >= 1.5f) return legendaryColor;
        if (rarityScore >= 1.0f) return rareColor;
        if (rarityScore >= 0.5f) return uncommonColor;
        return commonColor;
    }

    public Sprite GetArtifactShape(string dimensionType)
    {
        if (artifactShapes == null || artifactShapes.Length == 0)
            return null;

        int index = Mathf.Abs(dimensionType.GetHashCode()) % artifactShapes.Length;
        return artifactShapes[index];
    }

    public Sprite GetUpgradeIcon(string upgradeName)
    {
        if (upgradeIcons == null || upgradeIcons.Length == 0)
            return null;

        int index = Mathf.Abs(upgradeName.GetHashCode()) % upgradeIcons.Length;
        return upgradeIcons[index];
    }

    private void OnDestroy()
    {
        // Clean up procedural textures
        if (artifactShapes != null)
        {
            foreach (var sprite in artifactShapes)
            {
                if (sprite != null)
                    Destroy(sprite.texture);
            }
        }

        if (upgradeIcons != null)
        {
            foreach (var sprite in upgradeIcons)
            {
                if (sprite != null)
                    Destroy(sprite.texture);
            }
        }
    }
} 