using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneSelector_script : MonoBehaviour
{
    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
