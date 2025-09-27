using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System;

public class MapController : MonoBehaviour
{
    [Header("Narration Settings")]
    public TMP_Dropdown languageDropdown;

    [System.Serializable]
    public struct MonumentInfo
    {
        public string displayName;
        public string documentId;
    }

    [Header("UI References")]
    public TMP_Dropdown monumentDropdown;
    public GameObject loadingPanel;
    public Button viewInARButton;
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public GameObject errorPanel;
    public TextMeshProUGUI errorText;

    [Header("Monument List")]
    public List<MonumentInfo> monuments;

    private string currentLanguage = "en-US";
    private string selectedMonumentId = null;
    private bool isDownloading = false;

    void Start()
    {
        InitializeUI();
        FirebaseDataManager.Initialize();

        monumentDropdown.onValueChanged.AddListener(OnMonumentSelected);
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void InitializeUI()
    {
        loadingPanel.SetActive(false);
        errorPanel.SetActive(false);
        viewInARButton.interactable = false;
        progressBar.gameObject.SetActive(false);

        // Setup Monument Dropdown
        monumentDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var monument in monuments)
        {
            options.Add(monument.displayName);
        }
        monumentDropdown.AddOptions(options);
    }

    private void OnMonumentSelected(int index)
    {
        selectedMonumentId = monuments[index].documentId;
        CheckIfSelectionsComplete();
    }

    private void OnLanguageChanged(int index)
    {
        currentLanguage = (index == 0) ? "en-US" : "hi-IN";

        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.currentLanguage = currentLanguage;
        }

        CheckIfSelectionsComplete();
    }

    private void CheckIfSelectionsComplete()
    {
        bool isReady = (!string.IsNullOrEmpty(selectedMonumentId) && !string.IsNullOrEmpty(currentLanguage));
        viewInARButton.interactable = isReady && !isDownloading;
    }

    public async void OnViewInARButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedMonumentId) || isDownloading) return;

        StartDownload();

        try
        {
            var progress = new Progress<float>(UpdateProgressBar);
            MonumentData monumentData = await FirebaseDataManager.FetchAndLoadMonumentData(selectedMonumentId, progress);

            if (monumentData != null && monumentData.loadedPrefab != null)
            {
                // Ensure a SelectionManager instance exists (create if missing)
                if (SelectionManager.Instance == null)
                {
                    GameObject sm = new GameObject("SelectionManager");
                    sm.AddComponent<SelectionManager>();
                    // give Unity one frame to run Awake() (not strictly necessary but safe)
                    await System.Threading.Tasks.Task.Yield();
                }

                // Now safely assign the downloaded data
                SelectionManager.Instance.selectedMonument = monumentData;
                SelectionManager.Instance.currentLanguage = currentLanguage;

                SelectionManager.Instance.DebugCurrentState(); // debug helper while testing

                Debug.Log("Data loaded and set in SelectionManager. Loading AR scene...");
                SceneManager.LoadScene("new ARscene");
            }
            else
            {
                ShowError("Failed to load monument. Please check your connection and try again.");
            }
        }
        catch (Exception e)
        {
            ShowError($"Error: {e.Message}");
        }
        finally
        {
            EndDownload();
        }
    }


    private void StartDownload()
    {
        isDownloading = true;
        viewInARButton.interactable = false;
        loadingPanel.SetActive(true);
        progressBar.gameObject.SetActive(true);
        progressBar.value = 0f;
        progressText.text = "0%";
        errorPanel.SetActive(false);
    }

    private void UpdateProgressBar(float progress)
    {
        progressBar.value = progress;
        progressText.text = $"{(progress * 100):0}%";
    }

    private void EndDownload()
    {
        loadingPanel.SetActive(false);
        progressBar.gameObject.SetActive(false);
        isDownloading = false;
        CheckIfSelectionsComplete();
    }

    private void ShowError(string message)
    {
        errorPanel.SetActive(true);
        errorText.text = message;
        Debug.LogError(message);
    }

    public void OnBackToIntroButtonClicked()
    {
        if (isDownloading)
        {
            //FirebaseDataManager.CancelCurrentDownload();
        }
        SceneManager.LoadScene("IntroScene");
    }

    public void OnErrorRetryButtonClicked()
    {
        errorPanel.SetActive(false);
        if (!string.IsNullOrEmpty(selectedMonumentId))
        {
            OnViewInARButtonClicked();
        }
    }

    public void OnErrorCancelButtonClicked()
    {
        errorPanel.SetActive(false);
        EndDownload();
    }
}