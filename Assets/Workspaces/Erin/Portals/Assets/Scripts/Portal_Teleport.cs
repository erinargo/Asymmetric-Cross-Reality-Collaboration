using System;
using System.Collections;
using UnityEngine;

public class Portal_Teleport : MonoBehaviour {
    private Transform _player;
    private Transform camera;

    [SerializeField] private Transform _otherPortal;
    [SerializeField] private bool AR_VR;
    [SerializeField] private float teleportCooldown = 2.0f; // Cooldown in seconds

    private bool canTeleport = true;

    private void Start() {
        _player = GameManager.Singleton.player.transform;
        camera = GameManager.Singleton.mainCamera.transform;
    }

    void Teleport() {
        canTeleport = false; // Disable teleporting
        StartCoroutine(ResetTeleportCooldown()); // Start cooldown timer

        foreach (Transform child in _player.GetComponentsInChildren<Transform>()) {
            var translatedPosition = new Vector3(
                child.position.x + (transform.position.x - camera.position.x) + 1.5f, 
                child.position.y, 
                child.position.z + (transform.position.z - camera.position.z) + 1.5f
            );

            child.position = translatedPosition;
            child.rotation = Quaternion.Euler(
                child.rotation.eulerAngles.x,
                child.rotation.eulerAngles.y + (transform.rotation.eulerAngles.y - camera.rotation.eulerAngles.y) + 180f,
                child.rotation.eulerAngles.z
            );
        }

        if (AR_VR) PassthroughManager.Singleton.Toggle();
    }

    IEnumerator ResetTeleportCooldown() {
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true; // Re-enable teleporting
    }

    void Update() {
        float distance = Vector3.Distance(camera.position, _otherPortal.position);

        Debug.Log("Distance: " + distance);

        if (distance <= 0.9f && canTeleport) {
            Teleport();
        }
    }
}