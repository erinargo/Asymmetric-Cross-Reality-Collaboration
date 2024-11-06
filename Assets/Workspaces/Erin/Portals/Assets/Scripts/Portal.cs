using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Camera bluePortalCamera;
    [SerializeField] private Camera orangePortalCamera;
    [Space] 
    [SerializeField] private Transform bluePortal;
    [SerializeField] private Transform orangePortal;
    [Space]
    [SerializeField] private Camera mainCamera;
    [Space]
    [SerializeField] private Material bluePortalMaterial;
    [SerializeField] private Material orangePortalMaterial;
    [Space]
    [SerializeField] private bool AR_VR;

    // TEMPORARY SERIALIZABLE, ACTUAL IMPLEMENTATION TBD
    [SerializeField] private GameObject cameraRig;

    void Awake()
    {
        if (bluePortalCamera.targetTexture != null)
        {
            bluePortalCamera.targetTexture.Release();
        }
        
        bluePortalCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        
        if (orangePortalCamera.targetTexture != null)
        {
            orangePortalCamera.targetTexture.Release();
        }
        
        orangePortalCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        
        // Blue portal displays what orange can see and vice versa
        bluePortalMaterial.mainTexture = orangePortalCamera.targetTexture;
        if(!AR_VR) orangePortalMaterial.mainTexture = bluePortalCamera.targetTexture;
    }
    
    void ClampCameraPosition(Transform portal, Camera portalCamera)
    {
        // Calculate the vector from the portal to the camera
        Vector3 directionToCamera = portalCamera.transform.position - portal.position;

        // Get the portal's FOV (vertical)
        float halfVerticalFOV = Mathf.Tan(Mathf.Deg2Rad * mainCamera.fieldOfView / 2);
    
        // Get the distance from the portal to the camera
        float distanceToCamera = directionToCamera.magnitude;

        // Get the size of the portal based on its scale (height)
        float portalHeight = portal.localScale.y;
    
        // The maximum distance at which the camera can be from the portal before it goes beyond the FOV
        float maxDistance = portalHeight / (2 * halfVerticalFOV);
    
        // Clamp the camera's distance to the portal so it doesn't go beyond the FOV
        if (distanceToCamera > maxDistance)
        {
            portalCamera.transform.position = portal.position + directionToCamera.normalized * maxDistance;
        }
    }

    void Update()
    {
        float mainCameraZRotation = mainCamera.transform.eulerAngles.z;
        Quaternion invertedRotation = Quaternion.Euler(0, 0, mainCameraZRotation);
        
        // Adjust Projection Matrix
        float FOV = mainCamera.fieldOfView;
        float aspectRatio = mainCamera.aspect;
        
        bluePortalCamera.fieldOfView = FOV;
        orangePortalCamera.fieldOfView = FOV;
        
        bluePortalCamera.aspect = aspectRatio;
        orangePortalCamera.aspect = aspectRatio;
        
        Matrix4x4 projectionMatrix = mainCamera.projectionMatrix;
        
        bluePortalCamera.projectionMatrix = projectionMatrix;
        orangePortalCamera.projectionMatrix = projectionMatrix;
        
        // Blue Camera

        Vector3 relativePositionToOrangePortal = orangePortal.InverseTransformPoint(mainCamera.transform.position);
        relativePositionToOrangePortal = Vector3.Scale(relativePositionToOrangePortal, new Vector3(-1, 0, -1));
        bluePortalCamera.transform.position = bluePortal.TransformPoint(relativePositionToOrangePortal);
        
        ClampCameraPosition(bluePortal, bluePortalCamera);
        
        float angularDifferenceBetweenBluePortalRotations = Quaternion.Angle(bluePortal.rotation, orangePortal.rotation);

        Quaternion bluePortalRotationalDifference = Quaternion.AngleAxis(angularDifferenceBetweenBluePortalRotations, Vector3.up);
        Vector3 newBlueCameraDirection = bluePortalRotationalDifference * mainCamera.transform.forward;
        bluePortalCamera.transform.rotation = Quaternion.LookRotation(newBlueCameraDirection, Vector3.up);
        
        bluePortalCamera.transform.rotation = bluePortalCamera.transform.rotation * invertedRotation;
        
        // Orange Camera

        Vector3 relativePositionToBluePortal = bluePortal.InverseTransformPoint(mainCamera.transform.position);
        relativePositionToBluePortal = Vector3.Scale(relativePositionToBluePortal, new Vector3(-1, 0, -1));
        orangePortalCamera.transform.position = orangePortal.TransformPoint(relativePositionToBluePortal);
        
        float angularDifferenceBetweenOrangePortalRotations = Quaternion.Angle(orangePortal.rotation, bluePortal.rotation);

        Quaternion orangePortalRotationalDifference = Quaternion.AngleAxis(angularDifferenceBetweenOrangePortalRotations, Vector3.up);
        Vector3 newOrangeCameraDirection = orangePortalRotationalDifference * mainCamera.transform.forward;
        orangePortalCamera.transform.rotation = Quaternion.LookRotation(newOrangeCameraDirection, Vector3.up);
        
        orangePortalCamera.transform.rotation = orangePortalCamera.transform.rotation * invertedRotation;
        
        ClampCameraPosition(orangePortal, orangePortalCamera);
    }
}
