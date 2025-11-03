using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Scale Settings")]
    [Tooltip("Adjust to change icon size (0.1-0.3 recommended for large spheres)")]
    public float iconSizeMultiplier = 0.15f;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Calculate direction FROM hotspot TO camera (correct direction!)
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;

            // Make the hotspot look at the camera
            transform.rotation = Quaternion.LookRotation(directionToCamera);

            // Scale based on distance for consistent size
            float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
            transform.localScale = Vector3.one * distance * iconSizeMultiplier;
        }
    }
}