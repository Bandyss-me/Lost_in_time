using TMPro;
using UnityEngine;

public class ShowLobbyCode : MonoBehaviour
{
    void Start()
    {
        GetComponent<TMP_Text>().text = "Code: " + LobbyManager.LobbyCode;
    }
}