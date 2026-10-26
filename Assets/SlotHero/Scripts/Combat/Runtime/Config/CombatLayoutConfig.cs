using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.Combat
{
    /// <summary>
    /// 전투 화면의 자리와 크기. 1920 × 1080 기준이고 좌표는 왼쪽 위에서 잰다.
    ///
    /// 전투 UI 및 연출 초안 / 03 기초 화면 정보 의 크기를 따른다.
    /// 자리는 그 와이어프레임에서 재고 아래를 맞추느라 몇 군데 옮겼다.
    ///   - 위쪽 64 는 상단 표시줄 자리라 비운다
    ///   - 오른쪽 아래 덱 확인 버튼은 두지 않는다. 런 화면 공통의 현재 빌드 버튼(Hud)이 같은 일을 같은 자리에서 한다
    ///   - 코인/태그 원판은 그 버튼과 겹치지 않게 와이어프레임보다 왼쪽 위로 올렸다
    ///
    /// 2026년 10월 26일 원재가 피그마 `Jackpot 아트웍 (복사)` 의 전투 화면 디자인을 넣으라고 했다.
    /// 릴, 유물 칸, 원판은 지금 자리와 크기에 그림만 입힌다. 체력과 주인공 초상은 피그마 자리를 따른다.
    /// 피그마 화면은 1920 × 1080 의 4.84 배로 그려져 있어 1920 × 1080 으로 줄여 잰 값을 적었다.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatLayoutConfig", menuName = "Slot Hero/전투/자리 설정")]
    public class CombatLayoutConfig : ScriptableObject
    {
        [Header("문양")]
        [Tooltip("첫 릴의 왼쪽 위. 초안은 문양 6개를 위쪽에 일자로 놓는다.")]
        public Vector2 ReelPosition = UiScale.V(330f, 96f);

        [Tooltip("릴 하나. 초안의 \"각각 200x200\".")]
        public Vector2 ReelSize = UiScale.V(200f, 200f);

        public float ReelGap = UiScale.Px(20f);

        [Tooltip("릴 안의 문양 그림.")]
        public float ReelIconSize = UiScale.Px(150f);

        [Tooltip("릴 모서리에 놓는 태그 그림. 초안 08 의 태그 이미지 자리다.")]
        public float ReelTagSize = UiScale.Px(46f);

        [Tooltip("강조 테두리 두께.")]
        public float ReelHighlightThickness = UiScale.Px(8f);

        [Tooltip("릴 틀 그림 안쪽으로 바탕을 들이는 거리. 틀 가장자리의 깨진 부분 밖으로 바탕이 비치지 않게 한다.")]
        [SerializeField] private float _reelBackInset = UiScale.Px(8f);
        public float ReelBackInset { get { return _reelBackInset; } set { _reelBackInset = value; } }

        [Tooltip("릴 틀 안쪽으로 흑요석 판, 문양, 태그 테두리를 들이는 거리. 피그마 릴 틀 두께가 칸의 8 퍼센트쯤이다.")]
        [SerializeField] private float _reelContentInset = UiScale.Px(16f);
        public float ReelContentInset { get { return _reelContentInset; } set { _reelContentInset = value; } }

        [Header("유물")]
        [Tooltip("유물 목록. 초안의 \"200x700\".")]
        public Vector2 RelicPanelPosition = UiScale.V(60f, 140f);

        public Vector2 RelicPanelSize = UiScale.V(200f, 700f);

        [Tooltip("유물 칸 하나. 초안의 \"각 유물 100x100\".")]
        public float RelicSlotSize = UiScale.Px(100f);

        public float RelicSlotGap = UiScale.Px(12f);

        [Tooltip("유물 칸 틀 안쪽으로 그림을 들이는 거리. 틀 두께가 칸의 8 퍼센트쯤이다.")]
        [SerializeField] private float _relicIconInset = UiScale.Px(16f);
        public float RelicIconInset { get { return _relicIconInset; } set { _relicIconInset = value; } }

        [Tooltip("유물 정보 팝업. 유물 목록 오른쪽, 그 칸 높이에 뜬다.")]
        public Vector2 RelicPopupSize = UiScale.V(340f, 220f);

        public float RelicPopupGap = UiScale.Px(12f);

        [Tooltip("유물 고유 수치 글자 칸. 유물 칸 오른쪽 아래에 붙는다.")]
        public Vector2 RelicCounterSize = UiScale.V(56f, 32f);

        [Header("플레이어")]
        [Tooltip("플레이어 체력. 피그마의 하트와 숫자(177, 893)부터 체력 틀 아래(974)까지 한 칸이다. 유물 목록 아래다.")]
        public Vector2 PlayerHealthPosition = UiScale.V(177f, 893f);

        [Tooltip("피그마 체력 틀의 너비 384, 하트 윗줄부터 틀 아래까지 81.")]
        public Vector2 PlayerHealthSize = UiScale.V(384f, 81f);

        [Tooltip("체력 칸 안의 틀 그림. 왼쪽 위 기준 비율이다. 피그마 (177, 926) 384 × 48.")]
        [SerializeField] private Rect _playerFrameArea = new Rect(0f, 0.407f, 1f, 0.593f);
        public Rect PlayerFrameArea { get { return _playerFrameArea; } set { _playerFrameArea = value; } }

        [Tooltip("체력 칸 안의 막대. 피그마 (210, 942) 317 × 18.")]
        [SerializeField] private Rect _playerBarArea = new Rect(0.0859f, 0.605f, 0.8255f, 0.222f);
        public Rect PlayerBarArea { get { return _playerBarArea; } set { _playerBarArea = value; } }

        [Tooltip("체력 칸 안의 하트. 피그마 (210, 896) 26 × 21.")]
        [SerializeField] private Rect _playerHeartArea = new Rect(0.0859f, 0.037f, 0.0677f, 0.259f);
        public Rect PlayerHeartArea { get { return _playerHeartArea; } set { _playerHeartArea = value; } }

        [Tooltip("체력 칸 안의 숫자. 피그마 (249, 895) 에서 시작한다.")]
        [SerializeField] private Rect _playerNumberArea = new Rect(0.1875f, 0f, 0.6f, 0.321f);
        public Rect PlayerNumberArea { get { return _playerNumberArea; } set { _playerNumberArea = value; } }

        [Tooltip("주인공 깃발과 초상. 피그마 (68, 893) 89 × 114.")]
        [SerializeField] private Vector2 _playerPortraitPosition = UiScale.V(68f, 893f);
        public Vector2 PlayerPortraitPosition { get { return _playerPortraitPosition; } set { _playerPortraitPosition = value; } }

        [SerializeField] private Vector2 _playerPortraitSize = UiScale.V(89f, 114f);
        public Vector2 PlayerPortraitSize { get { return _playerPortraitSize; } set { _playerPortraitSize = value; } }

        [Header("몬스터")]
        [Tooltip("몬스터 체력. 피그마 체력 틀(450 × 149)을 일러스트에 맞춰 줄이고 일러스트 바로 위 가운데에 놓았다.")]
        public Vector2 EnemyHealthPosition = UiScale.V(730f, 308f);

        public Vector2 EnemyHealthSize = UiScale.V(400f, 132f);

        [Tooltip("체력 틀 안의 막대. 왼쪽 위 기준 비율이다. 피그마 틀 (729, 334) 안의 막대 (772, 412) 364 × 18.")]
        [SerializeField] private Rect _enemyBarArea = new Rect(0.0956f, 0.523f, 0.8094f, 0.121f);
        public Rect EnemyBarArea { get { return _enemyBarArea; } set { _enemyBarArea = value; } }

        [Tooltip("체력 틀 안의 하트. 피그마 (906, 385) 26 × 24.")]
        [SerializeField] private Rect _enemyHeartArea = new Rect(0.393f, 0.342f, 0.058f, 0.161f);
        public Rect EnemyHeartArea { get { return _enemyHeartArea; } set { _enemyHeartArea = value; } }

        [Tooltip("체력 틀 안의 숫자. 피그마 (945, 385) 에서 시작한다.")]
        [SerializeField] private Rect _enemyNumberArea = new Rect(0.48f, 0.322f, 0.4f, 0.2f);
        public Rect EnemyNumberArea { get { return _enemyNumberArea; } set { _enemyNumberArea = value; } }

        [Tooltip("몬스터 일러스트. 초안의 \"300x416\".")]
        public Vector2 EnemyArtPosition = UiScale.V(780f, 440f);

        public Vector2 EnemyArtSize = UiScale.V(300f, 416f);

        [Tooltip("몬스터 정보(보스 등 특수 능력). 초안의 \"300x300\". 일러스트 왼쪽이다.")]
        public Vector2 EnemyInfoPosition = UiScale.V(460f, 440f);

        public Vector2 EnemyInfoSize = UiScale.V(300f, 300f);

        [Header("소환수")]
        [Tooltip("첫 소환수 칸(가장 왼쪽 자리)의 왼쪽 위. 정보 칸 아래, 일러스트 왼쪽이다. " +
                 "몬스터 기획서는 소환수 자리를 그리지 않아 정보 칸 아래 빈 곳에 두었다.")]
        public Vector2 SummonPosition = UiScale.V(460f, 756f);

        [Tooltip("소환수 칸 하나. 셋이 정보 칸 너비(300)에 들어간다.")]
        public Vector2 SummonSize = UiScale.V(92f, 180f);

        public float SummonGap = UiScale.Px(12f);

        [Header("코인/태그 원판")]
        [Tooltip("두 UI 가 같은 자리, 같은 크기로 바뀌어 든다. 초안의 \"600x600 원\".")]
        public Vector2 DialPosition = UiScale.V(1150f, 340f);

        public float DialSize = UiScale.Px(600f);

        [Tooltip("원판 위의 안내 글.")]
        public Vector2 HintSize = UiScale.V(600f, 36f);

        [Header("코인 UI")]
        [Tooltip("코인 하나. 초안 글은 100x100 인데 피그마 디자인은 130 으로 그렸다. 그림을 따랐다.")]
        public float CoinSize = UiScale.Px(130f);

        [Tooltip("코인 셋의 가운데. 원판 가운데 기준이고 y 는 아래가 + 다. 원판 바탕 그림의 코인 자리 세 곳에서 쟀다. " +
                 "피그마는 삼각형이 원 가운데보다 아래에 있었는데 원재가 가운데로 맞추라고 해 바탕 그림을 고쳤다(2026년 10월 26일). " +
                 "세 자리가 원 가운데에서 같은 거리(118)에 있다.")]
        [SerializeField] private Vector2[] _coinOffsets = { UiScale.V(0f, -118f), UiScale.V(-105f, 54f), UiScale.V(103f, 54f) };
        public Vector2[] CoinOffsets { get { return _coinOffsets; } set { _coinOffsets = value; } }

        [Tooltip("코인을 고를 때 두르는 붉은 빛 테두리. 코인 지름에 곱한다.")]
        [SerializeField] private float _coinGlowScale = 1.26f;
        public float CoinGlowScale { get { return _coinGlowScale; } set { _coinGlowScale = value; } }

        [Tooltip("원판 가운데에서 코인 가운데까지. 셋을 위, 왼쪽 아래, 오른쪽 아래의 삼각형으로 놓는다.")]
        public float CoinDistance = UiScale.Px(175f);

        [Tooltip("가운데 작은 원. 초안에 설명이 없어 \"코인 없이\" 를 두었다.")]
        public float CoinCenterSize = UiScale.Px(100f);

        [Tooltip("코인 정보 팝업. 피그마 설명 풍선 314 × 183. 그림 둘레 14 쯤이 투명하다.")]
        public Vector2 CoinPopupSize = UiScale.V(314f, 183f);

        [Tooltip("코인과 팝업 사이. 풍선 그림 둘레가 투명해 조금 겹쳐 놓는다.")]
        public float CoinPopupGap = UiScale.Px(-3f);

        [Tooltip("팝업을 코인 가운데보다 올리는 거리. 피그마는 풍선 아래 끝을 코인 가운데쯤에 두었다.")]
        [SerializeField] private float _coinPopupLift = UiScale.Px(80f);
        public float CoinPopupLift { get { return _coinPopupLift; } set { _coinPopupLift = value; } }

        [Header("태그 UI")]
        [Tooltip("가운데 선택된 코인 자리(누르는 곳)의 지름. 열 칸 바퀴의 안쪽 원(193)에 들어간다.")]
        public float TagCenterSize = UiScale.Px(190f);

        [Tooltip("원판 가운데에서 태그 아이콘 가운데까지. 열 칸의 안팎 반지름 사이 가운데다.")]
        public float TagIconDistance = UiScale.Px(148f);

        public float TagIconSize = UiScale.Px(56f);

        [Tooltip("열 칸 바퀴의 바깥 지름. 칸 누르기도 이 원 안에서 받는다. 피그마 바퀴 바깥 선이 원판 반지름의 0.664 배다.")]
        [SerializeField] private float _tagWheelSize = UiScale.Px(398f);
        public float TagWheelSize { get { return _tagWheelSize; } set { _tagWheelSize = value; } }

        [Tooltip("열 칸 바퀴의 안쪽 반지름 비율. 그 안은 가운데 메달 자리다.")]
        [SerializeField] private float _tagWheelInnerRatio = 0.484f;
        public float TagWheelInnerRatio { get { return _tagWheelInnerRatio; } set { _tagWheelInnerRatio = value; } }

        [Tooltip("바퀴 그림과 칸 강조 그림의 크기를 바퀴 지름에 곱한다. 그림(1254)에서 바깥 선 지름이 1117 이다.")]
        [SerializeField] private float _tagWheelArtScale = 1.1227f;
        public float TagWheelArtScale { get { return _tagWheelArtScale; } set { _tagWheelArtScale = value; } }

        [Tooltip("원판 테두리 안쪽을 메우는 어두운 원의 지름. 테두리 그림 안쪽 빈 곳이 원판 지름의 0.716 배다.")]
        [SerializeField] private float _tagWheelBackSize = UiScale.Px(440f);
        public float TagWheelBackSize { get { return _tagWheelBackSize; } set { _tagWheelBackSize = value; } }

        [Tooltip("가운데 메달. 피그마 231 × 208.")]
        [SerializeField] private Vector2 _tagMedalSize = UiScale.V(231f, 208f);
        public Vector2 TagMedalSize { get { return _tagMedalSize; } set { _tagMedalSize = value; } }

        [Tooltip("메달 그림 가운데를 원판 가운데에서 옮기는 거리. y 는 아래가 + 다. 메달의 둥근 판이 그림 가운데보다 아래에 있다.")]
        [SerializeField] private Vector2 _tagMedalOffset = UiScale.V(-2f, -10f);
        public Vector2 TagMedalOffset { get { return _tagMedalOffset; } set { _tagMedalOffset = value; } }

        [Tooltip("메달 위에 놓는 고른 코인의 지름. 메달 판(지름 119)을 덮는다.")]
        [SerializeField] private float _tagCenterCoinSize = UiScale.Px(112f);
        public float TagCenterCoinSize { get { return _tagCenterCoinSize; } set { _tagCenterCoinSize = value; } }

        [Tooltip("칸 사이를 띄우는 각도. 칸을 이만큼 좁혀 경계가 보이게 한다.")]
        public float TagSectorGapDegrees = 1.2f;

        [Header("연출")]
        [Tooltip("피해 숫자가 떠오르는 거리.")]
        public float DamageRise = UiScale.Px(90f);

        [Tooltip("피해 숫자 뒤의 번개. 피그마 피해 숫자 300 × 224.")]
        [SerializeField] private Vector2 _damageSparkSize = UiScale.V(300f, 225f);
        public Vector2 DamageSparkSize { get { return _damageSparkSize; } set { _damageSparkSize = value; } }

        [Tooltip("숫자 그림 한 장(256 × 320)을 놓는 크기.")]
        [SerializeField] private Vector2 _damageDigitSize = UiScale.V(96f, 120f);
        public Vector2 DamageDigitSize { get { return _damageDigitSize; } set { _damageDigitSize = value; } }

        [Tooltip("숫자 사이 간격. 숫자 그림 양옆이 비어 있어 그림 너비보다 좁게 둔다.")]
        [SerializeField] private float _damageDigitAdvance = UiScale.Px(70f);
        public float DamageDigitAdvance { get { return _damageDigitAdvance; } set { _damageDigitAdvance = value; } }

        [Tooltip("피해 숫자 아래 \"막음\" 글 칸의 높이.")]
        [SerializeField] private float _damageNoteHeight = UiScale.Px(48f);
        public float DamageNoteHeight { get { return _damageNoteHeight; } set { _damageNoteHeight = value; } }

        [Header("코인 정보 팝업 안")]
        [Tooltip("코인 그림. 풍선 그림 왼쪽 위에서 잰 x, y, 너비, 높이다. 피그마 설명 풍선의 그림 자리.")]
        [SerializeField] private Rect _coinPopupIconArea = new Rect(UiScale.Px(24f), UiScale.Px(53f), UiScale.Px(78f), UiScale.Px(78f));
        public Rect CoinPopupIconArea { get { return _coinPopupIconArea; } set { _coinPopupIconArea = value; } }

        [Tooltip("코인 이름. 피그마 \"공격의 코인\" 자리.")]
        [SerializeField] private Rect _coinPopupNameArea = new Rect(UiScale.Px(117f), UiScale.Px(48f), UiScale.Px(180f), UiScale.Px(28f));
        public Rect CoinPopupNameArea { get { return _coinPopupNameArea; } set { _coinPopupNameArea = value; } }

        [Tooltip("이름 아래 구분선.")]
        [SerializeField] private Rect _coinPopupLineArea = new Rect(UiScale.Px(117f), UiScale.Px(79f), UiScale.Px(165f), UiScale.Px(1.5f));
        public Rect CoinPopupLineArea { get { return _coinPopupLineArea; } set { _coinPopupLineArea = value; } }

        [Tooltip("소모 체력과 가중치.")]
        [SerializeField] private Rect _coinPopupStatsArea = new Rect(UiScale.Px(117f), UiScale.Px(84f), UiScale.Px(180f), UiScale.Px(38f));
        public Rect CoinPopupStatsArea { get { return _coinPopupStatsArea; } set { _coinPopupStatsArea = value; } }

        [Tooltip("코인 설명. 체력이 모자라면 한 줄이 더 붙어 글자를 줄여 넣는다.")]
        [SerializeField] private Rect _coinPopupBodyArea = new Rect(UiScale.Px(117f), UiScale.Px(122f), UiScale.Px(180f), UiScale.Px(42f));
        public Rect CoinPopupBodyArea { get { return _coinPopupBodyArea; } set { _coinPopupBodyArea = value; } }

        [Tooltip("공격 구슬의 지름.")]
        public float ProjectileSize = UiScale.Px(90f);

        [Tooltip("적이 때릴 때 앞으로 나오는 거리.")]
        public float EnemyLunge = UiScale.Px(40f);

        /// <summary>그 릴의 왼쪽 위.</summary>
        public Vector2 GetReelPosition(int index)
        {
            return new Vector2(ReelPosition.x + index * (ReelSize.x + ReelGap), ReelPosition.y);
        }

        /// <summary>그 유물 칸의 왼쪽 위. 목록 안에서 가로 가운데에 세로로 늘어놓는다.</summary>
        public Vector2 GetRelicSlotPosition(int index)
        {
            float x = RelicPanelPosition.x + (RelicPanelSize.x - RelicSlotSize) * 0.5f;
            float y = RelicPanelPosition.y + RelicSlotGap + index * (RelicSlotSize + RelicSlotGap);
            return new Vector2(x, y);
        }

        /// <summary>그 자리 소환수 칸의 왼쪽 위. 0 이 가장 왼쪽이다.</summary>
        public Vector2 GetSummonPosition(int slot)
        {
            return new Vector2(SummonPosition.x + slot * (SummonSize.x + SummonGap), SummonPosition.y);
        }

        /// <summary>원판 가운데. 화면 왼쪽 위 기준이다.</summary>
        public Vector2 DialCenter
        {
            get { return new Vector2(DialPosition.x + DialSize * 0.5f, DialPosition.y + DialSize * 0.5f); }
        }

        /// <summary>안내 글의 왼쪽 위. 원판 바로 위다.</summary>
        public Vector2 HintPosition
        {
            get { return new Vector2(DialPosition.x + (DialSize - HintSize.x) * 0.5f, DialPosition.y - HintSize.y - UiScale.Px(4f)); }
        }

        /// <summary>
        /// 코인 하나의 가운데. 원판 가운데 기준이고 y 는 아래가 + 다.
        /// 0 은 위, 1 은 왼쪽 아래, 2 는 오른쪽 아래다. 초안 03 의 삼각형 배치다.
        /// 피그마 코인 자리(`CoinOffsets`)가 있으면 그 자리를, 없으면 120 도 간격 삼각형을 쓴다.
        /// </summary>
        public Vector2 GetCoinOffset(int index)
        {
            if (_coinOffsets != null && index >= 0 && index < _coinOffsets.Length)
            {
                return _coinOffsets[index];
            }

            float angle = (90f + index * 120f) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle) * CoinDistance, -Mathf.Sin(angle) * CoinDistance);
        }

        /// <summary>릴이 화면 안에 들어가고 원판과 겹치지 않는지.</summary>
        public bool ReelsFitAboveDial(int reelCount)
        {
            Vector2 last = GetReelPosition(reelCount - 1);
            bool insideScreen = last.x + ReelSize.x <= UiScale.Px(1920f) - UiScale.Px(40f);
            bool aboveDial = ReelPosition.y + ReelSize.y <= HintPosition.y;
            return insideScreen && aboveDial;
        }
    }
}
