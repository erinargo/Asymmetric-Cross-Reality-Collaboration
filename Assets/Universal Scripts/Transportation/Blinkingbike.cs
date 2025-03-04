using UnityEngine;

public class Blinkingbike : MonoBehaviour
{
    public GameObject planePrefab; // Assign the bike station prefab
    public float blinkInterval = 1f; // Time for blinking 

    private void Start()
    {
        if (planePrefab != null)
        {
            StartCoroutine(BlinkPlane());
        }
    }

    private System.Collections.IEnumerator BlinkPlane()
    {
        while (true) // Loop forever
        {
            planePrefab.SetActive(!planePrefab.activeSelf); // Toggle the plane
            yield return new WaitForSeconds(blinkInterval); // Wait for the interval
        }
    }
}