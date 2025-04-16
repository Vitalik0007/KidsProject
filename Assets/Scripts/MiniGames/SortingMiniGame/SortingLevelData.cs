using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSortingLevel", menuName = "Game/Sorting Level Data")]
public class SortingLevelData : ScriptableObject
{
    [System.Serializable]
    public class DraggableObjectSet
    {
        public List<Sprite> draggableSprites;
    }

    public Sprite backgroundImg;
    public Sprite frontLayerImg;
    public Sprite middleLayerImg;
    public Sprite middleFrontLayerImg;
    public List<DraggableObjectSet> draggableObjectSets = new List<DraggableObjectSet>();
    public List<ColorType> draggableColors;
    public List<Sprite> mainObjectSprites;
    public List<ColorType> mainObjectColors;

    public List<Vector2> mainObjectPositions = new List<Vector2>(4);
    public List<Vector2> draggableObjectPositions = new List<Vector2>(4);
}