using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingController : MonoBehaviour
{
    [System.Serializable]
    public struct TrackedImagePrefab
    {
        public string name; // The name from your Reference Image Library
        public GameObject prefab; // The video player prefab to spawn
    }

    public List<TrackedImagePrefab> prefabsToTrack;

    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> instantiatedPrefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // When an image is detected
        foreach (var trackedImage in eventArgs.added)
        {
            SpawnPrefabForImage(trackedImage);
        }

        // When a detected image is updated (e.g., its position)
        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                // If it's not already active, reactivate it
                if (!instantiatedPrefabs[trackedImage.referenceImage.name].activeSelf)
                {
                    instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(true);
                }
                // Update its position
                instantiatedPrefabs[trackedImage.referenceImage.name].transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);
            }
            else
            {
                // If tracking is lost, hide the prefab
                instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(false);
            }
        }
    }

    private void SpawnPrefabForImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        foreach (var item in prefabsToTrack)
        {
            if (item.name == imageName && !instantiatedPrefabs.ContainsKey(imageName))
            {
                // Find the correct prefab and instantiate it
                GameObject instance = Instantiate(item.prefab, trackedImage.transform.position, trackedImage.transform.rotation);
                instance.name = imageName + "_Instance";
                instantiatedPrefabs.Add(imageName, instance);
                Debug.Log($"Spawned prefab for image: {imageName}");
            }
        }
    }
}