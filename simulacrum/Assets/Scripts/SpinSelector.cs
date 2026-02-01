using UnityEngine;

public class SpinSelector : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private float rotationSpeed = 90f; // degrees per second

    private float currentRotation = 0f;
    private static readonly int RotationProperty = Shader.PropertyToID("_Rotation");

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Accumulate rotation in radians
        currentRotation += rotationSpeed * Mathf.Deg2Rad * Time.deltaTime;

        // Keep rotation in 0 to 2π range
        if (currentRotation > Mathf.PI * 2f)
            currentRotation -= Mathf.PI * 2f;

        // Update the shader property
        spriteRenderer.material.SetFloat(RotationProperty, currentRotation);
    }
}