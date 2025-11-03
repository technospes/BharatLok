using UnityEngine;

public class PanoramaCameraController : MonoBehaviour
{
    [Header("Controls")]
    public float rotationSpeed = 0.2f;
    public float zoomSpeed = 20f;

    [Header("Zoom Limits")]
    public float minFieldOfView = 20f;
    public float maxFieldOfView = 90f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            // If it is, do nothing. Exit the method.
            return;
        }
        // Handle rotation with a single touch or mouse drag
        if (Input.GetMouseButton(0) && Input.touchCount < 2)
        {
            transform.Rotate(Vector3.up, -Input.GetAxis("Mouse X") * rotationSpeed, Space.World);
            transform.Rotate(Vector3.right, Input.GetAxis("Mouse Y") * rotationSpeed, Space.Self);
        }

        // Handle pinch-to-zoom on mobile devices
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            // Adjust the camera's field of view
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - difference * 0.1f, minFieldOfView, maxFieldOfView);
        }

        // Handle scroll wheel zoom for testing in the Editor
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - scroll * zoomSpeed, minFieldOfView, maxFieldOfView);
    }
}