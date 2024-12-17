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

    private void Start() {
        _playerOrigin = GameManager.Singleton.player.transform;
        camera = GameManager.Singleton.mainCamera.transform;
        
        _portalReceiver = _otherPortal.GetComponent<Portal_Teleport>();
        
        this.AssertField(_playerOrigin, nameof(_playerOrigin));
    }

    void Teleport() {
        GameManager.Singleton.SpawnPlayerObject(camera.position);
        
        if (AR_VR) PassthroughManager.Singleton.Toggle();
        
        _portalReceiver.canTeleport = false;

        var translatedPosition = new Vector3(
            _playerOrigin.position.x + (transform.position.x - camera.position.x), 
            _playerOrigin.position.y, 
            _playerOrigin.position.z + (transform.position.z - camera.position.z));

        _playerOrigin.position = translatedPosition;
        _playerOrigin.rotation = Quaternion.Euler(
            _playerOrigin.rotation.eulerAngles.x,
            _playerOrigin.rotation.eulerAngles.y + (transform.rotation.eulerAngles.y - camera.rotation.eulerAngles.y) + 180f,
            _playerOrigin.rotation.eulerAngles.z
        );
    }

    void Update() {
        float distance = Vector3.Distance(camera.position, _otherPortal.position);
        if (distance <= 0.9f && canTeleport) Teleport();

        if (distance >= 1.5f) canTeleport = true;
    }
}