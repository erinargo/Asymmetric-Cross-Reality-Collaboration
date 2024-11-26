using System.Collections;
using System.Collections.Generic;
using Meta.XR.MultiplayerBlocks.NGO;
using UnityEngine;

public class PassthroughManager : MonoBehaviour {
    public static PassthroughManager Singleton { get; private set; }

    private bool passthroughOn = false;
    private bool selectivePassthroughOn = false;
    private OVRCameraRig rig;
    
    [SerializeField] private OVRPassthroughLayer layer;
    [SerializeField] private Color defaultBackground;
    [SerializeField] private int hideOnPassthrough;
    
    void Awake() {
        if (Singleton != null && Singleton != this) Destroy(this.gameObject);
        else Singleton = this;
        
        rig = GameManager.Singleton.ovrCameraRig;
    }
    public void Toggle() {
        Camera centerCamera = rig.centerEyeAnchor.GetComponent<Camera>();
        if (passthroughOn) {
            layer.enabled = false;
            centerCamera.clearFlags = CameraClearFlags.Skybox;
            centerCamera.backgroundColor = defaultBackground; 
            
            // enable the things in hide on passthrough
        } else {
            layer.enabled = true;
            
            centerCamera.clearFlags = CameraClearFlags.SolidColor;
            centerCamera.backgroundColor = Color.clear;
            
            // disable the things in hide on passthrough 
        }

        passthroughOn = !passthroughOn;
    }
}
