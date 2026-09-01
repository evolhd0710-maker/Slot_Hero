using UnityEngine;
[CreateAssetMenu(fileName = "SwordBookRelic", menuName = "Relic/SwordBook")]
public class SwordBookRelicData : RelicData
{
    [Header("검술 교본 설정")]
    [SerializeField] private int attackPerSword = 1;

    public int AttackPerSword => attackPerSword;

    public override RelicInstance CreateInstance()
    {
        return new SwordBookRelicInstance(this);
    }
}
