using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
public class PanoramaData
{
    [FirestoreProperty]
    public string panoramaImageUrl { get; set; }
    [FirestoreProperty]
    public string thumbnailUrl { get; set; }
    [FirestoreProperty]
    public List<HotspotData> hotspots { get; set; }
}