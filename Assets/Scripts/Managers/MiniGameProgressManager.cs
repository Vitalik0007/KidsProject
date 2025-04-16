using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameProgressManager : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    [SerializeField] private Button menuButton;

    [SerializeField] private SkeletonGraphic starAnimation;
    private string starAnimName = "animation";

    private void Start()
    {
        nextButton.onClick.AddListener(LoadNextMiniGame);
        menuButton.onClick.AddListener(ReturnToMenu);

        nextButton.gameObject.SetActive(false);
        menuButton.gameObject.SetActive(false);

        if (starAnimation != null)
        {
            starAnimation.gameObject.SetActive(false);
        }
    }

    public void ShowWinAnimation()
    {
        int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 0);
        int currentMiniGame = PlayerPrefs.GetInt($"Level_LastGameIndex_{currentLevel}", 0);

        string completionKey = $"Level_{currentLevel}_MiniGame_{currentMiniGame}_Completed";
        PlayerPrefs.SetInt(completionKey, 1);
        PlayerPrefs.Save();

        if (starAnimation != null)
        {
            starAnimation.gameObject.SetActive(true);
            AudioManager.Instance.PlaySound(SoundType.Star_Win);
            starAnimation.AnimationState.SetAnimation(0, starAnimName, false);
        }

        Invoke(nameof(ShowWinButtons), 1.5f);
    }

    private void ShowWinButtons()
    {
        int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 0);
        int currentMiniGame = PlayerPrefs.GetInt($"Level_LastGameIndex_{currentLevel}", 0);

        if (currentMiniGame >= 3)
        {
            nextButton.gameObject.SetActive(false);
            menuButton.gameObject.SetActive(true);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
            menuButton.gameObject.SetActive(false);
        }
    }

    private void LoadNextMiniGame()
    {
        int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 0);
        int currentMiniGame = PlayerPrefs.GetInt($"Level_LastGameIndex_{currentLevel}", 0);

        int nextMiniGame = currentMiniGame + 1;

        PlayerPrefs.SetInt("SelectedMiniGame", nextMiniGame);
        PlayerPrefs.SetInt($"Level_LastGameIndex_{currentLevel}", nextMiniGame);
        PlayerPrefs.Save();

        MiniGameLoader.MiniGameType nextType = (MiniGameLoader.MiniGameType)nextMiniGame;
        switch (nextType)
        {
            case MiniGameLoader.MiniGameType.ColorSorting:
                SceneManager.LoadScene("SortingMiniGame");
                break;
            case MiniGameLoader.MiniGameType.SilhouetteMatching:
                SceneManager.LoadScene("SilhouetteMiniGame");
                break;
            case MiniGameLoader.MiniGameType.Puzzle:
                SceneManager.LoadScene("PuzzleMiniGame");
                break;
            case MiniGameLoader.MiniGameType.Coloring:
                SceneManager.LoadScene("ColoringMiniGame");
                break;
        }
    }

    private void ReturnToMenu()
    {
        AudioManager.Instance.PlaySound(SoundType.Star_Menu);
        SceneManager.LoadScene(0);
    }
}
