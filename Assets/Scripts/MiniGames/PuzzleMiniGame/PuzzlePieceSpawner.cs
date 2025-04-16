using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PuzzlePieceSpawner : MonoBehaviour
{
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private int minPieceSize = 1000;
    [SerializeField] private float scatterRadius = 5f;
    [SerializeField] private float animationDuration = 1.5f;

    private Texture2D puzzleImage;
    private Texture2D maskImage;

    private Dictionary<Color, List<Vector2Int>> colorRegions = new Dictionary<Color, List<Vector2Int>>();
    private Dictionary<GameObject, Vector3> piecePositions = new Dictionary<GameObject, Vector3>();
    private List<GameObject> spawnedPuzzlePieces = new List<GameObject>();
    private GameObject background;
    private Vector3 puzzleCenter = Vector3.zero;

    private PuzzleGameManager puzzleGameManager;

    public void Initialize(Texture2D puzzleImage, Texture2D maskImage, PuzzleGameManager puzzleGameManager)
    {
        colorRegions.Clear();
        piecePositions.Clear();
        ClearPuzzlePieces();
        Destroy(background);

        this.puzzleImage = puzzleImage;
        this.maskImage = maskImage;
        this.puzzleGameManager = puzzleGameManager;

        GeneratePuzzlePieces();
        AnimatePiecesScale(Vector3.one);
        ScatterPieces();
    }

    private void GeneratePuzzlePieces()
    {
        FindColorRegions();
        CreatePuzzlePieces();
    }

    private void FindColorRegions()
    {
        Color[] maskPixels = maskImage.GetPixels();
        bool[,] visited = new bool[maskImage.width, maskImage.height];

        for (int y = 0; y < maskImage.height; y++)
        {
            for (int x = 0; x < maskImage.width; x++)
            {
                if (!visited[x, y])
                {
                    Color color = maskPixels[y * maskImage.width + x];
                    if (color.a > 0)
                    {
                        List<Vector2Int> region = FloodFill(maskPixels, visited, x, y, color);
                        if (region.Count >= minPieceSize)
                        {
                            if (!colorRegions.ContainsKey(color))
                                colorRegions[color] = new List<Vector2Int>();
                            colorRegions[color].AddRange(region);
                        }
                    }
                }
            }
        }
    }

    private List<Vector2Int> FloodFill(Color[] maskPixels, bool[,] visited, int startX, int startY, Color targetColor)
    {
        List<Vector2Int> region = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        region.Clear();
        queue.Clear();

        queue.Enqueue(new Vector2Int(startX, startY));

        while (queue.Count > 0)
        {
            Vector2Int pixel = queue.Dequeue();
            if (pixel.x < 0 || pixel.y < 0 || pixel.x >= maskImage.width || pixel.y >= maskImage.height)
                continue;
            if (visited[pixel.x, pixel.y])
                continue;

            Color currentColor = maskPixels[pixel.y * maskImage.width + pixel.x];
            if (!AreColorsSimilar(targetColor, currentColor))
                continue;

            visited[pixel.x, pixel.y] = true;
            region.Add(pixel);

            queue.Enqueue(new Vector2Int(pixel.x + 1, pixel.y));
            queue.Enqueue(new Vector2Int(pixel.x - 1, pixel.y));
            queue.Enqueue(new Vector2Int(pixel.x, pixel.y + 1));
            queue.Enqueue(new Vector2Int(pixel.x, pixel.y - 1));
        }

        return region;
    }

    private bool AreColorsSimilar(Color a, Color b)
    {
        float threshold = 0.2f;
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }

    private void CreatePuzzlePieces()
    {
        List<List<Vector2Int>> validRegions = new List<List<Vector2Int>>();
        validRegions.Clear();

        foreach (var region in colorRegions.Values)
        {
            if (region.Count >= minPieceSize)
                validRegions.Add(region);
        }

        CreatePuzzleBackground();

        foreach (var region in validRegions)
        {
            GeneratePiece(region);
        }
    }

    private void CreatePuzzleBackground()
    {
        background = new GameObject("PuzzleBackground");
        background.transform.position = Vector3.zero;
        background.transform.localScale = Vector3.zero;
        //background.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        SpriteRenderer backgroundRenderer = background.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = Sprite.Create(puzzleImage, new Rect(0, 0, puzzleImage.width, puzzleImage.height), new Vector2(0.5f, 0.5f));
        backgroundRenderer.color = new Color(0.376f, 0.376f, 0.376f, 1f);
        backgroundRenderer.sortingOrder = 3;
    }

    private void GeneratePiece(List<Vector2Int> region)
    {
        if (puzzleImage == null || maskImage == null) return;

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (Vector2Int pixel in region)
        {
            if (pixel.x < minX) minX = pixel.x;
            if (pixel.x > maxX) maxX = pixel.x;
            if (pixel.y < minY) minY = pixel.y;
            if (pixel.y > maxY) maxY = pixel.y;
        }

        int pieceWidth = maxX - minX + 1;
        int pieceHeight = maxY - minY + 1;

        Texture2D pieceTexture = new Texture2D(pieceWidth, pieceHeight);
        pieceTexture.SetPixels(new Color[pieceWidth * pieceHeight]);
        pieceTexture.Apply();

        foreach (Vector2Int pixel in region)
        {
            pieceTexture.SetPixel(pixel.x - minX, pixel.y - minY, puzzleImage.GetPixel(pixel.x, pixel.y));
        }

        pieceTexture.Apply();

        GameObject piece = Instantiate(piecePrefab, Vector3.zero, Quaternion.identity);
        spawnedPuzzlePieces.Add(piece);
        piece.transform.localScale = Vector3.zero;
        //piece.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        SpriteRenderer renderer = piece.GetComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(pieceTexture, new Rect(0, 0, pieceWidth, pieceHeight), new Vector2(0.5f, 0.5f));

        float puzzleWidth = puzzleImage.width;
        float puzzleHeight = puzzleImage.height;

        float pieceCenterX = (minX + maxX) / 2f;
        float pieceCenterY = (minY + maxY) / 2f;

        float worldX = (pieceCenterX - puzzleWidth / 2f) / 100f;
        float worldY = (pieceCenterY - puzzleHeight / 2f) / 100f;

        //worldX *= 0.5f;
        //worldY *= 0.5f;

        Vector3 correctPosition = new Vector3(worldX, worldY, 0);

        piece.transform.position = correctPosition;

        piecePositions[piece] = correctPosition;

        BoxCollider2D collider = piece.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(pieceWidth / 100f, pieceHeight / 100f);
        collider.offset = Vector2.zero;

        piece.AddComponent<PuzzlePiece>().Initialize(correctPosition, puzzleGameManager);
    }

    public void AnimatePiecesScale(Vector3 scale)
    {
        foreach (var piece in spawnedPuzzlePieces)
        {
            piece.transform.DOScale(scale, 1.0f).SetEase(Ease.OutQuad);
        }

        if (background != null)
        {
            background.transform.DOScale(scale, 1.0f).SetEase(Ease.OutQuad);
        }
    }

    private void ScatterPieces()
    {
        AudioManager.Instance.PlaySound(SoundType.Puzzle_Separate, 0.7f);

        HashSet<Vector3> usedPositions = new HashSet<Vector3>();
        usedPositions.Clear();

        foreach (var piece in piecePositions.Keys)
        {
            Vector3 randomPosition = GetValidRandomScatterPosition(usedPositions, piece);
            usedPositions.Add(randomPosition);
            piece.transform.DOMove(randomPosition, animationDuration).SetEase(Ease.OutBack);
        }
    }

    private Vector3 GetValidRandomScatterPosition(HashSet<Vector3> usedPositions, GameObject piece)
    {
        int maxAttempts = 100;
        Vector3 randomPosition;

        for (int i = 0; i < maxAttempts; i++)
        {
            randomPosition = GetRandomScatterPosition();

            if (!IsWithinScreenBounds(randomPosition, piece) || IsOverlapping(usedPositions, randomPosition) || IsOverlappingWithPuzzle(randomPosition))
                continue;

            return randomPosition;
        }

        return GetRandomScatterPosition();
    }

    private Vector3 GetRandomScatterPosition()
    {
        float angle = Random.Range(0, Mathf.PI * 2);
        float radius = Random.Range(scatterRadius * 0.5f, scatterRadius);
        return new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
    }

    private bool IsWithinScreenBounds(Vector3 position, GameObject piece)
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(position);

        return screenPoint.x > 0.1 && screenPoint.x < 0.9 &&
               screenPoint.y > 0.1 && screenPoint.y < 0.9;
    }

    private bool IsOverlapping(HashSet<Vector3> usedPositions, Vector3 position)
    {
        float minDistance = 3.0f;

        foreach (var usedPos in usedPositions)
        {
            if (Vector3.Distance(usedPos, position) < minDistance)
                return true;
        }
        return false;
    }

    private bool IsOverlappingWithPuzzle(Vector3 position)
    {
        float puzzleWidth = puzzleImage.width / 200f;
        float puzzleHeight = puzzleImage.height / 200f;
        Vector3 puzzleMin = puzzleCenter - new Vector3(puzzleWidth / 2, puzzleHeight / 2, 0);
        Vector3 puzzleMax = puzzleCenter + new Vector3(puzzleWidth / 2, puzzleHeight / 2, 0);
        return position.x > puzzleMin.x && position.x < puzzleMax.x && position.y > puzzleMin.y && position.y < puzzleMax.y;
    }

    private void ClearPuzzlePieces()
    {
        foreach (var obj in spawnedPuzzlePieces)
        {
            Destroy(obj);
        }
        spawnedPuzzlePieces.Clear();
    }
}