using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Locomotion;
using UnityEngine;

public class Portal_Teleport : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _otherPortal;
    [SerializeField] private bool AR_VR;

    void Teleport()
    {
        Debug.Log("Teleport");
        
        float rotationDiff = -Quaternion.Angle(transform.rotation, _otherPortal.rotation);
        rotationDiff += 180;
        
        _player.Rotate(Vector3.up, rotationDiff);
        
        Vector3 positionOffset = transform.InverseTransformPoint(_player.transform.position);
        positionOffset = Vector3.Scale(positionOffset, new Vector3(-1, 1, -1));
        _player.transform.position = _otherPortal.TransformPoint(positionOffset);
    }

    void Update()
    {
        Vector3 playerPos = _player.position - transform.position;
        float dotProduct = Vector3.Dot(transform.up, playerPos);
        
        Debug.Log("dotProduct: " + dotProduct);
        Debug.Log("playerPos: " + playerPos);

        if (dotProduct < 0.5f)
        {
            Teleport();
            
            if(AR_VR) PassthroughManager.Singleton.Toggle();
        }
    }
}
