using UnityEngine;

[CreateAssetMenu(fileName = "EagleFlag", menuName = "Relic/EagleFlag")]
public class EagleFlagRelicData : RelicData
{
    [Header("독수리 군기 설정")]
    [SerializeField] private int requiredEmpireCount = 3;
    [SerializeField] private float damageMultiplier = 1.5f;

    public int RequiredEmpireCount => requiredEmpireCount;
    public float DamageMultiplier => damageMultiplier;

    public override RelicInstance CreateInstance()
    {
        return new EagleFlagRelicInstance(this);
    }
}
