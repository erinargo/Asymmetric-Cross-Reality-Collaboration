using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuManager : MonoBehaviour {
    public OVRHand leftHand;
    public Transform LeftControllerAnchor;

    public Transform menuHolder;
    public GameObject _menuActive;
    
    public bool isIndexFingerPinching;

    bool IsFollow = false;
    
    [Space]
    public GameObject menuPrefab;
    public bool toggle;

    private bool _placing;
    [Space]
    
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private Vector3 posOffset = new(0, 0.1875f, 0.25f);
    [Space]
    
    [SerializeField] private float maxLookAngle;
    [SerializeField] private float maxLookDistance;
    
    [Space]
    [SerializeField] private bool _invert;

    void Awake() { // Expensive but only once. 
        leftHand = 
            OVRManager.instance.gameObject.GetComponentsInChildren<OVRHand>()
                .FirstOrDefault(h => h.GetHand() == OVRPlugin.Hand.HandLeft ? h : null); 
        
        LeftControllerAnchor = 
            OVRManager.instance.gameObject.GetComponentsInChildren<Transform>()
                .FirstOrDefault(a => a.name == "LeftHandAnchor" ? a : null);
        
        menuHolder = 
            OVRManager.instance.gameObject.GetComponentsInChildren<Camera>()
                .FirstOrDefault(c => c.gameObject.transform.name == "CenterEyeAnchor" ? c : null)
                ?.gameObject.transform;
    }

    bool IsLookingAway() {
        Vector3 menuForward = _menuActive.transform.rotation * Vector3.forward;
        Vector3 playerForward = LeftControllerAnchor.forward;
        float angle = Vector3.Angle(menuForward, playerForward);

        float distance = Vector3.Distance(_menuActive.transform.position, 
            LeftControllerAnchor.position);

        return angle > maxLookAngle || distance > maxLookDistance;
    }

    public void PositionMenu() {
        Vector3 forwardOffset = LeftControllerAnchor.forward * posOffset.z;
        Vector3 upOffset = Vector3.up * posOffset.y;
        Vector3 rightOffset = LeftControllerAnchor.right * posOffset.x;
    
        Vector3 targetPos = LeftControllerAnchor.position + forwardOffset + upOffset + rightOffset;
    
        _menuActive.transform.position = Vector3.Slerp(_menuActive.transform.position, targetPos, lerpSpeed * Time.deltaTime);
    
        Vector3 headsetForward = menuHolder.forward;
        if (_invert) headsetForward = -headsetForward;

        Quaternion targetRotation = Quaternion.LookRotation(headsetForward, Vector3.up);
        _menuActive.transform.rotation = Quaternion.Slerp(_menuActive.transform.rotation, targetRotation, lerpSpeed * Time.deltaTime);
    }

    void Update() {
        if(isIndexFingerPinching) IsFollow = true;
        else IsFollow = false;
        
        if (_menuActive&& IsFollow) PositionMenu();
        
        if (toggle) {
            toggle = false;
            _menuActive = Instantiate(GetComponent<MenuManager>().menuPrefab);
            PositionMenu();
        }
    }
}
