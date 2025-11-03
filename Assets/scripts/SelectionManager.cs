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
    // Debug method to check current state
    public void DebugCurrentState()
    {
        Debug.Log("=== SELECTION MANAGER STATE ===");
        Debug.Log($"Instance exists: {Instance != null}");

        // FIX: We now log the ID string directly, instead of trying to access ".name"
        Debug.Log($"Selected Monument ID: {(    selectedMonument != null ? selectedMonument : "NULL")}");

        Debug.Log($"Language: {currentLanguage}");

        // REMOVED: The line that tried to access ".loadedPrefab" has been removed,
        // as this script no longer tracks the loaded 3D model.

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