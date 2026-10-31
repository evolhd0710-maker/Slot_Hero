namespace SlotHero.Sanctum
{
    /// <summary>
    /// 야영이 회복시킬 체력을 들고 있는 쪽.
    /// 성소 기획서 v0.2 / 04 야영 은 회복 수치와 최대 초과 금지만 정하고
    /// 체력 자체는 전투 쪽이 들고 있으므로 이 창구로만 건드린다.
    /// </summary>
    public interface IPlayerVitals
    {
        /// <summary>지금 체력.</summary>
        int Health { get; }

        /// <summary>최대 체력.</summary>
        int MaxHealth { get; }

        /// <summary>체력을 올린다. 넘겨 주는 값은 최대 체력을 넘지 않도록 성소 쪽에서 이미 잘라 둔 값이다.</summary>
        void Heal(int amount);
    }
}
