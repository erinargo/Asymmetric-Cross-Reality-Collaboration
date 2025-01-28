using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TrackPlayer : MonoBehaviour
{
    
    public Transform XRcamera; // Assign the first player in the Inspector
    public Transform player; // Assign the second player in the Inspector
    public Transform miniMapObject;

    public GameObject playerObj;
    public GameObject minimapPlayerRepresentation;

    public LayerMask obstacleMask; // Set the layer(s) to detect obstacles
    public float miniMapScale = 0.001f;
    private LineRenderer lineRenderer;

    public void MovePlayer()
    {
        // Move the player in the main map
        Vector3 worldPosition = playerObj.transform.position;

        // Update the minimap position
        Vector3 minimapPosition = new Vector3(
            worldPosition.x * miniMapScale,
            worldPosition.y * miniMapScale,  // Adjust if Y represents height or Z for depth
            worldPosition.z * miniMapScale
        );

        minimapPlayerRepresentation.transform.position = minimapPosition;

        // Sync the rotation (if needed)
        minimapPlayerRepresentation.transform.rotation = playerObj.transform.rotation;
    }

    
    
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2; // Line needs two points
        
    }

    void Update()
    {
        if (XRcamera != null && player != null)
        {
            // Calculate the direction and distance to the second player
            Vector3 direction = miniMapObject.position - XRcamera.position;
            float distance = direction.magnitude;

            // Perform the raycast
            if (Physics.Raycast(XRcamera.position, direction.normalized, out RaycastHit hit, distance, obstacleMask))
            {
                Debug.Log($"Obstruction detected: {hit.collider.name}");
                lineRenderer.SetPosition(0, XRcamera.position);
                lineRenderer.SetPosition(1, hit.point);
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;

               

            }
            else
            {
                Debug.Log("Clear line of sight between players.");
                lineRenderer.SetPosition(0, XRcamera.position);
                lineRenderer.SetPosition(1, miniMapObject.position);
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;

                MovePlayer();

                //// Get the world position of the object to be placed
                //Vector3 worldPosition = miniMapObject.position;

                //// Convert world position to local position relative to the player
                //Vector3 localPosition = player.InverseTransformPoint(worldPosition);

                //// Scale down the local position for the mini-map
                //localPosition *= miniMapScale;

                //// Apply the local position to the mini-map object
                //miniMapObject.localPosition = localPosition;
            }

            // Optional: Visualize the ray in the Scene view
            //Debug.DrawRay(player1.position, direction.normalized * distance, Color.green);
        }
    }
}
