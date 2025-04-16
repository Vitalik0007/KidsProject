using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class TimingBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image leftRedZoneImage;
    [SerializeField] private Image greenZoneImage;
    [SerializeField] private Image rightRedZoneImage;

    [SerializeField] private float speed = 2f;

    [SerializeField, Range(0f, 0.5f)] private float leftRedZone;
    [SerializeField, Range(0f, 0.5f)] private float rightRedZone;

    private float greenZoneStart;
    private float greenZoneEnd;

    [Header("Finger Animation Settings")]
    [SerializeField] private GameObject fingerAnimationInstance;
    private Coroutine fingerLoopCoroutine;
    private bool animationLoopActive = true;

    private bool movingRight = true;

    private void Start()
    {
        UpdateVisualZones();

        if (fingerAnimationInstance != null)
        {
            fingerLoopCoroutine = StartCoroutine(FingerAnimationLoop());
        }
    }

    private void FixedUpdate()
    {
        float move = speed * Time.fixedDeltaTime * (movingRight ? 1 : -1);
        slider.value += move;
        if (slider.value >= 1f) movingRight = false;
        if (slider.value <= 0f) movingRight = true;
    }

    private void UpdateVisualZones()
    {
        greenZoneStart = leftRedZone;
        greenZoneEnd = 1f - rightRedZone;

        // LEFT RED
        SetAnchor(leftRedZoneImage.rectTransform, 0f, leftRedZone);

        // GREEN
        if (greenZoneEnd > greenZoneStart)
        {
            greenZoneImage.gameObject.SetActive(true);
            SetAnchor(greenZoneImage.rectTransform, greenZoneStart, greenZoneEnd);
        }
        else
        {
            greenZoneImage.gameObject.SetActive(false);
        }

        // RIGHT RED
        SetAnchor(rightRedZoneImage.rectTransform, greenZoneEnd, 1f);
    }

    private void SetAnchor(RectTransform rt, float minX, float maxX)
    {
        rt.anchorMin = new Vector2(minX, 0f);
        rt.anchorMax = new Vector2(maxX, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    public bool IsInGreenZone()
    {
        float sliderValue = slider.value;
        return sliderValue >= greenZoneStart && sliderValue <= greenZoneEnd;
    }

    private IEnumerator FingerAnimationLoop()
    {
        while (animationLoopActive)
        {
            if (!fingerAnimationInstance)
                yield break;

            if (IsInGreenZone())
            {
                fingerAnimationInstance.SetActive(true);

                var spineAnimator = fingerAnimationInstance.GetComponent<SkeletonAnimation>();
                if (spineAnimator != null && spineAnimator.Skeleton != null)
                {
                    var animation = spineAnimator.Skeleton.Data.FindAnimation("animation");
                    if (animation != null)
                    {
                        spineAnimator.AnimationState.SetAnimation(0, animation, false);
                        yield return new WaitForSeconds(animation.Duration);
                    }
                    else
                    {
                        Debug.LogWarning("Spine: Animation 'animation' not found.");
                        yield return new WaitForSeconds(1f);
                    }
                }
                else
                {
                    Debug.LogWarning("Spine: Missing SkeletonAnimation component or skeleton data.");
                    yield return new WaitForSeconds(1f);
                }

                if (fingerAnimationInstance)
                    fingerAnimationInstance.SetActive(false);

                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    public void StopFingerAnimation()
    {
        animationLoopActive = false;

        if (fingerLoopCoroutine != null)
        {
            StopCoroutine(fingerLoopCoroutine);
            fingerLoopCoroutine = null;
        }

        if (fingerAnimationInstance != null)
        {
            Destroy(fingerAnimationInstance);
        }
    }
}