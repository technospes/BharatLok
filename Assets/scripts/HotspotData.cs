using Firebase.Firestore;

[FirestoreData]
public class HotspotData
{
    [FirestoreProperty]
    public string title { get; set; }
    [FirestoreProperty]
    public string description { get; set; }

    // This now correctly matches the 'position' map in your database
    [FirestoreProperty]
    public PositionData position { get; set; }
}