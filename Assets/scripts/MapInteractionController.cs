using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapInteractionController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject infoPanel;
    public Image monumentImageUI;
    public TextMeshProUGUI nameTextUI;
    public TextMeshProUGUI locationTextUI;
    public TextMeshProUGUI descriptionTextUI;
    public Button viewInARButton;
    public GameObject blockerPanel;
    private Camera mapCamera;
    private WaypointData selectedWaypoint;
    private MapController mapController; // Reference to your other script

    void Start()
    {
        mapCamera = GetComponent<Camera>();
        mapController = FindObjectOfType<MapController>(); // Find the MapController in the scene
        infoPanel.SetActive(false);
        viewInARButton.onClick.AddListener(OnViewInARClicked);
    }

    void Update()
    {
        // Use this for testing in Editor
        if (Input.GetMouseButtonDown(0))
        {
            HandleTap(Input.mousePosition);
        }
        // Use this for touch on a device
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleTap(Input.GetTouch(0).position);
        }
    }
    void HandleTap(Vector2 screenPosition)
    {
        // Create a 3D ray from the camera through the point that was tapped
        Ray ray = mapCamera.ScreenPointToRay(screenPosition);

        // Fire the ray and check if it hits anything
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // The ray hit something! Check if it was a waypoint.
            WaypointData waypoint = hit.collider.GetComponent<WaypointData>();
            if (waypoint != null)
            {
                // It was a waypoint! Show its info.
                ShowInfoPanel(waypoint);
            }
        }
    }
    public void HideInfoPanel()
    {
        infoPanel.SetActive(false);
        blockerPanel.SetActive(false);
    }
    void ShowInfoPanel(WaypointData waypoint)
    {
        selectedWaypoint = waypoint;

        monumentImageUI.sprite = waypoint.monumentImage;
        nameTextUI.text = waypoint.monumentName;
        locationTextUI.text = waypoint.location;
        descriptionTextUI.text = waypoint.description;
        infoPanel.SetActive(true);
        blockerPanel.SetActive(true);
    }

    void OnViewInARClicked()
    {
        if (selectedWaypoint != null && mapController != null)
        {
            // Tell the MapController to start the AR experience with the selected ID
            mapController.StartARExperienceFor(selectedWaypoint.monumentId);
        }
        else
        {
            Debug.LogError("Could not start AR Experience. SelectedWaypoint or MapController is missing!");
        }
    }
}