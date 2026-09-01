using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBarController : MonoBehaviour
{
    [Header("추적할 대상 유닛 (Player 혹은 Enemy)")]
    [SerializeField] private NewUnitBase targetUnit;

    [Header("UI 컴포넌트 연결")]
    [SerializeField] private Slider hpSlider;

    // ★ [기획 반영]: 슬라이더 대신 체력바 옆에 배치할 TMP 텍스트 컴포넌트 선언
    [SerializeField] private TMP_Text shieldText;

    private void Start()
    {
        if (targetUnit != null)
        {
            // 유닛 변동 이벤트 파이프라인 구독 시작
            targetUnit.OnHpChanged += UpdateHpSlider;
            targetUnit.OnShieldChanged += UpdateShieldUI; // 텍스트 전용 갱신 함수 매핑

            // 게임 시작 시 최초 1회 화면 데이터 동기화
            UpdateHpSlider(targetUnit.CurrentHealth, targetUnit.data.maxHealth);
            UpdateShieldUI(targetUnit.CurrentShield);
        }
    }

    private void UpdateHpSlider(int currentHp, int maxHp)
    {
        if (hpSlider == null) return;
        hpSlider.value = (float)currentHp / maxHp;
    }

    /// <summary>
    /// ★ 핵심 API: 실드 수치가 바뀔 때마다 호출되어 텍스트와 활성화 상태를 제어하는 함수
    /// </summary>
    private void UpdateShieldUI(int currentShield)
    {
        if (shieldText == null) return;

        // [현업 UI 팁]: 실드가 0일 때도 '+0' 혹은 '0'이라고 계속 떠 있으면 UI 공간이 지저분해집니다.
        // 실드가 0보다 클 때만 텍스트 오브젝트를 활성화하여 플레이어가 버프 상태를 극명하게 인지하도록 만듭니다.
        if (currentShield > 0)
        {
            shieldText.gameObject.SetActive(true);

            // 문자열 보간을 이용해 "+15" 형태로 가독성 있게 표현합니다.
            shieldText.text = $"+{currentShield}";
        }
        else
        {
            // 실드가 소멸하거나 0이 되면 텍스트 상자 자체의 불을 끕니다.
            shieldText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // 오브젝트 소멸 시 이벤트 링크 안전하게 해제하여 메모리 누수 봉쇄
        if (targetUnit != null)
        {
            targetUnit.OnHpChanged -= UpdateHpSlider;
            targetUnit.OnShieldChanged -= UpdateShieldUI;
        }
    }
}
