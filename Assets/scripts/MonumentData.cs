using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Monument", menuName = "BharatLok/Monument Data")]
public class MonumentData : ScriptableObject
{
    [Header("Core Info")]
    public string monumentName;
    public GameObject monumentARPrefab; // The 3D model for AR

    [Header("Narration Text (AR Scene)")]
    [TextArea(5, 10)] public string narrationScript_English;
    [TextArea(5, 10)] public string narrationScript_Hindi;

    [Header("Interior View")]
    public Material panoramaMaterial; // The 360 degree skybox material
    public List<HotspotInfo> hotspots;
}

[System.Serializable]
public class HotspotInfo
{
    [TextArea(3, 5)] public string infoText_English;
    [TextArea(3, 5)] public string infoText_Hindi;
    public Vector3 hotspotPosition; // 3D position for the button
}