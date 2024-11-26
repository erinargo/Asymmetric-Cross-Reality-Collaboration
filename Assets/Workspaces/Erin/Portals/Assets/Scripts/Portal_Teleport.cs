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
        foreach (Transform child in _player.GetComponentsInChildren<Transform>()) {
            var translatedPosition = new Vector3(
                child.position.x + (transform.position.x - camera.position.x) + 0.5f, 
                child.position.y, 
                child.position.z + (transform.position.z - camera.position.z));
            
            child.position = translatedPosition;
            child.rotation = Quaternion.Euler(
                child.rotation.eulerAngles.x,
                child.rotation.eulerAngles.y + (transform.rotation.eulerAngles.y - camera.rotation.eulerAngles.y) + 180f,
                child.rotation.eulerAngles.z
                ); //Quaternion.Euler(rotationDiff.x, rotationDiff.y + (), rotationDiff.z);
            //child.Rotate(Vector3.up, rotationDiff);
        }
    }

    void Update() {
        //Vector3 playerPos = _player.position - transform.position;
        //float dotProduct = Vector3.Dot(transform.up, playerPos);
        float distance = Vector3.Distance(camera.position, _otherPortal.position);
        
        Debug.Log("Distance: " + distance);

        if (distance <= 0.75f){
            Teleport();
            
            if(AR_VR) PassthroughManager.Singleton.Toggle();
        }
    }
}
