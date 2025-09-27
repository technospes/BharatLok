using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;
    public MonumentData selectedMonument { get; set; }
    public string currentLanguage { get; set; } = "en-US";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SelectionManager created and protected with DontDestroyOnLoad");

            // Initialize Firebase
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                if (task.Exception != null)
                {
                    Debug.LogError($"Firebase initialization failed: {task.Exception}");
                }
                else
                {
                    Debug.Log("Firebase initialized successfully");
                }
            });
        }
        else
        {
            Debug.Log("Duplicate SelectionManager found, destroying...");
            Destroy(gameObject);
        }
    }

    // Debug method to check current state
    public void DebugCurrentState()
    {
        Debug.Log("=== SELECTION MANAGER STATE ===");
        Debug.Log($"Instance exists: {Instance != null}");
        Debug.Log($"Monument: {(selectedMonument != null ? selectedMonument.name : "NULL")}");
        Debug.Log($"Language: {currentLanguage}");
        Debug.Log($"Prefab: {(selectedMonument?.loadedPrefab != null ? selectedMonument.loadedPrefab.name : "NULL")}");
        Debug.Log("================================");
    }

    void OnDestroy()
    {
        Debug.Log("SelectionManager being destroyed!");
        if (Instance == this)
        {
            Instance = null;
        }
    }
}