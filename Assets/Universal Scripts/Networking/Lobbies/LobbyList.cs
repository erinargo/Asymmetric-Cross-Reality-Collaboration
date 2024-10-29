using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

using System.Threading.Tasks;

/**
 * LobbyList handles Populating a Lobby List UI and Handles Joining a Game
 */
public class LobbyList : MonoBehaviour {
    [SerializeField] private LobbyItem lobbyItemPrefab;
    [SerializeField] private Transform lobbyItemParent;
    
    private bool _isRefreshing;
    private bool _isJoining;
    
    private void OnEnable() {
        RefreshList();
    }

    public async void RefreshList() {
        if (_isRefreshing) return;
        _isRefreshing = true;

        try {
            var options = new QueryLobbiesOptions();
            options.Count = 2;

            options.Filters = new List<QueryFilter>() {
                new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"
                    ),
                new QueryFilter(
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0"
                    ) 
            };

            var lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

            foreach (Transform child in lobbyItemParent) {
                Destroy(child.gameObject);
            }

            foreach (Lobby lobby in lobbies.Results) {
                var lobbyInstance = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyInstance.Init(this, lobby);
            }
        }
        catch(LobbyServiceException e) {
            Debug.Log(e);
            _isRefreshing = false;
            throw;
        }

        _isRefreshing = false;
    }
    
    public async void JoinAsync(Lobby lobby) {
        if (_isJoining) return;
        _isJoining = true;

        try {
            var joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joiningLobby.Data["JoinCode"].Value;

            Debug.Log(joinCode);

            await RelayManager.Singleton.JoinRelay(joinCode);
        }
        catch (LobbyServiceException e) {
            Debug.Log(e);
            throw;
        }

        _isJoining = false;
    }
}
