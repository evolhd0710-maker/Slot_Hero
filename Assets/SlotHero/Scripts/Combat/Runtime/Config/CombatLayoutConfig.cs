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

        [Header("유물")]
        [Tooltip("유물 목록. 초안의 \"200x700\".")]
        public Vector2 RelicPanelPosition = UiScale.V(60f, 140f);

        public Vector2 RelicPanelSize = UiScale.V(200f, 700f);

        [Tooltip("유물 칸 하나. 초안의 \"각 유물 100x100\".")]
        public float RelicSlotSize = UiScale.Px(100f);

        public float RelicSlotGap = UiScale.Px(12f);

        [Tooltip("유물 정보 팝업. 유물 목록 오른쪽, 그 칸 높이에 뜬다.")]
        public Vector2 RelicPopupSize = UiScale.V(340f, 220f);

        public float RelicPopupGap = UiScale.Px(12f);

        [Tooltip("유물 고유 수치 글자 칸. 유물 칸 오른쪽 아래에 붙는다.")]
        public Vector2 RelicCounterSize = UiScale.V(56f, 32f);

        [Header("플레이어")]
        [Tooltip("플레이어 체력. 초안의 \"300x80\". 유물 목록 아래다.")]
        public Vector2 PlayerHealthPosition = UiScale.V(60f, 870f);

        public Vector2 PlayerHealthSize = UiScale.V(300f, 80f);

        [Header("몬스터")]
        [Tooltip("몬스터 체력. 초안의 \"300X80\". 일러스트 바로 위다.")]
        public Vector2 EnemyHealthPosition = UiScale.V(780f, 340f);

        public Vector2 EnemyHealthSize = UiScale.V(300f, 80f);

        [Tooltip("몬스터 일러스트. 초안의 \"300x416\".")]
        public Vector2 EnemyArtPosition = UiScale.V(780f, 440f);

        public Vector2 EnemyArtSize = UiScale.V(300f, 416f);

        [Tooltip("몬스터 정보(보스 등 특수 능력). 초안의 \"300x300\". 일러스트 왼쪽이다.")]
        public Vector2 EnemyInfoPosition = UiScale.V(460f, 440f);

        public Vector2 EnemyInfoSize = UiScale.V(300f, 300f);

        [Header("코인/태그 원판")]
        [Tooltip("두 UI 가 같은 자리, 같은 크기로 바뀌어 든다. 초안의 \"600x600 원\".")]
        public Vector2 DialPosition = UiScale.V(1150f, 340f);

        public float DialSize = UiScale.Px(600f);

        [Tooltip("원판 위의 안내 글.")]
        public Vector2 HintSize = UiScale.V(600f, 36f);

        [Header("코인 UI")]
        [Tooltip("코인 하나. 초안 글은 100x100 인데 초안 그림과 아트웍은 원판의 4분의 1 쯤으로 크게 그렸다. 그림을 따랐다.")]
        public float CoinSize = UiScale.Px(150f);

        [Tooltip("원판 가운데에서 코인 가운데까지. 셋을 위, 왼쪽 아래, 오른쪽 아래의 삼각형으로 놓는다.")]
        public float CoinDistance = UiScale.Px(175f);

        [Tooltip("가운데 작은 원. 초안에 설명이 없어 \"코인 없이\" 를 두었다.")]
        public float CoinCenterSize = UiScale.Px(100f);

        [Tooltip("코인 정보 팝업. 초안의 \"200x300\".")]
        public Vector2 CoinPopupSize = UiScale.V(200f, 300f);

        [Tooltip("코인과 팝업 사이.")]
        public float CoinPopupGap = UiScale.Px(10f);

        [Header("태그 UI")]
        [Tooltip("가운데 선택된 코인 자리의 지름.")]
        public float TagCenterSize = UiScale.Px(220f);

        [Tooltip("원판 가운데에서 태그 아이콘 가운데까지.")]
        public float TagIconDistance = UiScale.Px(205f);

        public float TagIconSize = UiScale.Px(70f);

        [Tooltip("칸 사이를 띄우는 각도. 칸을 이만큼 좁혀 경계가 보이게 한다.")]
        public float TagSectorGapDegrees = 1.2f;

        [Header("연출")]
        [Tooltip("피해 숫자가 떠오르는 거리.")]
        public float DamageRise = UiScale.Px(90f);

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
        /// </summary>
        public Vector2 GetCoinOffset(int index)
        {
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
