#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SortingLevelsEditor : EditorWindow
{
    private Transform levelsRoot;
    private List<SortingLevelData> levelDataList = new List<SortingLevelData>();

    [MenuItem("Tools/Sorting Levels Position Saver")]
    public static void ShowWindow()
    {
        GetWindow<SortingLevelsEditor>("Sorting Levels Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Sorting Levels Position Saver", EditorStyles.boldLabel);

        levelsRoot = (Transform)EditorGUILayout.ObjectField("Levels Root", levelsRoot, typeof(Transform), true);

        EditorGUILayout.LabelField("Sorting Level Data List:", EditorStyles.boldLabel);
        int newSize = Mathf.Max(0, EditorGUILayout.IntField("Number of Levels", levelDataList.Count));

        while (newSize > levelDataList.Count)
            levelDataList.Add(null);
        while (newSize < levelDataList.Count)
            levelDataList.RemoveAt(levelDataList.Count - 1);

        for (int i = 0; i < levelDataList.Count; i++)
        {
            levelDataList[i] = (SortingLevelData)EditorGUILayout.ObjectField($"Level {i + 1}", levelDataList[i], typeof(SortingLevelData), false);
        }

        if (GUILayout.Button("Save All Levels Positions"))
        {
            SaveAllLevelsPositions();
        }
    }

    private void SaveAllLevelsPositions()
    {
        if (levelsRoot == null || levelDataList.Count == 0)
        {
            Debug.LogError("Levels Root або список LevelData не призначено!");
            return;
        }

        for (int i = 0; i < levelDataList.Count; i++)
        {
            if (levelDataList[i] == null)
            {
                Debug.LogError($"SortingLevelData для рівня {i + 1} не призначено!");
                continue;
            }

            Transform levelTransform = levelsRoot.Find($"Level_{i + 1}");
            if (levelTransform == null)
            {
                Debug.LogError($"Level_{i + 1} не знайдено у сцені!");
                continue;
            }

            Transform mainObjectsRoot = levelTransform.Find("MainObjects");
            Transform draggableObjectsRoot = levelTransform.Find("DraggableObjects");

            if (mainObjectsRoot == null || draggableObjectsRoot == null)
            {
                Debug.LogError($"Level_{i + 1} не має об'єктів MainObjects або DraggableObjects!");
                continue;
            }

            levelDataList[i].mainObjectPositions.Clear();
            levelDataList[i].draggableObjectPositions.Clear();

            foreach (Transform obj in mainObjectsRoot)
                levelDataList[i].mainObjectPositions.Add(obj.position);

            foreach (Transform obj in draggableObjectsRoot)
                levelDataList[i].draggableObjectPositions.Add(obj.position);

            EditorUtility.SetDirty(levelDataList[i]);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Позиції для всіх рівнів збережено!");
    }
}
#endif