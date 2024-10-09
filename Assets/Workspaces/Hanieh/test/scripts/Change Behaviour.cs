using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeBehaviour : MonoBehaviour
{
    void Start()
    {
        GetComponent<NewBehaviourScript>().color = Color.red;
    }
}
