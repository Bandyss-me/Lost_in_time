using System;
using Unity.Netcode;
using UnityEngine;

public class win_script : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        NetworkManager.Singleton.GetComponent<LobbyManager>().StopSession();
    }
}
