using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
   public void resume()
    {
        gamemanager.instance.stateUnpause();
    }
    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gamemanager.instance.stateUnpause();
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
