using UnityEngine;

[DefaultExecutionOrder(-100)]
public class NewPlayer : NewUnitBase
{
    [Header("플레이어 애니메이션")]
    [SerializeField] public Animator animator;

    private PlayerRunData runData;
    private bool setupCompleted;

    private void Awake()
    {
        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>(true);
        }
    }

    private void Start()
    {
        // 다른 초기화 코드가 이미 Setup을 불렀어도
        // 재초기화하지 않는다.
        Setup();
    }

    public override void Setup()
    {
        if (setupCompleted)
            return;

        if (data == null || data.maxHealth <= 0)
        {
            Debug.LogError(
                "NewPlayer의 CharacterData와 maxHealth를 확인하세요.",
                this
            );

            return;
        }

        runData = PlayerRunData.GetOrCreate();

        // 처음 시작한 런에만 최대 체력으로 초기화한다.
        runData.InitializeHealthIfNeeded(data.maxHealth);

        // base.Setup()이 체력을 최대치로 바꾸기 전에 보존한다.
        int savedHealth = runData.CurrentHealth;

        // 초기화 과정의 임시 최대 체력을
        // 런 데이터에 기록하지 않는다.
        OnHpChanged -= HandleHealthChanged;

        // 실드, 전투 효과 등 기존 전투 초기화는 유지한다.
        base.Setup();

        // 체력만 이전 전투 종료 시점의 값으로 복원한다.
        CurrentHealth = Mathf.Clamp(
            savedHealth,
            0,
            data.maxHealth
        );

        setupCompleted = true;

        OnHpChanged += HandleHealthChanged;

        runData.StoreHealth(
            CurrentHealth,
            data.maxHealth
        );
    }

    private void HandleHealthChanged(
        int health,
        int maximumHealth
    )
    {
        if (runData != null)
        {
            runData.StoreHealth(
                health,
                maximumHealth
            );
        }
    }

    /// <summary>
    /// 별도 체력 비용 처리용.
    /// 실드와 피격 효과를 거치지 않는다.
    /// 기존 코인 구매 API의 호환성을 위해 제공한다.
    /// </summary>
    public bool TryPayHealthCost(
        int amount,
        int minimumRemainingHealth = 1
    )
    {
        if (!setupCompleted)
            Setup();

        if (!setupCompleted ||
            amount < 0 ||
            CurrentHealth <= 0)
        {
            return false;
        }

        minimumRemainingHealth =
            Mathf.Max(0, minimumRemainingHealth);

        if (CurrentHealth - amount < minimumRemainingHealth)
            return false;

        if (amount > 0)
            CurrentHealth -= amount;

        return true;
    }

    public void PlayAttack()
    {
        PlayTrigger("Attack");
    }

    public void PlayHit()
    {
        PlayTrigger("Hit");
    }

    public void PlayDeath()
    {
        PlayTrigger("Death");
    }

    private void PlayTrigger(string triggerName)
    {
        if (animator == null ||
            !animator.isActiveAndEnabled ||
            animator.runtimeAnimatorController == null)
        {
            return;
        }

        animator.ResetTrigger(triggerName);
        animator.SetTrigger(triggerName);
    }

    private void OnDestroy()
    {
        // 여기서 저장하면 오래된 값으로 덮어쓸 수 있으므로
        // 구독만 해제한다.
        OnHpChanged -= HandleHealthChanged;
    }
}