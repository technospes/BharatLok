using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
public class MonumentInteriorsData
{
    [FirestoreProperty]
    public List<PanoramaData> panoramas { get; set; }
}