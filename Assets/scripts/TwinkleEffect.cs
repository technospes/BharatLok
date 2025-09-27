using UnityEngine;
using UnityEngine.UI;

public class TwinkleEffect : MonoBehaviour
{
    public float speed = 2f;      // twinkle speed
    public float minAlpha = 0.5f; // how dim stars get
    public float maxAlpha = 1f;   // max brightness

    private Image starImage;
    private float alpha;

    void Start()
    {
        starImage = GetComponent<Image>();
    }

    void Update()
    {
        // Ping-pong alpha between min and max
        alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * speed) + 1f) / 2f);

        Color c = starImage.color;
        c.a = alpha;
        starImage.color = c;
    }
}