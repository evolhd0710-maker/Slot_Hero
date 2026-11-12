using System;
using SlotHero.Combat;
using SlotHero.Combat.UI;
using SlotHero.CurrentBuild;
using SlotHero.Ui;
using TMPro;
using UnityEngine;

namespace SlotHero.Flow.UI
{
    /// <summary>
    /// 이벤트의 태그 선택 창. 이벤트 기획서 07 기록 말살 화면 3 의 "태그 선택 창에서 전체 태그 중 1개를 고른다" 를 맡는다.
    ///
    /// **임시 창이다.** 태그 선택 창을 따로 만들기 전까지 전투의 태그 원판(`TagDialView`)을 빌려 쓴다(2026년 11월 11일 원재).
    /// 화면을 어둡게 덮고 가운데에 원판을 띄운다. 아홉 태그 칸은 모두 누를 수 있고 소용돌이 칸은 끈다.
    /// 칸에 마우스를 올리면 아래 글에 그 태그 이름이 나온다. 가운데 메달을 누르거나 ESC 를 누르면 고르지 않고 닫는다.
    /// </summary>
    public class EventTagPicker : MonoBehaviour, IEventTagPicker
    {
        [Header("참조")]
        [SerializeField] private TagDialView _dial;
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _hintLabel;

        [Header("설정")]
        [Tooltip("원판 크기와 칸 각도. 전투 화면과 같은 설정이다.")]
        [SerializeField] private CombatLayoutConfig _layout;
        [SerializeField] private CombatVisualConfig _visual;

        [Tooltip("태그 색과 아이콘과 이름. 현재 빌드 화면과 같은 설정이다.")]
        [SerializeField] private CurrentBuildVisualConfig _tags;

        [Header("글")]
        [SerializeField] private string _title = "태그 선택";
        [Tooltip("가운데 메달에는 글을 쓰지 않는다. 메달 그림에 묻혀 읽히지 않아 취소하는 법도 여기에 적는다(2026년 11월 11일).")]
        [SerializeField] private string _hint = "태그를 누르면 고르고, 가운데를 누르면 닫는다";

        [Tooltip("이 키로 취소한다.")]
        [SerializeField] private KeyCode[] _cancelKeys = { KeyCode.Escape };

        private Action<SymbolTagType> _onPicked;
        private Action _onCancelled;
        private bool _subscribed;

        /// <summary>창이 열려 있는지.</summary>
        public bool IsOpen
        {
            get { return gameObject.activeInHierarchy; }
        }

        /// <summary>빌려 쓰는 원판. 검사가 칸을 눌러 본다.</summary>
        public TagDialView Dial
        {
            get { return _dial; }
        }

        /// <summary>아래 글. 검사가 마우스를 올린 태그 이름이 나오는지 본다.</summary>
        public string HintText
        {
            get { return _hintLabel != null ? _hintLabel.text : string.Empty; }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (!IsOpen || _cancelKeys == null)
            {
                return;
            }

            for (int i = 0; i < _cancelKeys.Length; i++)
            {
                // 같은 ESC 를 흐름도 읽는다. 먼저 가져간 쪽만 쓴다. KeyGate 참고.
                if (KeyGate.TryUse(_cancelKeys[i]))
                {
                    Cancel();
                    return;
                }
            }
        }

        /// <summary>창을 연다. 태그를 누르면 onPicked, 취소하면 onCancelled 를 부른다.</summary>
        public void Open(Action<SymbolTagType> onPicked, Action onCancelled)
        {
            _onPicked = onPicked;
            _onCancelled = onCancelled;

            // 켜야 원판의 Awake 가 칸 누르기를 잇는다. 그 뒤에 칸을 맞춘다.
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Subscribe();

            if (_titleLabel != null)
            {
                _titleLabel.text = _title;
            }

            SetHint(_hint);

            if (_dial != null)
            {
                _dial.Configure(_layout, _visual, _tags);
                _dial.SetAvailable(tag => true, false);
                _dial.SetCenterCoin(null, string.Empty);
            }
        }

        /// <summary>창만 닫는다. 아무것도 알리지 않는다. 흐름이 화면을 치울 때 쓴다.</summary>
        public void Close()
        {
            _onPicked = null;
            _onCancelled = null;
            gameObject.SetActive(false);
        }

        /// <summary>고르지 않고 닫는다.</summary>
        public void Cancel()
        {
            if (!IsOpen)
            {
                return;
            }

            Action callback = _onCancelled;
            Close();

            if (callback != null)
            {
                callback();
            }
        }

        private void Subscribe()
        {
            if (_subscribed || _dial == null)
            {
                return;
            }

            _dial.TagClicked += HandleTagClicked;
            _dial.TagHoverChanged += HandleTagHover;
            _dial.CenterClicked += Cancel;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed || _dial == null)
            {
                return;
            }

            _dial.TagClicked -= HandleTagClicked;
            _dial.TagHoverChanged -= HandleTagHover;
            _dial.CenterClicked -= Cancel;
            _subscribed = false;
        }

        private void HandleTagClicked(SymbolTagType tag)
        {
            if (!IsOpen)
            {
                return;
            }

            Action<SymbolTagType> callback = _onPicked;
            Close();

            if (callback != null)
            {
                callback(tag);
            }
        }

        private void HandleTagHover(SymbolTagType tag, bool hovering)
        {
            SetHint(hovering ? SymbolTags.GetName(tag, _tags) : _hint);
        }

        private void SetHint(string text)
        {
            if (_hintLabel != null)
            {
                _hintLabel.text = text;
            }
        }
    }
}
