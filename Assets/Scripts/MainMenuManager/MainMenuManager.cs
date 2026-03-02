using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene vào game")]
    public string gameplayScene = "Demo_Terrain";

    [Header("UI Panels")]
    public GameObject guidePanel;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        if (guidePanel != null) guidePanel.SetActive(false);
    }

    public void StartGame()
    {
        // vào game thì khóa chuột để chơi (hợp camera)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!Application.CanStreamedLevelBeLoaded(gameplayScene))
        {
            Debug.LogError($"MainMenu: Scene '{gameplayScene}' chưa add vào Build Settings!");
            return;
        }

        SceneManager.LoadScene(gameplayScene);
    }

    public void OpenGuide()
    {
        if (guidePanel != null) guidePanel.SetActive(true);
    }

    public void CloseGuide()
    {
        if (guidePanel != null) guidePanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game (chỉ hoạt động khi Build)");
    }
}