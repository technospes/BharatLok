using UnityEngine;
using UnityEngine.SceneManagement;

public class MapController : MonoBehaviour
{
    // Drag your data assets into these slots in the Inspector
    public MonumentData tajMahalData;
    public MonumentData elloraCavesData;

    public void OnMonumentSelected(MonumentData monument)
    {
        // Store the chosen monument in our global manager
        SelectionManager.selectedMonument = monument;
        // Load the main AR Scene
        SceneManager.LoadScene("SampleScene"); // Make sure your AR Scene is named "SampleScene"
    }

    // These two functions are helpers to call from the buttons
    public void SelectTajMahal() { OnMonumentSelected(tajMahalData); }
    public void SelectElloraCaves() { OnMonumentSelected(elloraCavesData); }
}