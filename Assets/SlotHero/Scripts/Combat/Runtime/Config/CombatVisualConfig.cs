using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.Combat
{
    /// <summary>
    /// 전투 화면의 색과 글자와 연출 시간.
    ///
    /// 전투 UI 및 연출 초안은 회색 와이어프레임뿐이라 색은 Jackpot 아트웍의 어두운 빈티지 톤에서 골랐다.
    /// 태그 색과 아이콘은 현재 빌드 화면 설정(`CurrentBuildVisualConfig`)을 함께 쓴다. 초안 06 의 "태그별 색상 활용" 이다.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatVisualConfig", menuName = "Slot Hero/전투/표시 설정")]
    public class CombatVisualConfig : ScriptableObject
    {
        [Header("칸 색")]
        [Tooltip("릴, 유물, 정보 칸의 바탕.")]
        public Color PanelColor = new Color(0.13f, 0.12f, 0.11f, 0.92f);

        [Tooltip("빈 칸. 유물이 없는 자리나 꺼낸 코인이 없는 자리다.")]
        public Color EmptyColor = new Color(0.22f, 0.21f, 0.2f, 0.7f);

        [Tooltip("몬스터 일러스트가 아직 없어 그 자리에 까는 색.")]
        public Color EnemyArtStandIn = new Color(0.32f, 0.2f, 0.17f, 1f);

        [Tooltip("체력 막대 바탕. 피그마 체력 틀 안쪽처럼 거의 검다.")]
        public Color BarBackColor = new Color(0.07f, 0.065f, 0.06f, 1f);

        [Tooltip("체력 막대. 피그마는 주인공과 몬스터 모두 #DC5143 으로 칠했다.")]
        public Color PlayerBarColor = new Color(0.863f, 0.318f, 0.263f, 1f);
        public Color EnemyBarColor = new Color(0.863f, 0.318f, 0.263f, 1f);

        [Tooltip("체력 숫자의 \"/최대\" 부분. 피그마는 지금 체력을 희게, 최대 체력을 회색으로 적었다.")]
        [SerializeField] private Color _healthMaxColor = new Color(0.62f, 0.6f, 0.58f, 1f);
        public Color HealthMaxColor { get { return _healthMaxColor; } set { _healthMaxColor = value; } }

        [Tooltip("릴 틀 안쪽 바탕. 피그마는 검게 비워 두었다.")]
        [SerializeField] private Color _reelBackColor = new Color(0.02f, 0.02f, 0.024f, 0.92f);
        public Color ReelBackColor { get { return _reelBackColor; } set { _reelBackColor = value; } }

        [Tooltip("유물 칸 틀 색. 유물 등급 차례(일반, 고급, 희귀, 특급, 전설)로 초록, 파랑, 노랑, 보라, 빨강이다(2026년 10월 26일 원재). " +
                 "흰 틀 그림에 곱한다. 값은 피그마 유물 칸 틀의 색에서 땄다.")]
        [SerializeField] private Color[] _relicGradeColors =
        {
            new Color(0.299f, 0.449f, 0.347f, 1f),
            new Color(0.237f, 0.434f, 0.64f, 1f),
            new Color(0.669f, 0.529f, 0.262f, 1f),
            new Color(0.48f, 0.296f, 0.541f, 1f),
            new Color(0.6f, 0.254f, 0.213f, 1f),
        };
        public Color[] RelicGradeColors { get { return _relicGradeColors; } set { _relicGradeColors = value; } }

        [Tooltip("유물이 없는 칸의 틀 색. 회색이다.")]
        [SerializeField] private Color _relicEmptyFrameColor = new Color(0.42f, 0.42f, 0.42f, 1f);
        public Color RelicEmptyFrameColor { get { return _relicEmptyFrameColor; } set { _relicEmptyFrameColor = value; } }

        [Tooltip("유물 목록 바탕. 피그마는 칸 틀만 세로로 늘어놓아 투명하게 둔다.")]
        [SerializeField] private Color _relicPanelColor = Color.clear;
        public Color RelicPanelColor { get { return _relicPanelColor; } set { _relicPanelColor = value; } }

        [Tooltip("태그 원판 테두리 안쪽의 어두운 원.")]
        [SerializeField] private Color _tagWheelBackColor = new Color(0.02f, 0.02f, 0.02f, 1f);
        public Color TagWheelBackColor { get { return _tagWheelBackColor; } set { _tagWheelBackColor = value; } }

        [Tooltip("공격할 태그를 고른 뒤 그 칸을 태그 색으로 채우는 진하기. 초안 06 의 \"공격 예정\".")]
        [Range(0f, 1f)]
        [SerializeField] private float _tagChargeAlpha = 0.45f;
        public float TagChargeAlpha { get { return _tagChargeAlpha; } set { _tagChargeAlpha = value; } }

        [Tooltip("영웅의 공격을 받을 소환수 칸. 방패병이 장군 대신 맞는다는 것을 보인다.")]
        public Color TargetColor = new Color(0.42f, 0.24f, 0.18f, 0.95f);

        [Tooltip("방어도 숫자. 몬스터 기획서의 [방어] 분류 색 쪽이다.")]
        public Color BlockColor = new Color(0.55f, 0.72f, 0.92f, 1f);

        [Tooltip("버프(기세, 투창 명령). [버프] 분류 색 쪽이다.")]
        public Color BuffColor = new Color(0.95f, 0.78f, 0.35f, 1f);

        [Tooltip("원판 바탕. 피그마 디자인을 넣은 뒤로 코인 UI 와 태그 UI 가 저마다 바탕 그림을 깔아 투명하게 둔다.")]
        public Color DialColor = Color.clear;

        [Tooltip("태그 UI 가운데의 코인 자리. 메달 그림이 보이도록 투명하게 두고 누르기만 받는다.")]
        public Color DialCenterColor = Color.clear;

        [Tooltip("소용돌이 칸. 아직 기획 중이라 효과가 없다(2026년 10월 25일).")]
        public Color VortexColor = new Color(0.3f, 0.27f, 0.35f, 1f);

        [Tooltip("코인 한 개의 바탕. 코인 그림이 없으면 이 색 원만 남는다.")]
        public Color CoinColor = new Color(0.86f, 0.74f, 0.42f, 1f);

        [Tooltip("유물 정보 팝업 바탕.")]
        public Color PopupColor = new Color(0.94f, 0.92f, 0.88f, 1f);

        [Tooltip("코인 정보 팝업 글자. 피그마 설명 풍선은 검은 바탕에 밝은 글자다.")]
        [SerializeField] private Color _coinPopupTextColor = new Color(0.93f, 0.91f, 0.87f, 1f);
        public Color CoinPopupTextColor { get { return _coinPopupTextColor; } set { _coinPopupTextColor = value; } }

        [Tooltip("코인 정보 팝업의 이름 아래 구분선.")]
        [SerializeField] private Color _coinPopupLineColor = new Color(0.42f, 0.4f, 0.37f, 1f);
        public Color CoinPopupLineColor { get { return _coinPopupLineColor; } set { _coinPopupLineColor = value; } }

        [Tooltip("그 태그 문양이 하나도 없어 고를 수 없는 칸에 곱하는 밝기.")]
        [Range(0f, 1f)]
        public float DisabledBrightness = 0.35f;

        [Tooltip("호버한 칸에 곱하는 밝기. 04 화면 공통 규칙 의 10퍼센트다.")]
        public float HoverBrightness = 1.15f;

        [Tooltip("호버한 태그 칸이 커지는 비율. 초안 04 의 \"확대되거나 색상 변경\".")]
        public float HoverScale = 1.06f;

        [Header("글자 색")]
        public Color TextColor = new Color(0.93f, 0.9f, 0.84f, 1f);
        public Color DarkTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);
        public Color SubTextColor = new Color(0.68f, 0.64f, 0.58f, 1f);

        [Tooltip("코인 소모값처럼 체력이 깎이는 숫자. 1.0.b 의 \"-10 은 빨간색상\".")]
        public Color CostColor = new Color(0.9f, 0.25f, 0.2f, 1f);

        [Header("글자 크기")]
        [Tooltip("체력 숫자. 피그마 \"20/20\" 의 글자 높이 24 에 맞췄다.")]
        public float HealthFontSize = UiScale.Px(22f);
        public float NameFontSize = UiScale.Px(30f);
        public float BodyFontSize = UiScale.Px(22f);

        [Tooltip("몬스터 정보 칸 글이 칸을 넘칠 때 줄여 가는 가장 작은 글자 크기. 가장 큰 크기는 BodyFontSize 다.")]
        public float EnemyInfoMinFontSize = UiScale.Px(12f);
        public float HintFontSize = UiScale.Px(24f);
        public float TagSymbolFontSize = UiScale.Px(40f);
        public float CoinLabelFontSize = UiScale.Px(22f);
        public float DamageFontSize = UiScale.Px(64f);

        [Header("레이저 발사음")]
        [Tooltip("레이저 발사음 크기. 레이저가 원판에서 뻗기 시작할 때 난다. 소리는 Art/Combat/LaserSound(Unity AI 로 만듦, \"지잉\")다.")]
        [Range(0f, 1f)]
        public float LaserSoundVolume = 0.7f;

        [Tooltip("코인 정보 팝업의 이름. 피그마 \"공격의 코인\".")]
        [SerializeField] private float _coinPopupNameFontSize = UiScale.Px(17f);
        public float CoinPopupNameFontSize { get { return _coinPopupNameFontSize; } set { _coinPopupNameFontSize = value; } }

        [Tooltip("코인 정보 팝업의 수치와 설명.")]
        [SerializeField] private float _coinPopupBodyFontSize = UiScale.Px(14f);
        public float CoinPopupBodyFontSize { get { return _coinPopupBodyFontSize; } set { _coinPopupBodyFontSize = value; } }

        [Tooltip("피해 숫자 아래 \"막음\" 글.")]
        [SerializeField] private float _damageNoteFontSize = UiScale.Px(26f);
        public float DamageNoteFontSize { get { return _damageNoteFontSize; } set { _damageNoteFontSize = value; } }

        [Header("글")]
        public string PlayerHealthLabel = "체력";
        public string ChooseCoinHint = "코인을 고르세요";
        public string ChooseTagHint = "코인을 걸 태그를 고르세요";
        public string NoCoinText = "코인\n없이";
        public string NoCoinHint = "코인 없이 태그를 고릅니다";
        [Tooltip("꺼낸 코인이 없는 자리에 적는 글. 피그마 원판 바탕에 빈 코인 자리가 그려져 있어 비워 둔다.")]
        public string EmptyCoinText = string.Empty;
        public string NotEnoughHealthText = "체력이 모자랍니다";
        public string CostLabel = "소모 체력";
        public string WeightLabel = "가중치";
        public string AttackLabel = "공격력";
        public string BlockLabel = "방어도";
        public string MomentumLabel = "기세";
        public string LastActionLabel = "지난 행동";

        [Tooltip("정보 칸 제목. 적 이름 대신 적는다. 그 아래에 다음 턴 행동과 설명이 온다.")]
        public string NextActionTitle = "다음 행동";

        [Tooltip("다음 행동 이름 줄의 색.")]
        public Color NextActionColor = new Color(1f, 0.82f, 0.4f, 1f);

        public string BlockedText = "막음";
        public string VortexHint = "아직 효과가 없는 칸입니다";
        public string VortexRelicHint = "사건의 지평선 · 가장 많은 태그로 공격합니다";
        public string VictoryText = "승리";
        public string DefeatText = "패배";

        [Header("연출 시간(초)")]
        [Tooltip("큰 슬롯(SlotSpinOverlay)이 없을 때 원래 칸에서 도는 시간. 왼쪽부터 차례로 멈춘다.")]
        public float SpinDuration = 0.9f;

        [Tooltip("릴 사이 멈추는 간격.")]
        public float SpinStopInterval = 0.12f;

        [Tooltip("돌 때 그림을 바꾸는 간격.")]
        public float SpinFrame = 0.05f;

        [Header("큰 슬롯 회전")]
        [Tooltip("첫 열이 도는 시간. 처음에 빠르다가 점점 느려져 멈춘다(2026년 10월 27일 원재).")]
        public float SpinRollDuration = 1.6f;

        [Tooltip("다음 열이 멈추기까지 더 도는 시간. 왼쪽 열부터 차례로 멈춘다.")]
        public float SpinStopStagger = 0.28f;

        [Tooltip("느려지는 정도. 1 - (1 - t)^n 의 n 이다. 클수록 처음이 빠르고 끝에서 오래 천천히 돈다.")]
        [Range(1.5f, 6f)]
        public float SpinEasePower = 3.2f;

        [Tooltip("첫 열이 굴러가는 칸 수.")]
        [Min(4)]
        public int SpinTravelCells = 20;

        [Tooltip("다음 열마다 더 굴러가는 칸 수. 늦게 멈추는 열도 끝까지 빠르게 돌게 한다.")]
        [Min(0)]
        public int SpinTravelPerColumn = 4;

        [Tooltip("결과 칸을 지나치는 정도. 칸 높이에 대한 비율이다. 0 이면 지나치지 않고 바로 선다(2026년 10월 27일 원재).")]
        [Range(0f, 0.9f)]
        public float SpinOvershoot = 0.35f;

        [Tooltip("지나쳤다가 결과 칸으로 되돌아오는 시간.")]
        public float SpinSettleDuration = 0.18f;

        [Tooltip("열이 멈추는 순간 열 바탕이 밝아지는 색. 이 색에서 원래 열 바탕색으로 돌아간다.")]
        public Color SpinStopFlashColor = new Color(0.55f, 0.42f, 0.18f, 1f);

        [Tooltip("멈출 때 밝아졌다 돌아오는 시간.")]
        public float SpinStopFlashDuration = 0.28f;

        [Tooltip("다 멈춘 뒤 결과 줄을 보여 주는 시간.")]
        public float SpinResultHold = 0.5f;

        [Tooltip("큰 슬롯이 나타나고 사라지는 시간. 사라지면서 결과가 원래 칸에 나타난다.")]
        public float SpinOverlayFade = 0.2f;

        [Tooltip("큰 슬롯 뒤를 덮는 막.")]
        public Color SpinOverlayDim = new Color(0f, 0f, 0f, 0.72f);

        [Tooltip("슬롯 몸통.")]
        public Color SpinMachineColor = new Color(0.11f, 0.09f, 0.08f, 0.97f);

        [Tooltip("열 바탕.")]
        public Color SpinColumnColor = new Color(0.05f, 0.05f, 0.06f, 1f);

        [Tooltip("위 두 행, 아래 두 행을 덮는 막. 결과 줄만 밝게 남긴다.")]
        public Color SpinShadeColor = new Color(0f, 0f, 0f, 0.5f);

        [Tooltip("결과 줄 금테.")]
        public Color SpinLineColor = new Color(0.92f, 0.74f, 0.36f, 1f);

        [Tooltip("결과 줄 금테 안쪽 선.")]
        public Color SpinLineInnerColor = new Color(0.36f, 0.24f, 0.08f, 1f);

        [Tooltip("결과 줄 바깥 빛. 멈춘 뒤 한 번 부푼다.")]
        public Color SpinLineGlowColor = new Color(1f, 0.78f, 0.32f, 0.3f);

        public string SpinSkipHint = "클릭 또는 스페이스바로 건너뛰기";

        [Tooltip("두 UI 가 바뀔 때 사라지고 나타나는 시간. 초안 05 의 전환이다.")]
        public float DialSwapDuration = 0.2f;

        [Tooltip("태그를 고른 뒤 원판이 그 태그 색으로 차 있는 시간. 초안 06 의 \"공격 예정\".")]
        public float AttackChargeDuration = 0.45f;

        [Header("공격 레이저")]
        [Tooltip("레이저가 원판에서 적까지 뻗는 시간(초). 그림은 Art/Combat/LaserBeam(Unity AI 로 만듦)이고 태그 색으로 물들인다.")]
        public float LaserGrowDuration = 0.1f;

        [Tooltip("레이저가 적에 닿은 채 머무는 시간(초).")]
        public float LaserHoldDuration = 0.12f;

        [Tooltip("레이저가 가늘어지며 사라지는 시간(초).")]
        public float LaserFadeDuration = 0.18f;

        [Tooltip("태그 색에 흰빛을 섞는 정도. 0 이면 태그 색 그대로, 1 이면 흰색이다.")]
        [Range(0f, 1f)]
        public float LaserWhiteMix = 0.3f;

        [Tooltip("피해 숫자가 떠 있는 시간.")]
        public float DamageShowDuration = 0.8f;

        [Tooltip("적이 때리는 시간.")]
        public float EnemyAttackDuration = 0.5f;

        [Tooltip("전투가 끝난 뒤 결과 글을 보여 주는 시간.")]
        public float EndDelay = 1.0f;

        [Header("연출 세기")]
        [Tooltip("적 최대 체력 대비 피해 비율 25 / 50 / 75 / 100 퍼센트마다 세기를 올린다. 초안 06. " +
                 "단계마다 피해 숫자 배율이다.")]
        public float[] DamageTierScales = { 1f, 1.2f, 1.45f, 1.75f, 2.1f };

        [Tooltip("단계마다 몬스터가 흔들리는 폭(픽셀).")]
        public float[] DamageTierShakes = { 6f, 12f, 20f, 30f, 44f };

        /// <summary>
        /// 피해가 적 최대 체력의 몇 단계인지. 25 퍼센트 아래가 0, 100 퍼센트 이상이 4 다.
        /// </summary>
        public static int GetDamageTier(int damage, int enemyMaxHealth)
        {
            if (enemyMaxHealth <= 0 || damage <= 0)
            {
                return 0;
            }

            float ratio = damage / (float)enemyMaxHealth;
            if (ratio >= 1f)
            {
                return 4;
            }

            if (ratio >= 0.75f)
            {
                return 3;
            }

            if (ratio >= 0.5f)
            {
                return 2;
            }

            return ratio >= 0.25f ? 1 : 0;
        }

        /// <summary>그 단계의 피해 숫자 배율.</summary>
        public float GetTierScale(int tier)
        {
            return Pick(DamageTierScales, tier, 1f);
        }

        /// <summary>그 단계의 흔들림 폭.</summary>
        public float GetTierShake(int tier)
        {
            return UiScale.Px(Pick(DamageTierShakes, tier, 10f));
        }

        private static float Pick(float[] values, int tier, float fallback)
        {
            if (values == null || values.Length == 0)
            {
                return fallback;
            }

            int at = Mathf.Clamp(tier, 0, values.Length - 1);
            return values[at];
        }

        /// <summary>그 등급의 유물 칸 틀 색. 등급이 표를 넘으면 가까운 끝 색을 쓴다.</summary>
        public Color GetRelicGradeColor(int grade)
        {
            if (RelicGradeColors == null || RelicGradeColors.Length == 0)
            {
                return Color.white;
            }

            return RelicGradeColors[Mathf.Clamp(grade, 0, RelicGradeColors.Length - 1)];
        }

        /// <summary>밝기를 곱한 색.</summary>
        public static Color Scale(Color color, float brightness)
        {
            return new Color(
                Mathf.Clamp01(color.r * brightness),
                Mathf.Clamp01(color.g * brightness),
                Mathf.Clamp01(color.b * brightness),
                color.a);
        }
    }
}
