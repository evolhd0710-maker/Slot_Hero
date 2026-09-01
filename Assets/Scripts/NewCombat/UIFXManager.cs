using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIFXManager : MonoBehaviour
{
    public static UIFXManager Instance { get; private set; }

    [Header("프리팹 및 풀링 세팅")]
    [SerializeField] private GameObject flyingTextPrefab;
    [SerializeField] private int poolSize = 10;

    [Header("목적지 UI 위치 (트래킹용 RectTransform)")]
    public RectTransform totalDamageTargetUI;
    public RectTransform totalShieldTargetUI;

    [Header("★ 실제 수치가 반영될 총량 텍스트 컴포넌트")]
    // 에디터 인스펙터 창에서 화면 상단의 진짜 텍스트 오브젝트들을 드래그 앤 드롭 하세요!
    public TMP_Text totalDamageText;
    public TMP_Text totalShieldText;

    private Queue<TMP_Text> textPool = new Queue<TMP_Text>();

    private void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        if (flyingTextPrefab == null) return;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(flyingTextPrefab, this.transform);
            TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                obj.SetActive(false);
                textPool.Enqueue(text);
            }
        }
    }

    private TMP_Text GetPooledText()
    {
        while (textPool.Count > 0)
        {
            TMP_Text text = textPool.Dequeue();
            if (text != null && text.gameObject != null)
            {
                text.gameObject.SetActive(true);
                return text;
            }
        }
        GameObject obj = Instantiate(flyingTextPrefab, this.transform);
        return obj.GetComponentInChildren<TMP_Text>();
    }

    private void ReturnToPool(TMP_Text text)
    {
        if (text == null || text.gameObject == null) return;
        text.gameObject.SetActive(false);
        textPool.Enqueue(text);
    }

    /// <summary>
    /// 숫자를 날려 보내고, 충돌 순간 주입받은 텍스트 컴포넌트의 수치를 동기화하는 핵심 연출 코루틴
    /// </summary>
    public IEnumerator Co_FlyNumber(int value, Vector3 startWorldPos, RectTransform targetUI, Color textColor, TMP_Text totalTextComponent, int targetTotalValue, float duration = 0.4f)
    {
        if (value <= 0) yield break;

        TMP_Text flyingText = GetPooledText();
        if (flyingText == null) yield break;

        flyingText.text = value.ToString();
        flyingText.color = textColor;

        RectTransform textRect = flyingText.GetComponent<RectTransform>();
        textRect.position = startWorldPos;
        textRect.localScale = Vector3.one;

        float elapsedTime = 0f;
        Vector3 startPos = textRect.position;

        // 2차 가속도 비행 연출 진행
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = t * t;

            if (targetUI == null || textRect == null) yield break;

            Vector3 destinationPos = targetUI.position;
            textRect.position = Vector3.Lerp(startPos, destinationPos, t);

            yield return null;
        }

        // 목적지 좌표 강제 보정
        if (textRect != null && targetUI != null) textRect.position = targetUI.position;

        // ======================================================================
        // ★ [현업 핵심 베스트 프랙티스]: 투사체가 목적지에 충돌한 바로 '이 순간'
        // 주입받은 상단 텍스트 컴포넌트의 숫자를 최종 누적값으로 교체합니다!
        // ======================================================================
        if (totalTextComponent != null)
        {
            totalTextComponent.text = targetTotalValue.ToString();
        }

        ReturnToPool(flyingText);
    }
}