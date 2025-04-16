using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPuzzleLevel", menuName = "Game/Puzzle Level Data")]
public class PuzzleLevelData : ScriptableObject
{
    public Sprite backgroundImg;
    public Sprite frontLayerImg;

    public List<PuzzleRoundData> roundsData;
}

[System.Serializable]
public class PuzzleRoundData
{
    public Texture2D puzzleImg;
    public Texture2D maskImg;
}