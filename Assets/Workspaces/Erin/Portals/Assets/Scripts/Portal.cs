using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portals : MonoBehaviour
{
    [SerializeField] private Camera bluePortalCamera;
    [SerializeField] private Camera orangePortalCamera;
    [Space] 
    [SerializeField] private Transform bluePortal;
    [SerializeField] private Transform orangePortal;
    [Space]
    [SerializeField] private Material bluePortalMaterial;
    [SerializeField] private Material orangePortalMaterial;
    [Space]
    [SerializeField] private Vector3 inverseTransformDirection;

    private Camera mainCamera;
    private OVRCameraRig ovrCameraRig;
    
    void Awake() {
        mainCamera = GameManager.Singleton.mainCamera;
        ovrCameraRig = GameManager.Singleton.ovrCameraRig;
        
        FixRTResolution(); // init
    }
    
    void ClampCameraPosition(Transform portal, Camera portalCamera) {
        Vector3 directionToCamera = portalCamera.transform.position - portal.position;
        float halfVerticalFOV = Mathf.Tan(Mathf.Deg2Rad * mainCamera.fieldOfView / 2);
        float distanceToCamera = directionToCamera.magnitude;
        float portalHeight = portal.localScale.y;
        float maxDistance = portalHeight / (2 * halfVerticalFOV);
    
        if (distanceToCamera > maxDistance) {
            portalCamera.transform.position = portal.position + directionToCamera.normalized * maxDistance;
        }
    }

    void PositionPortalCamera(Transform otherPortal, Transform portal, Camera thisPortalCamera) {
        Quaternion adjustedRotation = 
            Quaternion.Euler(
                0, 0, mainCamera.transform.rotation.eulerAngles.z
            );
        
        Vector3 relativePosOtherPortal = otherPortal.InverseTransformPoint(mainCamera.transform.position);
        relativePosOtherPortal = Vector3.Scale(relativePosOtherPortal, new Vector3(-1, 1, -1));
        thisPortalCamera.transform.position = portal.TransformPoint(relativePosOtherPortal);
        
        var relativeRotationToOtherPortal = portal.transform.InverseTransformDirection(mainCamera.transform.forward);
        relativeRotationToOtherPortal = Vector3.Scale(relativeRotationToOtherPortal, inverseTransformDirection);
        thisPortalCamera.transform.forward = otherPortal.TransformDirection(relativeRotationToOtherPortal);
        
        thisPortalCamera.transform.rotation = thisPortalCamera.transform.rotation * adjustedRotation;
    }

    void FixRTResolution() {
        if (bluePortalCamera.targetTexture != null) {
            bluePortalCamera.targetTexture.Release();
        }
        
        bluePortalCamera.targetTexture = 
            new RenderTexture(
                ovrCameraRig.leftEyeCamera.pixelWidth, 
                ovrCameraRig.leftEyeCamera.pixelHeight, 
                24
            );
        
        /*bluePortalCamera.targetTexture = 
            new RenderTexture(
                Screen.width, 
                Screen.height, 
                24
            );*/
        
        if (orangePortalCamera.targetTexture != null) {
            orangePortalCamera.targetTexture.Release();
        }
        
        orangePortalCamera.targetTexture = 
            new RenderTexture(
                ovrCameraRig.leftEyeCamera.pixelWidth, 
                ovrCameraRig.leftEyeCamera.pixelHeight, 
                24
            );
        
        /*orangePortalCamera.targetTexture = 
            new RenderTexture(
                Screen.width, 
                Screen.height, 
                24
            );*/
        
        // Blue portal displays what orange can see and vice versa
        bluePortalMaterial.mainTexture = orangePortalCamera.targetTexture;
        orangePortalMaterial.mainTexture = bluePortalCamera.targetTexture;
    }

    void LateUpdate() {
        FixRTResolution();
        
        // Blue Camera
        bluePortalCamera.projectionMatrix = mainCamera.projectionMatrix;
        bluePortalCamera.fieldOfView = mainCamera.fieldOfView;
        bluePortalCamera.aspect = (float)ovrCameraRig.leftEyeCamera.pixelWidth / (float)ovrCameraRig.leftEyeCamera.pixelHeight;
        bluePortalCamera.depth = mainCamera.depth;
        
        Debug.Log(bluePortalCamera.aspect);
        
        PositionPortalCamera(orangePortal, bluePortal, bluePortalCamera);
        ClampCameraPosition(bluePortal, bluePortalCamera);
        
        // Orange Camera
        orangePortalCamera.projectionMatrix = mainCamera.projectionMatrix;
        orangePortalCamera.fieldOfView = mainCamera.fieldOfView;
        orangePortalCamera.aspect = (float)ovrCameraRig.leftEyeCamera.pixelWidth / (float)ovrCameraRig.leftEyeCamera.pixelHeight;
        orangePortalCamera.depth = mainCamera.depth;
        
        PositionPortalCamera(bluePortal, orangePortal, orangePortalCamera);
        ClampCameraPosition(orangePortal, orangePortalCamera);
    }
}