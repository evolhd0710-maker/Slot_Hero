using System;
using UnityEngine;

public class NewUnitBase : MonoBehaviour
{
    private int currentHealth;
    private int shield;

    public CharacterData data;

    public event Action<int, int> OnHpChanged;
    public event Action<int> OnShieldChanged;

    // 실드에 막히는지와 관계없이 들어온 전체 피해량
    public event Action<int> OnDamageReceived;

    // 실드를 통과하여 실제 체력에 적용된 피해량
    public event Action<int> OnDamageTaken;

    // 새롭게 획득한 실드량
    public event Action<int> OnShieldGained;

    private UnitEffectController effectController;

    public UnitEffectController Effects
    {
        get
        {
            if (effectController == null)
                effectController = new UnitEffectController(this);

            return effectController;
        }
    }

    public int CurrentHealth
    {
        get => currentHealth;

        protected set
        {
            if (data == null)
            {
                Debug.LogError(
                    $"{name}에 CharacterData가 연결되지 않았습니다.",
                    this
                );

                return;
            }

            currentHealth = Mathf.Clamp(
                value,
                0,
                data.maxHealth
            );

            OnHpChanged?.Invoke(
                currentHealth,
                data.maxHealth
            );
        }
    }

    public int CurrentShield => shield;

    public virtual void Setup()
    {
        if (data == null)
        {
            Debug.LogError(
                $"{name}에 CharacterData가 연결되지 않았습니다.",
                this
            );

            return;
        }

        CurrentHealth = data.maxHealth;
        shield = 0;

        Effects.ClearAll();

        OnShieldChanged?.Invoke(shield);
    }

    public virtual void TakeDamage(
        int damage,
        string reason
    )
    {
        if (damage <= 0)
            return;

        int requestedDamage = damage;

        // 실드가 피해를 전부 막더라도 피해 팝업을 발생시킨다.
        OnDamageReceived?.Invoke(requestedDamage);

        int absorbedDamage = 0;

        if (shield > 0)
        {
            absorbedDamage = Mathf.Min(
                shield,
                damage
            );

            shield -= absorbedDamage;
            damage -= absorbedDamage;

            OnShieldChanged?.Invoke(shield);
        }

        int healthBeforeDamage = CurrentHealth;

        if (damage > 0)
            CurrentHealth -= damage;

        int actualHealthDamage =
            healthBeforeDamage - CurrentHealth;

        if (actualHealthDamage > 0)
            OnDamageTaken?.Invoke(actualHealthDamage);

        string unitName =
            data != null ? data.name : name;

        Debug.Log(
            $"{unitName} 피해 처리 | " +
            $"받은 피해: {requestedDamage}, " +
            $"실드 흡수: {absorbedDamage}, " +
            $"체력 피해: {actualHealthDamage}, " +
            $"남은 체력: {CurrentHealth}, " +
            $"남은 실드: {shield}, " +
            $"이유: {reason}",
            this
        );

        TurnContext context = new TurnContext();

        Effects.Trigger(
            TriggerTiming.OnDamaged,
            new EffectTriggerContext(
                context,
                target: this
            )
        );
    }

    public virtual void AddShield(int amount)
    {
        if (amount <= 0)
            return;

        shield += amount;

        OnShieldChanged?.Invoke(shield);

        // 획득한 실드량을 팝업 UI에 전달한다.
        OnShieldGained?.Invoke(amount);

        string unitName =
            data != null ? data.name : name;

        Debug.Log(
            $"{unitName}에게 {amount} 실드 부여, 남은 실드 {shield}",
            this
        );
    }

    public void ApplyEffect(UnitEffect effect)
    {
        if (effect == null)
            return;

        Effects.AddEffect(effect);
    }

    public bool HasEffect<T>() where T : UnitEffect
    {
        return Effects.HasEffect<T>();
    }

    public bool RaiseTiming(
        TriggerTiming timing,
        TurnContext turnContext,
        NewUnitBase source = null,
        Symbol symbol = null
    )
    {
        if (turnContext == null)
            turnContext = new TurnContext();

        turnContext.actionCancelled = false;

        EffectTriggerContext effectContext =
            new EffectTriggerContext(
                turnContext,
                source,
                this,
                symbol
            );

        Effects.Trigger(
            timing,
            effectContext
        );

        return turnContext.actionCancelled;
    }

    public int ModifyOutgoingDamage(
        int rawDamage,
        Symbol symbol,
        TurnContext turnContext = null
    )
    {
        int modifiedDamage =
            Effects.ModifyOutgoingDamage(
                rawDamage,
                symbol,
                turnContext
            );

        if (rawDamage != modifiedDamage)
        {
            Debug.Log(
                $"효과로 인한 데미지 변경: {rawDamage} → {modifiedDamage}",
                this
            );
        }

        return modifiedDamage;
    }

    public void ClearTurnLimitedEffects()
    {
        Effects.ClearTurnLimitedEffects();
    }

    public void ClearTurnLimitedModifiers()
    {
        ClearTurnLimitedEffects();
    }

    public virtual void Heal(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0)
            return;

        int healthBefore = CurrentHealth;

        CurrentHealth += amount;

        int actualHealing =
            CurrentHealth - healthBefore;

        if (actualHealing <= 0)
            return;

        string unitName =
            data != null ? data.name : name;

        Debug.Log(
            $"{unitName} 체력 회복: {actualHealing}, 현재 체력: {CurrentHealth}",
            this
        );
    }

    public void NotifyDamageDealt(
        int damage,
        SymbolExecutionContext context = null
    )
    {
        if (damage <= 0)
            return;

        Effects.OnDamageDealt(
            damage,
            context
        );
    }
}