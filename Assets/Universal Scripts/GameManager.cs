using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Singleton { get; private set; }
    public Camera mainCamera; 
    public OVRCameraRig ovrCameraRig;
    
    public GameObject player;

    void Awake() {
        if (Singleton != null && Singleton != this) Destroy(this.gameObject);
        else Singleton = this;
    }
}
