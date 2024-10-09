using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using TMPro;

using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

using System.Threading.Tasks;

/**
 * TODO, ERIN:
 * Cleanup + move menu logic to other scripts
 */

public class RelayManager : MonoBehaviour
{
    public static RelayManager Singleton { get; private set; }
    private static string joinCode;

    [SerializeField] private string hostScreen = "Host";
    [SerializeField] private string mainMenu = "MMenu";
    [SerializeField] private string clientScreen = "Join";
    [SerializeField] private string loadingScreen = "Loading";
    [SerializeField] public string gameplayScene = "Start";
    [SerializeField] private TMP_Text joinCodeContainer;
    private bool hosting = false;
    private bool joined = false;
    private bool started = false;
    private string previousScreen;

    public string JoinCode => joinCode;

    private string lobbyId;

    public Dictionary<ulong, ClientData> ClientData { get; private set; }

    void Awake() {
        if (Singleton != null && Singleton != this) Destroy(this.gameObject);
        else Singleton = this;
    }

    void Update() {
        if (SceneManager.GetActiveScene().name == "Host") joinCodeContainer = GameObject.Find("JoinCode").GetComponent<TMP_Text>();
        if (joinCodeContainer != null) joinCodeContainer.text = "Join Code: " + joinCode;
    }

    async void Start() {
        DontDestroyOnLoad(this.gameObject);
        await UnityServices.InitializeAsync(); // JAVASCRIPT HAS COME TO BITE ME IN THE ASS . . . AGAIN. FML.

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void Host() {
        try {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

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
                            value: joinCode
                        )
                    }
                };

                Lobby lobby = await Lobbies.Instance.CreateLobbyAsync("Game", 13, createLobbyOption);
                lobbyId = lobby.Id;

                StartCoroutine(HeartBeat(15f));
            }
            catch(LobbyServiceException e) {
                Debug.Log(e);
                throw;
            }

            // END LOBBY
            ClientData = new Dictionary<ulong, ClientData>();
            NetworkManager.Singleton.StartHost();
            
            hosting = true;

            previousScreen = "MMenu";
        } catch(RelayServiceException e) {
            Debug.LogError(e);
        }
        
    }

    private bool ErinLovesAdam() {
        return true;
    }

    IEnumerator HeartBeat(float timeSecs) {
        var delay = new WaitForSeconds(timeSecs);

        while (ErinLovesAdam()) {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnNetworkReady() {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.SceneManager.LoadScene(hostScreen, LoadSceneMode.Single);
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        if(ClientData.Count > 13 || started) {
            response.Approved = false;
            return;
        }

        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;

        ClientData[request.ClientNetworkId] = new ClientData(request.ClientNetworkId);
    }

    public void Join() {
        SceneManager.LoadScene(clientScreen, LoadSceneMode.Single);
        hosting = false;

        previousScreen = "MMenu";
    }

    public async Task JoinRelay(string join) {
        joinCode = join.Substring(0, 6); // Relay, TMP? 
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        if(!started) NetworkManager.Singleton.StartClient();

        joined = true;
    }

    public void MainMenu() {
        Debug.Log("Main Menu Called");
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        SceneManager.LoadScene(mainMenu, LoadSceneMode.Single);

        hosting = false;
        joined = false;
    }

    public void Quit() {
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();

        Application.Quit();
    }

    public void Back() {
        if (hosting || joined) {

            // TODO: Kick everyone in Lobby
            NetworkManager.Singleton.Shutdown();
            if (hosting) {
                NetworkManager.Singleton.SceneManager.LoadScene(previousScreen, LoadSceneMode.Single);
                Destroy(NetworkManager.Singleton.gameObject);
            } else {
                Destroy(NetworkManager.Singleton.gameObject);
                SceneManager.LoadScene(previousScreen, LoadSceneMode.Single);
            }

            hosting = false;
            joined = false;
        }
        else {
            SceneManager.LoadScene(previousScreen, LoadSceneMode.Single);
        }
    }

    public void OnClientDisconnect(ulong clientId) {
        if (ClientData.ContainsKey(clientId)) {
            ClientData.Remove(clientId); //Debug.Log($"Removed ClientId {clientId}");
        }
    }

    public void OnClientConnect(ulong clientId) {
        ClientData[clientId] = new ClientData(clientId);
    }

    public void SetCharacter(ulong clientId, int characterId) {
        if (ClientData.TryGetValue(clientId, out ClientData data)) {
            data.CharacterId = characterId;
        }
    }

    public int GetCharacterByClientId(ulong clientId) {
        if(ClientData.TryGetValue(clientId, out ClientData data)) return data.CharacterId;
        return -1;
    }

    public List<ClientData> GetAllPlayers() {
        List<ClientData> players = new List<ClientData>();

        foreach(var client in this.ClientData) {
            players.Add(client.Value);
        }

        return players;
    }

    public void StartGame() { 
        NetworkManager.Singleton.SceneManager.LoadScene(gameplayScene, LoadSceneMode.Single);
    }
}
