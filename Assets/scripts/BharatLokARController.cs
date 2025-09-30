using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class BharatLokARController : MonoBehaviour
{
    [Header("System References")]
    public ARSession arSession;
    // REMOVED: public Camera arCamera; - Not needed, use Camera.main

    [Header("Prefabs")]
    public GameObject placementIndicator;

    [Header("Settings")]
    public float initialScale = 0.1f;  // INCREASED from 0.05f for visibility
    public float rotationSpeed = 0.4f;
    public float zoomSpeed = 0.005f;   // INCREASED for better sensitivity

    [Header("UI")]
    public GameObject instructionPanel;
    public GameObject interactionUIPanel;
    public TextMeshProUGUI onScreenDebugText;

    private enum ARState { AwaitingPermission, Scanning, ReadyToPlace, ObjectPlaced }
    private ARState currentState;

    private GameObject spawnedObject;
    private ARRaycastManager arRaycastManager;
    private ARAnchorManager arAnchorManager;
    private ARPlaneManager arPlaneManager;
    private List<ARAnchor> allAnchors = new List<ARAnchor>();
    private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    private bool isPlacementPoseValid = false;
    private bool isNarrationPlaying = false;
    private bool hasPlacedObject = false;

    void Awake()
    {
        Application.targetFrameRate = 60;

        // Get AR components
        arRaycastManager = GetComponent<ARRaycastManager>();
        arAnchorManager = GetComponent<ARAnchorManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();

        // CRITICAL: Ensure AR Session is active
        if (arSession != null)
        {
            arSession.gameObject.SetActive(true);
            Debug.Log("AR Session activated in Awake");
        }
        else
        {
            Debug.LogError("AR Session reference is null! Assign it in inspector.");
        }

        // Configure ARPlaneManager
        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = true;
            arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            Debug.Log($"ARPlaneManager configured: {arPlaneManager.requestedDetectionMode}");
        }

        // Initialize UI state
        if (placementIndicator != null) placementIndicator.SetActive(false);
        if (interactionUIPanel != null) interactionUIPanel.SetActive(false);
        if (instructionPanel != null) instructionPanel.SetActive(true);

        Debug.Log("BharatLokARController Awake complete");
    }

    IEnumerator Start()
    {
        Debug.Log("Starting AR initialization...");
        currentState = ARState.AwaitingPermission;

        yield return RequestCameraPermission();

        // Initialize TTS after permission granted
        NativeTTS.Initialize();
        Debug.Log("AR Controller Start complete");
    }

    void Update()
    {
        // Debug AR state every 60 frames (1 second at 60fps)
        if (Time.frameCount % 60 == 0)
        {
            //DebugARState();
        }

        switch (currentState)
        {
            case ARState.AwaitingPermission:
                // Wait for permission coroutine to complete
                break;

            case ARState.Scanning:
                if (instructionPanel != null && instructionPanel.activeSelf)
                {
                    instructionPanel.SetActive(false);
                }

                UpdatePlacementPose();
                UpdatePlacementIndicator();

                if (isPlacementPoseValid)
                {
                    Debug.Log("Valid placement pose found, switching to ReadyToPlace");
                    currentState = ARState.ReadyToPlace;
                }
                break;

            case ARState.ReadyToPlace:
                UpdatePlacementPose();
                UpdatePlacementIndicator();

                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !hasPlacedObject)
                {
                    if (!IsPointerOverUI(Input.GetTouch(0).position))
                    {
                        Debug.Log("Touch detected, attempting to place object");
                        PlaceObject();
                    }
                }
                break;

            case ARState.ObjectPlaced:
                HandleInteraction();
                break;
        }
    }

    // Debug method to track AR state
    //private void DebugARState()
    //{
    //    string debugInfo = $"AR State: {currentState}\n";
    //    debugInfo += $"Session State: {ARSession.state}\n";
    //    debugInfo += $"Placement Valid: {isPlacementPoseValid}\n";
    //    debugInfo += $"Planes Detected: {(arPlaneManager != null ? arPlaneManager.trackables.count : 0)}\n";

    //    if (SelectionManager.Instance?.selectedMonument != null)
    //    {
    //        debugInfo += $"Monument: {SelectionManager.Instance.selectedMonument.name}\n";
    //        debugInfo += $"Prefab Loaded: {SelectionManager.Instance.selectedMonument.loadedPrefab != null}\n";
    //    }
    //    else
    //    {
    //        debugInfo += "No monument selected\n";
    //    }

    //    if (onScreenDebugText != null)
    //    {
    //        onScreenDebugText.text = debugInfo;
    //    }

    //    Debug.Log($"AR Debug - {debugInfo.Replace('\n', '|')}");
    //}

    // Check if touch is over UI elements
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        return UnityEngine.EventSystems.EventSystem.current != null &&
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    // FIXED: Proper PlaceObject method
    private void PlaceObject()
    {
        Debug.Log("=== PlaceObject Called ===");

        // Comprehensive validation
        if (SelectionManager.Instance?.selectedMonument?.loadedPrefab == null)
        {
            Debug.LogError("No monument prefab available to place!");
            return;
        }

        if (s_Hits.Count == 0)
        {
            Debug.LogError("No raycast hits available for placement!");
            return;
        }

        Debug.Log($"Placing monument: {SelectionManager.Instance.selectedMonument.name}");
        Debug.Log($"Placement position: {s_Hits[0].pose.position}");

        // Create anchor using new method (not deprecated AttachAnchor)
        GameObject anchorGameObject = new GameObject("MonumentAnchor");
        anchorGameObject.transform.SetPositionAndRotation(s_Hits[0].pose.position, s_Hits[0].pose.rotation);

        ARAnchor anchor = anchorGameObject.AddComponent<ARAnchor>();
        if (anchor == null)
        {
            Debug.LogError("Failed to create ARAnchor component!");
            Destroy(anchorGameObject);
            return;
        }

        allAnchors.Add(anchor);
        Debug.Log("Anchor created successfully");

        // Instantiate monument
        GameObject monumentPrefab = SelectionManager.Instance.selectedMonument.loadedPrefab;
        spawnedObject = Instantiate(monumentPrefab, anchorGameObject.transform);

        // Configure spawned object
        spawnedObject.SetActive(true);
        spawnedObject.transform.localPosition = Vector3.zero;
        spawnedObject.transform.localRotation = Quaternion.identity;
        spawnedObject.transform.localScale = Vector3.zero; // Start from zero for animation

        Debug.Log($"Monument instantiated: {spawnedObject.name}");
        Debug.Log($"Monument world position: {spawnedObject.transform.position}");

        // Start animation
        StartCoroutine(AnimateSpawn());

        // Update UI and state
        if (placementIndicator != null) placementIndicator.SetActive(false);
        if (interactionUIPanel != null) interactionUIPanel.SetActive(true);

        // IMPORTANT: Keep plane manager enabled for stability
        arPlaneManager.enabled = true;

        hasPlacedObject = true;
        currentState = ARState.ObjectPlaced;

        Debug.Log("=== PlaceObject Complete ===");
    }

    private void HandleInteraction()
    {
        if (spawnedObject == null) return;

        // Single finger rotation
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved && !IsPointerOverUI(touch.position))
            {
                float rotationAmount = -touch.deltaPosition.x * rotationSpeed;
                spawnedObject.transform.Rotate(0f, rotationAmount, 0f, Space.World);
            }
        }

        // Two finger zoom
        if (Input.touchCount == 2)
        {
            HandlePinchToZoom();
        }
    }

    private void HandlePinchToZoom()
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        Vector2 t0PrevPos = t0.position - t0.deltaPosition;
        Vector2 t1PrevPos = t1.position - t1.deltaPosition;

        float prevMag = (t0PrevPos - t1PrevPos).magnitude;
        float currentMag = (t0.position - t1.position).magnitude;
        float diff = currentMag - prevMag;

        float currentScale = spawnedObject.transform.localScale.x;
        float newScale = Mathf.Clamp(currentScale + diff * zoomSpeed, 0.05f, 5.0f);

        spawnedObject.transform.localScale = Vector3.one * newScale;
        Debug.Log($"Zoom scale: {newScale}");
    }

    // Reset functionality
    public void ResetExperience()
    {
        Debug.Log("Resetting AR experience...");

        if (spawnedObject != null)
        {
            Transform anchorTransform = spawnedObject.transform.parent;
            Destroy(spawnedObject);
            if (anchorTransform != null) Destroy(anchorTransform.gameObject);
            spawnedObject = null;
        }

        // Clear anchor list
        for (int i = allAnchors.Count - 1; i >= 0; i--)
        {
            if (allAnchors[i] != null) Destroy(allAnchors[i].gameObject);
        }
        allAnchors.Clear();

        // Reset state
        arPlaneManager.enabled = true;
        hasPlacedObject = false;
        currentState = ARState.Scanning;

        // Reset UI
        if (interactionUIPanel != null) interactionUIPanel.SetActive(false);
        if (instructionPanel != null) instructionPanel.SetActive(true);

        Debug.Log("AR experience reset complete");
    }

    public void GoToMapScene()
    {
        ResetExperience();
        FirebaseDataManager.CleanUpLoadedAssets();
        SceneManager.LoadScene("MapScene");
    }

    public void PlayPauseNarration()
    {
        if (SelectionManager.Instance?.selectedMonument == null)
        {
            Debug.LogError("No monument data available for narration!");
            return;
        }

        if (isNarrationPlaying)
        {
            NativeTTS.Stop();
            isNarrationPlaying = false;
            Debug.Log("Narration stopped");
        }
        else
        {
            string scriptToSpeak = (SelectionManager.Instance.currentLanguage == "en-US")
                ? SelectionManager.Instance.selectedMonument.narration_en
                : SelectionManager.Instance.selectedMonument.narration_hi;

            if (!string.IsNullOrEmpty(scriptToSpeak))
            {
                NativeTTS.Speak(scriptToSpeak);
                isNarrationPlaying = true;
                Debug.Log($"Playing narration in {SelectionManager.Instance.currentLanguage}");
            }
            else
            {
                Debug.LogWarning("No narration text available");
            }
        }
    }

    // FIXED: Use Camera.main instead of separate arCamera reference
    private void UpdatePlacementPose()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Camera.main is null! Make sure AR Camera is tagged as MainCamera");
            isPlacementPoseValid = false;
            return;
        }

        var screenCenter = mainCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));

        // Clear previous hits
        s_Hits.Clear();

        bool hitFound = arRaycastManager.Raycast(screenCenter, s_Hits, TrackableType.PlaneWithinPolygon);
        isPlacementPoseValid = hitFound && s_Hits.Count > 0;

        if (isPlacementPoseValid)
        {
            Debug.Log($"Valid placement pose at: {s_Hits[0].pose.position}");
        }
    }

    private void UpdatePlacementIndicator()
    {
        if (placementIndicator == null)
        {
            Debug.LogWarning("Placement indicator is null! Assign it in inspector.");
            return;
        }

        bool shouldShow = isPlacementPoseValid && !hasPlacedObject;
        placementIndicator.SetActive(shouldShow);

        if (shouldShow && s_Hits.Count > 0)
        {
            placementIndicator.transform.position = s_Hits[0].pose.position;
            placementIndicator.transform.rotation = Quaternion.FromToRotation(Vector3.up, s_Hits[0].pose.up);

            // Scale indicator based on distance for better visibility
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                float distance = Vector3.Distance(mainCamera.transform.position, s_Hits[0].pose.position);
                float scale = Mathf.Clamp(distance * 0.1f, 0.1f, 0.5f);
                placementIndicator.transform.localScale = Vector3.one * scale;
            }
        }
    }

    // Camera permission handling
    private IEnumerator RequestCameraPermission()
    {
        Debug.Log("Requesting camera permission...");

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            
            while (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                yield return null;
            }
        }
#endif

        Debug.Log("Camera permission granted, waiting for AR session...");

        // Wait for AR session to start tracking
        while (ARSession.state < ARSessionState.SessionTracking)
        {
            Debug.Log($"Waiting for AR session... Current state: {ARSession.state}");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("AR Session is now tracking, switching to Scanning state");
        currentState = ARState.Scanning;
    }

    // Enhanced spawn animation
    IEnumerator AnimateSpawn()
    {
        if (spawnedObject == null)
        {
            Debug.LogError("Cannot animate - spawned object is null!");
            yield break;
        }

        Debug.Log("Starting spawn animation...");

        float duration = 1.5f;
        float elapsedTime = 0f;
        Vector3 targetScale = Vector3.one * initialScale;

        spawnedObject.transform.localScale = Vector3.zero;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            progress = 1f - Mathf.Pow(1f - progress, 3f); // Ease-out curve

            spawnedObject.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spawnedObject.transform.localScale = targetScale;
        Debug.Log($"Spawn animation complete. Final scale: {spawnedObject.transform.localScale}");
    }
}