using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] string lobbyName = "Lobby";
    [SerializeField] int maxPlayers = 10;
    [SerializeField] private TMP_InputField joinCode;
    
    public static string LobbyCode;
    Lobby currentLobby;
    float heartbeatTimer;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    void Update()
    {
        if (currentLobby == null || currentLobby.HostId != AuthenticationService.Instance.PlayerId) return;
        heartbeatTimer -= Time.deltaTime;
        if (heartbeatTimer <= 0f)
        {
            heartbeatTimer = 15f;
            LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
        }
    }

    public async void CreateLobby()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
        string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        var options = new CreateLobbyOptions
        {
            IsPrivate = false,
            Data = new Dictionary<string, DataObject>
            {
                { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
            }
        };
        currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
        LobbyCode = currentLobby.LobbyCode;
        Debug.Log("Lobby code to share: " + currentLobby.LobbyCode);
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetHostRelayData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData);
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("demo2", UnityEngine.SceneManagement.LoadSceneMode.Single); 
    }

    public async void JoinLobbyByCode()
    {
        currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode.text);
        LobbyCode = currentLobby.LobbyCode;
        string relayCode = currentLobby.Data["RelayCode"].Value;
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayCode);
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetClientRelayData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData,
            allocation.HostConnectionData);
        NetworkManager.Singleton.StartClient();
    }
    
    public async void StopSession()
    {
        try
        {
            if (currentLobby != null)
            {
                if (currentLobby.HostId == AuthenticationService.Instance.PlayerId)
                    await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
                else
                    await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
        currentLobby = null;
        LobbyCode = null;
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}