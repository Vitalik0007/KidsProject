using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ExitManager : MonoBehaviour
{
    private Button exitButton;

    private void Start()
    {
        exitButton = GetComponent<Button>();
        exitButton.onClick.AddListener(Exit);
    }

    private void Exit()
    {
        AudioManager.Instance.PlaySound(SoundType.Button_Click);

        SceneManager.LoadScene(0);
    }
}
