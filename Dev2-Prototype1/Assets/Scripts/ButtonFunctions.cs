using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
   public void resume()
    {
        gamemanager.instance.UnPauseState();
    }
    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gamemanager.instance.UnPauseState();
    }
    public void Quit()
    {
    #if UNITY_EDITOR
          UnityEditor.EditorApplication.isPlaying = false;
    #else
             Application.Quit();
    #endif

    }
}
