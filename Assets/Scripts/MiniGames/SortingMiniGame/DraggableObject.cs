using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableObject : MonoBehaviour
{
    private Vector2 startPosition;
    private float startZ;
    private Collider2D targetCollider;
    public ColorType colorType;
    private bool isDragging = false;

    private SortingGameManager sortingGameManager;

    private void Start()
    {
        startPosition = transform.position;
        startZ = transform.position.z;
    }

    public void Initialize(SortingGameManager sortingGameManager)
    {
        this.sortingGameManager = sortingGameManager;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsTouchingUI())
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(mousePos, transform.position) < 1.0f)
            {
                AudioManager.Instance.PlaySound(SoundType.ColorMatch_Pick);
                isDragging = true;
            }
        }

        if (isDragging)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            newPosition.z = startZ;
            transform.position = newPosition;
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            HandleDrop();
        }
    }

    private void HandleDrop()
    {
        if (targetCollider != null)
        {
            AudioManager.Instance.PlaySound(SoundType.ColorMatch_Press2);

            MainObject mainObject = targetCollider.GetComponent<MainObject>();
            if (mainObject != null && mainObject.colorType == colorType)
            {
                transform.position = mainObject.transform.position;
                gameObject.SetActive(false);
                sortingGameManager.CheckGameProgress();
            }
            else
            {
                transform.position = startPosition;
                mainObject?.ResetScaling();
            }
        }
        else
        {
            AudioManager.Instance.PlaySound(SoundType.ColorMatch_WrongPress);

            transform.position = startPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out MainObject mainObject))
        {
            if (mainObject.colorType == colorType)
            {
                targetCollider = collision;
                mainObject.StartScaling();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (targetCollider == collision)
        {
            targetCollider = null;
            collision.GetComponent<MainObject>()?.ResetScaling();
        }
    }

    private bool IsTouchingUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
