using TMPro;
using UnityEngine;

namespace SlotHero.Hud.UI
{
    /// <summary>
    /// 자동 저장 표시.
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 고정 자리 중
    /// 현재 빌드 버튼 아래 200 × 40 자리를 맡는다.
    /// 06 오버레이 목록 에서 "자동 저장 직후 저장 중 문구 출력"으로 정한 것이다.
    ///
    /// 언제 저장이 일어나는지는 저장 시스템 기획서 소관이라
    /// 이쪽은 저장이 끝났다는 신호를 받아 문구를 잠시 띄우는 일만 한다.
    /// </summary>
    public class AutoSaveNotice : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TMP_Text _label;

        [Header("설정")]
        [SerializeField] private HudVisualConfig _visual;

        private float _remainingSeconds;

        /// <summary>
        /// 설정 화면의 자동 저장 알림 항목. 꺼 두면 저장이 일어나도 문구를 띄우지 않는다.
        /// 설정 시스템이 아직 없으므로 값을 넣어 주는 쪽이 챙긴다.
        /// </summary>
        public bool NoticeEnabled { get; set; }

        /// <summary>지금 문구가 떠 있는지.</summary>
        public bool IsShowing
        {
            get { return _remainingSeconds > 0f; }
        }

        private void Awake()
        {
            NoticeEnabled = true;
            ApplyStyle();
            HideNow();
        }

        /// <summary>
        /// 꺼질 때 표시를 바로 지운다.
        ///
        /// 시간은 `Update` 로 재는데 꺼진 오브젝트는 `Update` 가 돌지 않는다.
        /// 안 지우면 흐려지던 중에 얼어 있다가 다음에 켜질 때 남은 시간만큼 이어서 떠 버린다.
        /// 런 종료 저장 직후 결과 화면으로 가며 HUD 가 꺼지는 길이 그렇다.
        /// </summary>
        private void OnDisable()
        {
            HideNow();
        }

        private void Update()
        {
            if (_remainingSeconds <= 0f)
            {
                return;
            }

            // 연출 속도 설정이나 일시 정지와 무관하게 흘러야 하므로 실제 시간을 쓴다.
            _remainingSeconds -= Time.unscaledDeltaTime;

            if (_remainingSeconds <= 0f)
            {
                HideNow();
                return;
            }

            SetAlpha(GetAlpha(_remainingSeconds));
        }

        /// <summary>표시 크기를 맞춘다. 고정 자리 설정이 부른다.</summary>
        public void ApplySize(Vector2 size)
        {
            RectTransform rect = (RectTransform)transform;
            rect.sizeDelta = size;
        }

        /// <summary>색과 문구를 설정대로 맞춘다.</summary>
        public void ApplyStyle()
        {
            if (_label == null || _visual == null)
            {
                return;
            }

            _label.text = _visual.AutoSaveText;
            _label.color = _visual.AutoSaveColor;
            _label.fontSize = _visual.AutoSaveFontSize;
        }

        /// <summary>자동 저장이 끝났을 때 부른다. 문구를 띄우고 시간이 지나면 흐려지며 사라진다.</summary>
        public void Show()
        {
            if (!NoticeEnabled || _visual == null)
            {
                return;
            }

            ApplyStyle();
            _remainingSeconds = _visual.AutoSaveShowSeconds + _visual.AutoSaveFadeSeconds;
            SetAlpha(GetAlpha(_remainingSeconds));

            if (_group != null && !_group.gameObject.activeSelf)
            {
                _group.gameObject.SetActive(true);
            }
        }

        /// <summary>문구를 바로 지운다.</summary>
        public void HideNow()
        {
            _remainingSeconds = 0f;
            SetAlpha(0f);
        }

        /// <summary>남은 시간으로 투명도를 구한다. 흐려지는 구간에서만 값이 내려간다.</summary>
        private float GetAlpha(float remaining)
        {
            if (_visual == null)
            {
                return 1f;
            }

            float fade = _visual.AutoSaveFadeSeconds;
            if (fade <= 0f || remaining > fade)
            {
                return 1f;
            }

            return remaining / fade;
        }

        private void SetAlpha(float alpha)
        {
            if (_group == null)
            {
                return;
            }

            _group.alpha = alpha;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }
    }
}
