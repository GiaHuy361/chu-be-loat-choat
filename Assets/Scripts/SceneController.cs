using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Target scene name")]
    public string targetScene; // Nh? nh?p "InCave" vào ?ây trong Inspector

    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("SceneController: targetScene ch?a ???c set! Hãy nh?p tên Scene trong Inspector.");
            return;
        }

        // Ki?m tra xem Scene có trong Build Settings ch?a
        if (Application.CanStreamedLevelBeLoaded(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogError($"Scene '{targetScene}' ch?a ???c add vào Build Settings!");
        }
    }
}