using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InteriorSceneController : MonoBehaviour
{
    [Header("Scene References")]
    public Renderer panoramaSphere;
    public GameObject hotspotPrefab;
    public Transform hotspotsParent;

    [Header("UI References")]
    public GameObject loadingIndicator;
    public GameObject infoPanel;
    public TextMeshProUGUI infoTitle;
    public TextMeshProUGUI infoDescription;
    public Button closeInfoPanelButton;
    public Button backButton;

    [Header("Panorama Switcher UI")]
    public Button moreOptionsButton;
    public GameObject panoramaSelectionPanel;
    public Transform panoramaSwitchParent;
    public GameObject thumbnailButtonPrefab;

    private MonumentInteriorsData currentMonumentData;
    private Camera mainCamera;
    private List<GameObject> thumbnailButtons = new List<GameObject>();
    private Coroutine panelTimerCoroutine;

    async void Start()
    {
        mainCamera = Camera.main;
        infoPanel.SetActive(false);
        panoramaSelectionPanel.SetActive(false);
        loadingIndicator.SetActive(true);

        string monumentId = SelectionManager.Instance.selectedMonument.documentId;
        await LoadDataFromFirebase(monumentId);

        if (currentMonumentData != null && currentMonumentData.panoramas.Count > 0)
        {
            SetupPanoramaSwitchUI();
            StartCoroutine(LoadPanorama(0));
        }

        closeInfoPanelButton.onClick.AddListener(() => infoPanel.SetActive(false));
        backButton.onClick.AddListener(() => SceneManager.LoadScene("new ARscene"));
        moreOptionsButton.onClick.AddListener(TogglePanoramaPanel);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Hotspot"))
            {
                HotspotData data = hit.collider.GetComponent<HotspotDataHolder>().data;
                ShowInfoPanel(data);
            }
        }
    }

    public void TogglePanoramaPanel()
    {
        bool isPanelActive = !panoramaSelectionPanel.activeSelf;
        panoramaSelectionPanel.SetActive(isPanelActive);

        if (panelTimerCoroutine != null) StopCoroutine(panelTimerCoroutine);
        if (isPanelActive)
        {
            panelTimerCoroutine = StartCoroutine(HidePanelAfterDelay(5.0f));
        }
    }

    private IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        panoramaSelectionPanel.SetActive(false);
    }

    async Task LoadDataFromFirebase(string monumentId)
    {
        if (string.IsNullOrEmpty(monumentId)) return;
        DocumentReference docRef = FirebaseFirestore.DefaultInstance.Collection("monument_interiors").Document(monumentId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            currentMonumentData = snapshot.ConvertTo<MonumentInteriorsData>();
        }
    }

    IEnumerator LoadPanorama(int index)
    {
        if (currentMonumentData == null || index >= currentMonumentData.panoramas.Count) yield break;

        loadingIndicator.SetActive(true);
        infoPanel.SetActive(false);
        foreach (Transform child in hotspotsParent) { Destroy(child.gameObject); }

        HighlightActiveButton(index);
        PanoramaData data = currentMonumentData.panoramas[index];

        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(data.panoramaImageUrl))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                panoramaSphere.material.mainTexture = DownloadHandlerTexture.GetContent(webRequest);
            }
            else { Debug.LogError("Failed to download image: " + webRequest.error); }
        }

        foreach (var hotspot in data.hotspots)
        {
            // Get position from Firebase
            Vector3 hotspotPosition = new Vector3(hotspot.position.x, hotspot.position.y, hotspot.position.z);

            // Pull hotspot INWARD from sphere surface towards center
            // This prevents clipping with the sphere mesh
            Vector3 directionFromCenter = hotspotPosition.normalized;
            float currentDistance = hotspotPosition.magnitude;

            // Move it 3-5 units inward from the sphere surface
            Vector3 finalPosition = directionFromCenter * (currentDistance - 3f);

            GameObject hotspotInstance = Instantiate(hotspotPrefab, finalPosition, Quaternion.identity, hotspotsParent);
            hotspotInstance.GetComponent<HotspotDataHolder>().data = hotspot;
        }

        loadingIndicator.SetActive(false);
    }

    void ShowInfoPanel(HotspotData data)
    {
        infoTitle.text = data.title;
        infoDescription.text = data.description;
        infoPanel.SetActive(true);
    }

    void SetupPanoramaSwitchUI()
    {
        // If only 1 or no panoramas, hide the button and don't create thumbnails
        if (currentMonumentData.panoramas.Count <= 1)
        {
            moreOptionsButton.gameObject.SetActive(false);  // ← Changed back to false
            panoramaSelectionPanel.SetActive(false);
            return;
        }

        // If we have multiple panoramas, ensure button is visible
        moreOptionsButton.gameObject.SetActive(true);  // ← Add this line
        panoramaSelectionPanel.SetActive(false);  // Start with panel hidden

        foreach (Transform child in panoramaSwitchParent) { Destroy(child.gameObject); }

        for (int i = 0; i < currentMonumentData.panoramas.Count; i++)
        {
            int index = i;
            GameObject buttonGO = Instantiate(thumbnailButtonPrefab, panoramaSwitchParent);
            Button button = buttonGO.GetComponent<Button>();
            button.onClick.AddListener(() => {
                StartCoroutine(LoadPanorama(index));
                if (panelTimerCoroutine != null) StopCoroutine(panelTimerCoroutine);
                panoramaSelectionPanel.SetActive(false);
            });

            buttonGO.GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();

            thumbnailButtons.Add(buttonGO);
            StartCoroutine(LoadThumbnailImage(buttonGO, currentMonumentData.panoramas[i].thumbnailUrl));
        }
    }

    IEnumerator LoadThumbnailImage(GameObject buttonGO, string url)
    {
        Image thumbnailImage = buttonGO.transform.Find("ThumbnailImage")?.GetComponent<Image>();
        if (thumbnailImage != null && !string.IsNullOrEmpty(url))
        {
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Texture2D tex = DownloadHandlerTexture.GetContent(webRequest);
                    thumbnailImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
        }
    }

    void HighlightActiveButton(int activeIndex)
    {
        for (int i = 0; i < thumbnailButtons.Count; i++)
        {
            Transform border = thumbnailButtons[i].transform.Find("HighlightBorder");
            if (border != null)
            {
                border.gameObject.SetActive(i == activeIndex);
            }
        }
    }
}