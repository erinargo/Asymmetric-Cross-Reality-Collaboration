using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField]
    public Color color;
    
    [SerializeField]
    private float rateOfChange;
    
    // Start is called before the first frame update
    void Start()
    {
        //Materials.color = "Blue";

        GetComponent<Renderer>().material.color = color; //Color.blue;
    }

    void Update()
    {
        //transform.Translate(Vector3.up * Time.deltaTime);
        float newX = gameObject.transform.position.x + 0.01f;
        gameObject.transform.position = new Vector3(newX, gameObject.transform.position.y, gameObject.transform.position.z);
        
        Quaternion newRotation = Quaternion.Euler(0, gameObject.transform.rotation.eulerAngles.y + rateOfChange, 0);
        gameObject.transform.rotation = newRotation;
    }
}
