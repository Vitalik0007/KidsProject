using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ColoringColorButton : MonoBehaviour
{
    [SerializeField] private Image image;
    private Color defaultColor;
    private Color selectedColor;
    private Color assignedColor;
    private ColoringGameManager gameManager;
    private Button button;
    private Image buttonImage;
    private RectTransform rectTransform;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Init(Color defaultColor, Color selectedColor, Color assignedColor, ColoringGameManager manager)
    {
        this.defaultColor = defaultColor;
        this.selectedColor = selectedColor;
        this.assignedColor = assignedColor;

        gameManager = manager;
        buttonImage.color = this.defaultColor;

        if (image != null)
            image.color = this.assignedColor;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    public void OnClick()
    {
        AudioManager.Instance.PlaySound(SoundType.ColoringBook_ColorChanging);

        gameManager.SetSelectedColor(assignedColor);
        buttonImage.color = selectedColor;

        rectTransform.DOScale(1.15f, 0.2f).SetEase(Ease.OutBack);
    }

    public void SetColorToDefault()
    {
        buttonImage.color = defaultColor;

        rectTransform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine);
    }
}
