using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour {
    [SerializeField] public NetworkObject playerPrefab;
    
    public static GameManager Singleton { get; private set; }
    public Camera mainCamera; 
    public OVRCameraRig ovrCameraRig;
    public Transform minimap;
    public Transform realMap;

    public Vector3 realOrigin;
    
    public GameObject player;

    [SerializeField] private Transform windowOfTime;
    [SerializeField] public GameObject connectorPrefab;

    [SerializeField] public Transform colocator;
    
    [SerializeField] private NetworkObject networkedCamera;
    [SerializeField] private RenderTexture renderTexture;

    public int car, bus, bike;
    public int solar, gas;
    public int recycle;

    [SerializeField] private GameObject BusHolder;
    [SerializeField] private GameObject CarHolder;
    [SerializeField] private GameObject BikeHolder;
    // [SerializeField] private GameObject SolarHolder;
    // [SerializeField] private GameObject RecycleHolder;
    // [SerializeField] private GameObject GasHolder;

    [SerializeField] public GameObject realMapBuildings;
    [SerializeField] public GameObject miniMapBuildings;

    private int processedCameras = 0;
    
    public float CarbonImpact = 1.0f; // 0-1
    
    public NetworkList<NetworkObjectReference> connectedCameras = new(writePerm: NetworkVariableWritePermission.Server);
    
    void Awake() {
        if (Singleton != null && Singleton != this) Destroy(this.gameObject);
        else Singleton = this;
    }
    
    [ServerRpc(RequireOwnership = false)] 
    public void SpawnPlayerObjectServerRpc(ulong clientId, Vector3 truePos, ServerRpcParams serverRpcParams = default) {
        if (!IsServer) return; 
        
        NetworkObject playerObject1 = Instantiate(GameManager.Singleton.playerPrefab);
        NetworkObject playerObject2 = Instantiate(GameManager.Singleton.playerPrefab);
        
        playerObject1.transform.SetParent(colocator);
        playerObject2.transform.SetParent(colocator);

        playerObject1.transform.localScale = GameManager.Singleton.playerPrefab.transform.localScale;
        playerObject2.transform.localScale = GameManager.Singleton.playerPrefab.transform.localScale;
        
        playerObject1.SpawnWithOwnership(clientId);
        playerObject2.SpawnWithOwnership(clientId);
        
        playerObject2.GetComponent<InverseFollowPlayer>().origin.Value = truePos;

        playerObject1.GetComponent<InverseFollowPlayer>()._mapPos.Value = true;
        playerObject2.GetComponent<InverseFollowPlayer>()._truePos.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCameraServerRpc(ulong clientId, ServerRpcParams serverRpcParams = default) {
        if (!IsServer) return;
        
        NetworkObject camera = Instantiate(networkedCamera);
        camera.SpawnWithOwnership(clientId);
        
        connectedCameras.Add(camera);
        
        Debug.Log("Second call");
    }

    public void SpawnPlayerObject(Vector3 location) {
        SpawnPlayerObjectServerRpc(NetworkManager.Singleton.LocalClientId, location);
        SpawnCameraServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    public void Activate(MenuItems.ItemType item) {
        // calculate carbon impact
        // tie impact to sliders
        // toggle visibility

        switch(item) 
        {
        case MenuItems.ItemType.Bus:
            bus = 1;
            BusHolder.SetActive(!BusHolder.activeSelf);
            break;
        case MenuItems.ItemType.Bike:
            bike = 1;
            BikeHolder.SetActive(!BikeHolder.activeSelf);
            break;
        case MenuItems.ItemType.Car:
            car = 1;
            CarHolder.SetActive(!CarHolder.activeSelf);
            break;
        default:
            Debug.Log("Can Not Activate invalid Item");
            break;
        }

        CarbonImpact = 1.0f - ((-(car * 50) + (bus * 33) + (bike * 33) + (solar * 33) + -(gas * 50) + (recycle * 33)) / 100); 

    }

    void Update() {
        if (processedCameras == connectedCameras.Count) return;
        if (connectedCameras[processedCameras].TryGet(out NetworkObject cameraObject)) {
            processedCameras++;
            if(cameraObject.IsOwner) cameraObject.GetComponent<Camera>().targetDisplay = 10;
            else cameraObject.GetComponent<Camera>().targetTexture = renderTexture;
        }
        //foreach (var camera in connectedCameras) {
        //}
    }
    
    private void OnDestroy() {
        if (connectedCameras != null) {
            connectedCameras.Dispose();
        }
    }

    /*public void OnSceneMesh() {
        Debug.Log("OnSceneMesh");

        var room = GameObject.FindObjectOfType<MRUKRoom>();
        Transform closestTable = null;

        float minDistance = float.MaxValue;
        foreach (var child in room.GetComponentsInChildren<Transform>()) {
            if (child.name == "TABLE") {
                int distance = Mathf.RoundToInt(Vector3.Distance(child.position, mainCamera.transform.position));
                if (distance <= minDistance) {
                    minDistance = distance;
                    closestTable = child;
                }
            }
        }
    }*/
}
