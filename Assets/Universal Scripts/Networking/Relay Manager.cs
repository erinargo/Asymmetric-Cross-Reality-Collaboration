using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

using UnityEngine;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;

using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

/**
 * Connects to Relay Service
 * 
 * Quick Rundown on Relay:
 * P2P service offered by Unity
 * Allows us to use Netcode and the Network Manager to create a multiplayer experience without having
 * to host servers
 *
 * +Singleton is used for exposing relaymanager gameobject as an interface
 * -joinCode is used for connecting clients to the correct lobby
 * -maxPlayers is used to control how many clients can connect to the lobby
 */
public class RelayManager : MonoBehaviour
{
    public static RelayManager Singleton { get; private set; }
    private string _lobbyId;
    private static string _joinCode;
    
    [SerializeField]
    private int maxPlayers = 3;

    // Exposes joinCode;
    public string JoinCode => _joinCode;
    
    // Exposes maxPlayers;
    public int MaxPlayers => maxPlayers;
    
    public Dictionary<ulong, ClientData> ClientData { get; private set; }
    
    /**
     * -Awake(): Void
     * ; Inits Class
     */
    
    void Awake() {
        if (Singleton != null && Singleton != this) Destroy(this.gameObject);
        else Singleton = this;
    }

    /**
     * -Start(): Void
     * ; DontDestroyOnLoad (Persists between Scenes)
     * ; Connects to UnityServices
     * ; Signs in Individual Client
     */
    
    async void Start() {
        DontDestroyOnLoad(this.gameObject);
        await UnityServices.InitializeAsync(); 

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    /**
     * +Host(): Void
     * ; Inits Connection Criteria
     * ; Starts Lobby
     * ; Populates ClientData
     */
    
    public async void Host() {
        try {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            // LOBBY
            try {
                var createLobbyOption = new CreateLobbyOptions();
                createLobbyOption.IsPrivate = false;
                createLobbyOption.Data = new Dictionary<string, DataObject>() {
                    {
                        "JoinCode", new DataObject( 
                            visibility: DataObject.VisibilityOptions.Member,
                            value: _joinCode
                        )
                    }
                };

                Lobby lobby = await Lobbies.Instance.CreateLobbyAsync("lobbyName", 2, createLobbyOption);
                _lobbyId = lobby.Id;

                StartCoroutine(HeartBeat(15f));
            }
            catch(LobbyServiceException e) {
                Debug.Log(e);
                throw;
            }
            
            Debug.Log("Lobby Code: " + _lobbyId);

            // END LOBBY
            ClientData = new Dictionary<ulong, ClientData>();
            NetworkManager.Singleton.StartHost();
        } catch(RelayServiceException e) {
            Debug.LogError(e);
        }
        
    }
    
    /**
     * -HeartBeat(timeSecs: float): IEnumerator
     * ; Pings Relay Service to Keep Connection Awake
     * ; Ah ah ah ah stayin' alive
     * ; stayin' alive
     */
    IEnumerator HeartBeat(float timeSecs) {
        var delay = new WaitForSeconds(timeSecs);

        while (true) {
            Lobbies.Instance.SendHeartbeatPingAsync(_lobbyId);
            yield return delay;
        }
    }

    /**
     * -OnNetworkReady(): Void
     * ; Callback for Network Init
     * ; Assigns Client Connection and Disconnection Callbacks 
     */
    private void OnNetworkReady() {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }
    
    /**
     * -ApprovalCheck(request: ConnectionApprovalRequest, response: ConnectionApprovalResponse): Void
     * ; Checks if client is allowed to join lobby
     */
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        if(ClientData.Count > maxPlayers) {
            response.Approved = false;
            return;
        }

        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;

        ClientData[request.ClientNetworkId] = new ClientData(request.ClientNetworkId);
    }

    /**
     * +JoinRelay(join: String) Void
     * ; Joins a Lobby using the join code
     * ; NOTE: this join code is automatically populated by the lobby service
     */
    public async Task JoinRelay(string join) {
        _joinCode = join.Substring(0, 6);
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(_joinCode);

        RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();
    }

    /**
     * -ShutDown(): Void
     * ; safely closes connection before disconnect and application quit
     */
    private void ShutDown() {
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        // Properly DC?
    }

    /**
     * +Quit(): Void
     * ; Closes Application
     */
    public void Quit() {
        ShutDown();

        Application.Quit();
    }

    /**
     * +OnClientDisconnect(clientId: ulong): Void
     * ; Removes client data from ClientData
     */
    public void OnClientDisconnect(ulong clientId) {
        if (ClientData.ContainsKey(clientId)) {
            ClientData.Remove(clientId); //Debug.Log($"Removed ClientId {clientId}");
        }
    }

    
    /**
     * +OnClientConnect(clientId: ulong): Void
     * ; Populates ClientData with client data
     */
    public void OnClientConnect(ulong clientId) {
        ClientData[clientId] = new ClientData(clientId);
    }

    /** Character Selection Code; Not necessary yet.
    public void SetCharacter(ulong clientId, int characterId) {
        if (ClientData.TryGetValue(clientId, out ClientData data)) {
            data.CharacterId = characterId;
        }
    }

    public int GetCharacterByClientId(ulong clientId) {
        if(ClientData.TryGetValue(clientId, out ClientData data)) return data.CharacterId;
        return -1;
    }
    */

    /**
     * +GetAllPlayers(): List<ClientData>
     * ; returns a list of clients
     * ; ClientData: ClientId
     * ; // MORE DATA WILL BE ADDED TO CLIENT DATA AS NEEDED BY GAME;
     */
    public List<ClientData> GetAllPlayers() {
        List<ClientData> players = new List<ClientData>();

        foreach(var client in this.ClientData) {
            players.Add(client.Value);
        }

        return players;
    }
}
