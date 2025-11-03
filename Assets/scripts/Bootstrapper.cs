using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    void Start()
    {
        // This script's only job is to immediately load your first real scene.
        SceneManager.LoadScene("IntroScene");
    }
}