using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColocatorRotationFix : MonoBehaviour {
    public Vector3 allowedAxis = Vector3.up; 
    
    private float rotationAngle;

    void Update() {
        // Calculate the current angle of rotation around the allowed axis
        rotationAngle = Vector3.Dot(transform.eulerAngles, allowedAxis);

        // Constrain the rotation to the allowed axis
        transform.rotation = Quaternion.Euler(
            allowedAxis.x * rotationAngle,
            allowedAxis.y * rotationAngle,
            allowedAxis.z * rotationAngle
        );
    }
}
