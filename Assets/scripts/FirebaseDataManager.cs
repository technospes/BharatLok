using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine.Networking;
using GLTFast;
using System;
using System.IO;

public static class FirebaseDataManager
{
    private static FirebaseFirestore db;
    public static void Initialize() { db = FirebaseFirestore.DefaultInstance; }

    private static string GetCachePath(string monumentId, bool isHighRes)
    {
        string quality = isHighRes ? "high" : "low";
        return Path.Combine(Application.persistentDataPath, $"{monumentId}_{quality}.glb");
    }

    public static async Task<MonumentData> FetchAndLoadMonumentData(string monumentId, IProgress<float> progress = null)
    {
        DocumentReference docRef = db.Collection("monuments").Document(monumentId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) { Debug.LogError("Document does not exist!"); return null; }

        MonumentData data = snapshot.ConvertTo<MonumentData>();
        progress?.Report(0.1f);

        bool useHighRes = (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork);
        string modelUrl = useHighRes ? data.model_url_high : data.model_url_low;
        string cachePath = GetCachePath(monumentId, useHighRes);

        if (File.Exists(cachePath))
        {
            byte[] fileData = await File.ReadAllBytesAsync(cachePath);
            data.loadedPrefab = await LoadGltfFromBytes(fileData);
        }
        else
        {
            byte[] downloadedData = await DownloadFile(modelUrl, progress, 0.1f, 0.9f);
            if (downloadedData != null)
            {
                await File.WriteAllBytesAsync(cachePath, downloadedData);
                data.loadedPrefab = await LoadGltfFromBytes(downloadedData);
            }
        }

        progress?.Report(1.0f);
        return data;
    }

    private static async Task<byte[]> DownloadFile(string url, IProgress<float> progress, float startProgress, float endProgress)
    {
        using (var webRequest = UnityWebRequest.Get(url))
        {
            var op = webRequest.SendWebRequest();
            while (!op.isDone)
            {
                progress?.Report(Mathf.Lerp(startProgress, endProgress, op.progress));
                await Task.Yield();
            }
            return webRequest.result == UnityWebRequest.Result.Success ? webRequest.downloadHandler.data : null;
        }
    }

    private static async Task<GameObject> LoadGltfFromBytes(byte[] glbData)
    {
        var gltf = new GltfImport();
        bool success = await gltf.Load(glbData);
        if (success)
        {
            GameObject instance = new GameObject("LoadedMonument");
            await gltf.InstantiateMainSceneAsync(instance.transform);
            instance.SetActive(false);
            // THIS IS THE CRITICAL FIX TO PREVENT THE MODEL FROM BEING DESTROYED
            UnityEngine.Object.DontDestroyOnLoad(instance);
            return instance;
        }
        return null;
    }

    // This function must be called when leaving the AR Scene to clean up.
    public static void CleanUpLoadedAssets()
    {
        if (SelectionManager.Instance?.selectedMonument?.loadedPrefab != null)
        {
            UnityEngine.Object.Destroy(SelectionManager.Instance.selectedMonument.loadedPrefab);
            SelectionManager.Instance.selectedMonument.loadedPrefab = null;
        }
    }
}