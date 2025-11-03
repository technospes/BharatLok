using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Monument Interior", menuName = "BharatLok/Monument Interior Data")]
public class MonumentInteriors : ScriptableObject
{
    public string monumentId;
    public List<PanoramaData> panoramas;
}