using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ColoringArea : MonoBehaviour
{
    private Texture2D texture;
    private Texture2D exampleTexture;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private ColoringGameManager gameManager;
    [SerializeField] private int colorTolerance = 20;

    private int levelRegions;
    private int correctlyFilledRegions = 0;

    private HashSet<string> filledRegionKeys = new HashSet<string>();

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Sprite sprite = spriteRenderer.sprite;

        texture = Instantiate(sprite.texture);
        texture.filterMode = FilterMode.Point;

        Rect rect = new Rect(0, 0, texture.width, texture.height);
        spriteRenderer.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), sprite.pixelsPerUnit);

        exampleTexture = gameManager.GetExampleTexture();
        levelRegions = gameManager.GetLevelRegions();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                HandleClick(mouseWorldPos);
            }
        }
    }

    private void HandleClick(Vector2 worldPos)
    {
        Vector2 localPos = transform.InverseTransformPoint(worldPos);
        Sprite sprite = spriteRenderer.sprite;
        Rect rect = sprite.rect;
        float unitsPerPixel = 1f / sprite.pixelsPerUnit;

        int x = Mathf.FloorToInt(rect.width * 0.5f + localPos.x / unitsPerPixel);
        int y = Mathf.FloorToInt(rect.height * 0.5f + localPos.y / unitsPerPixel);

        if (x < 0 || x >= texture.width || y < 0 || y >= texture.height)
            return;

        Color32 clickedColor = texture.GetPixel(x, y);
        if (IsBlackColor(clickedColor))
        {
            AudioManager.Instance.PlaySound(SoundType.ColoringBook_WrongColor);
            return;
        }

        Color32 exampleColor = exampleTexture.GetPixel(x, y);
        if (IsWhiteColor(exampleColor))
        {
            AudioManager.Instance.PlaySound(SoundType.ColoringBook_WrongColor);
            return;
        }

        AudioManager.Instance.PlaySound(SoundType.ColoringBook_Coloring);
        Color32 fillColor = gameManager.GetSelectedColor();
        FloodFillSmart(x, y, clickedColor, fillColor);
    }

    private void FloodFillSmart(int startX, int startY, Color32 targetColor, Color32 fillColor)
    {
        int width = texture.width;
        int height = texture.height;
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        bool[] visited = new bool[width * height];
        Color32[] pixels = texture.GetPixels32();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        List<Vector2Int> pixelsToFill = new List<Vector2Int>();
        stack.Push(new Vector2Int(startX, startY));

        while (stack.Count > 0)
        {
            Vector2Int p = stack.Pop();
            int index = p.y * width + p.x;

            if (visited[index])
                continue;

            Color32 currentColor = pixels[index];
            if (IsBlackColor(currentColor))
                continue;

            if (!IsTransparentColor(currentColor) && !AreColorsSimilar(currentColor, targetColor, colorTolerance))
                continue;

            pixels[index] = fillColor;
            visited[index] = true;
            pixelsToFill.Add(p);

            foreach (Vector2Int dir in directions)
            {
                Vector2Int np = p + dir;
                if (np.x >= 0 && np.y >= 0 && np.x < width && np.y < height)
                {
                    int nIndex = np.y * width + np.x;
                    if (!visited[nIndex])
                    {
                        stack.Push(np);
                    }
                }
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        string regionKey = GetStableRegionKey(pixelsToFill);

        if (!filledRegionKeys.Contains(regionKey))
        {
            filledRegionKeys.Add(regionKey);
            correctlyFilledRegions++;

            if (correctlyFilledRegions >= levelRegions)
            {
                AudioManager.Instance.PlaySound(SoundType.ColoringBook_Complete);
                gameManager.OnLevelCompleted();
            }
        }
    }

    private string GetStableRegionKey(List<Vector2Int> region)
    {
        region.Sort((a, b) =>
        {
            int cmp = a.x.CompareTo(b.x);
            return cmp != 0 ? cmp : a.y.CompareTo(b.y);
        });

        int sampleCount = Mathf.Min(region.Count, 5);
        System.Text.StringBuilder key = new System.Text.StringBuilder();
        for (int i = 0; i < sampleCount; i++)
        {
            key.Append(region[i].x);
            key.Append(',');
            key.Append(region[i].y);
            key.Append(';');
        }
        return key.ToString();
    }

    private bool IsBlackColor(Color32 color)
    {
        return color.r < 20 && color.g < 20 && color.b < 20 && color.a > 200;
    }

    private bool IsTransparentColor(Color32 color)
    {
        return color.a < 128;
    }

    private bool IsWhiteColor(Color32 color, int tolerance = 20)
    {
        return color.r >= (255 - tolerance) &&
               color.g >= (255 - tolerance) &&
               color.b >= (255 - tolerance);
    }

    private bool AreColorsSimilar(Color32 a, Color32 b, int tolerance)
    {
        int dr = a.r - b.r;
        int dg = a.g - b.g;
        int db = a.b - b.b;
        int da = a.a - b.a;
        int distance = dr * dr + dg * dg + db * db + da * da;
        return distance <= tolerance * tolerance;
    }
}