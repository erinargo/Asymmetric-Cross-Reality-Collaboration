using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Portal : MonoBehaviour
{
    [SerializeField] private Camera bluePortalCamera;
    [SerializeField] private Camera orangePortalCamera;
    [Space] 
    [SerializeField] private Transform bluePortal;
    [SerializeField] private Transform orangePortal;
    [Space]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private OVRCameraRig ovrCameraRig;
    [Space]
    [SerializeField] private Material bluePortalMaterial;
    [SerializeField] private Material orangePortalMaterial;
    [Space]
    [SerializeField] private bool AR_VR;
    
    // TEMPORARY SERIALIZABLE, ACTUAL IMPLEMENTATION TBD
    [SerializeField] private GameObject cameraRig;

    
    
    void Awake()
    {
        
    }
    
    void ClampCameraPosition(Transform portal, Camera portalCamera)
    {
        Vector3 directionToCamera = portalCamera.transform.position - portal.position;
        float halfVerticalFOV = Mathf.Tan(Mathf.Deg2Rad * mainCamera.fieldOfView / 2);
        float distanceToCamera = directionToCamera.magnitude;
        float portalHeight = portal.localScale.y;
        float maxDistance = portalHeight / (2 * halfVerticalFOV);
    
        if (distanceToCamera > maxDistance)
        {
            portalCamera.transform.position = portal.position + directionToCamera.normalized * maxDistance;
        }
    }

    void LateUpdate()
    {
        if (bluePortalCamera.targetTexture != null)
        {
            bluePortalCamera.targetTexture.Release();
        }
        
        bluePortalCamera.targetTexture = 
            new RenderTexture(
                ovrCameraRig.leftEyeCamera.pixelWidth, 
                ovrCameraRig.leftEyeCamera.pixelHeight, 
                24);
        
        if (orangePortalCamera.targetTexture != null)
        {
            orangePortalCamera.targetTexture.Release();
        }
        
        orangePortalCamera.targetTexture = 
            new RenderTexture(
                ovrCameraRig.leftEyeCamera.pixelWidth, 
                ovrCameraRig.leftEyeCamera.pixelHeight, 
                24);
        
        // Blue portal displays what orange can see and vice versa
        bluePortalMaterial.mainTexture = orangePortalCamera.targetTexture;
        if(!AR_VR) orangePortalMaterial.mainTexture = bluePortalCamera.targetTexture;
        
        float aspectRatio = 
            ovrCameraRig.leftEyeCamera.pixelWidth / (float) ovrCameraRig.leftEyeCamera.pixelHeight;
        float FOV = ovrCameraRig.leftEyeCamera.fieldOfView;

        bluePortalCamera.aspect = aspectRatio;
        orangePortalCamera.aspect = aspectRatio;

        bluePortalCamera.fieldOfView = FOV;
        orangePortalCamera.fieldOfView = FOV;

        Matrix4x4 customProjectionMatrix = ovrCameraRig.leftEyeCamera.projectionMatrix;
        
        bluePortalCamera.projectionMatrix = customProjectionMatrix;
        orangePortalCamera.projectionMatrix = customProjectionMatrix;
        
        Quaternion adjustedRotation = 
            Quaternion.Euler(
                0, 
                0, 
                mainCamera.transform.eulerAngles.z
            );

        
        // Blue Camera

        Vector3 relativePositionToOrangePortal = orangePortal.InverseTransformPoint(mainCamera.transform.position);
        relativePositionToOrangePortal = Vector3.Scale(relativePositionToOrangePortal, new Vector3(-0.1f, -1, 0.01f));
        bluePortalCamera.transform.position = bluePortal.TransformPoint(relativePositionToOrangePortal);
        
        ClampCameraPosition(bluePortal, bluePortalCamera);
        
        Vector3 relativeRotationToBluePortal = orangePortal.InverseTransformDirection(mainCamera.transform.forward);
        relativeRotationToBluePortal = Vector3.Scale(relativeRotationToBluePortal, new Vector3(1, 1, -1));
        bluePortalCamera.transform.forward = bluePortal.TransformDirection(relativeRotationToBluePortal);
        
        // Adjust rotation to move opposite to main camera z to give the illusion of being still
        // Adjust rotation to preserve y rotation
        bluePortalCamera.transform.rotation = bluePortalCamera.transform.rotation * adjustedRotation;
        
        // Orange Camera

        Vector3 relativePositionToBluePortal = bluePortal.InverseTransformPoint(mainCamera.transform.position);
        relativePositionToBluePortal = Vector3.Scale(relativePositionToBluePortal, new Vector3(-0.1f, -1, 0.01f));
        orangePortalCamera.transform.position = orangePortal.TransformPoint(relativePositionToBluePortal);
        
        /*float angularDifferenceBetweenOrangePortalRotations = Quaternion.Angle(orangePortal.rotation, bluePortal.rotation);

        Quaternion orangePortalRotationalDifference = Quaternion.AngleAxis(angularDifferenceBetweenOrangePortalRotations, Vector3.up);
        Vector3 newOrangeCameraDirection = orangePortalRotationalDifference * mainCamera.transform.forward;
        orangePortalCamera.transform.rotation = Quaternion.LookRotation(newOrangeCameraDirection, Vector3.up);*/
        
        Vector3 relativeRotationToOrangePortal = bluePortal.InverseTransformDirection(mainCamera.transform.forward);
        relativeRotationToOrangePortal = Vector3.Scale(relativeRotationToOrangePortal, new Vector3(1, 1, -1));
        orangePortalCamera.transform.forward = orangePortal.TransformDirection(relativeRotationToOrangePortal);
        
        // Adjust rotation to move opposite to main camera z to give the illusion of being still
        // Adjust rotation to preserve y rotation
        //orangePortalCamera.transform.rotation = orangePortalCamera.transform.rotation * adjustedRotation;
        
        ClampCameraPosition(orangePortal, orangePortalCamera);
    }
}
