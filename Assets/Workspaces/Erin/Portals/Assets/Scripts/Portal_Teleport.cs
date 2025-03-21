using System;
using System.Collections;
using Oculus.Interaction;
using UnityEngine;

public class Portal_Teleport : MonoBehaviour {
    private Transform _playerOrigin;
    private Transform camera;

    [SerializeField] private Transform _otherPortal;
    [SerializeField] private bool AR_VR;
    
    private Portal_Teleport _portalReceiver;
    public bool canTeleport = true;

    void Start() {
        _playerOrigin = GameManager.Singleton.player.transform;
        camera = GameManager.Singleton.mainCamera.transform;
        
        _portalReceiver = _otherPortal.GetComponent<Portal_Teleport>();
        
        this.AssertField(_playerOrigin, nameof(_playerOrigin));
    }

    void Teleport() {
        _portalReceiver.canTeleport = false;
        
        GameManager.Singleton.SpawnPlayerObject(camera.position);
        GameManager.Singleton.realOrigin.position = camera.position;
        GameManager.Singleton.realOrigin.rotation = camera.rotation;
        
        var trueTeleportPosition = PassthroughManager.Singleton.passthroughOn ? transform.position : _playerOrigin.position;
        
        // if(PassthroughManager.Singleton.passthroughOn) 

        var translatedPosition = new Vector3(
            transform.position.x - 1.2f, 
            _playerOrigin.transform.position.y, 
            transform.position.z - 1.2f
            );
        
        if (AR_VR) PassthroughManager.Singleton.Toggle();
        
        _playerOrigin.position = translatedPosition;
        
        /*_playerOrigin.rotation = Quaternion.Euler(
            _playerOrigin.rotation.eulerAngles.x,
            _playerOrigin.rotation.eulerAngles.y + (camera.rotation.eulerAngles.y - transform.rotation.eulerAngles.y),
            _playerOrigin.rotation.eulerAngles.z
        );*/
    }

    void Update() {
        float distance = Vector3.Distance(camera.position, _otherPortal.position);
        if (distance <= 0.8f && canTeleport) Teleport();

        if (distance >= 1f) canTeleport = true;
    }
}