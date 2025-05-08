using UnityEngine;

public class ButterflyPathOffset : MonoBehaviour, IPathOffsetProvider
{
    public float horizontalAmplitude = 0.5f;
    public float verticalAmplitude = 0.5f;
    public float horizontalFrequency = 10f;
    public float verticalFrequency = 10f;

    public Vector3 GetOffset(float t)
    {
        return new Vector3(
            Mathf.Sin(t * horizontalFrequency) * horizontalAmplitude,
            Mathf.Sin(t * verticalFrequency * Mathf.PI) * verticalAmplitude,
            0f
        );
    }
}