using UnityEngine;

public class MainObject : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float scaleIncreaseAmount = 1.2f;
    [SerializeField] private float scaleSpeed = 5f;
    [SerializeField] private float positionSpeed = 5f;
    [SerializeField] private float scaleThreshold = 0.01f;

    private float startY;
    private float randomOffset;
    private bool isFloating = true;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Vector3 targetPosition;
    private Vector3 currentPosition;

    private bool isScaling = false;

    public ColorType colorType;

    private void Start()
    {
        startY = transform.position.y;
        originalScale = transform.localScale;
        randomOffset = Random.Range(0f, Mathf.PI * 2f);
        floatSpeed *= Random.Range(0.8f, 1.2f);
        targetScale = originalScale;
        targetPosition = transform.position;
        currentPosition = transform.position;
    }

    private void Update()
    {
        if (isFloating)
        {
            float newY = startY + Mathf.Sin(Time.time * floatSpeed + randomOffset) * floatAmplitude;
            targetPosition = new Vector3(transform.position.x, newY, transform.position.z);
        }

        currentPosition = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * positionSpeed);
        transform.position = currentPosition;

        if (isScaling)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);

            if (Vector3.Distance(transform.localScale, targetScale) < scaleThreshold)
            {
                transform.localScale = targetScale;
                isScaling = false;
            }
        }
    }

    public void StartScaling()
    {
        isFloating = false;
        targetScale = originalScale * scaleIncreaseAmount;
        isScaling = true;
    }

    public void ResetScaling()
    {
        targetScale = originalScale;
        isFloating = true;
        isScaling = true;
    }
}
