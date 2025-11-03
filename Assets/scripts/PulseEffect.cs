using UnityEngine;
public class PulseEffect : MonoBehaviour
{
    void Start()
    {
        LeanTween.scale(gameObject, transform.localScale * 1.2f, 1.5f).setEasePunch().setLoopPingPong();
    }
}