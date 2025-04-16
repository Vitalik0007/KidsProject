using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class ColoringGameManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private GameObject levelBackgroundImg;
    [SerializeField] private GameObject levelMiddleLayerImg;
    [SerializeField] private GameObject coloringBook;
    [SerializeField] private GameObject example;
    [SerializeField] private List<ColoringColorButton> colorButtons;
    [SerializeField] private Color defaultButtonsColor;
    [SerializeField] private Color selectedButtonsColor;
    [SerializeField] private RectTransform colorPanel;
    private List<ColoringColorButton> buttonsClasses = new List<ColoringColorButton>();
    private List<Color> regionColors = new List<Color>();

    [Header("Settings")]
    [SerializeField] private int totalRounds;

    private int currentRound = 0;
    private int levelRegions;
    private ColoringLevelData coloringLevelData;
    private Color selectedColor;
    private Texture2D exampleTexture;

    [SerializeField] private MiniGameProgressManager miniGameProgressManager;

    private void Start()
    {
        LoadLevelData();
        StartRound();
        SetupColorButtons();
        GetButtonsClasses();
        SetActiveColorAtStart();
    }

    private void LoadLevelData()
    {
        int levelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);
        coloringLevelData = Resources.Load<ColoringLevelData>($"Levels/ColoringMiniGame/ColoringMiniGame{levelIndex + 1}");

        if (coloringLevelData == null)
        {
            Debug.LogError($"Level data for level {levelIndex} not found!");
        }
    }

    private void StartRound()
    {
        if (currentRound >= totalRounds)
        {
            SetButtonsDefaultState();
            FinishRoundWithAnimation();
            currentRound = 0;
            return;
        }

        levelBackgroundImg.GetComponent<SpriteRenderer>().sprite = coloringLevelData.backgroundImg;
        levelMiddleLayerImg.GetComponent<SpriteRenderer>().sprite = coloringLevelData.middleLayerImg;
        coloringBook.GetComponent<SpriteRenderer>().sprite = coloringLevelData.coloringBook;
        example.GetComponent<SpriteRenderer>().sprite = coloringLevelData.example;
        levelRegions = coloringLevelData.levelRegions;
    }

    private void SetupColorButtons()
    {
        exampleTexture = coloringLevelData.example.texture;
        int width = exampleTexture.width;
        int height = exampleTexture.height;

        Color[,] pixels = new Color[width, height];
        bool[,] visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                pixels[x, y] = exampleTexture.GetPixel(x, y);
            }
        }

        for (int x = 0; x < width && regionColors.Count < colorButtons.Count; x++)
        {
            for (int y = 0; y < height && regionColors.Count < colorButtons.Count; y++)
            {
                if (visited[x, y]) continue;

                Color pixelColor = pixels[x, y];
                if (IsApproximatelyBlack(pixelColor) || IsApproximatelyWhite(pixelColor) || IsApproximatelyGray(pixelColor)) continue;

                List<Color> region = new List<Color>();
                FloodFill(pixels, visited, x, y, region);

                if (region.Count < 30) continue;

                Color avgColor = AverageColor(region);
                Color smoothColor = SmoothColor(avgColor);

                if (!regionColors.Any(c => AreColorsSimilar(c, smoothColor)))
                {
                    regionColors.Add(smoothColor);
                }
            }
        }

        for (int i = 0; i < colorButtons.Count; i++)
        {
            if (i < regionColors.Count)
                colorButtons[i].Init(defaultButtonsColor, selectedButtonsColor, regionColors[i], this);
            else
                colorButtons[i].gameObject.SetActive(false);
        }
    }

    private void FloodFill(Color[,] pixels, bool[,] visited, int startX, int startY, List<Color> region)
    {
        int width = pixels.GetLength(0);
        int height = pixels.GetLength(1);
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();
            region.Add(pixels[p.x, p.y]);

            foreach (Vector2Int n in GetNeighbors(p, width, height))
            {
                if (!visited[n.x, n.y])
                {
                    Color neighborColor = pixels[n.x, n.y];
                    if (!IsApproximatelyBlack(neighborColor) && !IsApproximatelyWhite(neighborColor) && !IsApproximatelyGray(neighborColor))
                    {
                        visited[n.x, n.y] = true;
                        queue.Enqueue(n);
                    }
                }
            }
        }
    }

    private List<Vector2Int> GetNeighbors(Vector2Int p, int width, int height)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        foreach (var dir in dirs)
        {
            Vector2Int neighbor = p + dir;
            if (neighbor.x >= 0 && neighbor.y >= 0 && neighbor.x < width && neighbor.y < height)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    private Color AverageColor(List<Color> colors)
    {
        Vector3 sum = Vector3.zero;
        foreach (var c in colors)
        {
            sum.x += c.r;
            sum.y += c.g;
            sum.z += c.b;
        }

        float count = colors.Count;
        return new Color(sum.x / count, sum.y / count, sum.z / count);
    }

    private Color SmoothColor(Color color)
    {
        return new Color(
            Mathf.Round(color.r * 100f) / 100f,
            Mathf.Round(color.g * 100f) / 100f,
            Mathf.Round(color.b * 100f) / 100f
        );
    }

    private bool IsApproximatelyWhite(Color color)
    {
        float maxValue = Mathf.Max(color.r, color.g, color.b);
        return maxValue > 0.95f && Mathf.Abs(color.r - color.g) < 0.05f && Mathf.Abs(color.g - color.b) < 0.05f;
    }

    private bool IsApproximatelyBlack(Color color)
    {
        float threshold = 0.2f;
        return color.r < threshold && color.g < threshold && color.b < threshold;
    }

    private bool IsApproximatelyGray(Color color)
    {
        float tolerance = 0.05f;
        return Mathf.Abs(color.r - color.g) < tolerance && Mathf.Abs(color.g - color.b) < tolerance && Mathf.Abs(color.r - color.b) < tolerance;
    }

    private bool AreColorsSimilar(Color a, Color b)
    {
        const float tolerance = 0.15f;
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    private void SetActiveColorAtStart()
    {
        int randomButton = Random.Range(0, buttonsClasses.Count);
        buttonsClasses[randomButton].OnClick();
    }

    private void GetButtonsClasses()
    {
        buttonsClasses.Clear();

        foreach (var button in colorButtons)
        {
            var colorButton = button.GetComponent<ColoringColorButton>();
            if (colorButton != null)
            {
                buttonsClasses.Add(colorButton);
            }
            else
            {
                Debug.LogWarning($"Button {button.name} is missing ColoringColorButton component!");
            }
        }
    }

    public void SetSelectedColor(Color color)
    {
        SetButtonsDefaultState();

        selectedColor = color;
    }

    private void SetButtonsDefaultState()
    {
        foreach (var button in buttonsClasses)
        {
            button.SetColorToDefault();
        }
    }

    public Color GetSelectedColor()
    {
        return selectedColor;
    }

    public Texture2D GetExampleTexture()
    {
        return exampleTexture;
    }

    public int GetLevelRegions()
    {
        return levelRegions;
    }

    public void OnLevelCompleted()
    {
        currentRound++;
        StartRound();
    }

    private void FinishRoundWithAnimation()
    {
        Transform exampleTransform = example.transform;
        Transform coloringBookTransform = coloringBook.transform;

        Sequence animationSequence = DOTween.Sequence();

        animationSequence.Append(
            colorPanel.DOAnchorPosY(-Screen.height, 0.4f).SetEase(Ease.InBack)
        );

        animationSequence.Append(
            exampleTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
        );

        animationSequence.AppendCallback(() =>
        {
            example.SetActive(false);
        });

        Vector3 targetPos = coloringBookTransform.position;
        targetPos.x = 0f;

        animationSequence.Append(
            coloringBookTransform.DOMoveX(targetPos.x, 1f).SetEase(Ease.OutExpo)
        );

        animationSequence.Append(
            coloringBookTransform.DOPunchRotation(
                new Vector3(0, 0, 15f),
                1.0f,
                6,
                0.5f
            )
        );

        animationSequence.AppendInterval(0.5f);

        animationSequence.Append(
            coloringBookTransform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack)
        );

        animationSequence.AppendCallback(() =>
        {
            coloringBook.SetActive(false);
            miniGameProgressManager.ShowWinAnimation();
        });
    }
}
