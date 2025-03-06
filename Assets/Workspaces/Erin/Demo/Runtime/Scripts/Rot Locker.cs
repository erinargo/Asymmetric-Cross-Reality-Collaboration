using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotLocker : MonoBehaviour {

    private Quaternion initRot;
    
    void Start() {
        initRot = transform.rotation;
    }
    
    void Update() {
        transform.rotation = initRot;
    }
}
