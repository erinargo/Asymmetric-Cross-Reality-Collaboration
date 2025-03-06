using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuItems : MonoBehaviour {
    public enum ItemType {
        Window,
        Bus,
        Bike,
        Car,
        Solar,
        Recycle // Added new item types
    }
    
    enum State {
        PickedUp,
        Placed,
        OnMap
    }

    [SerializeField] private Transform snapPoint; // Define snap points in the scene

    [SerializeField] private ItemType item;
    [SerializeField] private Vector3 resizeVal;

    [SerializeField] private float ResizeMultiplier;

    [SerializeField] private Vector3 maxSize;
    private Vector3 originalSize;
    private Vector3 lastSize;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;

    public bool resizeActive;

    private State state;
    
    void Awake() {
        originalSize = gameObject.transform.localScale;
        lastSize = originalSize;

        originalPosition = gameObject.transform.localPosition;
        originalRotation = gameObject.transform.localRotation;
        originalParent = transform.parent.transform.parent;

        state = State.Placed;
    }

    public void PickUp() {
        transform.parent.transform.parent = null;
        
        Debug.Log(state.ToString());
        
        if (state == State.OnMap) 
            GameManager.Singleton.Activate(item);

        //if (item == ItemType.Bike || item == ItemType.Bus) transform.localScale = resizeVal; // maybe rework?
        state = State.PickedUp;
    }

    public void PutDown() {
        float disToMap = 
            Vector3.Distance(gameObject.transform.parent.transform.position, GameManager.Singleton.minimap.transform.parent.transform.parent.position);
        
        state = State.Placed;
    }

    public void ResizeToggle() {
        if (item == ItemType.Window) resizeActive = !resizeActive;
    }

    void Update() {
        float disToMap = 
            Vector3.Distance(gameObject.transform.parent.transform.position, GameManager.Singleton.minimap.transform.parent.transform.parent.position);
        
        float disToMenu = 
            Vector3.Distance(gameObject.transform.parent.transform.position, MenuManager.Singleton._menuActive.transform.transform.position);

        bool menuActive = MenuManager.Singleton._menuActive.activeSelf;
        
        gameObject.transform.localScale = lastSize;
        
        if (MenuManager.Singleton.rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index) &&
            resizeActive && item == ItemType.Window) {
            float size = Vector3.Distance(MenuManager.Singleton.leftHand.transform.position, MenuManager.Singleton.rightHand.transform.position);
            size = size * ResizeMultiplier;
            
            Debug.Log(size);

            float normalSizex = 
                Mathf.Clamp((size * size) * gameObject.transform.localScale.x, originalSize.x, maxSize.x);
            float normalSizey = 
                Mathf.Clamp((size * size) * gameObject.transform.localScale.y, originalSize.y, maxSize.y);
            float normalSizez = 
                Mathf.Clamp((size * size) * gameObject.transform.localScale.z, originalSize.z, maxSize.z);

            lastSize = new Vector3(normalSizex, normalSizey, normalSizez);
        }

        if (disToMap <= 0.2f && state != State.OnMap && state != State.PickedUp && item != ItemType.Window) {
            //transform.parent = snapPoint;
            state = State.OnMap;
            
            transform.parent.position = snapPoint.position;
            transform.parent.rotation = snapPoint.rotation;
            
            transform.position = snapPoint.position;
            transform.rotation = snapPoint.rotation;
            
            GameManager.Singleton.Activate(item);
        }

        if (state == State.OnMap) {
            transform.parent.position = snapPoint.position;
            transform.parent.rotation = snapPoint.rotation;
            
            transform.position = snapPoint.position;
            transform.rotation = snapPoint.rotation;
        }

        if (disToMenu <= 0.2f && !resizeActive && menuActive && (state != State.OnMap && state != State.PickedUp)) {
            gameObject.transform.parent.transform.parent = originalParent;
            
            gameObject.transform.parent.transform.localPosition = originalPosition;
            gameObject.transform.parent.transform.localRotation = originalRotation;

            gameObject.transform.localScale = originalSize;
            
            lastSize = originalSize;
        } // transform.parent = originalPosition; // reparent
    }

}
