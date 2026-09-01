using UnityEngine;

[CreateAssetMenu(fileName = "ObsidianBladeRelic", menuName = "Relic/Obsidian Blade")]
public class ObsidianBladeRelicData : RelicData
{
    [Header("Èæ¿ä¼® Ä®³¯ ¼³Á¤")]
    [SerializeField] private int requiredTempleCount = 2;
    [SerializeField] private int lifestealPerTemple = 1;

    public int RequiredTempleCount => requiredTempleCount;
    public int LifestealPerTemple => lifestealPerTemple;

    public override RelicInstance CreateInstance()
    {
        return new ObsidianBladeRelicInstance(this);
    }
}
