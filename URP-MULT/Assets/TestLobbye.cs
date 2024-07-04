using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Rendering;

public class TestLobbye : MonoBehaviour
{
    private Lobby Hostlobby;
    private Lobby joinlobby;
    private float heartbeatTimer;
    private float lobbyUpdatetimer;
    private string PlayerName;

    public Lobby HostLobby { get; private set; }

    private async void Start()
    {
       await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Sign in" + AuthenticationService.Instance.PlayerId);

        };

       await AuthenticationService.Instance.SignInAnonymouslyAsync();
       PlayerName = "TestPlayer" + UnityEngine.Random.Range(10, 99);
       Debug.Log(PlayerName);
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyForUpdates();
    }

    private async void HandleLobbyHeartbeat()
    {
        if(Hostlobby != null)
        {
            heartbeatTimer -= Time.time;
            if(heartbeatTimer < 0)
            {
                float heartbeatTimerMax = 15;
                    heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(Hostlobby.Id);
            }
        }
    }

    private async void HandleLobbyForUpdates()
    {
        if (joinlobby != null)
        {
            lobbyUpdatetimer -= Time.time;
            if (lobbyUpdatetimer < 0)
            {
                float lobbyUpdatetimerMax = 1-1;
                lobbyUpdatetimer = lobbyUpdatetimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinlobby.Id);
                joinlobby = lobby;
            }
        }
    }

    [Command]
    private async void CreateLobby()
    {
        try { 

        string lobbyName = "MyLobby";
        int MaxPlayers = 100;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"Gamemode", new DataObject(DataObject.VisibilityOptions.Public, "MED") },
                    {"Map", new DataObject(DataObject.VisibilityOptions.Public, "eye") }
                    //{"Gamemode", new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag", DataObject.IndexOptions.S1) }
                }
               
            };
        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MaxPlayers,createLobbyOptions);

        Hostlobby = lobby;
            joinlobby = Hostlobby;

        Debug.Log("Create Lobby --"+ lobby.Name+" "+lobby.MaxPlayers + " "+ lobby.Id+" "+lobby.LobbyCode);
        PrintPlayers(Hostlobby);

        } catch(LobbyServiceException e) { Debug.Log(e); }



    }

    [Command]
    private async void ListLobby()
    {
        try {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter> { 
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    //new QueryFilter(QueryFilter.FieldOptions.S1, "MED", QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log("Lobbies found:"+ queryResponse.Results.Count);

            foreach (Lobby lobby in queryResponse.Results) {
                Debug.Log(lobby.Name+" "+lobby.MaxPlayers + " " + lobby.Data["Gamemode"].Value +" "+ lobby.Data["Map"].Value);
            }

        }catch(LobbyServiceException e) { Debug.Log(e); }
    }

    [Command]
    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            //await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinlobby = lobby;

            Debug.Log("Joined lobby with code" + lobbyCode);

            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e) { Debug.Log(e); }


        
    }

    private Player GetPlayer() 
    { 
     return new Player()
     {
         Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName) }
                    }

     };
    }


    [Command]
    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException ex) { Debug.Log(ex); }

    }

    private void PrintPlayers()
    {
        PrintPlayers(joinlobby);
    }

    public void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Player in lobby: " + lobby.Name);
            foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value+ " "+ lobby.Data["Gamemode"].Value);
        }
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            Hostlobby = await Lobbies.Instance.UpdateLobbyAsync(Hostlobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                {"Gamemode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
            }
            });
            joinlobby = Hostlobby;
            PrintPlayers(Hostlobby);
        }
        catch (LobbyServiceException ex) { Debug.Log(ex); }
    }

    private async void UpdatePlayerNames(string newPlayerName)
    {
        try
        {
            PlayerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinlobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions {
                Data = new Dictionary<string, PlayerDataObject> {
                    {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName) }
                }
            });

        }
        catch (LobbyServiceException ex) { Debug.Log(ex); }
    }

    private async void LeaveLobby()
    {
        try {
            await LobbyService.Instance.RemovePlayerAsync(joinlobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException ex) { Debug.Log(ex); }
    }

    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinlobby.Id, joinlobby.Players[1].Id);
        }
        catch (LobbyServiceException ex) { Debug.Log(ex); }
    }

    private async void MigrateLobbyHost()
    {
        try
        {
            Hostlobby = await Lobbies.Instance.UpdateLobbyAsync(Hostlobby.Id, new UpdateLobbyOptions
            {
                HostId = joinlobby.Players[1].Id
            });
            joinlobby = Hostlobby;
            PrintPlayers(Hostlobby);
        }
        catch (LobbyServiceException ex) { Debug.Log(ex); }
    }

    private async void DelateLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinlobby.Id);
        }
        catch (LobbyServiceException ex) { Debug.Log(ex); }
    }
}
