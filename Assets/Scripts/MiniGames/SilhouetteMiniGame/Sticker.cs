using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Sticker : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private int requiredClicks;
    private int currentClicks = 0;
    [SerializeField] private float scaleIncrease;
    private TimingBar timingBar;
    private SilhouetteGameManager gameManager;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalAnchoredPosition;
    private bool isSilhouettePhaseStarted = false;
    private Button sticker;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalAnchoredPosition = new Vector2(0, 0);

        sticker = GetComponent<Button>();
        sticker.onClick.AddListener(OnStickerClick);

        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }

    public void Initialize(TimingBar timingBar, SilhouetteGameManager gameManager)
    {
        this.timingBar = timingBar;
        this.gameManager = gameManager;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isSilhouettePhaseStarted) return;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out localPointerPosition);

        rectTransform.anchoredPosition = localPointerPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        bool isCorrect = false;
        foreach (var silhouette in gameManager.GetSilhouetteButtons())
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    silhouette.rectTransform,
                    Input.mousePosition,
                    eventData.pressEventCamera))
            {
                if (silhouette.sprite == gameManager.GetCorrectSilhouette())
                {
                    isCorrect = true;
                    break;
                }
            }
        }

        if (isCorrect)
        {
            AudioManager.Instance.PlaySound(SoundType.Silhouette_Press);
            gameManager.OnCorrectSilhouetteMatched();
        }
        else
        {
            AudioManager.Instance.PlaySound(SoundType.Silhouette_WrongPress);
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
    }

    public void OnStickerClick()
    {
        AudioManager.Instance.PlaySound(SoundType.Button_Click);

        if (timingBar.IsInGreenZone() && !isSilhouettePhaseStarted)
        {
            AudioManager.Instance.PlaySound(SoundType.Silhouette_Press2);
            timingBar.StopFingerAnimation();

            if (currentClicks < requiredClicks)
            {
                currentClicks++;
                transform.localScale *= scaleIncrease;
            }
            else
            {
                isSilhouettePhaseStarted = true;
                gameManager.StartSilhouettePhase();
            }
        }
        else
            AudioManager.Instance.PlaySound(SoundType.Silhouette_WrongPress2);
    }

    public void ResetSticker()
    {
        currentClicks = 0;
        transform.localScale = Vector3.one;

        rectTransform.anchoredPosition = originalAnchoredPosition;
        isSilhouettePhaseStarted = false;
    }
}