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
        // Solar,
        // Gas,
        // Recycle // Added new item types
    }
    
    enum State {
        PickedUp,
        Placed
    }

    [SerializeField] private GameManager gameManager;

    [SerializeField] private Transform snapPoint; // Define snap points in the scene

    [SerializeField] private ItemType item;
    [SerializeField] private Vector3 resizeVal;

    [SerializeField] private float ResizeMultiplier;

    [SerializeField] private Vector3 maxSize;
    private Vector3 minSize;
    private Vector3 lastSize;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;

    public bool resizeActive;

    private State state;
    
    void Awake() {
        minSize = gameObject.transform.localScale;
        lastSize = minSize;

        originalPosition = gameObject.transform.localPosition;
        originalRotation = gameObject.transform.localRotation;
        originalParent = transform.parent;

        state = State.Placed;
    }

    public void PickUp() {
        transform.parent = null;

        if (item == ItemType.Bike || item == ItemType.Bus) transform.localScale = resizeVal; // maybe rework?
        state = State.PickedUp;
    }

    public void PutDown() {
        state = State.Placed;
    }

    public void ResizeToggle() {
        if (item == ItemType.Window) resizeActive = !resizeActive;
    }

    void Update() {
        float disToMap = 
            Vector3.Distance(gameObject.transform.position, GameManager.Singleton.minimap.transform.position);
        
        float disToMenu = 
            Vector3.Distance(gameObject.transform.position, MenuManager.Singleton._menuActive.transform.position);

        bool menuActive = MenuManager.Singleton._menuActive.activeSelf;
        
        gameObject.transform.localScale = lastSize;
        
        if (MenuManager.Singleton.rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index) &&
            resizeActive && item == ItemType.Window) {
            float size = Vector3.Distance(MenuManager.Singleton.leftHand.transform.position, MenuManager.Singleton.rightHand.transform.position);
            size = size * ResizeMultiplier;
            
            Debug.Log(size);

            float normalSizex = 
                Mathf.Clamp((size * size) * gameObject.transform.localScale.x, minSize.x, maxSize.x);
            float normalSizey = 
                Mathf.Clamp((size * size) * gameObject.transform.localScale.y, minSize.y, maxSize.y);
            float normalSizez = 
                Mathf.Clamp((size * size) * gameObject.transform.localScale.z, minSize.z, maxSize.z);

            lastSize = new Vector3(normalSizex, normalSizey, normalSizez);
        }

        if (disToMap <= 0.2f && state == State.Placed && item != ItemType.Window) {
            transform.position = snapPoint.position;
            transform.rotation = snapPoint.rotation;
            
            // activate
            if (gameManager == null)
            {
                gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();;
            }
            gameManager.Activate(item);
        }

        if (disToMenu <= 0.2f && !resizeActive && menuActive && (item != ItemType.Bus && item != ItemType.Bike && item != ItemType.Car)) {
            gameObject.transform.parent = originalParent;
            
            gameObject.transform.localPosition = originalPosition;
            gameObject.transform.localRotation = originalRotation;
            lastSize = minSize;
        } // transform.parent = originalPosition; // reparent
    }

}
