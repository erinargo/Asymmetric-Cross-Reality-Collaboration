using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer rend;
    [SerializeField] Color color;
    [SerializeField] float rateOfChangeColor;

    Color startColor;

    private void Awake() {
       startColor = rend.material.color;
    }

    public IEnumerator TimeFunction(float time = 2.0f) {
        float elapsedTime = 0f;
        while (elapsedTime < time) {
            rend.material.color = Color.Lerp(startColor, color, elapsedTime / time);
            elapsedTime = Time.deltaTime;

            yield return null;
        }

        rend.material.color = color;
    }

    public void ChangeColorOverTime() {
        StartCoroutine(TimeFunction(rateOfChangeColor));
    }

    public void ChangeColor() {
        rend.material.color = color;
    }
}
