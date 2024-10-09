using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

using System.Threading.Tasks;

public class LobbyList : MonoBehaviour {
    [SerializeField] private LobbyItem _lobbyItemPrefab;
    [SerializeField] private Transform lobbyItemParent;
    
    private bool isRefreshing;
    private bool isJoining;
    
    private void OnEnable() {
        RefreshList();
    }

    public async void RefreshList() {
        if (isRefreshing) return;
        isRefreshing = true;

        try {
            var options = new QueryLobbiesOptions();
            options.Count = 25;

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
                var lobbyInstance = Instantiate(_lobbyItemPrefab, lobbyItemParent);
                lobbyInstance.Init(this, lobby);
            }
        }
        catch(LobbyServiceException e) {
            Debug.Log(e);
            isRefreshing = false;
            throw;
        }

        isRefreshing = false;
    }
    
    public async void JoinAsync(Lobby lobby) {
        if (isJoining) return;
        isJoining = true;

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

        isJoining = false;
    }
}
