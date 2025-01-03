using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffects : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> trees;

    private float timer = 0f;
    public float activationDelay = 2f;

    [Range(0, 100)]
    public float percentCarbon = 100f;

    [SerializeField]
    public float minSize = 1f;         // Minimum particle size
    [SerializeField]
    public float maxSize = 100f;         // Maximum particle size

    public bool isFullCarbon = true;
    public bool isMixedCarbon = false;
    public bool isNoCarbon = false;

    public ParticleSystem particleSystem;
    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();

        foreach (GameObject tree in trees)
        {
            tree.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        var mainModule = particleSystem.main;

        float scaledSize = Mathf.Lerp(minSize, maxSize , percentCarbon / 100f); // Map percentage to size range

        mainModule.startSize3D = true;
        mainModule.startSizeX = scaledSize;
        mainModule.startSizeY = scaledSize;
        mainModule.startSizeZ = scaledSize;
        
        if (percentCarbon < 100 && percentCarbon > 50)
        {
            isFullCarbon = false;
            isMixedCarbon = true;

        }
        else if (percentCarbon <= 50)
        {
            isFullCarbon = false;
            isMixedCarbon = false;
            isNoCarbon = true;
        }
    }

    public void GenerateTrees() {
        int rand = Random.Range(0, trees.Count);

        timer += Time.deltaTime;

        if (timer > activationDelay) {
            trees[rand].SetActive(true);    
        }
    }
}
