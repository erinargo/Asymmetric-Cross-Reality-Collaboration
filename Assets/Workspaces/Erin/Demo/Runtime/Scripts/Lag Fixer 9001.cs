using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LagFixer9001 : MonoBehaviour {
    bool hasFixed;
    
    // Doesn't work onStart 
    void LateUpdate() {
        if (!hasFixed) {
            foreach (var child in GetComponentsInChildren<Transform>(true)) 
                if(child.gameObject.name == "B U I L D I N G S") child.gameObject.SetActive(true);
            
            hasFixed = true;
        }
    }
}
