using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// This is the corrected, definitive script.
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class BharatLokARController : MonoBehaviour
{
    private string currentLanguage = "en-US"; // Default to English
    private bool isNarrationPlaying = false; // We still need this to track state
    [Header("1. Object Prefabs")]
    [Tooltip("The monument prefab to be placed. Use the 'Pivot' parent object.")]
    //public GameObject objectToPlace;
    private GameObject rotationPivot;
    private float smoothingSpeed = 10f; // increase = less lag, decrease = more smoothing
    [Tooltip("The visual guide that shows where you are aiming.")]
    public GameObject placementIndicator;
    // ... inside the class, with the other private variables
    [Header("Placement Indicator Prefab (optional)")]
    public GameObject placementIndicatorPrefab; // assign prefab asset if you don't assign a scene instance
    private GameObject placementIndicatorInstance;
    [Header("2. Placement & Scale Settings")]
    [Tooltip("The starting size of the monument. Adjust this in the Inspector.")]
    [Range(0.01f, 1.0f)]
    public float initialScale = 0.05f;
    [Header("4. Interaction Settings")]
    [Tooltip("How fast the object rotates when dragged.")]
    [Range(0.1f, 2.0f)]
    public float rotationSpeed = 0.5f;
    public AnimationCurve spawnAnimationCurve;
    [Tooltip("How fast the object scales when pinched.")]
    [Range(0.00001f, 0.01f)]
    public float zoomSpeed = 0.0002f;
    [Header("3. UI")]
    [Tooltip("The UI panel with instructions that shows at the start.")]
    public GameObject instructionPanel; // For "Move phone..." text
    [Tooltip("The UI panel with buttons (Reset, Audio) that appears after spawning.")]
    public GameObject interactionUIPanel; // For the "Reset" button
    private ARAnchor placedAnchor;
    private Vector3 smoothedPosition;
    private Quaternion smoothedRotation;
    private float userYaw = 0f;    // --- Private Internal Variables ---
    private GameObject spawnedObject;
    private ARRaycastManager arRaycastManager;
    private ARAnchorManager arAnchorManager;
    private ARPlaneManager arPlaneManager;
    private List<ARAnchor> allAnchors = new List<ARAnchor>();
    private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    private bool isPlacementPoseValid = false;
    [Header("4. Debugging")]
    [Tooltip("A UI Text element to show debug info on screen.")]
    public TextMeshProUGUI onScreenDebugText;
    // --- CHANGED LINE: Store the whole ARRaycastHit, not just the Pose ---
    private ARRaycastHit currentPlacementHit;

    void Awake()
    {
        Application.targetFrameRate = 60;
        // This is the correct way now that everything is on the same object.
        arRaycastManager = GetComponent<ARRaycastManager>();
        arAnchorManager = GetComponent<ARAnchorManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();
        NativeTTS.Initialize();
        // The rest of your script stays exactly the same.
        // If a scene instance wasn't assigned in Inspector but a prefab is, instantiate one
        if (placementIndicator == null && placementIndicatorPrefab != null)
        {
            placementIndicatorInstance = Instantiate(placementIndicatorPrefab);
            placementIndicator = placementIndicatorInstance;
        }

        // If it's still null, warn
        if (placementIndicator == null)
        {
            Debug.LogWarning("Placement indicator not assigned or instantiated. Assign scene instance or prefab.");
        }
        else
        {
            placementIndicator.SetActive(false);
        }
        if (interactionUIPanel != null) interactionUIPanel.SetActive(false);
    }

    void Update()
    {
        if (instructionPanel != null && instructionPanel.activeSelf && ARSession.state == ARSessionState.SessionTracking)
        {
            // ...then hide the instruction panel.
            instructionPanel.SetActive(false);
        }
        if (spawnedObject == null)
        {
            UpdatePlacementPose();
            UpdatePlacementIndicator();

            // Place only on SINGLE touch began (prevents 2-finger accidental placement)
            if (isPlacementPoseValid && Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    PlaceObject();
                }
            }
        }
        else
        {
            HandleInteraction();
        }
    }
    private void HandleInteraction()
    {
        if (spawnedObject == null) return; // don’t interact until something is placed

        // --- ONE FINGER INPUT ---
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // ✅ Tap-to-place is handled on Began
            //if (touch.phase == TouchPhase.Began)
            //{
            //TryPlaceObject(touch.position);
            //}
            // ✅ Drag rotation
            if (touch.phase == TouchPhase.Moved)
            {
            float horizontalDrag = touch.deltaPosition.x;
            float degreesPerPixelFactor = 0.1f;
            userYaw += -horizontalDrag * rotationSpeed * degreesPerPixelFactor;
            //spawnedObject.transform.Rotate(0f, -horizontalDrag * rotationSpeed, 0f, Space.World);
            }
        }
        // --- TWO FINGER PINCH-TO-ZOOM ---
        if (Input.touchCount == 2)
        {
            HandlePinchToZoom();
        }
    }
    //private void TryPlaceObject(Vector2 screenPosition)
    //{
    //    if (placedAnchor != null) return; // only place once

    //    List<ARRaycastHit> hits = new List<ARRaycastHit>();
    //    if (arRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
    //    {
    //        Pose hitPose = hits[0].pose;

    //        placedAnchor = arAnchorManager.AttachAnchor(hits[0].trackable as ARPlane, hitPose);

    //        if (placedAnchor != null)
    //        {
    //            // 1. Instantiate the prefab (which contains the Pivot and the Model)
    //            spawnedObject = Instantiate(objectToPlace, placedAnchor.transform.position, placedAnchor.transform.rotation);
    //            // 2. Parent the entire thing to the anchor
    //            spawnedObject.transform.SetParent(placedAnchor.transform);

    //            // +++ MOST IMPORTANT FIX: FIND THE PIVOT +++
    //            // This assumes your pivot object is named "RotationPivot"
    //            rotationPivot = spawnedObject.transform.Find("RotationPivot").gameObject;

    //            // Check if it was found to avoid errors
    //            if (rotationPivot == null)
    //            {
    //                Debug.LogError("Could not find 'RotationPivot' GameObject in the spawned prefab! Please check the prefab's structure.");
    //            }

    //            // 3. (Optional but good practice) Get a reference to the actual model
    //            // This is useful if your AnimateSpawn coroutine needs to scale the model, not the entire pivot.
    //            // This assumes the model is the first child of the RotationPivot.
    //            GameObject modelObject = null;
    //            if (rotationPivot != null && rotationPivot.transform.childCount > 0)
    //            {
    //                modelObject = rotationPivot.transform.GetChild(0).gameObject;
    //            }
    //            // Now you can pass 'modelObject' to your AnimateSpawn coroutine if needed.

    //            //smoothedPosition = spawnedObject.transform.position;
    //            Debug.Log("✅ Monument Spawned at " + hitPose.position);

    //            // Start your spawn animation
    //            StartCoroutine(AnimateSpawn());
    //        }
    //    }
    //}
    public void PlayPauseNarration()
    {
        if (isNarrationPlaying)
        {
            NativeTTS.Stop();
            isNarrationPlaying = false;
        }
        else
        {
            string scriptToSpeak = (currentLanguage == "en-US")
                ? SelectionManager.selectedMonument.narrationScript_English
                : SelectionManager.selectedMonument.narrationScript_Hindi;

            NativeTTS.Speak(scriptToSpeak);
            isNarrationPlaying = true;
        }
    }
    public void SetLanguage(string languageCode) // e.g., "en-US", "hi-IN"
    {
        currentLanguage = languageCode;
        NativeTTS.SetLanguage(languageCode);

        // If something was playing, stop it so the new language can be used next time.
        if (isNarrationPlaying)
        {
            NativeTTS.Stop();
            isNarrationPlaying = false;
        }
    }
    public void LoadInteriorScene()
    {
        SceneManager.LoadScene("InteriorViewScene");
    }

    public void LoadMapScene()
    {
        SceneManager.LoadScene("MapScene");
    }
    public void GoToMapScene()
{
    // First, it's good practice to reset the current AR session to clean everything up.
    // Our existing ResetExperience() function is perfect for this.
    ResetExperience();

    // Then, load the Map Scene.
    // Make sure your map scene is named "MapScene" in your Build Settings.
    SceneManager.LoadScene("MapScene");
}
    public void ResetExperience()
    {
        // Destroy the spawned object if it exists.
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }

        if (placedAnchor != null)
        {
            Destroy(placedAnchor.gameObject);
            placedAnchor = null;
        }

        // Destroy all the anchors we created.
        foreach (var anchor in allAnchors)
        {
            if (anchor != null) Destroy(anchor.gameObject);
        }
        allAnchors.Clear();

        // Re-enable the plane manager
        arPlaneManager.enabled = true;

        // Hide the interaction UI
        if (interactionUIPanel != null)
        {
            interactionUIPanel.SetActive(false);
        }
        // Show the instruction panel again
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(true);
        }
        // Clear the debug text
        if (onScreenDebugText != null)
        {
            onScreenDebugText.text = "";
        }
        Debug.Log("AR Experience has been reset. Ready to place a new object.");
    }
    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        arRaycastManager.Raycast(screenCenter, s_Hits, TrackableType.PlaneWithinPolygon);
        isPlacementPoseValid = s_Hits.Count > 0;
        if (isPlacementPoseValid)
        {
            // --- CHANGED LINE: Store the entire hit result ---
            currentPlacementHit = s_Hits[0];
        }
    }
    private void UpdatePlacementIndicator()
    {
        if (placementIndicator == null) return;

        placementIndicator.SetActive(isPlacementPoseValid);

        if (isPlacementPoseValid)
        {
            // Set the position and ensure it's flat on the surface
            placementIndicator.transform.position = currentPlacementHit.pose.position;
            placementIndicator.transform.rotation = Quaternion.FromToRotation(Vector3.up, currentPlacementHit.pose.up);

            // --- NEW DYNAMIC SCALING LOGIC ---
            // Calculate the distance from the camera to the indicator
            float distance = Vector3.Distance(Camera.main.transform.position, placementIndicator.transform.position);

            // Scale the indicator based on its distance from the camera.
            // You can tweak the '0.1f' to make the screen-space size bigger or smaller.
            // We also clamp the scale so it doesn't get ridiculously big or small.
            float targetScale = Mathf.Clamp(distance * 0.1f, 0.1f, 0.5f); // Clamp between 10cm and 50cm in world size

            // Smoothly change the scale over time to avoid jittery resizing
            placementIndicator.transform.localScale = Vector3.Lerp(placementIndicator.transform.localScale, Vector3.one * targetScale, Time.deltaTime * 10f);
        }
    }
    private void PlaceObject()
    {
        if (ARSession.state != ARSessionState.SessionTracking)
        {
            Debug.Log("Cannot place yet — session not tracking.");
            return;
        }

        Pose pose = currentPlacementHit.pose;

        GameObject anchorGameObject = new GameObject("PlacementAnchor");
        anchorGameObject.transform.SetPositionAndRotation(pose.position, pose.rotation);

        ARAnchor anchor = anchorGameObject.AddComponent<ARAnchor>();
        if (anchor == null)
        {
            Debug.LogError("Failed to create anchor.");
            Destroy(anchorGameObject);
            return;
        }
        placedAnchor = anchor;
        allAnchors.Add(anchor);

        // ✅ FIX 1: Use identity rotation - let the parenting handle positioning
        spawnedObject = Instantiate(SelectionManager.selectedMonument.monumentARPrefab, pose.position, Quaternion.identity);
        spawnedObject.transform.SetParent(anchorGameObject.transform);

        rotationPivot = spawnedObject.transform.Find("RotationPivot").gameObject;
        if (rotationPivot == null)
        {
            Debug.LogError("PlaceObject: Could not find 'RotationPivot' GameObject!");
        }

        // ✅ FIX 2: ONLY initialize userYaw to 0, DON'T apply rotation here
        userYaw = 0f; // Start with neutral rotation

        StartCoroutine(AnimateSpawn());

        // Rest of your existing code...
        FadeOut fadeScript = placementIndicator.GetComponent<FadeOut>();
        if (fadeScript != null)
        {
            fadeScript.StartFade();
        }
        else
        {
            placementIndicator.SetActive(false);
        }

        arPlaneManager.enabled = false;
        if (interactionUIPanel != null) interactionUIPanel.SetActive(true);
        StartCoroutine(LogAnchorPose());
    }
    void LateUpdate()
    {
        // If we have a pivot, rotate IT instead of the model directly.
        if (rotationPivot != null)
        {
            rotationPivot.transform.localRotation = Quaternion.Euler(0f, userYaw, 0f);
        }
    }
    private void HandlePinchToZoom()
    {
        if (spawnedObject == null || Input.touchCount < 2) return;
        if (rotationPivot == null || rotationPivot.transform.childCount == 0) return;

        // Get reference to the model
        Transform modelTransform = rotationPivot.transform.GetChild(0);

        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
        float difference = currentMagnitude - prevMagnitude;

        float scaleChange = difference * zoomSpeed;
        float newScale = modelTransform.localScale.x + scaleChange; // Scale the MODEL

        float minScale = 0.01f;
        float maxScale = 2.0f;
        newScale = Mathf.Clamp(newScale, minScale, maxScale);

        modelTransform.localScale = Vector3.one * newScale; // Apply to MODEL
    }
    private IEnumerator AnimateSpawn()
    {
        float duration = 1.0f;
        float elapsedTime = 0f;

        // 1. TARGET THE MODEL, NOT THE SPAWNED PREFAB
        // Assume the model is the first child of the rotationPivot
        if (rotationPivot == null || rotationPivot.transform.childCount == 0)
        {
            Debug.LogError("Cannot animate: No model found as a child of the rotationPivot.");
            yield break; // Exit the coroutine early
        }
        Transform modelTransform = rotationPivot.transform.GetChild(0);

        // 2. Set the initial and target scale for the MODEL
        Vector3 targetScale = Vector3.one * initialScale;
        modelTransform.localScale = Vector3.zero; // Start the model at zero scale

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float scaleFactor = spawnAnimationCurve.Evaluate(t);

            // 3. Animate the MODEL's scale
            modelTransform.localScale = targetScale * scaleFactor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 4. Final snap to the exact target scale for the MODEL
        modelTransform.localScale = targetScale;
    }
    IEnumerator LogAnchorPose()
    {
        while (placedAnchor != null)
        {
            Transform anchorTransform = placedAnchor.transform;

            string poseInfo = $"Anchor Pos: {anchorTransform.position.ToString("F3")}\n" +
                              $"Anchor Rot: {anchorTransform.rotation.eulerAngles.ToString("F3")}";

            if (onScreenDebugText != null)
            {
                onScreenDebugText.text = poseInfo;
            }

            Debug.Log(poseInfo);
            yield return new WaitForSeconds(0.5f);
        }
    }
}