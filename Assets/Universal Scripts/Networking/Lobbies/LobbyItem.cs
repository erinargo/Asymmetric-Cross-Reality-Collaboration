using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

/**
 * Custom GameObject
 */
public class LobbyItem : MonoBehaviour {
    [SerializeField] private TMP_Text name;
    [SerializeField] private TMP_Text capacity;

    private Lobby Lobby;
    private LobbyList LobbyList;

    public void Init(LobbyList llist, Lobby lobby) {
        this.Lobby = lobby;
        this.LobbyList = llist;

        name.text = lobby.Name;
        capacity.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void Join() {
        LobbyList.JoinAsync(Lobby);
    }
}