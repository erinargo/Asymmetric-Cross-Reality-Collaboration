using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Random = System.Random;

public class GameManager : NetworkBehaviour {

    [Serializable]
    public class EnvFeedback {
        [SerializeField] private GameObject MapResponse;
        [SerializeField] private GameObject EnvironmentResponse;
        
        public bool Active() => MapResponse.activeSelf;
        public GameObject MapResponseObject() => MapResponse;
        public GameObject EnvironmentResponseObject() => EnvironmentResponse;

        public void Activate() {
            MapResponse.SetActive(!MapResponse.activeSelf);
            EnvironmentResponse.SetActive(!EnvironmentResponse.activeSelf);
        }
    }

    [SerializeField] public NetworkObject playerPrefab;
    public List<GameObject> trees = new List<GameObject>();
    public List<GameObject> cars = new List<GameObject>();
    public static GameManager Singleton { get; private set; }
    public Camera mainCamera; 
    public OVRCameraRig ovrCameraRig;
    public Transform minimap;
    public Transform realMap;

    public Transform realOrigin;
    
    public GameObject player;

    [SerializeField] private Transform windowOfTime;
    [SerializeField] public GameObject connectorPrefab;

    [SerializeField] public Transform colocator;
    
    [SerializeField] private NetworkObject networkedCamera;
    [SerializeField] private RenderTexture renderTexture;
    
    [HideInInspector]
    public NetworkVariable<int> car = new(writePerm: NetworkVariableWritePermission.Server), 
        bus = new(writePerm: NetworkVariableWritePermission.Server), 
        bike = new(writePerm: NetworkVariableWritePermission.Server);
    [HideInInspector]
    public NetworkVariable<int> solar = new(writePerm: NetworkVariableWritePermission.Server), 
        gas = new(writePerm: NetworkVariableWritePermission.Server);
    [HideInInspector]
    public NetworkVariable<int> recycle = new(writePerm: NetworkVariableWritePermission.Server);

    [Space]
    [SerializeField] private EnvFeedback Bus;
    [SerializeField] private EnvFeedback Bike;
    [Space]
    [SerializeField] private EnvFeedback Solar;
    [Space]
    [SerializeField] private EnvFeedback Recycle;
    [Space]
    [SerializeField] private GameObject PowerPlant;
    [SerializeField] private GameObject Trash;

    [SerializeField] public GameObject realMapBuildings;
    [SerializeField] public GameObject miniMapBuildings;

    private int processedCameras = 0;
    
    [HideInInspector]
    public float CarbonImpact = 1.0f; // 0-1
    
    [SerializeField] private int FogImpact = 50;

    private NetworkVariable<bool> activateRequested = new(writePerm: NetworkVariableWritePermission.Server);
    
    [HideInInspector]
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


    [ServerRpc(RequireOwnership = false)]
    public void ActivateServerRpc(MenuItems.ItemType item, ServerRpcParams serverRpcParams = default) {
        switch(item) {
            case MenuItems.ItemType.Bus:
                bus.Value = Bus.Active() ? 0 : 1;
                break;
            case MenuItems.ItemType.Bike:
                bike.Value = Bike.Active() ? 0 : 1;
                break;
            case MenuItems.ItemType.Solar:
                solar.Value = Solar.Active() ? 0 : 1;
                break;
            case MenuItems.ItemType.Recycle:
                recycle.Value = Recycle.Active() ? 0 : 1;
                break;
            default:
                Debug.Log("Can Not Activate invalid Item");
                return;
        }

        // Notify the client that the server operation is complete
        ActivateCompletedClientRpc(item);
    }

    [ClientRpc]
    private void ActivateCompletedClientRpc(MenuItems.ItemType item) {
        // Continue the activation process after the server has completed the update
        ContinueActivation(item);
    }


    public void Activate(MenuItems.ItemType item) {
        ActivateServerRpc(item);
    }

    private void ContinueActivation(MenuItems.ItemType item) {
        switch(item) {
            case MenuItems.ItemType.Bus:
                Bus.Activate();
                break;
            case MenuItems.ItemType.Bike:
                Bike.Activate();
                break;
            case MenuItems.ItemType.Solar:
                Solar.Activate();
                PowerPlant.SetActive(!Solar.Active());
                break;
            case MenuItems.ItemType.Recycle:
                Recycle.Activate();
                Trash.SetActive(!Recycle.Active());
                break;
            default:
                Debug.Log("Can Not Activate invalid Item");
                return;
        }

        CarbonImpact = 1.0f - ((-(car.Value * 50f) + (bus.Value * 33f) + (bike.Value * 33f) + (solar.Value * 33f) + -(gas.Value * 50f) + (recycle.Value * 33f)) / 100f);
        RenderSettings.fogEndDistance = (FogImpact / CarbonImpact);

        foreach (var tree in trees) tree.SetActive(false);
        foreach (var car in cars) car.SetActive(true);

        for (int i = 0; (i < trees.Count * CarbonImpact); i++) trees[UnityEngine.Random.Range(0, trees.Count - 1)].SetActive(true);
        for (int i = 0; (i < cars.Count * CarbonImpact); i++) trees[UnityEngine.Random.Range(0, cars.Count - 1)].SetActive(false);
    }


    void Update() {
        //if (Recycle.Active()) Recycle.MapResponseObject().transform.Rotate(new Vector3(0, 0, 50) * Time.deltaTime);
        #if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
        #else
            Debug.unityLogger.logEnabled = false;
        #endif
        
        foreach (var camera in connectedCameras) {
            if (camera.TryGet(out NetworkObject cameraObject)) {
                if(cameraObject.IsOwner) cameraObject.GetComponent<Camera>().targetDisplay = 10;
                else cameraObject.GetComponent<Camera>().targetTexture = renderTexture;
            }
        }
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
