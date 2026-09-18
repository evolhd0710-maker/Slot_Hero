using TMPro;
using UnityEngine;

public class UnitHealthUI : MonoBehaviour
{
    [Header("대상")]
    [SerializeField]
    private NewUnitBase targetUnit;


    [Header("체력 텍스트")]
    [SerializeField]
    private TMP_Text healthText;


    [Header("표시 설정")]
    [SerializeField]
    private bool showMaxHealth = true;

    [SerializeField]
    private bool showPredictedHealth = true;


    // =========================================================
    // 내부 상태
    // =========================================================

    private int lastHealth = -1;
    private int lastMaxHealth = -1;

    private bool isShowingPrediction;

    private int predictedHealth;
    private int predictedDamage;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        Refresh();
    }


    private void Update()
    {
        if (targetUnit == null)
            return;


        int currentHealth =
            targetUnit.CurrentHealth;

        int maxHealth =
            targetUnit.data.maxHealth;


        // 실제 체력이 변했을 때만 갱신
        if (currentHealth != lastHealth ||
            maxHealth != lastMaxHealth)
        {
            /*
             * 실제 피해가 적용된 순간에는
             * 예상 피해 표시를 종료한다.
             */
            isShowingPrediction = false;

            Refresh();
        }
    }


    // =========================================================
    // 대상 설정
    // =========================================================

    public void SetTarget(
        NewUnitBase unit
    )
    {
        targetUnit =
            unit;


        lastHealth =
            -1;

        lastMaxHealth =
            -1;


        isShowingPrediction =
            false;

        predictedHealth =
            0;

        predictedDamage =
            0;


        Refresh();
    }


    // =========================================================
    // 일반 갱신
    // =========================================================

    public void Refresh()
    {
        if (targetUnit == null)
        {
            ClearUI();
            return;
        }


        int currentHealth =
            targetUnit.CurrentHealth;

        int maxHealth =
            targetUnit.data.maxHealth;


        lastHealth =
            currentHealth;

        lastMaxHealth =
            maxHealth;


        RefreshText(
            currentHealth,
            maxHealth
        );
    }


    // =========================================================
    // 예상 피해 표시
    // =========================================================

    public void ShowPredictedDamage(
        int damage
    )
    {
        if (targetUnit == null)
            return;


        int currentHealth =
            targetUnit.CurrentHealth;

        int maxHealth =
            targetUnit.data.maxHealth;


        predictedDamage =
            Mathf.Max(
                0,
                damage
            );


        predictedHealth =
            Mathf.Clamp(
                currentHealth - predictedDamage,
                0,
                maxHealth
            );


        isShowingPrediction =
            true;


        RefreshText(
            currentHealth,
            maxHealth
        );
    }


    // =========================================================
    // 예상 피해 제거
    // =========================================================

    public void ClearPredictedDamage()
    {
        isShowingPrediction =
            false;

        predictedDamage =
            0;


        Refresh();
    }


    // =========================================================
    // 텍스트
    // =========================================================

    private void RefreshText(
        int currentHealth,
        int maxHealth
    )
    {
        if (healthText == null)
            return;


        // =====================================================
        // 예상 피해 표시 중
        // =====================================================

        if (isShowingPrediction &&
            showPredictedHealth)
        {
            if (showMaxHealth)
            {
                healthText.text =
                    $"{currentHealth} → {predictedHealth} / {maxHealth}";
            }
            else
            {
                healthText.text =
                    $"{currentHealth} → {predictedHealth}";
            }


            return;
        }


        // =====================================================
        // 평상시
        // =====================================================

        if (showMaxHealth)
        {
            healthText.text =
                $"{currentHealth} / {maxHealth}";
        }
        else
        {
            healthText.text =
                currentHealth.ToString();
        }
    }


    // =========================================================
    // 초기화
    // =========================================================

    private void ClearUI()
    {
        lastHealth =
            -1;

        lastMaxHealth =
            -1;

        isShowingPrediction =
            false;

        predictedHealth =
            0;

        predictedDamage =
            0;


        if (healthText != null)
        {
            healthText.text =
                "";
        }
    }
}