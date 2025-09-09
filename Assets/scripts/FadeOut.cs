using UnityEngine;

public class FadeOut : MonoBehaviour
{
    private Material mat;
    private Color startColor;
    private bool fading = false;
    private float fadeDuration = 1.0f; // 1 second fade
    private float elapsed = 0f;
    private Vector3 initialScale;
    private Vector3 targetScale;

    void Awake()
    {
        // Use an instance of the material
        mat = GetComponent<Renderer>().material;
        startColor = mat.color;

        // Save initial scale
        initialScale = transform.localScale;
        targetScale = initialScale * 1.5f; // broad pop before vanishing
    }

    public void StartFade()
    {
        fading = true;
        elapsed = 0f;
    }

    void Update()
    {
        if (!fading) return;

        elapsed += Time.deltaTime;
        float t = elapsed / fadeDuration;

        // --- Fade color ---
        Color c = startColor;
        c.a = Mathf.Lerp(startColor.a, 0f, t);
        mat.color = c;

        // --- Scale pop effect ---
        float scaleT = Mathf.Sin(t * Mathf.PI); // grow then shrink
        transform.localScale = Vector3.Lerp(initialScale, targetScale, scaleT);

        // Disable when done
        if (t >= 1f)
        {
            gameObject.SetActive(false);
        }
    }
}