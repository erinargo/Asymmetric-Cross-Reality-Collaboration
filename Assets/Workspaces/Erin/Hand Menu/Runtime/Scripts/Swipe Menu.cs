using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using TMPro;

public class SwipeMenu : MonoBehaviour {
    public AudioSource open;
    public AudioSource close;
    
    [SerializeField] private float menuGestureToOpenSpeed = 1.0F;
    
    private float step;
    
    private float p0;
    private float p1;

    private int updateCounter = 0;

    private bool stored;

    [SerializeField] private MenuManager menuManager;

    public void playSound(AudioSource sound) {
        if(sound != null) sound.Play();
    }

    void storePosition (string state) {
        // Null Object Detector
        if(menuManager.leftHand == null && menuManager.LeftControllerAnchor == null) return;

        // If the left hand is tracked we want to get the position of the skeleton
        // index finger was an arbitrary decision
        if(menuManager.leftHand.IsTracked && menuManager.isIndexFingerPinching) {
            /*foreach (var b in LeftSkeleton.Bones) {
                if (b.Id == OVRSkeleton.BoneId.Hand_IndexTip) {
                    if(state == "init") p0 = b.Transform.position.y;
                    else p1 = b.Transform.position.y;
                    break;
                }
            }*/
            if(state == "init") p0 = menuManager.leftHand.transform.position.y;
            else p1 = menuManager.leftHand.transform.position.y;
            
        } else {
            // if the left hand isn't tracked we already accounted that this code will only run if the hand or controller is present so therefore this can only run when the controller is present
            if(state == "init") p0 = menuManager.LeftControllerAnchor.position.y;
            else p1 = menuManager.LeftControllerAnchor.position.y;
        }
    }

    void comparePosition() {
        // store new position as transformed value
        storePosition("update");
        
        // if new position differs by a vertical value and velocity greater than 15 "units" and the hand is pinching open or close the menu depending on the direction 
        // -15 means the hand went up
        if((menuManager.leftHand != null && menuManager.leftHand.IsTracked) || (menuManager.LeftControllerAnchor != null)) {
            
            // p0 is initial state and p1 is updated state
            if(((p0 - p1) / Time.deltaTime) > 15.0 && menuManager.isIndexFingerPinching && !menuManager._menuActive.activeSelf) {
                playSound(open);
                
                menuManager._menuActive.SetActive(true); 

                // Set position and rotation to Menu Position so that it doesn't track to the camera (when the user walks away, menu will close. It's also easier to interact with multiple panels this way -- 
                // before when I had multiple panels it was hard to focus one or the other because turning my head to look at it would also turn the menu)
                menuManager.PositionMenu();
                
                // store init
                storePosition("init");
            }

            if(((p0 - p1) / Time.deltaTime) < -15.0 && menuManager.isIndexFingerPinching && menuManager._menuActive.activeSelf) {
                playSound(close);
                menuManager._menuActive.SetActive(false); // Destroy(menuManager._menuActive);

                storePosition("init");
            }
        }
    }
    
    void Update() {
        // This is so we're not always setting the initial position otherwise the player's movement would have to be impossibly quick to open the menu
        updateCounter++;
        
        if(menuManager.leftHand.IsTracked || (menuManager.LeftControllerAnchor != null)) {
            // Pinching is part of the gesture to open the menu
            menuManager.isIndexFingerPinching = menuManager.leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index) || OVRInput.Get(OVRInput.RawButton.LIndexTrigger);
        }

        // store init before two counts if it's not currently stored
        if(updateCounter <= 2 && !stored && (menuManager.leftHand.IsTracked || menuManager.LeftControllerAnchor != null)) {
            storePosition("init");
            stored = true; 
        }

        // compare init to new position after 25 counts and set stored to false, reset updateCounter
        if(updateCounter > 25) {
            updateCounter = 0;

            comparePosition();
            stored = false;
        }
    }
}
