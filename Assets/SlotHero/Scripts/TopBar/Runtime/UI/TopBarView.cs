using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SlotHero.Ui;

namespace SlotHero.TopBar.UI
{
    /// <summary>
    /// 상단 표시줄 표시 전체.
    /// 상단 UI 바 기획서 v0.2 의 03 표시줄 규격 과 04 표시 항목 을 맡는다.
    /// 좌측에 런 요약 넷, 우측에 버튼 셋을 놓는다.
    ///
    /// 값이 언제 바뀌는지와 바뀔 때의 연출은 <see cref="TopBarController"/>가 정하고,
    /// 여기서는 받은 값을 그대로 그린다.
    /// </summary>
    public class TopBarView : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private TopBarLayoutConfig _layout;
        [SerializeField] private TopBarVisualConfig _visual;

        [Header("배경")]
        [SerializeField] private Image _background;

        [Tooltip("표시줄 아래 바깥에 붙는 장식 띠. 표시줄 64 영역에는 들어가지 않는다(2026년 10월 20일 원재).")]
        [SerializeField] private Image _bottomTrim;

        [Header("① 플레이 시간")]
        [SerializeField] private RectTransform _playTimeRoot;
        [SerializeField] private TMP_Text _playTimeLabel;

        [Header("② 위치")]
        [SerializeField] private RectTransform _locationRoot;
        [SerializeField] private Image _stageIcon;
        [SerializeField] private TMP_Text _stageLabel;
        [SerializeField] private Image _roomIcon;

        [Tooltip("현재 방 아이콘 위에 얹는 안쪽 무늬. 방 아이콘을 꽉 채우는 자식이다(2026년 10월 25일 원재).")]
        [SerializeField] private Image _roomIconDetail;
        [SerializeField] private TopBarHoverArea _locationHover;

        [Header("③ 체력")]
        [SerializeField] private TopBarField _healthField;

        [Header("④ 골드")]
        [SerializeField] private TopBarField _goldField;

        [Header("⑤ ~ ⑦ 버튼")]
        [SerializeField] private TopBarButton[] _buttons;

        [Header("호버 팝업")]
        [SerializeField] private TopBarTooltip _tooltip;

        /// <summary>위치 칸의 마우스 영역. 호버 팝업을 잇는 데 쓴다.</summary>
        public TopBarHoverArea LocationHover
        {
            get { return _locationHover; }
        }

        /// <summary>호버 팝업.</summary>
        public TopBarTooltip Tooltip
        {
            get { return _tooltip; }
        }

        /// <summary>표시줄에 놓인 버튼들.</summary>
        public TopBarButton[] Buttons
        {
            get { return _buttons; }
        }

        private void Awake()
        {
            ApplyLayout();
        }

        /// <summary>
        /// 기획서 03, 04 장의 자리를 실제 RectTransform 에 적용한다.
        /// 씬에서 자리를 눈으로 확인하려면 컴포넌트를 우클릭해 바로 부르면 된다.
        /// </summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            if (!_layout.SummaryOrderIsValid())
            {
                Debug.LogWarning(
                    "상단 표시줄 자리 설정에서 런 요약 항목이 순서대로 놓이지 않거나 버튼 자리까지 넘는다.",
                    this);
            }

            ApplyBar();

            float iconSize = _layout.GetIconSize();

            PlaceSummary(_playTimeRoot, _layout.PlayTimeX);
            PlaceSummary(_locationRoot, _layout.LocationX);
            PlaceSummary(_healthField != null ? _healthField.RectTransform : null, _layout.HealthX);
            PlaceSummary(_goldField != null ? _goldField.RectTransform : null, _layout.GoldX);

            ApplyIconSize(_stageIcon, iconSize);
            ApplyIconSize(_roomIcon, iconSize);

            if (_healthField != null)
            {
                _healthField.SetIcon(null, iconSize);
            }

            if (_goldField != null)
            {
                _goldField.SetIcon(null, iconSize);
            }

            if (_buttons != null)
            {
                for (int i = 0; i < _buttons.Length; i++)
                {
                    TopBarButton button = _buttons[i];
                    if (button == null)
                    {
                        continue;
                    }

                    RectTransform rect = button.RectTransform;
                    rect.anchorMin = new Vector2(1f, 0.5f);
                    rect.anchorMax = new Vector2(1f, 0.5f);
                    rect.pivot = new Vector2(1f, 0.5f);
                    rect.anchoredPosition = _layout.GetButtonPosition(button.Kind);
                    button.ApplySize(_layout.ButtonHitSize, iconSize);
                }
            }

            ApplyStyle();
        }

        /// <summary>색과 글자 크기를 설정대로 맞춘다.</summary>
        public void ApplyStyle()
        {
            if (_visual == null)
            {
                return;
            }

            if (_background != null)
            {
                _background.color = _visual.BackgroundColor;
            }

            if (_playTimeLabel != null)
            {
                _playTimeLabel.fontSize = _visual.PlayTimeFontSize;
                _playTimeLabel.color = _visual.PlayTimeColor;
                TextOutline.Apply(_playTimeLabel, _visual.PlayTimeOutlineColor, _visual.PlayTimeOutlineWidth);
            }

            if (_stageLabel != null)
            {
                _stageLabel.fontSize = _visual.StageFontSize;
                _stageLabel.color = _visual.StageColor;
            }

            if (_roomIcon != null)
            {
                _roomIcon.color = _visual.RoomIconColor;
            }

            if (_roomIconDetail != null)
            {
                _roomIconDetail.color = _visual.RoomIconDetailColor;
            }

            if (_healthField != null)
            {
                _healthField.SetStyle(_visual.HealthFontSize, _visual.HealthColor);
            }

            if (_goldField != null)
            {
                _goldField.SetStyle(_visual.GoldFontSize, _visual.GoldColor);
            }

            if (_tooltip != null)
            {
                _tooltip.ApplyStyle();
            }
        }

        /// <summary>① 플레이 시간.</summary>
        public void SetPlayTime(string text)
        {
            if (_playTimeLabel != null)
            {
                _playTimeLabel.text = text;
            }
        }

        /// <summary>
        /// ② 위치. 스테이지 아이콘과 단계 숫자, 현재 방 아이콘을 함께 보여 준다.
        /// roomDetail 은 방 아이콘의 안쪽 무늬다. 없으면 얹지 않는다.
        /// </summary>
        public void SetLocation(Sprite stageIcon, string stageText, Sprite roomIcon, Sprite roomDetail)
        {
            if (_stageIcon != null)
            {
                if (stageIcon != null)
                {
                    _stageIcon.sprite = stageIcon;
                }

                _stageIcon.enabled = _stageIcon.sprite != null;
            }

            if (_stageLabel != null)
            {
                _stageLabel.text = stageText;
            }

            // 방 아이콘은 받은 그대로 둔다. 아직 방에 들어가지 않아 비어 있으면 감춘다.
            // 예전 그림을 남겨 두면 런을 시작한 맵 화면에 확인용 견본 그림이 그대로 보였다(2026년 10월 20일 원재).
            if (_roomIcon != null)
            {
                _roomIcon.sprite = roomIcon;
                _roomIcon.enabled = roomIcon != null;
            }

            if (_roomIconDetail != null)
            {
                _roomIconDetail.sprite = roomDetail;
                _roomIconDetail.enabled = roomIcon != null && roomDetail != null;
            }
        }

        /// <summary>③ 체력.</summary>
        public void SetHealthText(string text)
        {
            if (_healthField != null)
            {
                _healthField.SetText(text);
            }
        }

        /// <summary>체력 숫자의 색을 짧은 시간 바꾼다.</summary>
        public void FlashHealth(Color color, float seconds)
        {
            if (_healthField != null)
            {
                _healthField.Flash(color, seconds);
            }
        }

        /// <summary>④ 골드.</summary>
        public void SetGoldText(string text)
        {
            if (_goldField != null)
            {
                _goldField.SetText(text);
            }
        }

        /// <summary>체력 칸의 아이콘을 넣는다.</summary>
        public void SetHealthIcon(Sprite sprite)
        {
            if (_healthField != null && _layout != null)
            {
                _healthField.SetIcon(sprite, _layout.GetIconSize());
            }
        }

        /// <summary>골드 칸의 아이콘을 넣는다.</summary>
        public void SetGoldIcon(Sprite sprite)
        {
            if (_goldField != null && _layout != null)
            {
                _goldField.SetIcon(sprite, _layout.GetIconSize());
            }
        }

        /// <summary>해당 항목의 버튼을 찾는다.</summary>
        public TopBarButton GetButton(TopBarButtonKind kind)
        {
            if (_buttons == null)
            {
                return null;
            }

            for (int i = 0; i < _buttons.Length; i++)
            {
                if (_buttons[i] != null && _buttons[i].Kind == kind)
                {
                    return _buttons[i];
                }
            }

            return null;
        }

        /// <summary>표시줄을 화면 상단에 좌우 여백 없이 붙인다.</summary>
        private void ApplyBar()
        {
            RectTransform rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, _layout.BarHeight);
            rect.anchoredPosition = Vector2.zero;

            // 장식 띠는 표시줄 아래 가장자리에 윗변을 붙여 바깥으로 늘어뜨린다.
            if (_bottomTrim != null)
            {
                RectTransform trim = _bottomTrim.rectTransform;
                trim.anchorMin = new Vector2(0f, 0f);
                trim.anchorMax = new Vector2(1f, 0f);
                trim.pivot = new Vector2(0.5f, 1f);
                trim.sizeDelta = new Vector2(0f, _layout.TrimHeight);
                trim.anchoredPosition = Vector2.zero;
                _bottomTrim.raycastTarget = false;
                _bottomTrim.enabled = _bottomTrim.sprite != null;
            }
        }

        /// <summary>런 요약 한 칸을 표시줄 왼쪽 기준 x 에 놓는다.</summary>
        private static void PlaceSummary(RectTransform rect, float x)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(x, 0f);
        }

        private static void ApplyIconSize(Image icon, float iconSize)
        {
            if (icon == null)
            {
                return;
            }

            icon.preserveAspect = true;
            icon.rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
        }

    }
}
