using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine.EventSystems;

public class LevelSelector : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [System.Serializable]
    public class LevelData
    {
        public Sprite backgroundImage;
        public Sprite gamePanelBackgroundImage;
        public Sprite gamePanelFrontImage;
        public Sprite levelLableImage;
        public Sprite bottomImage;
        public SkeletonDataAsset animationData;
    }

    public List<LevelData> levels = new List<LevelData>();
    private int currentLevelIndex;

    [Header("UI Elements")]
    [SerializeField] private GameObject backgroundImage;
    [SerializeField] private GameObject gamePanelBackgroundImage;
    [SerializeField] private GameObject gamePanelFrontImage;
    [SerializeField] private Image lableImage;
    [SerializeField] private GameObject bottomLevelImage;
    [SerializeField] private GameObject[] starObjects;
    private string starAnimationName = "animation";
    [SerializeField] private GameObject panelMiniGames;

    [SerializeField] private Button prevLevelButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button playLevelButton;
    [SerializeField] private Button closeLevelButton;

    [SerializeField] private Button[] miniGameButtons;

    [Header("Mini games panel")]
    [SerializeField] private GameObject[] filledStarImages;
    [SerializeField] private int totalMiniGames = 4;

    [Header("Swipe")]
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float swipeThreshold = 100f;

    [Header("Spine Animation")]
    [SerializeField] private SkeletonGraphic levelAnimation;

    [Header("Animation")]
    [SerializeField] private RectTransform levelUIContainer;
    [SerializeField] private float swipeAnimationDuration = 0.4f;
    [SerializeField] private float swipeDistance = 1000f;

    private Coroutine transitionCoroutine;

    private void Start()
    {
        currentLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);

        UpdateLevelUI();

        prevLevelButton.onClick.AddListener(() => AnimateLevelSwitch(-1));
        nextLevelButton.onClick.AddListener(() => AnimateLevelSwitch(1));
        playLevelButton.onClick.AddListener(OnLevelSelected);
        closeLevelButton.onClick.AddListener(CloseMiniGamePanel);

        for (int i = 0; i < miniGameButtons.Length; i++)
        {
            int index = i;
            miniGameButtons[i].onClick.AddListener(() => LoadMiniGame(index));
        }
    }

    private void UpdateLevelUI()
    {
        LevelData currentLevel = levels[currentLevelIndex];

        backgroundImage.GetComponent<SpriteRenderer>().sprite = currentLevel.backgroundImage;
        gamePanelBackgroundImage.GetComponent<SpriteRenderer>().sprite = currentLevel.gamePanelBackgroundImage;
        gamePanelFrontImage.GetComponent<SpriteRenderer>().sprite = currentLevel.gamePanelFrontImage;
        lableImage.sprite = currentLevel.levelLableImage;
        bottomLevelImage.GetComponent<SpriteRenderer>().sprite = currentLevel.bottomImage;

        if (levelAnimation != null && currentLevel.animationData != null)
        {
            levelAnimation.skeletonDataAsset = currentLevel.animationData;
            levelAnimation.Initialize(true);
            levelAnimation.AnimationState.SetAnimation(0, "animation", true);
        }

        int completedStars = GetCompletedMiniGamesCount(currentLevelIndex);
        ShowStars(completedStars);

        UpdateMiniGameStars();
    }

    private void ShowStars(int completedStars)
    {
        for (int i = 0; i < starObjects.Length; i++)
        {
            if (i < completedStars)
            {
                starObjects[i].SetActive(true);

                var skeleton = starObjects[i].GetComponent<SkeletonAnimation>();
                if (skeleton != null)
                {
                    skeleton.Initialize(true);
                    skeleton.AnimationState.SetAnimation(0, starAnimationName, false);
                }
            }
            else
            {
                starObjects[i].SetActive(false);
            }
        }
    }

    private void AnimateLevelSwitch(int direction)
    {
        AudioManager.Instance.PlaySound(SoundType.Menu_swipe, 0.7f);

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(AnimateLevelChangeCoroutine(direction));
    }

    private IEnumerator AnimateLevelChangeCoroutine(int direction)
    {
        Vector2 centerPos = Vector2.zero;
        Vector2 exitPos = centerPos - Vector2.right * direction * swipeDistance;
        Vector2 enterPos = centerPos + Vector2.right * direction * swipeDistance;

        yield return MoveRectTransform(levelUIContainer, centerPos, exitPos, swipeAnimationDuration / 2f);

        if (direction > 0)
            currentLevelIndex = (currentLevelIndex + 1) % levels.Count;
        else
            currentLevelIndex = (currentLevelIndex - 1 + levels.Count) % levels.Count;

        UpdateLevelUI();

        levelUIContainer.anchoredPosition = enterPos;

        yield return null;

        yield return MoveRectTransform(levelUIContainer, enterPos, centerPos, swipeAnimationDuration / 2f);
    }

    private IEnumerator MoveRectTransform(RectTransform rect, Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            rect.anchoredPosition = Vector2.LerpUnclamped(from, to, Mathf.SmoothStep(0, 1, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        rect.anchoredPosition = to;
    }

    private void OnLevelSelected()
    {
        AudioManager.Instance.PlaySound(SoundType.Button_Click);

        PlayerPrefs.SetInt("SelectedLevel", currentLevelIndex);
        PlayerPrefs.Save();

        bool isVisited = PlayerPrefs.GetInt($"Level_Visited_{currentLevelIndex}", 0) == 1;

        if (isVisited)
        {
            panelMiniGames.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt($"Level_Visited_{currentLevelIndex}", 1);
            MiniGameLoader loader = FindFirstObjectByType<MiniGameLoader>();
            if (loader != null)
            {
                loader.LoadMiniGame(0);
            }
        }
    }

    private void CloseMiniGamePanel()
    {
        AudioManager.Instance.PlaySound(SoundType.Button_Click);
        panelMiniGames.SetActive(false);
    }

    private void LoadMiniGame(int miniGameIndex)
    {
        AudioManager.Instance.PlaySound(SoundType.Button_Click);
        MiniGameLoader loader = FindFirstObjectByType<MiniGameLoader>();
        if (loader != null)
        {
            loader.LoadMiniGame(miniGameIndex);
        }
    }

    private int GetCompletedMiniGamesCount(int levelIndex)
    {
        int count = 0;
        for (int i = 0; i < 4; i++)
        {
            string key = $"Level_{levelIndex}_MiniGame_{i}_Completed";
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                count++;
            }
        }
        return count;
    }

    private void UpdateMiniGameStars()
    {
        for (int i = 0; i < totalMiniGames; i++)
        {
            string key = $"Level_{currentLevelIndex}_MiniGame_{i}_Completed";
            bool isCompleted = PlayerPrefs.GetInt(key, 0) == 1;

            if (i < filledStarImages.Length)
                filledStarImages[i].SetActive(isCompleted);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startTouchPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        endTouchPosition = eventData.position;
        Vector2 delta = endTouchPosition - startTouchPosition;

        if (Mathf.Abs(delta.x) > swipeThreshold && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0)
                AnimateLevelSwitch(-1);
            else
                AnimateLevelSwitch(1);
        }
    }
}
