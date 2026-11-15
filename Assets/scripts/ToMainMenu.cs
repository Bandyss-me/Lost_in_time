using Unity.Netcode;
using UnityEngine;

public class ToMainMenu : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
            NetworkManager.Singleton.GetComponent<LobbyManager>().StopSession();
    }
}
