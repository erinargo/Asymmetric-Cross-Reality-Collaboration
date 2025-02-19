using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotLocator : MonoBehaviour {
    void Update() {
        transform.forward = GameManager.Singleton.minimap.transform.forward;
    }
}
