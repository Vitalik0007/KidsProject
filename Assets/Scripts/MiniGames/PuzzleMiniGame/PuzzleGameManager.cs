using System.Collections;
using UnityEngine;

public class PuzzleGameManager : MonoBehaviour
{
    [SerializeField] private GameObject levelBackgroundImg;
    [SerializeField] private GameObject levelFrontLayerImg;
    [SerializeField] private PuzzlePieceSpawner pieceSpawner;
    [SerializeField] private int totalRounds = 4;

    private int currentRound = 0;
    private PuzzleLevelData puzzleLevelData;

    private int totalPuzzlePieces = 6;
    private int collectedPuzzlePieces = 0;

    [SerializeField] private MiniGameProgressManager miniGameProgressManager;

    private void Start()
    {
        LoadLevelData();
        StartRound();
    }

    private void LoadLevelData()
    {
        int levelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);
        puzzleLevelData = Resources.Load<PuzzleLevelData>($"Levels/PuzzleMiniGame/PuzzleMiniGame{levelIndex + 1}");

        if (puzzleLevelData == null)
        {
            Debug.LogError($"Level data for level {levelIndex} not found!");
            return;
        }
    }

    private void StartRound()
    {
        if (currentRound >= totalRounds)
        {
            Debug.Log("Level Completed!!!");
            miniGameProgressManager.ShowWinAnimation();
            return;
        }

        var roundData = puzzleLevelData.roundsData[currentRound];

        if (roundData.puzzleImg == null || roundData.maskImg == null)
        {
            Debug.LogError("Puzzle image or mask image is not set!");
            return;
        }

        levelBackgroundImg.GetComponent<SpriteRenderer>().sprite = puzzleLevelData.backgroundImg;
        levelFrontLayerImg.GetComponent<SpriteRenderer>().sprite = puzzleLevelData.frontLayerImg;

        pieceSpawner.Initialize(roundData.puzzleImg, roundData.maskImg, this);
    }

    public void CheckGameProgress()
    {
        collectedPuzzlePieces++;

        if (collectedPuzzlePieces == totalPuzzlePieces)
        {
            AudioManager.Instance.PlaySound(SoundType.Puzzle_Finish);
            collectedPuzzlePieces = 0;
            currentRound++;

            StartCoroutine(StartNewRaundWithDelay());
        }
    }

    private IEnumerator StartNewRaundWithDelay()
    {
        yield return new WaitForSeconds(1.0f);

        pieceSpawner.AnimatePiecesScale(Vector3.zero);

        yield return new WaitForSeconds(1.2f);

        StartRound();
    }
}