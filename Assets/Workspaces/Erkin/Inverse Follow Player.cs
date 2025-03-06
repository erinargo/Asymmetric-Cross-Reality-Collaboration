using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InverseFollowPlayer : NetworkBehaviour {
    private Transform minimap;
    private Transform realMap;

    public NetworkVariable<Vector3> origin;
    
    public NetworkVariable<bool> _mapPos = new(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> _truePos = new(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isPlayerAR = new(writePerm: NetworkVariableWritePermission.Owner);

    private NetworkVariable<Vector3> _netPos = new(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> _netScale = new(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> _netRot = new(writePerm: NetworkVariableWritePermission.Owner);
    
    private InverseFollowPlayer _otherPlayerPrefab;

    private GameObject line;

    private GameObject mapOrigin;
    private GameObject realOrigin;
    
    void Start() {
        minimap = GameManager.Singleton.minimap;
        realMap = GameManager.Singleton.realMap;

        foreach (var other in GameObject.FindObjectsOfType<InverseFollowPlayer>()) {
            if (other == this) continue;
            _otherPlayerPrefab = other;
        }

        if (_truePos.Value && IsOwner) {
            mapOrigin = new GameObject("originPos");
            mapOrigin.transform.position = GameManager.Singleton.mainCamera.transform.position;
            mapOrigin.transform.rotation = GameManager.Singleton.mainCamera.transform.rotation;

            realOrigin = new GameObject("originPos");
            realOrigin.transform.position = GameManager.Singleton.realOrigin.transform.position;
            realOrigin.transform.rotation = GameManager.Singleton.realOrigin.transform.rotation;
        }

        if (IsOwner) _netScale.Value = new Vector3(1, 1, 1);
    }

    float GetMinimapScale() {
        // Define two reference points
        Transform realPointA = GameManager.Singleton.realMapBuildings.transform.GetChild(0);
        Transform realPointB = GameManager.Singleton.realMapBuildings.transform.GetChild(1);
        Transform minimapPointA = GameManager.Singleton.miniMapBuildings.transform.GetChild(0);
        Transform minimapPointB = GameManager.Singleton.miniMapBuildings.transform.GetChild(1);

        float realWorldDistance = Vector3.Distance(realPointA.position, realPointB.position);
        float minimapDistance = Vector3.Distance(minimapPointA.position, minimapPointB.position);

        return minimapDistance / realWorldDistance;
    }
    

    void CalculateMinimapInversePosition() {
        float minimapScale = GetMinimapScale();
        Vector3 relativeToCamera = realMap.transform.InverseTransformPoint(GameManager.Singleton.mainCamera.transform.position);
        relativeToCamera *= minimapScale;
        _netPos.Value = relativeToCamera;
        _netScale.Value = new Vector3(0.01f, 0.01f, 0.01f);
        DrawConnections(transform, _otherPlayerPrefab.transform);
    }

    void CalculateTrueInversePosition() {
        Vector3 relativeToCamera = mapOrigin.transform.InverseTransformPoint(GameManager.Singleton.mainCamera.transform.position);
        relativeToCamera = Vector3.Scale(relativeToCamera, new Vector3(-1, 1, -1));
        _netPos.Value = realOrigin.transform.TransformPoint(relativeToCamera);
        
        Vector3 relativeRotation = mapOrigin.transform.TransformDirection(GameManager.Singleton.mainCamera.transform.forward);
        relativeRotation = Vector3.Scale(relativeRotation, new Vector3(1, 1, 1));
        _netRot.Value = realOrigin.transform.TransformDirection(relativeRotation);
        
        _netScale.Value = new Vector3(0.01f, 0.01f, 0.01f);
    }

    void DrawConnections(Transform start, Transform end) {       
        if(line != null) Destroy(line);
        
        Vector3 midPoint = (start.position + end.position) / 2;            
        Vector3 direction = end.position - start.position;            
        line = Instantiate(GameManager.Singleton.connectorPrefab);
        
        line.transform.position = midPoint;            
        line.transform.rotation = Quaternion.LookRotation(direction);            
        line.transform.localScale = new Vector3(0.002f, 0.002f, direction.magnitude);                     
    }

    void LateUpdate() {
        if (IsOwner) isPlayerAR.Value = PassthroughManager.Singleton.passthroughOn;
        
        if (isPlayerAR.Value) {
            if (line != null) Destroy(line);
            gameObject.SetActive(false);
        }
    }
        
    void Update() {
        if (_mapPos.Value && IsOwner) CalculateMinimapInversePosition();
        if (_truePos.Value && IsOwner) CalculateTrueInversePosition();

        if (_mapPos.Value) {
            Vector3 minimapPosition = minimap.transform.position + _netPos.Value;
            transform.position = new Vector3(minimapPosition.x, minimap.position.y, minimapPosition.z);
            
            transform.forward = _netRot.Value;  
            transform.localScale = _netScale.Value; 
        } else {
            transform.position = _netPos.Value;
            transform.localScale = _netScale.Value; 
            
            GetComponent<MeshRenderer>().enabled = false;
        }
        
        if (!IsOwner) DrawConnections(transform, _otherPlayerPrefab.transform);
    }
    
}