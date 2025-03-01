using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour {

    [Serializable]
    public class EnvFeedback {
        [SerializeField] private GameObject MapResponse;
        [SerializeField] private GameObject EnvironmentResponse;
        
        public bool Active() => MapResponse.activeSelf;

        public void Activate() {
            MapResponse.SetActive(!MapResponse.activeSelf);
            EnvironmentResponse.SetActive(!EnvironmentResponse.activeSelf);
        }
    }

    [SerializeField] public NetworkObject playerPrefab;
    
    public static GameManager Singleton { get; private set; }
    public Camera mainCamera; 
    public OVRCameraRig ovrCameraRig;
    public Transform minimap;
    public Transform realMap;

    [HideInInspector]
    public Vector3 realOrigin;
    
    public GameObject player;

    [SerializeField] private Transform windowOfTime;
    [SerializeField] public GameObject connectorPrefab;

    [SerializeField] public Transform colocator;
    
    [SerializeField] private NetworkObject networkedCamera;
    [SerializeField] private RenderTexture renderTexture;
    
    [HideInInspector]
    public int car, bus, bike;
    [HideInInspector]
    public int solar, gas;
    [HideInInspector]
    public int recycle;

    [Space]
    [SerializeField] private EnvFeedback Bus;
    [SerializeField] private EnvFeedback Bike;
    [SerializeField] private EnvFeedback Car;
    [Space]
    [SerializeField] private EnvFeedback Solar;
    [SerializeField] private EnvFeedback Gas;
    [Space]
    [SerializeField] private EnvFeedback Recycle;
    [SerializeField] private EnvFeedback Waste;
    [Space]

    [SerializeField] public GameObject realMapBuildings;
    [SerializeField] public GameObject miniMapBuildings;

    private int processedCameras = 0;
    
    [HideInInspector]
    public float CarbonImpact = 1.0f; // 0-1
    
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

    public void Activate(MenuItems.ItemType item) {
        // calculate carbon impact
        // tie impact to sliders
        // toggle visibility

        switch(item) {
        case MenuItems.ItemType.Bus:
            bus = Bus.Active() ? 0 : 1; // if active then deactivate, else activate
            Bus.Activate();
            break;
        case MenuItems.ItemType.Bike:
            bike = Bike.Active() ? 0 : 1;
            Bike.Activate();
            break;
        case MenuItems.ItemType.Car:
            car = Car.Active() ? 0 : 1; 
            Car.Activate();
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
