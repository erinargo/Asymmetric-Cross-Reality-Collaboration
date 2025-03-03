using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffects : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> trees;
    private float timer = 0f;
    public float activationDelay = 2f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateTrees() {
        int rand = Random.Range(0, trees.Count);

        timer += Time.deltaTime;

        if (timer > activationDelay) {
            trees[rand].SetActive(true);    
        }
    }
}
