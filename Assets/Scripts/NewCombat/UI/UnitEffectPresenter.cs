using System.Collections.Generic;
using UnityEngine;

public class UnitEffectPresenter : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private NewUnitBase targetUnit;

    [Header("UI")]
    [SerializeField] private Transform iconContainer;
    [SerializeField] private UnitEffectIconUI iconPrefab;

    [Header("Data")]
    [SerializeField] private EffectVisualDatabase visualDatabase;

    private readonly Dictionary<UnitEffect, UnitEffectIconUI>
        activeIcons = new();

    private readonly Queue<UnitEffectIconUI>
        iconPool = new();

    private readonly HashSet<UnitEffect>
        currentlyDisplayedEffects = new();

    private readonly List<UnitEffect>
        removalBuffer = new();

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void SetTarget(NewUnitBase newTarget)
    {
        Unsubscribe();

        targetUnit = newTarget;

        Subscribe();
        Refresh();
    }

    public void Refresh()
    {
        currentlyDisplayedEffects.Clear();

        if (targetUnit == null ||
            visualDatabase == null ||
            iconPrefab == null ||
            iconContainer == null)
        {
            ReleaseAllIcons();
            return;
        }

        foreach (UnitEffect effect in
                 targetUnit.Effects.ActiveEffects)
        {
            if (effect == null ||
                effect.IsExpired ||
                !effect.ShowInUI)
            {
                continue;
            }

            if (!visualDatabase.TryGetEntry(
                    effect.EffectId,
                    out EffectVisualDatabase.Entry visual))
            {
                continue;
            }

            currentlyDisplayedEffects.Add(effect);

            if (!activeIcons.TryGetValue(
                    effect,
                    out UnitEffectIconUI iconUI))
            {
                iconUI = GetIcon();
                activeIcons.Add(effect, iconUI);

                iconUI.transform.SetParent(
                    iconContainer,
                    false
                );
            }

            iconUI.Bind(
                effect,
                visual.icon,
                visual.showStackCount
            );
        }

        RemoveUnusedIcons();
    }

    private void Subscribe()
    {
        if (targetUnit == null)
            return;

        targetUnit.Effects.EffectsChanged -= Refresh;
        targetUnit.Effects.EffectsChanged += Refresh;
    }

    private void Unsubscribe()
    {
        if (targetUnit == null)
            return;

        targetUnit.Effects.EffectsChanged -= Refresh;
    }

    private UnitEffectIconUI GetIcon()
    {
        if (iconPool.Count > 0)
            return iconPool.Dequeue();

        return Instantiate(
            iconPrefab,
            iconContainer
        );
    }

    private void RemoveUnusedIcons()
    {
        removalBuffer.Clear();

        foreach (KeyValuePair<UnitEffect, UnitEffectIconUI> pair
                 in activeIcons)
        {
            if (!currentlyDisplayedEffects.Contains(pair.Key))
                removalBuffer.Add(pair.Key);
        }

        foreach (UnitEffect effect in removalBuffer)
        {
            UnitEffectIconUI icon = activeIcons[effect];

            activeIcons.Remove(effect);

            icon.Release();
            iconPool.Enqueue(icon);
        }
    }

    private void ReleaseAllIcons()
    {
        foreach (UnitEffectIconUI icon in activeIcons.Values)
        {
            icon.Release();
            iconPool.Enqueue(icon);
        }

        activeIcons.Clear();
    }
}