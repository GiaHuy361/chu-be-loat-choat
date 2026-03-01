using UnityEngine;

public class LanternFlicker : MonoBehaviour
{
    public Light lanternLight;
    public float minIntensity = 0.8f;
    public float maxIntensity = 1.2f;
    public float flickerSpeed = 0.1f;

    void Update()
    {
        // T?o s? thay ??i ng?u nhiên d?a trên th?i gian
        lanternLight.intensity = Mathf.Lerp(lanternLight.intensity, Random.Range(minIntensity, maxIntensity), flickerSpeed);
    }
}