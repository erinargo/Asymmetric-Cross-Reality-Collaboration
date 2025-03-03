using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour {
    public static PortalManager Singleton { get; private set; }
    
    [SerializeField] private GameObject _portal;
    private GameObject _openPortal;
    
    void Awake() {
        if (Singleton != null && Singleton != this) Destroy(this.gameObject);
        else Singleton = this;
    }
    
    public void Open(Vector3 portal1, Vector3 portal2, Transform orangeTrans) {
        if(_openPortal != null) Destroy(_openPortal);
        
        _openPortal = Instantiate(_portal);
        
        foreach (Transform child in _openPortal.GetComponent<Transform>()) {
            if(child.name == "Orange") child.transform.position = portal2;
            if(child.name == "Blue") child.transform.position = portal1;
        }

        _openPortal.GetComponent<Portals>().orangeTrans = orangeTrans;
    }
    
    public void Open(Vector3 portal1, Vector3 portal2, Quaternion portalRotation) {
        if(_openPortal != null) Destroy(_openPortal);
        
        _openPortal = Instantiate(_portal);
        
        foreach (Transform child in _openPortal.GetComponent<Transform>()) {
            if(child.name == "Orange") child.transform.position = portal2;
            if (child.name == "Blue") {
                child.transform.position = portal1;
                child.transform.rotation = portalRotation;
            }
        }
    }
    
}