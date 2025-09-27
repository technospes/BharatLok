using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float speed = 10f;

    // FixedUpdate is called on a consistent timer, independent of frame rate
    void FixedUpdate()
    {
        // Calculate rotation for this fixed time step
        transform.Rotate(Vector3.up, speed * Time.fixedDeltaTime, Space.World);
    }
}