using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Locomotion;
using UnityEngine;

public class Portal_Teleport : MonoBehaviour
{
    private Transform _player;
    private Transform camera;
    
    [SerializeField] private Transform _otherPortal;
    [SerializeField] private bool AR_VR;

    private void Start() {
        _player = GameManager.Singleton.player.transform;
        camera = GameManager.Singleton.mainCamera.transform;
    }

    void Teleport() {
        Debug.Log("Teleport");
        
        float rotationDiff = -Quaternion.Angle(transform.rotation, _otherPortal.rotation);
        rotationDiff += 180;
        
        _player.Rotate(Vector3.up, rotationDiff);
        
        Vector3 positionOffset = transform.InverseTransformPoint(camera.transform.position);
        positionOffset = Vector3.Scale(positionOffset, new Vector3(-1, 1, -1));
        _player.transform.position = _otherPortal.TransformPoint(positionOffset);
    }

    void Update() {
        //Vector3 playerPos = _player.position - transform.position;
        //float dotProduct = Vector3.Dot(transform.up, playerPos);
        float distance = Vector3.Distance(camera.position, _otherPortal.position);
        
        Debug.Log("Distance: " + distance);

        if (distance <= 0.9f){
            Teleport();
            
            if(AR_VR) PassthroughManager.Singleton.Toggle();
        }
    }
}
