using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    private Vector3 correctPosition;
    private bool isDragging = false;
    private bool isPlaced = false;
    private Vector3 offset;
    private float minDistance = 1.0f;
    private PuzzleGameManager puzzleGameManager;

    public void Initialize(Vector3 position, PuzzleGameManager puzzleGameManager)
    {
        correctPosition = position;
        this.puzzleGameManager = puzzleGameManager;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (isPlaced) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pointerPos = GetWorldPosition();
            if (IsPointerOverPiece(pointerPos))
            {
                AudioManager.Instance.PlaySound(SoundType.Puzzle_Pick);

                isDragging = true;
                offset = transform.position - pointerPos;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            CheckPlacement();
        }

        if (isDragging)
        {
            Vector3 pointerPos = GetWorldPosition();
            transform.position = pointerPos + offset;
        }
    }

    private Vector3 GetWorldPosition()
    {
        Vector3 pointerPos = Input.mousePosition;
        pointerPos.z = 10f;
        return Camera.main.ScreenToWorldPoint(pointerPos);
    }

    private bool IsPointerOverPiece(Vector3 pointerPosition)
    {
        Collider2D hit = Physics2D.OverlapPoint(pointerPosition);
        return hit != null && hit.gameObject == gameObject;
    }

    private void CheckPlacement()
    {
        if (Vector3.Distance(transform.position, correctPosition) < minDistance)
        {
            AudioManager.Instance.PlaySound(SoundType.Puzzle_Put);

            transform.position = correctPosition;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = 4;
            isPlaced = true;
            puzzleGameManager.CheckGameProgress();
        }
    }
}
