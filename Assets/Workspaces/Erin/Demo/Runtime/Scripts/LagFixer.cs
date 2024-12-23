using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LagFixer : MonoBehaviour {
    private bool set = false;
    
    // Start is called before the first frame update
    void Update() {
        if (!set) {
            foreach (var child in GetComponentsInChildren<Transform>(true)) 
                child.gameObject.SetActive(true);

            set = true;
        }
    }
    
}
