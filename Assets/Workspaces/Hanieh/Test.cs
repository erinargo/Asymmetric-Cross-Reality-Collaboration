using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private BFS bFS;
    [SerializeField] private BFS.Node node1;
    void Start() {
        
        // 1. Somehow figure out how to detect when cube has been dropped???
        // 2. Somehow figure out how to detect nearby nodes???
        // 3. Somehow pass nearest node into BFS script
        // 4. Activate function takes in type Node
            // 4.1 Somehow have to figure out how to convert position to node???
        

        // BFS.Singleton.Test();

        // print("node1: " + bFS.GetConnectedNodes());
        // bFS.
    }
    
    
}
