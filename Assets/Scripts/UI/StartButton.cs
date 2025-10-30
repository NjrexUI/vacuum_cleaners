using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void StartGame() 
    {
        SceneManager.LoadScene("Cutscene");
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("Main");
    }
}
