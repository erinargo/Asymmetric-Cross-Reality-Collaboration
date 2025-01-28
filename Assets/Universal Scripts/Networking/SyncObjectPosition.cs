using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class SyncObjectPosition : NetworkBehaviour
{
    // We only want owner of gameobject to be able to write but we want everyone to be able to read
    private NetworkVariable<Vector3> _netPos = new(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> _netRot = new(writePerm: NetworkVariableWritePermission.Owner);

    void Awake()
    {
        GetComponent<NetworkTransform>().enabled = !IsHost;
        
        _netPos.Value = transform.position;
        _netRot.Value = transform.rotation;
    }
    
    // GetComponent<TransferOwnershipNGO>().TransferOwnershipToLocalPlayer();

    void Update()
    {
        if (IsOwner)
        {
            if ((IsServer || IsHost) && !IsOwner) return; // seems redundant. Isn't.
            // Always read and write from value, the editor will scream at you otherwise
            _netPos.Value = transform.position;
            _netRot.Value = transform.rotation;
        } else if (!IsOwner) // seems redundant. Isn't. They could also be IsServer or IsHost which might lead to unintended behaviour. 
        {
            transform.position = _netPos.Value;
            transform.rotation = _netRot.Value;
        }
    }
}
