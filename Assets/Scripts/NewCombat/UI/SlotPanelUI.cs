using UnityEngine;

public class SlotPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject slotPanel;
    [SerializeField] private GameObject dimOverlay;

    public bool IsOpen => slotPanel != null && slotPanel.activeSelf;

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    public void Toggle()
    {
        SetVisible(!IsOpen);
    }

    private void SetVisible(bool visible)
    {
        if (dimOverlay != null)
            dimOverlay.SetActive(visible);

        if (slotPanel != null)
            slotPanel.SetActive(visible);
    }
}
