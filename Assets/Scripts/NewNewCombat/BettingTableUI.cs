using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BettingTableUI :
    MonoBehaviour,
    IPointerClickHandler
{
    private enum BetTargetType
    {
        None,
        Tag,
        Duplicate
    }

    private struct BetTarget
    {
        public BetTargetType type;
        public SymbolTagType tagType;
        public int duplicateCount;
    }


    // =========================================================
    // 기본 연결
    // =========================================================

    [Header("베팅판")]
    [SerializeField]
    private RectTransform bettingTableRect;

    [SerializeField]
    private RectTransform coinLayer;


    [Header("베팅")]
    [SerializeField]
    private BettingManager bettingManager;


    [Header("플레이어")]
    [SerializeField]
    private NewPlayer player;


    [Header("라운드 흐름")]
    [SerializeField]
    private SlotRoundFlowController roundFlowController;


    [Header("코인 프리팹")]
    [SerializeField]
    private BetChipUI chipPrefab;


    // =========================================================
    // 현재 선택 코인
    // =========================================================

    [Header("현재 선택 코인")]
    [SerializeField]
    private Image selectedCoinIcon;

    [SerializeField]
    private TMP_Text selectedCoinNameText;

    [SerializeField]
    private TMP_Text selectedCoinWeightText;


    // =========================================================
    // 중복 베팅 영역
    // =========================================================

    [Header("중복 베팅 영역 - 반지름")]
    [SerializeField, Min(0f)]
    private float duplicateInnerRadius = 100f;

    [SerializeField, Min(0f)]
    private float duplicateOuterRadius = 215f;


    [Header("중복 베팅 영역 - 각도")]
    [Tooltip(
        "1중복 영역의 중앙 각도입니다.\n" +
        "위 = 0 / 오른쪽 = 90 / 아래 = 180 / 왼쪽 = 270"
    )]
    [SerializeField]
    private float duplicateStartAngle = 0f;


    [SerializeField]
    private int[] duplicateSectorValues =
    {
        1,
        2,
        3,
        4,
        5,
        6
    };


    // =========================================================
    // 태그 베팅 영역
    // =========================================================

    [Header("태그 베팅 영역 - 반지름")]
    [SerializeField, Min(0f)]
    private float tagInnerRadius = 220f;

    [SerializeField, Min(0f)]
    private float tagOuterRadius = 380f;


    [Header("태그 베팅 영역 - 각도")]
    [Tooltip(
        "첫 번째 태그 영역의 중앙 각도입니다.\n" +
        "위 = 0 / 오른쪽 = 90 / 아래 = 180 / 왼쪽 = 270"
    )]
    [SerializeField]
    private float tagStartAngle = 0f;


    [SerializeField]
    private SymbolTagType[] tagSectorOrder =
    {
        SymbolTagType.Mercury,
        SymbolTagType.Venus,
        SymbolTagType.Earth,
        SymbolTagType.Mars,
        SymbolTagType.Jupiter,
        SymbolTagType.Saturn,
        SymbolTagType.Uranus,
        SymbolTagType.Neptune,
        SymbolTagType.Pluto
    };


    // =========================================================
    // 디버그
    // =========================================================

    [Header("디버그")]
    [SerializeField]
    private bool showDebugLog = true;


    // =========================================================
    // 내부 상태
    // =========================================================

    private BetCoinData selectedCoin;


    private readonly List<BetChipUI>
        spawnedChips =
            new List<BetChipUI>();


    public BetCoinData SelectedCoin =>
        selectedCoin;


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (bettingTableRect == null)
        {
            bettingTableRect =
                transform as RectTransform;
        }


        RefreshSelectedCoinUI();
    }


    private void OnEnable()
    {
        if (roundFlowController != null)
        {
            roundFlowController.OnRoundReset -=
                HandleRoundReset;

            roundFlowController.OnRoundReset +=
                HandleRoundReset;
        }
    }


    private void OnDisable()
    {
        if (roundFlowController != null)
        {
            roundFlowController.OnRoundReset -=
                HandleRoundReset;
        }
    }


    // =========================================================
    // 코인 선택
    // =========================================================

    public void SetSelectedCoin(
        BetCoinData coin
    )
    {
        if (roundFlowController != null &&
            !roundFlowController.CanChooseBetCoin)
        {
            return;
        }


        selectedCoin =
            coin;


        RefreshSelectedCoinUI();
    }


    public void ClearSelectedCoin()
    {
        selectedCoin =
            null;


        RefreshSelectedCoinUI();
    }


    private void RefreshSelectedCoinUI()
    {
        if (selectedCoinIcon != null)
        {
            if (selectedCoin != null &&
                selectedCoin.Icon != null)
            {
                selectedCoinIcon.sprite =
                    selectedCoin.Icon;

                selectedCoinIcon.enabled =
                    true;

                selectedCoinIcon.preserveAspect =
                    true;
            }
            else
            {
                selectedCoinIcon.sprite =
                    null;

                selectedCoinIcon.enabled =
                    false;
            }
        }


        if (selectedCoinNameText != null)
        {
            selectedCoinNameText.text =
                selectedCoin != null
                    ? selectedCoin.DisplayName
                    : "";
        }


        if (selectedCoinWeightText != null)
        {
            selectedCoinWeightText.text =
                selectedCoin != null
                    ? $"+{selectedCoin.Weight}"
                    : "";
        }
    }


    // =========================================================
    // 베팅판 클릭
    // =========================================================

    public void OnPointerClick(
        PointerEventData eventData
    )
    {
        // =====================================================
        // 라운드 상태 확인
        // =====================================================

        if (roundFlowController == null)
        {
            Debug.LogError(
                "SlotRoundFlowController가 연결되지 않았습니다.",
                this
            );

            return;
        }


        if (!roundFlowController.CanPlaceBet)
        {
            if (showDebugLog)
            {
                Debug.Log(
                    "현재는 베팅할 수 없는 상태입니다.",
                    this
                );
            }

            return;
        }


        // =====================================================
        // 코인 선택 확인
        // =====================================================

        if (selectedCoin == null)
        {
            if (showDebugLog)
            {
                Debug.Log(
                    "먼저 코인을 선택하세요.",
                    this
                );
            }

            return;
        }


        if (bettingManager == null)
        {
            Debug.LogError(
                "BettingManager가 연결되지 않았습니다.",
                this
            );

            return;
        }


        if (player == null)
        {
            Debug.LogError(
                "NewPlayer가 연결되지 않았습니다.",
                this
            );

            return;
        }


        // =====================================================
        // 체력 확인
        // =====================================================

        int healthCost =
            selectedCoin.HealthCost;


        if (player.CurrentHealth <
            healthCost)
        {
            Debug.Log(
                $"체력이 부족합니다. " +
                $"현재 체력={player.CurrentHealth}, " +
                $"필요 체력={healthCost}",
                this
            );

            return;
        }


        // =====================================================
        // 클릭 위치 변환
        // =====================================================

        if (bettingTableRect == null)
            return;


        bool converted =
            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    bettingTableRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint
                );


        if (!converted)
            return;


        Vector2 tableCenter =
            GetTableCenterLocal();


        Vector2 fromCenter =
            localPoint -
            tableCenter;


        // =====================================================
        // 정확히 하나의 베팅 영역 판정
        // =====================================================

        if (!TryGetBetTarget(
                fromCenter,
                out BetTarget target))
        {
            if (showDebugLog)
            {
                Debug.Log(
                    $"베팅 불가능 영역 클릭 | " +
                    $"거리={fromCenter.magnitude:F1}",
                    this
                );
            }


            return;
        }


        BetCoinData usedCoin =
            selectedCoin;


        // =====================================================
        // 베팅 등록
        // =====================================================

        bool success =
            PlaceBet(
                target,
                usedCoin
            );


        if (!success)
        {
            if (showDebugLog)
            {
                Debug.Log(
                    "베팅 등록에 실패했습니다.",
                    this
                );
            }


            return;
        }


        // =====================================================
        // 베팅 성공 시 체력 비용 지불
        // =====================================================

        if (usedCoin.HealthCost > 0)
        {
            player.TakeDamage(
                usedCoin.HealthCost,
                "베팅 비용"
            );


            if (showDebugLog)
            {
                Debug.Log(
                    $"[베팅 비용] " +
                    $"{usedCoin.DisplayName} | " +
                    $"-{usedCoin.HealthCost} HP | " +
                    $"현재 HP={player.CurrentHealth}",
                    this
                );
            }
        }


        // =====================================================
        // 코인 표시
        // =====================================================

        SpawnChip(
            eventData.position,
            eventData.pressEventCamera,
            usedCoin
        );


        // 배치 후 현재 선택 코인 해제
        ClearSelectedCoin();


        // =====================================================
        // 슬롯 진행
        // =====================================================

        roundFlowController
            .HandleBetPlaced();
    }


    // =========================================================
    // 베팅 등록
    // =========================================================

    private bool PlaceBet(
        BetTarget target,
        BetCoinData coin
    )
    {
        if (coin == null)
            return false;


        switch (target.type)
        {
            // =================================================
            // 태그 예측
            // =================================================

            case BetTargetType.Tag:

                if (showDebugLog)
                {
                    Debug.Log(
                        $"[태그 베팅] " +
                        $"{target.tagType} | " +
                        $"{coin.DisplayName}",
                        this
                    );
                }


                return bettingManager
                    .PlaceTagBet(
                        target.tagType,
                        coin
                    );


            // =================================================
            // 중복 예측
            // =================================================

            case BetTargetType.Duplicate:

                if (showDebugLog)
                {
                    Debug.Log(
                        $"[중복 베팅] " +
                        $"{target.duplicateCount}중복 이상 | " +
                        $"{coin.DisplayName}",
                        this
                    );
                }


                return bettingManager
                    .PlaceDuplicateBet(
                        target.duplicateCount,
                        coin
                    );
        }


        return false;
    }


    // =========================================================
    // 베팅 영역 판정
    // =========================================================

    private bool TryGetBetTarget(
        Vector2 fromCenter,
        out BetTarget target
    )
    {
        target =
            new BetTarget
            {
                type =
                    BetTargetType.None
            };


        float distance =
            fromCenter.magnitude;


        // =====================================================
        // 중복 영역
        //
        // Outer는 포함하지 않는다.
        //
        // 예:
        // 100 <= 거리 < 215
        //
        // 따라서 215~220 같은 빈 공간을 만들 수 있다.
        // =====================================================

        if (distance >= duplicateInnerRadius &&
            distance < duplicateOuterRadius)
        {
            if (duplicateSectorValues == null ||
                duplicateSectorValues.Length == 0)
            {
                return false;
            }


            int sectorIndex =
                GetSectorIndex(
                    fromCenter,
                    duplicateSectorValues.Length,
                    duplicateStartAngle
                );


            if (sectorIndex < 0 ||
                sectorIndex >=
                duplicateSectorValues.Length)
            {
                return false;
            }


            target.type =
                BetTargetType.Duplicate;


            target.duplicateCount =
                duplicateSectorValues[
                    sectorIndex
                ];


            if (showDebugLog)
            {
                Debug.Log(
                    $"[중복 영역 판정] " +
                    $"Sector={sectorIndex} | " +
                    $"조건={target.duplicateCount}중복 | " +
                    $"거리={distance:F1} | " +
                    $"각도={GetAngle(fromCenter):F1}",
                    this
                );
            }


            return true;
        }


        // =====================================================
        // 태그 영역
        //
        // 마찬가지로 Outer는 포함하지 않는다.
        //
        // 예:
        // 220 <= 거리 < 380
        // =====================================================

        if (distance >= tagInnerRadius &&
            distance < tagOuterRadius)
        {
            if (tagSectorOrder == null ||
                tagSectorOrder.Length == 0)
            {
                return false;
            }


            int sectorIndex =
                GetSectorIndex(
                    fromCenter,
                    tagSectorOrder.Length,
                    tagStartAngle
                );


            if (sectorIndex < 0 ||
                sectorIndex >=
                tagSectorOrder.Length)
            {
                return false;
            }


            target.type =
                BetTargetType.Tag;


            target.tagType =
                tagSectorOrder[
                    sectorIndex
                ];


            if (showDebugLog)
            {
                Debug.Log(
                    $"[태그 영역 판정] " +
                    $"Sector={sectorIndex} | " +
                    $"Tag={target.tagType} | " +
                    $"거리={distance:F1} | " +
                    $"각도={GetAngle(fromCenter):F1}",
                    this
                );
            }


            return true;
        }


        // 어디에도 속하지 않음
        return false;
    }


    // =========================================================
    // 원형 영역 Sector 계산
    //
    // startAngle은 첫 번째 Sector의 "중앙"
    //
    // 위쪽 = 0도
    // 오른쪽 = 90도
    // 아래쪽 = 180도
    // 왼쪽 = 270도
    // =========================================================

    private int GetSectorIndex(
    Vector2 fromCenter,
    int sectorCount,
    float startAngle
)
    {
        if (sectorCount <= 0)
            return 0;


        float angle =
            GetAngle(fromCenter);


        float sectorSize =
            360f / sectorCount;


        // startAngle은 첫 번째 구역의 "시작 각도"
        float adjustedAngle =
            Mathf.Repeat(
                angle - startAngle,
                360f
            );


        int sectorIndex =
            Mathf.FloorToInt(
                adjustedAngle / sectorSize
            );


        return Mathf.Clamp(
            sectorIndex,
            0,
            sectorCount - 1
        );
    }


    // =========================================================
    // 방향 → 각도
    //
    // 위 = 0
    // 오른쪽 = 90
    // 아래 = 180
    // 왼쪽 = 270
    // =========================================================

    private float GetAngle(
        Vector2 fromCenter
    )
    {
        float angle =
            Mathf.Atan2(
                fromCenter.x,
                fromCenter.y
            ) * Mathf.Rad2Deg;


        return Mathf.Repeat(
            angle,
            360f
        );
    }


    // =========================================================
    // 베팅 코인 생성
    // =========================================================

    private void SpawnChip(
        Vector2 screenPosition,
        Camera eventCamera,
        BetCoinData coin
    )
    {
        if (chipPrefab == null ||
            coinLayer == null ||
            coin == null)
        {
            return;
        }


        bool converted =
            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    coinLayer,
                    screenPosition,
                    eventCamera,
                    out Vector2 localPosition
                );


        if (!converted)
            return;


        BetChipUI chip =
            Instantiate(
                chipPrefab,
                coinLayer,
                false
            );


        chip.gameObject.SetActive(
            true
        );


        chip.Bind(
            coin
        );


        chip.SetPosition(
            localPosition
        );


        chip.transform.SetAsLastSibling();


        spawnedChips.Add(
            chip
        );
    }


    // =========================================================
    // 라운드 초기화
    // =========================================================

    private void HandleRoundReset()
    {
        ResetAfterResult();
    }


    public void ResetAfterResult()
    {
        ClearSelectedCoin();

        ClearBetVisualsOnly();
    }


    private void ClearBetVisualsOnly()
    {
        foreach (
            BetChipUI chip
            in spawnedChips
        )
        {
            if (chip == null)
                continue;


            Destroy(
                chip.gameObject
            );
        }


        spawnedChips.Clear();
    }


    // =========================================================
    // 베팅 취소
    // =========================================================

    public void CancelAllBets()
    {
        if (roundFlowController == null)
            return;


        roundFlowController
            .CancelRound();
    }


    // =========================================================
    // 베팅판 중심
    // =========================================================

    private Vector2 GetTableCenterLocal()
    {
        if (bettingTableRect == null)
            return Vector2.zero;


        Rect rect =
            bettingTableRect.rect;


        Vector2 pivot =
            bettingTableRect.pivot;


        return new Vector2(
            (0.5f - pivot.x) *
            rect.width,

            (0.5f - pivot.y) *
            rect.height
        );
    }


    // =========================================================
    // Inspector 값 검사
    // =========================================================

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (duplicateOuterRadius <
            duplicateInnerRadius)
        {
            duplicateOuterRadius =
                duplicateInnerRadius;
        }


        if (tagOuterRadius <
            tagInnerRadius)
        {
            tagOuterRadius =
                tagInnerRadius;
        }


        /*
         * 중복 영역과 태그 영역이 실제로 겹치는 경우
         * Inspector에서 바로 경고한다.
         */
        if (duplicateOuterRadius >
            tagInnerRadius)
        {
            Debug.LogWarning(
                $"베팅 영역이 겹칩니다. " +
                $"Duplicate Outer={duplicateOuterRadius}, " +
                $"Tag Inner={tagInnerRadius}. " +
                $"Duplicate Outer를 Tag Inner보다 작거나 같게 설정하세요.",
                this
            );
        }
    }

#endif
}