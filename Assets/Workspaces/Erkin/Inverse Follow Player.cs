using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InverseFollowPlayer : NetworkBehaviour {
    private bool followPlayer = false;
    private Transform minimap;
    private Transform realMap;

    public NetworkVariable<Vector3> origin;
    
    public NetworkVariable<bool> _mapPos = new(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> _truePos = new(writePerm: NetworkVariableWritePermission.Server);

    private InverseFollowPlayer _otherPlayerPrefab;

    private GameObject line;
    
    void Start() {
        minimap = GameManager.Singleton.minimap;
        realMap = GameManager.Singleton.realMap;

        foreach (var other in GameObject.FindObjectsOfType<InverseFollowPlayer>()) {
            if (other == this) continue;
            _otherPlayerPrefab = other;
        }
    }


    void CalculateMinimapInversePosition() {
        /*Vector3 playerWorldPosition = GameManager.Singleton.mainCamera.transform.position;

        Vector3 relativePos = realMap.TransformPoint(playerWorldPosition);
        relativePos = Vector3.Scale(relativePos, new Vector3(0.01f, 0.01f, 0.01f));

        transform.position = minimap.TransformPoint(relativePos);*/
        
        Vector3 playerOffset = GameManager.Singleton.mainCamera.transform.position - realMap.transform.position;

        playerOffset.x *= 0.001f;
        playerOffset.y = 0;
        playerOffset.z *= 0.001f;
        
        transform.position = (minimap.position + playerOffset);
        
        Vector3 adjustedPos = new Vector3(transform.position.x, minimap.position.y, transform.position.z);
        transform.position = adjustedPos;
        
        transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
    }

    void CalculateTrueInversePosition() {
        Vector3 playerPosition = GameManager.Singleton.mainCamera.transform.position;

        // Calculate the offset from the player to the origin
        Vector3 playerOffset = origin.Value - playerPosition;

        // Set the object position relative to the origin and offset
        transform.position = playerPosition + playerOffset;

        // Log the origin for debugging purposes
        Debug.Log(origin.Value);

        // Adjust the position by locking the Y-value to the minimap's Y
        Vector3 adjustedPos = new Vector3(transform.position.x, minimap.position.y, transform.position.z);
        transform.position = adjustedPos;
    }

    void DrawConnections(Transform start, Transform end) {       
        if(line != null) Destroy(line);
        
        Vector3 midPoint = (start.position + end.position) / 2;            
        Vector3 direction = end.position - start.position;            
        line = Instantiate(GameManager.Singleton.connectorPrefab);
        
        line.transform.position = midPoint;            
        line.transform.rotation = Quaternion.LookRotation(direction);            
        line.transform.localScale = new Vector3(0.2f, 0.2f, direction.magnitude);                        
        // connector.GetComponent<MeshRenderer>().material.SetColor("_Color", node.GetRouteColor());
    } 
        
    void Update() {
        if (_mapPos.Value) CalculateMinimapInversePosition();
        if (_truePos.Value) CalculateTrueInversePosition();
        
        DrawConnections(transform, _otherPlayerPrefab.transform);
    }
    
}