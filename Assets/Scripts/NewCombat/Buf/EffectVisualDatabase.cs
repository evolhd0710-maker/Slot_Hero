using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EffectVisualDatabase",
    menuName = "Combat/Effect Visual Database"
)]
public class EffectVisualDatabase : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [Tooltip("UnitEffect.EffectId와 동일해야 합니다.")]
        public string effectId;

        public Sprite icon;

        [Tooltip("1스택일 때도 숫자를 표시할지 결정합니다.")]
        public bool showStackCount = true;
    }

    [SerializeField] private List<Entry> entries = new();

    private Dictionary<string, Entry> entryLookup;

    public bool TryGetEntry(string effectId, out Entry entry)
    {
        BuildLookupIfNeeded();

        return entryLookup.TryGetValue(effectId, out entry);
    }

    private void BuildLookupIfNeeded()
    {
        if (entryLookup != null)
            return;

        entryLookup = new Dictionary<string, Entry>();

        foreach (Entry entry in entries)
        {
            if (entry == null ||
                string.IsNullOrWhiteSpace(entry.effectId))
            {
                continue;
            }

            entryLookup[entry.effectId] = entry;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        entryLookup = null;
    }
#endif
}
