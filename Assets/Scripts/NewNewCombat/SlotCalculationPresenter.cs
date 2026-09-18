using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotCalculationPresenter : MonoBehaviour
{
    private const int ReelCount = 6;


    // =========================================================
    // Main Info
    // =========================================================

    [Header("Main Info")]
    [SerializeField]
    private TMP_Text mainTitleText;

    [SerializeField]
    private TMP_Text roundAttackText;


    // =========================================================
    // Sub Info
    // =========================================================

    [Header("Sub Info")]
    [SerializeField]
    private TMP_Text tagNameText;

    [SerializeField]
    private TMP_Text formulaText;


    // =========================================================
    // 릴 하이라이트
    // =========================================================

    [Header("릴 하이라이트")]
    [SerializeField]
    private Image[] reelHighlights =
        new Image[ReelCount];


    [Header("숫자 팝업 위치")]
    [SerializeField]
    private RectTransform[] popupAnchors =
        new RectTransform[ReelCount];


    [Header("숫자 팝업")]
    [SerializeField]
    private CalculationValuePopup popupPrefab;


    // =========================================================
    // 연출 시간
    // =========================================================

    [Header("연출 시간")]

    [SerializeField, Min(0f)]
    private float tagStartDelay = 0.3f;

    [SerializeField, Min(0.01f)]
    private float symbolPulseDuration = 0.25f;

    [SerializeField, Min(0f)]
    private float symbolStepDelay = 0.2f;

    [SerializeField, Min(0f)]
    private float betWeightDelay = 0.5f;

    [SerializeField, Min(0f)]
    private float multiplierDelay = 0.5f;

    [SerializeField, Min(0f)]
    private float resultDelay = 0.6f;

    [SerializeField, Min(0f)]
    private float betweenTagDelay = 0.3f;

    [SerializeField, Min(0f)]
    private float roundAttackCountDuration = 0.3f;

    [SerializeField, Min(0f)]
    private float finalDelay = 0.5f;


    // =========================================================
    // 하이라이트 밝기
    // =========================================================

    [Header("하이라이트")]

    [Range(0f, 1f)]
    [SerializeField]
    private float normalHighlightAlpha = 0.35f;

    [Range(0f, 1f)]
    [SerializeField]
    private float pulseHighlightAlpha = 1f;


    // =========================================================
    // 런타임
    // =========================================================

    private int roundAttack;


    public int RoundAttack =>
        roundAttack;


    private void Awake()
    {
        ClearAllHighlights();

        ClearSubInfo();

        SetRoundAttack(
            0
        );
    }


    // =========================================================
    // 전체 계산 연출
    // =========================================================

    public IEnumerator PlayCalculation(
        SymbolCalculationResult result
    )
    {
        roundAttack =
            0;


        if (mainTitleText != null)
        {
            mainTitleText.text =
                "라운드 공격력";
        }


        SetRoundAttack(
            0
        );


        ClearSubInfo();

        ClearAllHighlights();


        if (result == null)
            yield break;


        foreach (
            TagCalculationResult tagResult
            in result.tagResults)
        {
            if (tagResult == null)
                continue;


            yield return PlayTagCalculation(
                tagResult
            );
        }


        // 최종 값 보정
        roundAttack =
            result.totalDamage;


        SetRoundAttack(
            result.totalDamage
        );


        ClearSubInfo();

        ClearAllHighlights();


        if (finalDelay > 0f)
        {
            yield return new WaitForSeconds(
                finalDelay
            );
        }
    }


    // =========================================================
    // 태그 하나 계산
    // =========================================================

    private IEnumerator PlayTagCalculation(
        TagCalculationResult tagResult
    )
    {
        ClearAllHighlights();


        // =====================================================
        // 1. 이번 태그에 해당하는 문양 전체 하이라이트
        // =====================================================

        foreach (
            SymbolContribution contribution
            in tagResult.contributions)
        {
            if (contribution == null)
                continue;


            SetReelHighlight(
                contribution.reelIndex,
                true,
                normalHighlightAlpha
            );
        }


        if (tagNameText != null)
        {
            tagNameText.text =
                tagResult.tagName;
        }


        if (formulaText != null)
        {
            formulaText.text =
                "";
        }


        if (tagStartDelay > 0f)
        {
            yield return new WaitForSeconds(
                tagStartDelay
            );
        }


        // =====================================================
        // 2. 왼쪽 문양부터 순차 계산
        // =====================================================

        List<string> valueParts =
            new List<string>();


        foreach (
            SymbolContribution contribution
            in tagResult.contributions)
        {
            if (contribution == null)
                continue;


            // 공식에 숫자 추가
            valueParts.Add(
                contribution.value.ToString()
            );


            UpdateFormula(
                string.Join(
                    " + ",
                    valueParts
                )
            );


            // 문양 위 +숫자 팝업
            ShowValuePopup(
                contribution.reelIndex,
                contribution.value
            );


            // 현재 문양 발광
            yield return PulseReel(
                contribution.reelIndex
            );


            if (symbolStepDelay > 0f)
            {
                yield return new WaitForSeconds(
                    symbolStepDelay
                );
            }
        }


        // =====================================================
        // 3. 태그 예측 코인 가중치
        //
        // 예:
        // 3 + 6 + 3
        // →
        // 3 + 6 + 3 + 5
        // =====================================================

        if (tagResult.tagBetWeight > 0)
        {
            valueParts.Add(
                tagResult.tagBetWeight.ToString()
            );


            UpdateFormula(
                string.Join(
                    " + ",
                    valueParts
                )
            );


            if (betWeightDelay > 0f)
            {
                yield return new WaitForSeconds(
                    betWeightDelay
                );
            }
        }


        // =====================================================
        // 4. 기본 중첩 배수
        //
        // (3 + 6 + 3 + 5) × 4
        // =====================================================

        string leftExpression =
            "(" +
            string.Join(
                " + ",
                valueParts
            ) +
            ")";


        string multiplierExpression =
            tagResult.baseMultiplier.ToString();


        UpdateFormula(
            $"{leftExpression} × {multiplierExpression}"
        );


        if (multiplierDelay > 0f)
        {
            yield return new WaitForSeconds(
                multiplierDelay
            );
        }


        // =====================================================
        // 5. 중첩 베팅 코인 가중치
        //
        // ( ... ) × (4 + 10)
        // =====================================================

        if (tagResult.duplicateBetWeight > 0)
        {
            multiplierExpression =
                $"({tagResult.baseMultiplier} + " +
                $"{tagResult.duplicateBetWeight})";


            UpdateFormula(
                $"{leftExpression} × " +
                $"{multiplierExpression}"
            );


            if (betWeightDelay > 0f)
            {
                yield return new WaitForSeconds(
                    betWeightDelay
                );
            }
        }


        // =====================================================
        // 6. 태그 최종 계산값 표시
        // =====================================================

        string completedFormula =
            $"{leftExpression} × " +
            $"{multiplierExpression} = " +
            $"{tagResult.finalScore}";


        UpdateFormula(
            completedFormula
        );


        if (resultDelay > 0f)
        {
            yield return new WaitForSeconds(
                resultDelay
            );
        }


        // =====================================================
        // 7. MainInfo의 라운드 공격력 증가
        // =====================================================

        int previousAttack =
            roundAttack;


        int nextAttack =
            roundAttack +
            tagResult.finalScore;


        yield return AnimateRoundAttack(
            previousAttack,
            nextAttack
        );


        roundAttack =
            nextAttack;


        SetRoundAttack(
            roundAttack
        );


        // =====================================================
        // 8. 현재 태그 계산 종료
        // =====================================================

        ClearAllHighlights();


        if (betweenTagDelay > 0f)
        {
            yield return new WaitForSeconds(
                betweenTagDelay
            );
        }
    }


    // =========================================================
    // 공격력 숫자 증가 연출
    // =========================================================

    private IEnumerator AnimateRoundAttack(
        int from,
        int to
    )
    {
        if (roundAttackCountDuration <= 0f)
        {
            SetRoundAttack(
                to
            );

            yield break;
        }


        float elapsed =
            0f;


        while (elapsed <
               roundAttackCountDuration)
        {
            elapsed +=
                Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    roundAttackCountDuration
                );


            int current =
                Mathf.RoundToInt(
                    Mathf.Lerp(
                        from,
                        to,
                        t
                    )
                );


            SetRoundAttack(
                current
            );


            yield return null;
        }


        SetRoundAttack(
            to
        );
    }


    // =========================================================
    // 개별 릴 발광
    // =========================================================

    private IEnumerator PulseReel(
        int reelIndex
    )
    {
        if (!IsValidReelIndex(
                reelIndex))
        {
            yield break;
        }


        Image highlight =
            reelHighlights[
                reelIndex
            ];


        if (highlight == null)
            yield break;


        highlight.enabled =
            true;


        float halfDuration =
            symbolPulseDuration *
            0.5f;


        if (halfDuration <= 0f)
        {
            SetImageAlpha(
                highlight,
                normalHighlightAlpha
            );

            yield break;
        }


        // 밝아짐
        float elapsed =
            0f;


        while (elapsed <
               halfDuration)
        {
            elapsed +=
                Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    halfDuration
                );


            float alpha =
                Mathf.Lerp(
                    normalHighlightAlpha,
                    pulseHighlightAlpha,
                    t
                );


            SetImageAlpha(
                highlight,
                alpha
            );


            yield return null;
        }


        // 다시 기본 하이라이트로
        elapsed =
            0f;


        while (elapsed <
               halfDuration)
        {
            elapsed +=
                Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    halfDuration
                );


            float alpha =
                Mathf.Lerp(
                    pulseHighlightAlpha,
                    normalHighlightAlpha,
                    t
                );


            SetImageAlpha(
                highlight,
                alpha
            );


            yield return null;
        }


        SetImageAlpha(
            highlight,
            normalHighlightAlpha
        );
    }


    // =========================================================
    // +Value 팝업
    // =========================================================

    private void ShowValuePopup(
     int reelIndex,
     int value
 )
    {
        Debug.Log(
            $"[팝업 생성 요청] Reel={reelIndex}, Value=+{value}",
            this
        );

        if (popupPrefab == null)
        {
            Debug.LogError(
                "[팝업 생성 실패] Popup Prefab이 연결되지 않았습니다.",
                this
            );

            return;
        }

        if (popupAnchors == null)
        {
            Debug.LogError(
                "[팝업 생성 실패] Popup Anchors 배열이 null입니다.",
                this
            );

            return;
        }

        if (reelIndex < 0 ||
            reelIndex >= popupAnchors.Length)
        {
            Debug.LogError(
                $"[팝업 생성 실패] Reel Index 범위 오류. " +
                $"Index={reelIndex}, " +
                $"PopupAnchors Length={popupAnchors.Length}",
                this
            );

            return;
        }

        RectTransform anchor =
            popupAnchors[reelIndex];

        if (anchor == null)
        {
            Debug.LogError(
                $"[팝업 생성 실패] PopupAnchors[{reelIndex}]가 비어 있습니다.",
                this
            );

            return;
        }

        CalculationValuePopup popup =
            Instantiate(
                popupPrefab,
                anchor
            );

        if (popup == null)
        {
            Debug.LogError(
                $"[팝업 생성 실패] Reel {reelIndex}에서 Instantiate 실패",
                this
            );

            return;
        }

        // 혹시 프리팹 자체가 비활성화되어 있어도 켜준다.
        popup.gameObject.SetActive(true);

        RectTransform popupRect =
            popup.GetComponent<RectTransform>();

        if (popupRect == null)
        {
            Debug.LogError(
                "CalculationValuePopup 프리팹에 RectTransform이 없습니다.",
                popup
            );

            Destroy(popup.gameObject);

            return;
        }

        // 반드시 PopupAnchor 기준 중앙에 생성
        popupRect.SetParent(
            anchor,
            false
        );

        popupRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        popupRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        popupRect.pivot =
            new Vector2(0.5f, 0.5f);

        popupRect.anchoredPosition =
            Vector2.zero;

        popupRect.localScale =
            Vector3.one;

        popupRect.localRotation =
            Quaternion.identity;

        popup.transform.SetAsLastSibling();

        Debug.Log(
            $"[팝업 생성 성공] " +
            $"Reel={reelIndex}, " +
            $"Parent={popup.transform.parent.name}, " +
            $"Value=+{value}",
            popup
        );

        popup.Play(value);
    }


    // =========================================================
    // 하이라이트
    // =========================================================

    private void SetReelHighlight(
        int reelIndex,
        bool active,
        float alpha
    )
    {
        if (!IsValidReelIndex(
                reelIndex))
        {
            return;
        }


        Image image =
            reelHighlights[
                reelIndex
            ];


        if (image == null)
            return;


        image.enabled =
            active;


        if (active)
        {
            SetImageAlpha(
                image,
                alpha
            );
        }
    }


    private void ClearAllHighlights()
    {
        if (reelHighlights == null)
            return;


        foreach (
            Image highlight
            in reelHighlights)
        {
            if (highlight == null)
                continue;


            highlight.enabled =
                false;
        }
    }


    private void SetImageAlpha(
        Image image,
        float alpha
    )
    {
        if (image == null)
            return;


        Color color =
            image.color;


        color.a =
            alpha;


        image.color =
            color;
    }


    private bool IsValidReelIndex(
        int reelIndex
    )
    {
        if (reelHighlights == null)
            return false;


        return
            reelIndex >= 0 &&
            reelIndex <
                reelHighlights.Length;
    }


    // =========================================================
    // 텍스트
    // =========================================================

    private void UpdateFormula(
        string formula
    )
    {
        if (formulaText != null)
        {
            formulaText.text =
                formula;
        }
    }


    private void SetRoundAttack(
        int value
    )
    {
        if (roundAttackText != null)
        {
            roundAttackText.text =
                value.ToString();
        }
    }


    private void ClearSubInfo()
    {
        if (tagNameText != null)
        {
            tagNameText.text =
                "";
        }


        if (formulaText != null)
        {
            formulaText.text =
                "";
        }
    }
}