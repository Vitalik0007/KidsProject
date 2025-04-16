using UnityEngine;

[CreateAssetMenu(fileName = "NewColoringLevel", menuName = "Game/Coloring Level Data")]
public class ColoringLevelData : ScriptableObject
{
    public Sprite backgroundImg;
    public Sprite middleLayerImg;
    public Sprite coloringBook;
    public Sprite example;
    public int levelRegions = 0;
}
