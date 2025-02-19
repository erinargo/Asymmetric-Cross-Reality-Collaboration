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

        if (_truePos.Value) {
            mapOrigin = new GameObject("originPos");
            mapOrigin.transform.position = GameManager.Singleton.mainCamera.transform.position;

            realOrigin = new GameObject("originPos");
            realOrigin.transform.position = GameManager.Singleton.realOrigin;
        }

        if (IsOwner) _netScale.Value = new Vector3(1, 1, 1);
    }


    void CalculateMinimapInversePosition() {
        Vector3 playerOffset = GameManager.Singleton.mainCamera.transform.position - realMap.transform.position;

        playerOffset.x *= 0.001f;
        playerOffset.y = 0;
        playerOffset.z *= 0.001f;
        
        
        Vector3 pos = (minimap.position + playerOffset);
        
        Vector3 adjustedPos = new Vector3(pos.x, minimap.position.y, pos.z);
        _netPos.Value = adjustedPos;
        _netScale.Value = new Vector3(0.01f, 0.01f, 0.01f);
        
        DrawConnections(transform, _otherPlayerPrefab.transform);
    }

    void CalculateTrueInversePosition() {
        Vector3 relativeToCamera = mapOrigin.transform.InverseTransformPoint(GameManager.Singleton.mainCamera.transform.position);
        _netPos.Value = realOrigin.transform.TransformPoint(relativeToCamera);
        
        Vector3 relativeRotation = mapOrigin.transform.TransformDirection(GameManager.Singleton.mainCamera.transform.forward);
        _netRot.Value = realOrigin.transform.TransformDirection(relativeRotation);
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
        
        transform.position = _netPos.Value;
        transform.localScale = _netScale.Value;
        transform.forward = _netRot.Value;
        
        if (!IsOwner) DrawConnections(transform, _otherPlayerPrefab.transform);
    }
    
}