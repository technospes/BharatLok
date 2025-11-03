using UnityEngine;

public class CoordinateFinder : MonoBehaviour
{
    private Camera cam;
    void Start() { cam = GetComponent<Camera>(); }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // This line draws a visible yellow line in the SCENE view for 5 seconds
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 5.0f);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // This will print if we successfully hit something
                Debug.Log($"SUCCESS! Clicked Position: new Vector3({hit.point.x}f, {hit.point.y}f, {hit.point.z}f)");
            }
            else
            {
                // This will print a warning if we hit NOTHING
                Debug.LogWarning("Coordinate Finder Raycast: Did not hit any collider!");
            }
        }
    }
}