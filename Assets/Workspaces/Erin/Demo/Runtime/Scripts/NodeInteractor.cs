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
        PortalManager.Singleton.Open((playerPosition.position - new Vector3(1.2f, -1, 1.2f)), (target.position - new Vector3(1, -1, 1)));
    }

}
