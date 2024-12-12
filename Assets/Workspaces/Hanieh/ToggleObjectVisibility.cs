using UnityEngine;

public class ToggleObjectsVisibility : MonoBehaviour
{
    // Public toggle to show or hide all objects
    public bool areObjectsVisible = true;

    // Array to hold multiple objects
    public GameObject[] targetObjects;

    void Start()
    {
        // Iterate through each object in the array
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                // Enable or disable the object based on the toggle
                obj.SetActive(areObjectsVisible);
            }
            else
            {
                Debug.LogWarning("One of the target objects is not assigned!");
            }
        }
    }

    void Update()
    {
        // Optional: Dynamically update visibility during runtime
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(areObjectsVisible);
            }
            else
            {
                Debug.LogWarning("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
        }
    }
}
