using System.Collections;
using System.Collections.Generic;
using Meta.XR.MultiplayerBlocks.NGO;
using UnityEngine;

public class PassthroughManager : MonoBehaviour
{
    public static PassthroughManager Singleton { get; private set; }

    private bool passthroughOn = false;
    private bool selectivePassthroughOn = false;
    
    [SerializeField] private OVRPassthroughLayer layer;
    [SerializeField] private OVRCameraRig rig;
    [SerializeField] private Color defaultBackground;

    [SerializeField] private GameObject[] selectiveElements;
    [SerializeField] private GameObject[] disableOnPassthrough;

    [SerializeField] private bool toggle;
    
    [SerializeField] private int hideOnPassthrough;
    
    private bool toggleSet;
    
    // BEGIN Example Code Only
    [Space] 
    [SerializeField] private GameObject passthroughButton;
    [SerializeField] private GameObject selectivePassthroughButton;
    // END Example Code Only
    
    void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this.gameObject);
        else Singleton = this;
    }

    void Update()
    {
        if (toggle != toggleSet)
        {
            Toggle();
            toggleSet = toggle;
        }
    }

    public void Toggle()
    
    {
        Camera centerCamera = rig.centerEyeAnchor.GetComponent<Camera>();
        
        if (passthroughOn)
        {
            layer.enabled = false;
            centerCamera.clearFlags = CameraClearFlags.Skybox;
            centerCamera.backgroundColor = defaultBackground; 
            
            // disable hide on passthrough
        }
        else
        {
            layer.enabled = true;
            
            centerCamera.clearFlags = CameraClearFlags.SolidColor;
            centerCamera.backgroundColor = Color.clear;
            
            //enable hide on passthrough 
            rig.centerEyeAnchor.GetComponent<Camera>().cullingMask &= ~(1 << hideOnPassthrough);
        }

        passthroughOn = !passthroughOn;
        
        // if Normal Passthrough On, Disable selectivePassthrough items
        selectivePassthroughButton.SetActive(!passthroughOn); 
    }

    public void SelectiveToggle()
    {
        Camera centerCamera = rig.centerEyeAnchor.GetComponent<Camera>();
        
        if (selectivePassthroughOn)
        {
            layer.enabled = false;
            
            centerCamera.backgroundColor = defaultBackground; 
            
        }
        else
        {
            layer.enabled = true;
            
            centerCamera.backgroundColor = Color.clear;
            
            foreach (var element in selectiveElements)
            {
                element.SetActive(true);
                element.GetComponent<TransferOwnershipNGO>().TransferOwnershipToLocalPlayer();
            }
        }
        
        selectivePassthroughOn = !selectivePassthroughOn;

        // if Selective Passthrough On, Disable passthrough items
        passthroughButton.SetActive(!selectivePassthroughOn); 
    }
}
