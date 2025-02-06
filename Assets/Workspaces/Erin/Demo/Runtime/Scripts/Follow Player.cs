using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FollowPlayer : NetworkBehaviour {
    public NetworkVariable<Vector3> position = new(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> rotation = new(writePerm: NetworkVariableWritePermission.Owner);
    
    void Update() {
        if (IsOwner) {
            position.Value = GameManager.Singleton.mainCamera.transform.position;
            rotation.Value = GameManager.Singleton.mainCamera.transform.rotation;
        } else {
            this.gameObject.transform.position = position.Value;
            this.gameObject.transform.rotation = rotation.Value;
        }
    }
}
