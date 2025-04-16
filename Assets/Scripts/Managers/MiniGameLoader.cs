using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameLoader : MonoBehaviour
{
    public enum MiniGameType
    {
        ColorSorting,
        SilhouetteMatching,
        Puzzle,
        Coloring
    }

    private MiniGameType selectedMiniGame;

    public void LoadMiniGame(int miniGameIndex)
    {
        selectedMiniGame = (MiniGameType)miniGameIndex;

        PlayerPrefs.SetInt("SelectedMiniGame", (int)selectedMiniGame);
        PlayerPrefs.SetInt($"Level_LastGameIndex_{PlayerPrefs.GetInt("SelectedLevel")}", miniGameIndex);
        PlayerPrefs.Save();

        switch (selectedMiniGame)
        {
            case MiniGameType.ColorSorting:
                SceneManager.LoadScene("SortingMiniGame");
                break;
            case MiniGameType.SilhouetteMatching:
                SceneManager.LoadScene("SilhouetteMiniGame");
                break;
            case MiniGameType.Puzzle:
                SceneManager.LoadScene("PuzzleMiniGame");
                break;
            case MiniGameType.Coloring:
                SceneManager.LoadScene("ColoringMiniGame");
                break;
        }
    }
}
