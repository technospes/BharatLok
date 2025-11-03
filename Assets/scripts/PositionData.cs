using Firebase.Firestore;

[FirestoreData]
public class PositionData
{
    [FirestoreProperty]
    public float x { get; set; }
    [FirestoreProperty]
    public float y { get; set; }
    [FirestoreProperty]
    public float z { get; set; }
}