using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using Unity.Netcode;

public class GameManager : NetworkBehaviour {
    [SerializeField] public NetworkObject playerPrefab;
    
    public static GameManager Singleton { get; private set; }
    public Camera mainCamera; 
    public OVRCameraRig ovrCameraRig;
    public Transform minimap;
    public Transform realMap;
    
    public GameObject player;

    [SerializeField] private Transform windowOfTime;
    [SerializeField] public GameObject connectorPrefab;

    [SerializeField] public Transform colocator;
    
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
    
    public void SpawnPlayerObject(Vector3 location) {
        SpawnPlayerObjectServerRpc(NetworkManager.Singleton.LocalClientId, location);
    }

    public void OnSceneMesh() {
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
    }
}
