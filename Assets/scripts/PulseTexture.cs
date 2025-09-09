using UnityEngine;

public class PulseTexture : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseSpeed = 2f;    // How fast it pulses
    public float minTiling = 0.8f;   // Minimum grid size
    public float maxTiling = 1.2f;   // Maximum grid size

    private Renderer rend;
    private MaterialPropertyBlock propBlock;
    void Awake()
    {
        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        // Oscillates between 0 and 1 (sin wave mapped to 0..1)
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        // Lerp tiling between min and max
        float tiling = Mathf.Lerp(minTiling, maxTiling, pulse);

        // Update material property
        rend.GetPropertyBlock(propBlock);
        propBlock.SetVector("_BaseMap_ST", new Vector4(tiling, tiling, 0f, 0f));
        rend.SetPropertyBlock(propBlock);
    }
}
