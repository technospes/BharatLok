using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    [Header("Transition Settings")]
    public RectTransform transitionPanel;
    public float duration = 0.3f;
    public SwipeDirection swipeDirection = SwipeDirection.Left;
    public enum SwipeDirection { Left, Right, Up, Down }
    void Awake()
    {
        Application.targetFrameRate = 60;
        if (transitionPanel != null)
        {
            Vector2 startPos = GetOffscreenPosition();
            transitionPanel.anchoredPosition = startPos;
        }
    }
    public void GoToMapScene()
    {
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.selectedMonument = null;
            // Reset other selection states if needed
        }
        if (transitionPanel != null)
        {
            LeanTween.move(transitionPanel, Vector2.zero, duration).setOnComplete(() =>
            {
                Destroy(transitionPanel.gameObject);
                GameObject globe = GameObject.Find("earthPrefab");
                if (globe != null)
                {
                    Destroy(globe);
                }
                SceneManager.LoadScene("MapScene");
            });
        }
        else
        {
            SceneManager.LoadScene("MapScene");
        }
    }
    public void OpenSettings()
    {
        Debug.Log("Settings button clicked!");
    }
    private Vector2 GetOffscreenPosition()
    {
        float width = Screen.width;
        float height = Screen.height;
        switch (swipeDirection)
        {
            case SwipeDirection.Left: return new Vector2(-width, 0);
            case SwipeDirection.Right: return new Vector2(width, 0);
            case SwipeDirection.Up: return new Vector2(0, height);
            case SwipeDirection.Down: return new Vector2(0, -height);
            default: return Vector2.zero;
        }
    }
}