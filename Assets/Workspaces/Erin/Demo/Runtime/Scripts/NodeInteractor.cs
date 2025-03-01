using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInteractor : MonoBehaviour {
    private Transform playerPosition;
    [SerializeField] private Transform target;

    void Start() {
        playerPosition = GameManager.Singleton.player.transform;
    }

    public void OpenPortal() {
        PortalManager.Singleton.Open(GameManager.Singleton.minimap.transform.position + (new Vector3(1, 0, 1)), //(playerPosition.position - new Vector3(1f, -1, 1f)), 
            (target.position - new Vector3(1, -1, 1)), target);
    }

}
