using System.Collections;
using System.Collections.Generic;
using Meta.XR.MultiplayerBlocks.NGO;
using UnityEngine;

public class PassthroughManager : MonoBehaviour {
    public static PassthroughManager Singleton { get; private set; }

    public bool passthroughOn = true;
    private OVRCameraRig rig;
    
    [SerializeField] private OVRPassthroughLayer layer;
    [SerializeField] private Color defaultBackground;
    [SerializeField] private int hideOnPassthrough;
    
    void Awake() {
        if (Singleton != null && Singleton != this) Destroy(this.gameObject);
        else Singleton = this;
    }

    void Start() {
        rig = GameManager.Singleton.ovrCameraRig;
    }

    public void Toggle() {
        Camera centerCamera = rig.centerEyeAnchor.GetComponent<Camera>();
        if (passthroughOn) {
            centerCamera.clearFlags = CameraClearFlags.Skybox;
            centerCamera.backgroundColor = defaultBackground; 
            
            // enable the things in hide on passthrough
            centerCamera.cullingMask |= (1 << hideOnPassthrough);
        } else {
            centerCamera.clearFlags = CameraClearFlags.SolidColor;
            centerCamera.backgroundColor = Color.clear;
            
            // disable the things in hide on passthrough 
            centerCamera.cullingMask &= ~(1 << hideOnPassthrough);
        }

        passthroughOn = !passthroughOn;
    }
}
