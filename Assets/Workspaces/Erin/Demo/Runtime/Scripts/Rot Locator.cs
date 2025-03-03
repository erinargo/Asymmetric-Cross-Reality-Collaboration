using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotLocator : MonoBehaviour {
    void Update() {
        transform.eulerAngles = new Vector3(0, GameManager.Singleton.minimap.rotation.eulerAngles.y,0);
    }
}
