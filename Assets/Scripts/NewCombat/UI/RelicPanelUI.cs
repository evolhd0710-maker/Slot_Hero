using System.Collections.Generic;
using UnityEngine;

public class RelicPanelUI : MonoBehaviour
{
    [Header("µ•¿Ã≈Õ")]
    [SerializeField] private RelicController relicController;

    [Header("UI")]
    [SerializeField] private RectTransform relicContainer;
    [SerializeField] private RelicIconUI relicIconPrefab;
    [SerializeField] private RelicTooltipUI tooltipUI;

    private readonly List<RelicIconUI> iconPool = new List<RelicIconUI>();

    private void OnEnable()
    {
        if (relicController != null)
            relicController.RelicsChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (relicController != null)
            relicController.RelicsChanged -= Refresh;

        tooltipUI?.Hide();
    }

    public void Refresh()
    {
        HideAllIcons();

        if (relicController == null || relicIconPrefab == null || relicContainer == null)
            return;

        IReadOnlyList<RelicInstance> relics = relicController.ActiveRelics;

        for (int i = 0; i < relics.Count; i++)
        {
            RelicIconUI iconUI = GetIcon(i);
            iconUI.Bind(relics[i], tooltipUI);
        }
    }

    private RelicIconUI GetIcon(int index)
    {
        while (iconPool.Count <= index)
        {
            RelicIconUI iconUI = Instantiate(relicIconPrefab, relicContainer);
            iconPool.Add(iconUI);
        }

        return iconPool[index];
    }

    private void HideAllIcons()
    {
        foreach (RelicIconUI iconUI in iconPool)
            iconUI.Clear();
    }
}