using UnityEngine;
using TMPro;

public class WaypointData : MonoBehaviour
{
    [Header("Monument Info")]
    public string monumentId; // The Document ID from Firebase
    public string monumentName;
    public string location;
    [TextArea(3, 5)]
    public string description;
    public Sprite monumentImage;
}