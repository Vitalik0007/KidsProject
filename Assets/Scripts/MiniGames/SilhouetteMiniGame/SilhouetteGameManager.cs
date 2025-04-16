using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;
using System.Collections;

public class SilhouetteGameManager : MonoBehaviour
{
    [SerializeField] private TimingBar timingBar;
    [SerializeField] private GameObject stickerPrefab;
    [SerializeField] private Transform stickerSpawnPoint;
    [SerializeField] private GameObject silhouettePanel;
    [SerializeField] private Transform silhouetteContainer;
    [SerializeField] private GameObject silhouetteButtonPrefab;
    [SerializeField] private GameObject levelBackgroundImg;
    [SerializeField] private GameObject levelFrontLayerImg;
    [SerializeField] private int totalRounds = 10;

    private int currentRound = 0;
    private GameObject currentSticker;
    private Sprite correctSilhouette;
    private SilhouetteLevelData currentLevelData;

    [SerializeField] private List<Image> silhouetteButtons;

    [SerializeField] private GameObject fingerAnimationPrefab;
    private GameObject fingerAnimationInstance;
    private bool isFingerAnimationCreated = false;

    private Coroutine fingerAnimationCoroutine;

    [SerializeField] private MiniGameProgressManager miniGameProgressManager;

    private void Start()
    {
        LoadLevelData();
        StartRound();
    }

    private void LoadLevelData()
    {
        int levelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);
        currentLevelData = Resources.Load<SilhouetteLevelData>($"Levels/SilhouetteMiniGame/SilhouetteMiniGame{levelIndex + 1}");

        if (currentLevelData == null)
        {
            Debug.LogError($"Level data for level{levelIndex} not found!");
        }
    }

    private void StartRound()
    {
        if (currentRound >= totalRounds)
        {
            Debug.Log("Level Completed!!!");
            miniGameProgressManager.ShowWinAnimation();
            currentSticker.SetActive(false);
            return;
        }

        if (currentSticker) Destroy(currentSticker);
        currentSticker = Instantiate(stickerPrefab, stickerSpawnPoint.position, Quaternion.identity, stickerSpawnPoint.parent);
        currentSticker.GetComponent<Image>().sprite = currentLevelData.stickerSprites[currentRound];
        levelBackgroundImg.GetComponent<SpriteRenderer>().sprite = currentLevelData.backgroundImg;
        levelFrontLayerImg.GetComponent<SpriteRenderer>().sprite = currentLevelData.frontLayer;
        silhouettePanel.GetComponent<Image>().sprite = currentLevelData.silhouettesPanelImg;

        Sticker stickerComponent = currentSticker.GetComponent<Sticker>();
        stickerComponent.Initialize(timingBar, this);

        silhouettePanel.SetActive(false);
        AnimateSticker();
    }

    private void AnimateSticker()
    {
        RectTransform stickerTransform = currentSticker.GetComponent<RectTransform>();
        Image stickerImage = currentSticker.GetComponent<Image>();

        stickerTransform.anchoredPosition += new Vector2(0, -200f);
        Color startColor = stickerImage.color;
        startColor.a = 0f;
        stickerImage.color = startColor;

        float appearDuration = 1.0f;

        stickerTransform.DOAnchorPosY(stickerTransform.anchoredPosition.y + 200f, appearDuration)
            .SetEase(Ease.OutBounce);

        stickerImage.DOFade(1f, appearDuration);
    }

    public void StartSilhouettePhase()
    {
        currentSticker.GetComponent<Button>().enabled = false;
        timingBar.gameObject.SetActive(false);
        silhouettePanel.SetActive(true);
        correctSilhouette = currentLevelData.silhouetteSprites[currentRound];
        GenerateSilhouettes();

        AudioManager.Instance.PlaySound(SoundType.Silhouette_Salto);

        currentSticker.transform
        .DOMove(Vector3.zero, 0.7f)
        .SetEase(Ease.OutBack);

        currentSticker.transform
            .DORotate(new Vector3(0, 0, 360), 0.7f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear);

        CreateFingerAnimation();
    }

    private void CreateFingerAnimation()
    {
        if (!isFingerAnimationCreated)
        {
            DestroyFingerAnimation();

            int correctButtonIndex = silhouetteButtons.FindIndex(button => button.sprite == correctSilhouette);
            Button correctButton = silhouetteButtons[correctButtonIndex].GetComponent<Button>();

            fingerAnimationInstance = Instantiate(fingerAnimationPrefab, correctButton.transform.position, Quaternion.identity);
            fingerAnimationInstance.transform.SetParent(correctButton.transform);

            var spineAnimator = fingerAnimationInstance.GetComponent<SkeletonAnimation>();
            spineAnimator.AnimationState.SetAnimation(0, "animation", false);

            isFingerAnimationCreated = true;

            if (fingerAnimationCoroutine != null)
                StopCoroutine(fingerAnimationCoroutine);

            fingerAnimationCoroutine = StartCoroutine(PlayFingerAnimationLoop());
        }
    }

    private IEnumerator PlayFingerAnimationLoop()
    {
        while (true)
        {
            if (fingerAnimationInstance == null)
                yield break;

            fingerAnimationInstance.SetActive(true);

            var spineAnimator = fingerAnimationInstance.GetComponent<SkeletonAnimation>();
            spineAnimator.AnimationState.SetAnimation(0, "animation", false);

            float animationDuration = spineAnimator.Skeleton.Data.FindAnimation("animation").Duration;
            yield return new WaitForSeconds(animationDuration);

            fingerAnimationInstance.SetActive(false);

            yield return new WaitForSeconds(1.5f);
        }
    }

    private void DestroyFingerAnimation()
    {
        if (fingerAnimationInstance != null)
        {
            Destroy(fingerAnimationInstance);
        }
    }

    private void GenerateSilhouettes()
    {
        List<Sprite> availableSilhouettes = new List<Sprite>(currentLevelData.silhouetteSprites);
        availableSilhouettes.Remove(correctSilhouette);
        availableSilhouettes.Shuffle();

        List<Sprite> selectedSilhouettes = availableSilhouettes.GetRange(0, 2);
        selectedSilhouettes.Add(correctSilhouette);
        selectedSilhouettes.Shuffle();

        for (int i = 0; i < silhouetteButtons.Count; i++)
        {
            silhouetteButtons[i].sprite = selectedSilhouettes[i];
            int index = i;
            silhouetteButtons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            silhouetteButtons[i].GetComponent<Button>().onClick.AddListener(() => CheckSilhouette(selectedSilhouettes[index]));
        }
    }

    private void CheckSilhouette(Sprite chosenSilhouette)
    {
        if (chosenSilhouette == correctSilhouette)
        {
            AudioManager.Instance.PlaySound(SoundType.Silhouette_Press);

            DestroyFingerAnimation();

            currentRound++;

            if (currentRound < totalRounds)
                timingBar.gameObject.SetActive(true);

            silhouettePanel.SetActive(false);
            StartRound();
        }
        else
            AudioManager.Instance.PlaySound(SoundType.Silhouette_WrongPress);
    }

    public List<Image> GetSilhouetteButtons()
    {
        return silhouetteButtons;
    }

    public Sprite GetCorrectSilhouette()
    {
        return correctSilhouette;
    }

    public void OnCorrectSilhouetteMatched()
    {
        DestroyFingerAnimation();

        currentRound++;

        if (currentRound < totalRounds)
            timingBar.gameObject.SetActive(true);

        silhouettePanel.SetActive(false);
        StartRound();
    }
}

public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}