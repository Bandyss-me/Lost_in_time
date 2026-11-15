using UnityEngine;
using UnityEngine.SceneManagement;

public class EOLCustomLvl1 : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("LevelSelectorPage");
    }
}