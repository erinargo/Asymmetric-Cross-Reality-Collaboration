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
    [Space]
    [SerializeField] private Material bluePortalMaterial;
    [SerializeField] private Material orangePortalMaterial;
    [Space]
    [SerializeField] private bool AR_VR;
    
    
    // Rotation limits
    public float pitchMin = 0; // Minimum pitch (look down)
    public float pitchMax = 0;  // Maximum pitch (look up)
    public float yawMin = 0;   // Minimum yaw (look left)
    public float yawMax = 175f;    // Maximum yaw (look right)
    
    // TEMPORARY SERIALIZABLE, ACTUAL IMPLEMENTATION TBD
    [SerializeField] private GameObject cameraRig;

    void Awake()
    {
        if (bluePortalCamera.targetTexture != null)
        {
            bluePortalCamera.targetTexture.Release();
        }
        
        bluePortalCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        bluePortalCamera.targetTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        
        if (orangePortalCamera.targetTexture != null)
        {
            orangePortalCamera.targetTexture.Release();
        }
        
        orangePortalCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        bluePortalCamera.targetTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        
        // Blue portal displays what orange can see and vice versa
        bluePortalMaterial.mainTexture = orangePortalCamera.targetTexture;
        if(!AR_VR) orangePortalMaterial.mainTexture = bluePortalCamera.targetTexture;
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

    void Update()
    {
        Quaternion adjustedRotation = 
            Quaternion.Euler(
                mainCamera.transform.eulerAngles.x, 
                mainCamera.transform.eulerAngles.y, 
                mainCamera.transform.eulerAngles.z
            );
        
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
        relativePositionToOrangePortal = Vector3.Scale(relativePositionToOrangePortal, new Vector3(-0.5f, 0, 0.01f));
        bluePortalCamera.transform.position = bluePortal.TransformPoint(relativePositionToOrangePortal);
        
        ClampCameraPosition(bluePortal, bluePortalCamera);
        
        Vector3 relativeRotationToBluePortal = orangePortal.InverseTransformDirection(mainCamera.transform.forward);
        relativeRotationToBluePortal = Vector3.Scale(relativeRotationToBluePortal, new Vector3(0, 0, -0.01f));
        bluePortalCamera.transform.forward = bluePortal.TransformDirection(relativeRotationToBluePortal);
        
        // Adjust rotation to move opposite to main camera z to give the illusion of being still
        // Adjust rotation to preserve y rotation
        bluePortalCamera.transform.rotation = bluePortalCamera.transform.rotation * adjustedRotation;
        
        // Orange Camera

        Vector3 relativePositionToBluePortal = bluePortal.InverseTransformPoint(mainCamera.transform.position);
        relativePositionToBluePortal = Vector3.Scale(relativePositionToBluePortal, new Vector3(-0.5f, -0.5f, 0.01f));
        orangePortalCamera.transform.position = orangePortal.TransformPoint(relativePositionToBluePortal);
        
        /*float angularDifferenceBetweenOrangePortalRotations = Quaternion.Angle(orangePortal.rotation, bluePortal.rotation);

        Quaternion orangePortalRotationalDifference = Quaternion.AngleAxis(angularDifferenceBetweenOrangePortalRotations, Vector3.up);
        Vector3 newOrangeCameraDirection = orangePortalRotationalDifference * mainCamera.transform.forward;
        orangePortalCamera.transform.rotation = Quaternion.LookRotation(newOrangeCameraDirection, Vector3.up);*/
        
        Vector3 relativeRotationToOrangePortal = bluePortal.InverseTransformDirection(mainCamera.transform.forward);
        relativeRotationToOrangePortal = Vector3.Scale(relativeRotationToOrangePortal, new Vector3(0, 0, -0.01f));
        orangePortalCamera.transform.forward = orangePortal.TransformDirection(relativeRotationToOrangePortal);
        
        // Adjust rotation to move opposite to main camera z to give the illusion of being still
        // Adjust rotation to preserve y rotation
        orangePortalCamera.transform.rotation = orangePortalCamera.transform.rotation * adjustedRotation;
        
        // Clamp the pitch and yaw of the orange portal camera's rotation
        Vector3 orangePortalEulerAngles = orangePortalCamera.transform.eulerAngles;
        //orangePortalEulerAngles.x = Mathf.Clamp(orangePortalEulerAngles.x, pitchMin, pitchMax);
        orangePortalEulerAngles.y = Mathf.Clamp(orangePortalEulerAngles.y, yawMin, yawMax);
        orangePortalCamera.transform.eulerAngles = orangePortalEulerAngles;
        
        ClampCameraPosition(orangePortal, orangePortalCamera);
    }
}
