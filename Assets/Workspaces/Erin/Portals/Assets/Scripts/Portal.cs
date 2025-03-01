using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portals : MonoBehaviour {
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

    [HideInInspector] public Transform orangeTrans;
    
    private Camera mainCamera;
    private OVRCameraRig ovrCameraRig;
    
    void Start() {
        mainCamera = GameManager.Singleton.mainCamera;
        ovrCameraRig = GameManager.Singleton.ovrCameraRig;
        
        FixRTResolution(); // init
        
        // Blue Camera
        bluePortalCamera.projectionMatrix = mainCamera.projectionMatrix;
        bluePortalCamera.fieldOfView = mainCamera.fieldOfView;
        bluePortalCamera.aspect = (float)ovrCameraRig.leftEyeCamera.pixelWidth / (float)ovrCameraRig.leftEyeCamera.pixelHeight;
        bluePortalCamera.depth = mainCamera.depth;
        
        // Orange Camera
        orangePortalCamera.projectionMatrix = mainCamera.projectionMatrix;
        orangePortalCamera.fieldOfView = mainCamera.fieldOfView;
        orangePortalCamera.aspect = (float)ovrCameraRig.leftEyeCamera.pixelWidth / (float)ovrCameraRig.leftEyeCamera.pixelHeight;
        orangePortalCamera.depth = mainCamera.depth;
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

    void PositionPortalCamera(Transform otherPortal, Transform portal, Camera thisPortalCamera, Transform mainCamTransform) {
        Quaternion adjustedRotation = Quaternion.Euler(0, 0, mainCamTransform.rotation.eulerAngles.z);

        Vector3 relativePosOtherPortal = otherPortal.InverseTransformPoint(mainCamTransform.position);
        relativePosOtherPortal = Vector3.Scale(relativePosOtherPortal, new Vector3(-1, 1, -1));
        thisPortalCamera.transform.position = portal.TransformPoint(relativePosOtherPortal);

        Vector3 relativeRotationToOtherPortal = portal.InverseTransformDirection(mainCamTransform.forward);
        relativeRotationToOtherPortal = Vector3.Scale(relativeRotationToOtherPortal, inverseTransformDirection);
        thisPortalCamera.transform.forward = otherPortal.TransformDirection(relativeRotationToOtherPortal);

        thisPortalCamera.transform.rotation *= adjustedRotation;
    }

    void FixRTResolution() {
        if (bluePortalCamera.targetTexture != null) {
            bluePortalCamera.targetTexture.Release();
        }
        
        bluePortalCamera.targetTexture = new RenderTexture(
            ovrCameraRig.leftEyeCamera.pixelWidth, 
            ovrCameraRig.leftEyeCamera.pixelHeight, 
            24
        );
        
        if (orangePortalCamera.targetTexture != null) {
            orangePortalCamera.targetTexture.Release();
        }
        
        orangePortalCamera.targetTexture = new RenderTexture(
            ovrCameraRig.leftEyeCamera.pixelWidth, 
            ovrCameraRig.leftEyeCamera.pixelHeight, 
            24
        );
        
        // Blue portal displays what orange can see and vice versa
        bluePortalMaterial.mainTexture = orangePortalCamera.targetTexture;
        orangePortalMaterial.mainTexture = bluePortalCamera.targetTexture;
    }

    void OrangePosition() {
        orangePortal.position = orangeTrans.position;
    }

    void LateUpdate() {
        // Cache main camera transform to avoid repeated Unity API calls
        Transform mainCamTransform = mainCamera.transform;

        // Update Blue Portal Camera
        PositionPortalCamera(orangePortal, bluePortal, bluePortalCamera, mainCamTransform);
        ClampCameraPosition(bluePortal, bluePortalCamera);

        // Update Orange Portal Camera
        PositionPortalCamera(bluePortal, orangePortal, orangePortalCamera, mainCamTransform);
        ClampCameraPosition(orangePortal, orangePortalCamera);
        OrangePosition();
    }
}
