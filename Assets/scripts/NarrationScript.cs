using UnityEngine;

public class SidebarMenu : MonoBehaviour
{
    public GameObject sidebarPanel;

    private bool isOpen = false;

    public void ToggleSidebar()
    {
        isOpen = !isOpen;
        sidebarPanel.SetActive(isOpen);
    }
}