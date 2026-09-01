/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ReelSlotGroup
{
    [Tooltip("순서: [0]위2 [1]위1 [2]중앙 [3]아래1 [4]아래2")]
    public RectTransform[] slotRects = new RectTransform[5];
    public Image[] slotImages = new Image[5];
}

public class SlotManagerTest : MonoBehaviour
{
    [Header("덱 데이터")]
    public List<Symbol> masterDeck = new List<Symbol>();
    private List<Symbol> runtimeDeck = new List<Symbol>();
    public List<Symbol> RolledResults { get; private set; } = new List<Symbol>();

    [Header("릴 UI (릴당 5슬롯)")]
    public ReelSlotGroup[] reels;

    [Header("연출 세팅")]

    [SerializeField] private float stepDuration = 0.08f;
    [SerializeField] private float baseRollDuration = 1.0f; // 첫 번째 릴 최소 회전 시간
    [SerializeField] private float reelStopInterval = 0.2f; // 릴간 정지 간격
    [SerializeField] private float overshootDistance = 40f;
    [SerializeField] private float overshootDuration = 0.12f;
    [SerializeField] private float settleDuration = 0.18f;

    [Header("슬라이드 연출")]
    public SlotPanelSlider panelSlider;

    public bool isRollEnd;

    [Header("슬롯 간격")]
    [SerializeField] private float slotSpacing = 0f;
    private void Awake()
    {
        isRollEnd = true;
        InitSlotPositions();
    }

    private void InitSlotPositions()
    {
        foreach (var reel in reels)
        {
            float stepDistance = GetSlotStepDistance(reel);

            if (stepDistance <= 0f)
                continue;

            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = reel.slotRects[i].anchoredPosition;
                pos.y = (2 - i) * stepDistance;
                reel.slotRects[i].anchoredPosition = pos;

                if (masterDeck.Count > 0)
                    reel.slotImages[i].sprite = GetRandomDummy().symbolSprite;
            }
        }
    }

    public void RollWrapper()
    {
        StopAllCoroutines();
        StartCoroutine(Roll());
    }

    private IEnumerator Roll()
    {
        isRollEnd = false;
        RolledResults.Clear();
        runtimeDeck = new List<Symbol>(masterDeck);
        ShuffleDeck(runtimeDeck);

        int maxSlots = Mathf.Min(5, reels.Length, runtimeDeck.Count);
        for (int i = 0; i < maxSlots; i++)
            RolledResults.Add(runtimeDeck[i]);

        for (int i = 0; i < maxSlots; i++)
            StartCoroutine(Co_RollReel(i, RolledResults[i]));

        float lastReelDuration = baseRollDuration + (maxSlots - 1) * reelStopInterval;
        float totalWait = lastReelDuration + overshootDuration + settleDuration + 0.1f;
        yield return new WaitForSeconds(totalWait);

        isRollEnd = true;
    }

    private IEnumerator Co_RollReel(int reelIndex, Symbol answer)
    {
        ReelSlotGroup reel = reels[reelIndex];

        float reelDuration = baseRollDuration + reelIndex * reelStopInterval;
        int totalSteps = Mathf.Max(5, Mathf.RoundToInt(reelDuration / stepDuration));
        int dummyCount = totalSteps - 5;

        Queue<Symbol> queue = new Queue<Symbol>();
        for (int i = 0; i < dummyCount; i++) queue.Enqueue(GetRandomDummy());
        queue.Enqueue(GetRandomDummy()); // 가운데 기준 아래2
        queue.Enqueue(GetRandomDummy()); // 아래1
        queue.Enqueue(answer);           // 중앙 정답
        queue.Enqueue(GetRandomDummy()); // 위1
        queue.Enqueue(GetRandomDummy()); // 위2 (마지막에 삽입)

        Symbol[] current = new Symbol[5];
        for (int i = 0; i < 5; i++)
        {
            current[i] = GetRandomDummy();
            reel.slotImages[i].sprite = current[i].symbolSprite;
        }

        while (queue.Count > 0)
            yield return StartCoroutine(Co_Step(reel, current, queue.Dequeue()));

        yield return StartCoroutine(Co_Overshoot(reel));
    }

    private IEnumerator Co_Step(ReelSlotGroup reel, Symbol[] current, Symbol next)
    {
        float stepDistance = GetSlotStepDistance(reel);

        if (stepDistance <= 0f)
            yield break;

        float elapsed = 0f;
        while (elapsed < stepDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / stepDuration;
            float offset = Mathf.Lerp(0f,-stepDistance,t);
            ApplyOffset(reel, offset, stepDistance);
            yield return null;
        }

        for (int i = 4; i > 0; i--)
            current[i] = current[i - 1];
        current[0] = next;

        for (int i = 0; i < 5; i++)
            reel.slotImages[i].sprite = current[i].symbolSprite;

        ApplyOffset(reel, 0f, stepDistance); // 내용이 같이 밀렸으므로 끊김 없이 리셋됨
    }

    private IEnumerator Co_Overshoot(ReelSlotGroup reel)
    {
        float stepDistance = GetSlotStepDistance(reel);

        if (stepDistance <= 0f)
            yield break;

        float elapsed = 0f;
        while (elapsed < overshootDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin((elapsed / overshootDuration) * Mathf.PI * 0.5f);
            ApplyOffset(reel, Mathf.Lerp(0f, -overshootDistance, t), stepDistance);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutBack(elapsed / settleDuration);
            ApplyOffset(reel, Mathf.Lerp(-overshootDistance, 0f, t),stepDistance);
            yield return null;
        }

        ApplyOffset(reel, 0f,stepDistance);
    }

    private void ApplyOffset(
        ReelSlotGroup reel,
        float offset,
        float stepDistance)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector2 pos =
                reel.slotRects[i].anchoredPosition;

            pos.y =
                (2 - i) * stepDistance +
                offset;

            reel.slotRects[i].anchoredPosition =
                pos;
        }
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private Symbol GetRandomDummy()
    {
        return masterDeck[Random.Range(0, masterDeck.Count)];
    }

    private void ShuffleDeck(List<Symbol> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
        }
    }

    private float GetSlotStepDistance(ReelSlotGroup reel)
    {
        if (reel == null ||
            reel.slotRects == null ||
            reel.slotRects.Length < 3 ||
            reel.slotRects[2] == null)
        {
            Debug.LogError(
                "릴의 중앙 슬롯 RectTransform이 연결되지 않았습니다.",
                this
            );

            return 0f;
        }

        float slotHeight = reel.slotRects[2].rect.height;

        return slotHeight + slotSpacing;
    }

}
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ReelSlotGroup
{
    [Tooltip("순서: [0]위2 [1]위1 [2]중앙 [3]아래1 [4]아래2")]
    public RectTransform[] slotRects = new RectTransform[5];
    public Image[] slotImages = new Image[5];
}

public class SlotManager : MonoBehaviour
{
    [Header("덱 데이터")]
    public List<Symbol> masterDeck = new List<Symbol>();

    private List<Symbol> runtimeDeck = new List<Symbol>();

    public List<Symbol> RolledResults { get; private set; } = new List<Symbol>();

    [Header("릴 UI (릴당 5슬롯)")]
    public ReelSlotGroup[] reels;

    [Header("연출 세팅")]
    [SerializeField] private float stepDuration = 0.08f;
    [SerializeField] private float baseRollDuration = 1f;
    [SerializeField] private float reelStopInterval = 0.2f;
    [SerializeField] private float overshootDistance = 40f;
    [SerializeField] private float overshootDuration = 0.12f;
    [SerializeField] private float settleDuration = 0.18f;

    [Header("슬라이드 연출")]
    public SlotPanelSlider panelSlider;

    [Header("슬롯 간격")]
    [SerializeField] private float slotSpacing;

    public bool isRollEnd;

    private void Awake()
    {
        isRollEnd = true;

        ApplySelectedDeck();
        RemoveNullSymbols();
        InitSlotPositions();
    }

    private void ApplySelectedDeck()
    {
        if (RunSelectionData.Instance == null)
        {
            Debug.Log("RunSelectionData가 없으므로 Inspector의 기본 덱을 사용합니다.");
            return;
        }

        if (!RunSelectionData.Instance.HasDeckSelection)
        {
            Debug.Log("선택한 덱이 없으므로 Inspector의 기본 덱을 사용합니다.");
            return;
        }

        SetMasterDeck(RunSelectionData.Instance.CurrentDeck);

        Debug.Log($"선택한 시작 덱 적용 완료. 심볼 수: {masterDeck.Count}");
    }

    public void RefreshDeckFromRunData()
    {
        ApplySelectedDeck();
        RemoveNullSymbols();
    }

    public void SetMasterDeck(IEnumerable<Symbol> symbols)
    {
        masterDeck.Clear();

        if (symbols == null)
            return;

        foreach (Symbol symbol in symbols)
        {
            if (symbol != null)
                masterDeck.Add(symbol);
        }
    }

    private void RemoveNullSymbols()
    {
        masterDeck.RemoveAll(symbol => symbol == null);
    }

    private void InitSlotPositions()
    {
        if (reels == null)
            return;

        foreach (ReelSlotGroup reel in reels)
        {
            float stepDistance = GetSlotStepDistance(reel);

            if (stepDistance <= 0f)
                continue;

            for (int i = 0; i < 5; i++)
            {
                if (reel.slotRects == null || i >= reel.slotRects.Length || reel.slotRects[i] == null)
                    continue;

                Vector2 position = reel.slotRects[i].anchoredPosition;
                position.y = (2 - i) * stepDistance;
                reel.slotRects[i].anchoredPosition = position;

                if (reel.slotImages == null || i >= reel.slotImages.Length || reel.slotImages[i] == null)
                    continue;

                Symbol dummy = GetRandomDummy();

                if (dummy != null)
                    reel.slotImages[i].sprite = dummy.symbolSprite;
            }
        }
    }

    public void RollWrapper()
    {
        if (masterDeck == null || masterDeck.Count == 0)
        {
            Debug.LogError("슬롯에 사용할 심볼 덱이 비어 있습니다.");
            isRollEnd = true;
            return;
        }

        if (reels == null || reels.Length == 0)
        {
            Debug.LogError("슬롯 릴이 연결되지 않았습니다.");
            isRollEnd = true;
            return;
        }

        StopAllCoroutines();
        StartCoroutine(Roll());
    }

    private IEnumerator Roll()
    {
        isRollEnd = false;
        RolledResults.Clear();

        runtimeDeck = new List<Symbol>(masterDeck);
        runtimeDeck.RemoveAll(symbol => symbol == null);
        ShuffleDeck(runtimeDeck);

        int maxSlots = Mathf.Min(5, reels.Length, runtimeDeck.Count);

        if (maxSlots <= 0)
        {
            Debug.LogError("굴릴 수 있는 심볼이 없습니다.");
            isRollEnd = true;
            yield break;
        }

        for (int i = 0; i < maxSlots; i++)
            RolledResults.Add(runtimeDeck[i]);

        for (int i = 0; i < maxSlots; i++)
            StartCoroutine(Co_RollReel(i, RolledResults[i]));

        float lastReelDuration = baseRollDuration + (maxSlots - 1) * reelStopInterval;
        float totalWait = lastReelDuration + overshootDuration + settleDuration + 0.1f;

        yield return new WaitForSeconds(totalWait);

        isRollEnd = true;
    }

    private IEnumerator Co_RollReel(int reelIndex, Symbol answer)
    {
        ReelSlotGroup reel = reels[reelIndex];

        float reelDuration = baseRollDuration + reelIndex * reelStopInterval;
        int totalSteps = Mathf.Max(5, Mathf.RoundToInt(reelDuration / stepDuration));
        int dummyCount = totalSteps - 5;

        Queue<Symbol> queue = new Queue<Symbol>();

        for (int i = 0; i < dummyCount; i++)
            queue.Enqueue(GetRandomDummy());

        queue.Enqueue(GetRandomDummy());
        queue.Enqueue(GetRandomDummy());
        queue.Enqueue(answer);
        queue.Enqueue(GetRandomDummy());
        queue.Enqueue(GetRandomDummy());

        Symbol[] current = new Symbol[5];

        for (int i = 0; i < 5; i++)
        {
            current[i] = GetRandomDummy();

            if (current[i] != null && reel.slotImages[i] != null)
                reel.slotImages[i].sprite = current[i].symbolSprite;
        }

        while (queue.Count > 0)
            yield return Co_Step(reel, current, queue.Dequeue());

        yield return Co_Overshoot(reel);
    }

    private IEnumerator Co_Step(ReelSlotGroup reel, Symbol[] current, Symbol next)
    {
        float stepDistance = GetSlotStepDistance(reel);

        if (stepDistance <= 0f)
            yield break;

        float elapsed = 0f;

        while (elapsed < stepDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / stepDuration;
            float offset = Mathf.Lerp(0f, -stepDistance, t);

            ApplyOffset(reel, offset, stepDistance);

            yield return null;
        }

        for (int i = 4; i > 0; i--)
            current[i] = current[i - 1];

        current[0] = next;

        for (int i = 0; i < 5; i++)
        {
            if (current[i] != null && reel.slotImages[i] != null)
                reel.slotImages[i].sprite = current[i].symbolSprite;
        }

        ApplyOffset(reel, 0f, stepDistance);
    }

    private IEnumerator Co_Overshoot(ReelSlotGroup reel)
    {
        float stepDistance = GetSlotStepDistance(reel);

        if (stepDistance <= 0f)
            yield break;

        float elapsed = 0f;

        while (elapsed < overshootDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Sin((elapsed / overshootDuration) * Mathf.PI * 0.5f);

            ApplyOffset(reel, Mathf.Lerp(0f, -overshootDistance, t), stepDistance);

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;

            float t = EaseOutBack(elapsed / settleDuration);

            ApplyOffset(reel, Mathf.Lerp(-overshootDistance, 0f, t), stepDistance);

            yield return null;
        }

        ApplyOffset(reel, 0f, stepDistance);
    }

    private void ApplyOffset(ReelSlotGroup reel, float offset, float stepDistance)
    {
        for (int i = 0; i < 5; i++)
        {
            if (reel.slotRects == null || i >= reel.slotRects.Length || reel.slotRects[i] == null)
                continue;

            Vector2 position = reel.slotRects[i].anchoredPosition;
            position.y = (2 - i) * stepDistance + offset;
            reel.slotRects[i].anchoredPosition = position;
        }
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private Symbol GetRandomDummy()
    {
        if (masterDeck == null || masterDeck.Count == 0)
            return null;

        return masterDeck[Random.Range(0, masterDeck.Count)];
    }

    private void ShuffleDeck(List<Symbol> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
        }
    }

    private float GetSlotStepDistance(ReelSlotGroup reel)
    {
        if (reel == null || reel.slotRects == null || reel.slotRects.Length < 3 || reel.slotRects[2] == null)
        {
            Debug.LogError("릴의 중앙 슬롯 RectTransform이 연결되지 않았습니다.", this);
            return 0f;
        }

        float slotHeight = reel.slotRects[2].rect.height;

        return slotHeight + slotSpacing;
    }
}