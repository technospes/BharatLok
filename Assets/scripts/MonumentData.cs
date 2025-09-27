using UnityEngine;
using Firebase.Firestore;

[FirestoreData]
public class MonumentData
{
    // CRITICAL: Firebase requires parameterless constructor
    public MonumentData() { }

    [FirestoreProperty("name")]
    public string name { get; set; } = "";

    [FirestoreProperty("Location")]
    public string Location { get; set; } = "";

    [FirestoreProperty("narration_en")]
    public string narration_en { get; set; } = "";

    [FirestoreProperty("narration_hi")]
    public string narration_hi { get; set; } = "";

    [FirestoreProperty("model_url_low")]
    public string model_url_low { get; set; } = "";

    [FirestoreProperty("model_url_high")]
    public string model_url_high { get; set; } = "";

    [FirestoreProperty("panorama_url")]
    public string panorama_url { get; set; } = "";

    // Runtime-only fields
    [System.NonSerialized]
    public GameObject loadedPrefab;

    [System.NonSerialized]
    public Material loadedPanorama;
}