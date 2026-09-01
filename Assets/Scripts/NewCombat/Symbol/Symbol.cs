using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SymbolTag
{
    제국,
    도검,
    신전,
    고대,
    둔기,
    방패,
    상징
}

public enum SymbolType
{
    글라디우스,
    사이포스,
    실드,
    몽둥이,
    스쿠툼,
    조각상
}

[CreateAssetMenu(fileName = "NewSymbol", menuName = "Symbol")]
public class Symbol : ScriptableObject
{
    [Header("기본 정보")]
    public SymbolType symbolType;
    public Sprite symbolSprite;
    public List<SymbolTag> symbolTags = new List<SymbolTag>();

    [Header("무기 고유 스탯")]
    public int baseAttack;
    public int baseDefense;

    [Header("이 무기가 발동할 효과들")]
    public List<SymbolEffect> basicEffects = new List<SymbolEffect>();
    public List<SymbolEffect> specialEffects = new List<SymbolEffect>();

    [Header("실행 연출")]
    [SerializeField] private float effectInterval = 0.3f;
    [SerializeField] private float pulseScale = 1.3f;

    public IEnumerator Execute(NewPlayer player, NewEnemy enemy, TurnContext turnContext, int countInTurn, RectTransform targetUI, RelicController relicController = null, bool isReplay = false)
    {
        Debug.Log($"[Symbol.Execute 시작] {symbolType}, Count: {countInTurn}, Frame: {Time.frameCount}");

        if (player == null)
        {
            Debug.LogError($"{name} 실행 실패: Player가 없습니다.");
            yield break;
        }

        if (enemy == null)
        {
            Debug.LogError($"{name} 실행 실패: Enemy가 없습니다.");
            yield break;
        }

        if (turnContext == null)
        {
            Debug.LogError($"{name} 실행 실패: TurnContext가 없습니다.");
            yield break;
        }

        SymbolExecutionContext executionContext = new SymbolExecutionContext(player, enemy, turnContext, this, countInTurn, baseAttack, baseDefense, relicController, isReplay);

        turnContext.BeginSymbolExecution(executionContext);

        try
        {
            relicController?.BeforeSymbolExecute(executionContext);

            if (!executionContext.Cancelled)
            {
                CalculateFinalValues(executionContext, relicController);
                if (targetUI != null && UIFXManager.Instance != null)
                    UIFXManager.Instance.StartCoroutine(Co_PlayPulseAnimation(targetUI, effectInterval, pulseScale));
                yield return ExecuteEffectList(specialEffects, executionContext, targetUI);
                if (targetUI != null && UIFXManager.Instance != null)
                    UIFXManager.Instance.StartCoroutine(Co_PlayPulseAnimation(targetUI, effectInterval, pulseScale));
                yield return ExecuteEffectList(basicEffects, executionContext, targetUI);
            }

            relicController?.AfterSymbolExecute(executionContext);
        }
        finally
        {
            turnContext.EndSymbolExecution(executionContext);
        }
    }

    public bool HasTag(SymbolTag tag)
    {
        return symbolTags != null && symbolTags.Contains(tag);
    }

    private void CalculateFinalValues(SymbolExecutionContext context, RelicController relicController)
    {
        int finalAttack = context.Player.ModifyOutgoingDamage(context.BaseAttack, this, context.TurnContext);

        if (relicController != null)
            finalAttack = relicController.ModifySymbolPower(context, finalAttack);

        context.SetFinalAttack(finalAttack);
        context.SetFinalDefense(context.BaseDefense);
    }

    private IEnumerator ExecuteEffectList(List<SymbolEffect> effects, SymbolExecutionContext context, RectTransform targetUI)
    {
        if (effects == null)
            yield break;

        foreach (SymbolEffect effect in effects)
        {
            if (context.Cancelled)
                yield break;

            if (effect == null)
                continue;

            effect.Apply(context);

            if (effectInterval > 0f)
                yield return new WaitForSeconds(effectInterval);
        }
    }

    private IEnumerator Co_PlayPulseAnimation(RectTransform uiTransform, float duration, float maxScale)
    {
        if (uiTransform == null)
            yield break;

        Vector3 originScale = uiTransform.localScale;
        Vector3 targetScale = originScale * maxScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / duration);
            float pulseProgress = Mathf.Sin(t * Mathf.PI);

            uiTransform.localScale = Vector3.Lerp(originScale, targetScale, pulseProgress);

            yield return null;
        }

        uiTransform.localScale = originScale;
    }
}