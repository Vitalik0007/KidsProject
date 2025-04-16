using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SortingGameManager : MonoBehaviour
{
    [SerializeField] private GameObject levelBackgroundImg;
    [SerializeField] private GameObject levelFrontLayerImg;
    [SerializeField] private GameObject levelMiddleLayerImg;
    [SerializeField] private GameObject levelMiddleFrontLayerImg;

    [SerializeField] private GameObject mainObjectPrefab;
    [SerializeField] private GameObject draggableObjectPrefab;

    private List<GameObject> spawnedMainObjects = new List<GameObject>();
    private List<GameObject> spawnedDraggableObjects = new List<GameObject>();

    [SerializeField] private Transform mainObjectsParent;
    [SerializeField] private Transform draggableObjectsParent;

    [SerializeField] private int totalRounds = 10;
    private int currentRound = 0;
    private int totalDraggableObjects = 4;
    private int collectedDraggableObjects = 0;
    private SortingLevelData currentLevelData;
    private int levelIndex;

    [SerializeField] private MiniGameProgressManager miniGameProgressManager;

    private void Start()
    {
        LoadLevelData();
        SpawnMainObjects();
        StartRound();
    }

    private void LoadLevelData()
    {
        levelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);
        currentLevelData = Resources.Load<SortingLevelData>($"Levels/SortingMiniGame/SortingMiniGame{levelIndex + 1}");

        if (currentLevelData == null)
        {
            Debug.LogError($"Level data for level {levelIndex} not found!");
        }
    }

    private void StartRound()
    {
        if (currentRound >= totalRounds)
        {
            Debug.Log("Level Completed!!!");
            currentRound = 0;
            //ClearMainObjects();
            //ClearDraggableObjects();
            miniGameProgressManager.ShowWinAnimation();
            return;
        }

        levelBackgroundImg.GetComponent<SpriteRenderer>().sprite = currentLevelData.backgroundImg;
        levelFrontLayerImg.GetComponent<SpriteRenderer>().sprite = currentLevelData.frontLayerImg;

        if (currentLevelData.middleLayerImg != null)
        {
            levelMiddleLayerImg.SetActive(true);
            levelMiddleLayerImg.GetComponent<SpriteRenderer>().sprite = currentLevelData.middleLayerImg;
        }
        else
            levelMiddleLayerImg.SetActive(false);

        if (currentLevelData.middleFrontLayerImg != null)
        {
            levelMiddleFrontLayerImg.SetActive(true);
            levelMiddleFrontLayerImg.GetComponent<SpriteRenderer>().sprite = currentLevelData.middleFrontLayerImg;
        }
        else
            levelMiddleFrontLayerImg.SetActive(false);

        ClearDraggableObjects();
        SpawnDraggableObjects();
    }

    private void SpawnDraggableObjects()
    {
        if (currentRound < currentLevelData.draggableObjectSets.Count)
        {
            List<Sprite> currentDraggableSet = new List<Sprite>(currentLevelData.draggableObjectSets[currentRound].draggableSprites);
            List<GameObject> draggableObjects = new List<GameObject>();

            for (int i = 0; i < currentDraggableSet.Count; i++)
            {
                GameObject draggableObj = Instantiate(
                    draggableObjectPrefab,
                    currentLevelData.draggableObjectPositions[i],
                    Quaternion.Euler(0, 0, Random.Range(0f, 360f)),
                    draggableObjectsParent
                );

                draggableObj.GetComponent<SpriteRenderer>().sprite = currentDraggableSet[i];
                draggableObj.GetComponent<DraggableObject>().colorType = currentLevelData.draggableColors[i];
                draggableObj.GetComponent<DraggableObject>().Initialize(this);

                draggableObj.transform.localScale = Vector3.zero;

                draggableObj.transform.DOScale(Vector3.one * 0.4f, 0.5f).SetEase(Ease.OutBack);

                draggableObjects.Add(draggableObj);
            }

            for (int i = draggableObjects.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                Vector3 tempPos = draggableObjects[i].transform.position;
                Quaternion tempRot = draggableObjects[i].transform.rotation;

                draggableObjects[i].transform.position = draggableObjects[randomIndex].transform.position;
                draggableObjects[i].transform.rotation = draggableObjects[randomIndex].transform.rotation;

                draggableObjects[randomIndex].transform.position = tempPos;
                draggableObjects[randomIndex].transform.rotation = tempRot;
            }

            spawnedDraggableObjects.AddRange(draggableObjects);
        }
        else
        {
            Debug.LogWarning("Not enough objects!");
        }
    }

    private void SpawnMainObjects()
    {
        for (int i = 0; i < currentLevelData.mainObjectSprites.Count; i++)
        {
            GameObject mainObj = Instantiate(mainObjectPrefab, currentLevelData.mainObjectPositions[i], Quaternion.identity, mainObjectsParent);
            mainObj.GetComponent<SpriteRenderer>().sprite = currentLevelData.mainObjectSprites[i];
            mainObj.GetComponent<MainObject>().colorType = currentLevelData.mainObjectColors[i];
            spawnedMainObjects.Add(mainObj);

            if ((levelIndex == 2 || levelIndex == 3) && (i == 0 || i == 3))
            {
                mainObj.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }

            if ((levelIndex == 0 || levelIndex == 1 || levelIndex == 4 || levelIndex == 5 || levelIndex == 6 || levelIndex == 7 || levelIndex == 8) && (i == 1 || i == 2))
            {
                mainObj.GetComponent<SpriteRenderer>().sortingOrder = 4;
            }
        }
    }

    private void ClearDraggableObjects()
    {
        foreach (var obj in spawnedDraggableObjects)
        {
            Destroy(obj);
        }
        spawnedDraggableObjects.Clear();
    }

    private void ClearMainObjects()
    {
        foreach (var obj in spawnedMainObjects)
        {
            Destroy(obj);
        }
        spawnedMainObjects.Clear();
    }

    public void CheckGameProgress()
    {
        collectedDraggableObjects++;

        if (collectedDraggableObjects == totalDraggableObjects)
        {
            collectedDraggableObjects = 0;
            currentRound++;
            StartRound();
        }
    }
}
