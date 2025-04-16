using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSilhouetteLevel", menuName = "Game/Silhouette Level Data")]
public class SilhouetteLevelData : ScriptableObject
{
    public Sprite backgroundImg;
    public Sprite frontLayer;
    public Sprite silhouettesPanelImg;
    public List<Sprite> stickerSprites;
    public List<Sprite> silhouetteSprites;
}