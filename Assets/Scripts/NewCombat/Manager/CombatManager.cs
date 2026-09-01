using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    [Header("전투 구성")]
    [SerializeField] private SlotManager slotManager;
    [SerializeField] private NewPlayer player;
    [SerializeField] private NewEnemy enemy;
    [SerializeField] private EnemyView enemyView;
    [SerializeField] private RelicController relicController;
    [SerializeField] private Animator playerAnimator;


    //전투 화면만 실행하고 싶을 때 사용하는 적 데이터
    [Header("전투 씬 직접 실행용")]
    [SerializeField] private EnemyDefinitionData fallbackEnemyDefinition;

    [Header("전투 UI")]
    [SerializeField] private Slider playerHp;
    [SerializeField] private Slider enemyHp;
    [SerializeField] private TMP_Text playerShieldText;
    [SerializeField] private TMP_Text enemyShieldText;
    [SerializeField] private GameObject totalDamageText;
    [SerializeField] private ResultPanelUI resultPanelUI;
    [SerializeField] private SlotPanelUI slotPanelUI;

    //문양 무한 반복 실행을 막기 위해서 임시로 설정해 놓았음, 무한 반복 실행 조건이 맞춰졌을 경우 어떻게 처리할지 기획측에 전달해야함
    [Header("문양 재사용 제한 횟수??")] 
    [SerializeField] private int maximumSymbolExecutions = 10;

    [Header("전투 종료")]
    [Tooltip("전투 종료 후 맵으로 돌아가기 전까지 기다리는 시간입니다.")]
    [SerializeField, Min(0f)] private float returnToMapDelay = 1f;

    //전투 관리를 위한 bool 
    private bool isTurnRunning;
    private bool isBattleEnded;
    private bool isReturningToMap;

    //현재 턴 실행 맥락 
    private TurnContext currentTurnContext;

    private void Awake()
    {
        //
        if (!ValidateBattleReferences())
        {
            enabled = false;
            return;
        }

        if (!InitializeSelectedEnemy())
        {
            enabled = false;
            return;
        }

        // Setup 안에서 체력과 실드 이벤트가 발생하므로 먼저 구독한다.
        SubscribeUnitEvents();

        player.Setup();
        enemy.Setup();

        InitializeRelics();
        SetTotalDamageVisible(false);
        RefreshBattleUI();
    }

    private bool ValidateBattleReferences()
    {
        if (slotManager == null)
        {
            Debug.LogError("CombatManager에 SlotManager가 연결되지 않았습니다.", this);
            return false;
        }

        if (player == null)
        {
            Debug.LogError("CombatManager에 Player가 연결되지 않았습니다.", this);
            return false;
        }

        if (enemy == null)
        {
            Debug.LogError("CombatManager에 Enemy가 연결되지 않았습니다.", this);
            return false;
        }

        if (enemyView == null)
        {
            Debug.LogError("CombatManager에 EnemyView가 연결되지 않았습니다.", this);
            return false;
        }

        return true;
    }

    private bool InitializeSelectedEnemy()
    {
        EnemyDefinitionData definition = BattleSelectionData.HasSelectedEnemy
            ? BattleSelectionData.SelectedEnemy
            : fallbackEnemyDefinition;

        if (definition == null)
        {
            Debug.LogError("전투에 사용할 EnemyDefinitionData가 없습니다.", this);
            return false;
        }

        if (definition.UnitData == null)
        {
            Debug.LogError($"{definition.name}에 Unit Data가 연결되지 않았습니다.", definition);
            return false;
        }

        if (definition.BrainData == null)
        {
            Debug.LogError($"{definition.name}에 Brain Data가 연결되지 않았습니다.", definition);
            return false;
        }

        enemy.ActionSelected -= HandleEnemyActionSelected;
        enemy.ActionSelected += HandleEnemyActionSelected;

        enemy.ApplyDefinition(definition);
        enemyView.ApplyDefinition(definition);

        BattleSelectionData.Clear();

        return true;
    }

    private void InitializeRelics()
    {
        if (relicController == null)
        {
            Debug.LogWarning("NewCombatManager에 RelicController가 연결되지 않았습니다.", this);
            return;
        }

        relicController.Initialize(player, enemy);
        relicController.OnBattleStart(new TurnContext());
    }

    private void SubscribeUnitEvents()
    {
        player.OnHpChanged -= HandlePlayerHpChanged;
        player.OnShieldChanged -= HandlePlayerShieldChanged;

        enemy.OnHpChanged -= HandleEnemyHpChanged;
        enemy.OnShieldChanged -= HandleEnemyShieldChanged;

        player.OnHpChanged += HandlePlayerHpChanged;
        player.OnShieldChanged += HandlePlayerShieldChanged;

        enemy.OnHpChanged += HandleEnemyHpChanged;
        enemy.OnShieldChanged += HandleEnemyShieldChanged;
    }

    public void OnSlotButtonClicked()
    {
        if (isTurnRunning || isBattleEnded)
            return;

        OpenSlotPanels();
        StartCoroutine(FullTurn());
    }

    public void OnPanelToggleButtonClicked()
    {
        if (slotPanelUI == null)
            return;

        if (slotPanelUI.IsOpen)
            CloseSlotPanels();
        else
            OpenSlotPanels();
    }

    public void TurnStart()
    {
        OnSlotButtonClicked();
    }

    public void SlotPanelToggle()
    {
        if (slotManager == null || !slotManager.isRollEnd)
            return;

        OnPanelToggleButtonClicked();
    }

    private IEnumerator FullTurn()
    {
        isTurnRunning = true;

        yield return PlayerTurnStart();
        yield return new WaitForSeconds(1f);
        if (!IsBattleOver())
            yield return EnemyTurnStart();

        RefreshBattleUI();

        if (IsBattleOver())
        {
            EndBattle();
            isTurnRunning = false;
            yield break;
        }

        isTurnRunning = false;
    }

    private IEnumerator PlayerTurnStart()
    {
        SetTotalDamageVisible(true);

        currentTurnContext = new TurnContext();

        Dictionary<SymbolType, int> symbolCounts =
            new Dictionary<SymbolType, int>();

        relicController?.OnTurnStart(currentTurnContext);

        if (resultPanelUI != null)
            resultPanelUI.Refresh(currentTurnContext);

        slotManager.RollWrapper();

        yield return new WaitUntil(() => slotManager.isRollEnd);

        BuildTurnTagCounts(
            currentTurnContext,
            slotManager.RolledResults
        );

        relicController?.OnRollResolved(
            currentTurnContext,
            slotManager.RolledResults
        );

        for (int i = 0; i < slotManager.RolledResults.Count; i++)
        {
            Symbol symbol = slotManager.RolledResults[i];

            if (symbol == null)
                continue;

            int occurrenceCount = IncreaseSymbolCount(
                symbolCounts,
                symbol.symbolType
            );

            yield return ProcessSymbol(
                symbol,
                i,
                occurrenceCount,
                currentTurnContext
            );

            if (resultPanelUI != null)
                resultPanelUI.Refresh(currentTurnContext);

            if (IsBattleOver())
                break;
        }

        SetTotalDamageVisible(false);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));


        CloseSlotPanels();

        ApplyTurnResult(currentTurnContext);
        player.animator.SetTrigger("Attack");
        relicController?.OnTurnEnd(currentTurnContext);

        player.ClearTurnLimitedModifiers();

        RefreshBattleUI();
    }

    private IEnumerator EnemyTurnStart()
    {
        if (enemy == null || player == null)
            yield break;

        yield return enemy.ExecuteTurn(player);

        RefreshBattleUI();
    }

    private IEnumerator ProcessSymbol(
        Symbol symbol,
        int reelIndex,
        int occurrenceCount,
        TurnContext turnContext
    )
    {
        if (symbol == null || turnContext == null)
            yield break;

        int damageBefore = turnContext.totalDamage;
        int defenseBefore = turnContext.totalDefense;

        RectTransform currentReelUI =
            slotManager.reels[reelIndex].slotRects[2];

        int pendingExecutions = 1;
        int executionCount = 0;
        int executionLimit = Mathf.Max(1, maximumSymbolExecutions);

        while (pendingExecutions > 0 &&
               executionCount < executionLimit)
        {
            pendingExecutions--;

            bool isReplay = executionCount > 0;

            if (isReplay)
            {
                Debug.Log(
                    $"{symbol.name} 문양 재사용 실행. 재사용 횟수: {executionCount}"
                );
            }

            yield return symbol.Execute(
                player,
                enemy,
                turnContext,
                occurrenceCount,
                currentReelUI,
                relicController,
                isReplay
            );

            int requestedReplayCount =
                turnContext.ConsumeSymbolReplay(symbol);

            if (requestedReplayCount > 0)
                pendingExecutions += requestedReplayCount;

            executionCount++;

            RefreshBattleUI();

            if (IsBattleOver())
                break;
        }

        if (executionCount >= executionLimit &&
            pendingExecutions > 0)
        {
            turnContext.ConsumeSymbolReplay(symbol);

            Debug.LogError(
                $"{symbol.name} 문양의 재사용 횟수가 안전 제한 {executionLimit}회를 초과했습니다."
            );
        }

        AddSymbolLog(
            symbol,
            turnContext,
            damageBefore,
            defenseBefore,
            executionCount
        );
    }

    private void AddSymbolLog(
        Symbol symbol,
        TurnContext turnContext,
        int damageBefore,
        int defenseBefore,
        int executionCount
    )
    {
        int damageDelta =
            turnContext.totalDamage - damageBefore;

        int defenseDelta =
            turnContext.totalDefense - defenseBefore;

        int totalBaseAttack =
            symbol.baseAttack * executionCount;

        int totalBaseDefense =
            symbol.baseDefense * executionCount;

        turnContext.symbolLogs.Add(
            new SymbolResultLog
            {
                symbolName = symbol.symbolType.ToString(),
                baseAttack = totalBaseAttack,
                bonusAttack = damageDelta - totalBaseAttack,
                baseDefense = totalBaseDefense,
                bonusDefense = defenseDelta - totalBaseDefense
            }
        );
    }

    private int IncreaseSymbolCount(
        Dictionary<SymbolType, int> symbolCounts,
        SymbolType symbolType
    )
    {
        if (!symbolCounts.ContainsKey(symbolType))
            symbolCounts[symbolType] = 0;

        symbolCounts[symbolType]++;

        return symbolCounts[symbolType];
    }

    private void BuildTurnTagCounts(
        TurnContext turnContext,
        IReadOnlyList<Symbol> symbols
    )
    {
        turnContext.tagCounts.Clear();

        foreach (Symbol symbol in symbols)
        {
            if (symbol == null || symbol.symbolTags == null)
                continue;

            foreach (SymbolTag tag in symbol.symbolTags)
                turnContext.AddTagCount(tag);
        }
    }

    private void ApplyTurnResult(TurnContext turnContext)
    {
        if (turnContext == null)
            return;

        if (turnContext.totalDamage > 0)
        {
            enemy.TakeDamage(
                turnContext.totalDamage,
                "슬롯 굴림"
            );

            player.NotifyDamageDealt(
                turnContext.totalDamage
            );
        }

        if (turnContext.totalDefense > 0)
            player.AddShield(turnContext.totalDefense);

        RefreshBattleUI();
    }

    private bool IsBattleOver()
    {
        if (player == null || enemy == null)
            return true;

        return player.CurrentHealth <= 0 ||
               enemy.CurrentHealth <= 0;
    }

    private void EndBattle()
    {
        if (isBattleEnded)
            return;

        isBattleEnded = true;

        enemy?.NotifyBattleEnd();

        relicController?.OnBattleEnd(
            currentTurnContext ?? new TurnContext()
        );

        CloseSlotPanels();
        SetTotalDamageVisible(false);
        RefreshBattleUI();

        if (enemy != null && enemy.CurrentHealth <= 0)
        {
            enemyView?.PlayDeath();
            Debug.Log("적 처치. 전투에서 승리했습니다.");
        }
        else if (player != null && player.CurrentHealth <= 0)
        {
            Debug.Log("플레이어가 쓰러졌습니다.");
        }

        StartCoroutine(ReturnToMapAfterBattle());
    }

    private IEnumerator ReturnToMapAfterBattle()
    {
        if (isReturningToMap)
            yield break;

        isReturningToMap = true;

        if (returnToMapDelay > 0f)
            yield return new WaitForSecondsRealtime(returnToMapDelay);

        MapManager mapManager = FindLoadedMapManager();

        if (mapManager == null)
        {
            Debug.LogError(
                "로드된 맵 씬에서 MapManager를 찾을 수 없습니다. 전투 씬을 단독 실행했다면 맵 복귀는 실행되지 않습니다.",
                this
            );

            isReturningToMap = false;
            yield break;
        }

        mapManager.ReturnToMap();

        Scene battleScene = gameObject.scene;

        if (!battleScene.IsValid() || !battleScene.isLoaded)
        {
            Debug.LogError("언로드할 전투 씬을 찾을 수 없습니다.", this);
            isReturningToMap = false;
            yield break;
        }

        SceneManager.UnloadSceneAsync(battleScene);
    }

    private MapManager FindLoadedMapManager()
    {
        MapManager[] mapManagers =
            Resources.FindObjectsOfTypeAll<MapManager>();

        foreach (MapManager candidate in mapManagers)
        {
            if (candidate == null)
                continue;

            Scene candidateScene = candidate.gameObject.scene;

            if (candidateScene.IsValid() &&
                candidateScene.isLoaded &&
                candidateScene != gameObject.scene)
            {
                return candidate;
            }
        }

        return null;
    }

    private void HandleEnemyActionSelected(
        EnemyActionData action
    )
    {
        if (action == null || enemyView == null)
            return;

        string triggerName =
            string.IsNullOrWhiteSpace(action.AnimationTrigger)
                ? "Attack"
                : action.AnimationTrigger;

        enemyView.PlayAnimation(triggerName);
    }

    private void HandlePlayerHpChanged(
        int currentHealth,
        int maxHealth
    )
    {
        if (playerHp == null)
            return;

        playerHp.value = maxHealth > 0
            ? (float)currentHealth / maxHealth
            : 0f;
    }

    private void HandleEnemyHpChanged(
        int currentHealth,
        int maxHealth
    )
    {
        if (enemyHp == null)
            return;

        enemyHp.value = maxHealth > 0
            ? (float)currentHealth / maxHealth
            : 0f;
    }

    private void HandlePlayerShieldChanged(
        int currentShield
    )
    {
        if (playerShieldText != null)
            playerShieldText.text = currentShield.ToString();
    }

    private void HandleEnemyShieldChanged(
        int currentShield
    )
    {
        if (enemyShieldText != null)
            enemyShieldText.text = currentShield.ToString();
    }

    private void RefreshBattleUI()
    {
        CheckHp();
        CheckShield();
    }

    public void CheckHp()
    {
        if (playerHp != null &&
            player != null &&
            player.data != null &&
            player.data.maxHealth > 0)
        {
            playerHp.value =
                (float)player.CurrentHealth /
                player.data.maxHealth;
        }

        if (enemyHp != null &&
            enemy != null &&
            enemy.data != null &&
            enemy.data.maxHealth > 0)
        {
            enemyHp.value =
                (float)enemy.CurrentHealth /
                enemy.data.maxHealth;
        }
    }

    public void CheckShield()
    {
        if (playerShieldText != null)
        {
            playerShieldText.text =
                player != null
                    ? player.CurrentShield.ToString()
                    : "0";
        }

        if (enemyShieldText != null)
        {
            enemyShieldText.text =
                enemy != null
                    ? enemy.CurrentShield.ToString()
                    : "0";
        }
    }

    private void OpenSlotPanels()
    {
        slotPanelUI?.Show();
        resultPanelUI?.Show();
    }

    private void CloseSlotPanels()
    {
        resultPanelUI?.Hide();
        slotPanelUI?.Hide();
    }

    private void SetTotalDamageVisible(bool visible)
    {
        if (totalDamageText != null)
            totalDamageText.SetActive(visible);
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnHpChanged -= HandlePlayerHpChanged;
            player.OnShieldChanged -= HandlePlayerShieldChanged;
        }

        if (enemy != null)
        {
            enemy.OnHpChanged -= HandleEnemyHpChanged;
            enemy.OnShieldChanged -= HandleEnemyShieldChanged;
            enemy.ActionSelected -= HandleEnemyActionSelected;
        }
    }
}