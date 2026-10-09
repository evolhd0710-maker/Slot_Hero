using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Title.UI
{
    /// <summary>
    /// 타이틀 화면 표시.
    /// 인게임 화면 기획서 v0.2 / 05 타이틀 화면 의 배치를 맡는다.
    ///
    /// 로고가 화면 왼쪽 위, 메뉴가 그 아래 세로, 버전이 오른쪽 아래다.
    /// 오른쪽 위의 현재 프로필 버튼은 `Profile` 폴더가 맡으므로 여기서 건드리지 않는다.
    /// </summary>
    public class TitleScreenView : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private TitleLayoutConfig _layout;
        [SerializeField] private TitleVisualConfig _visual;

        [Header("바탕")]
        [SerializeField] private Image _background;
        [SerializeField] private Image _logo;

        [Header("메뉴")]
        [SerializeField] private RectTransform _menuLayer;
        [SerializeField] private TitleMenuItemView _menuPrefab;

        [Header("버전")]
        [SerializeField] private TMP_Text _versionLabel;

        private readonly List<TitleMenuItemView> _items = new List<TitleMenuItemView>();

        /// <summary>메뉴 항목을 눌렀을 때.</summary>
        public event Action<TitleMenuKind> MenuClicked;

        /// <summary>메뉴 항목에 마우스가 올라가거나 벗어났을 때.</summary>
        public event Action<TitleMenuKind, bool, string> MenuHoverChanged;

        /// <summary>지금 화면에 놓인 메뉴 항목.</summary>
        public IList<TitleMenuItemView> Items
        {
            get { return _items; }
        }

        private void Awake()
        {
            ApplyLayout();
        }

        /// <summary>
        /// 기획서 05장의 자리를 실제 RectTransform 에 적용한다.
        /// 씬에서 자리를 눈으로 확인하려면 컴포넌트를 우클릭해 바로 부르면 된다.
        /// </summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            if (!_layout.LogoAndMenuShareLeft())
            {
                Debug.LogWarning("로고와 메뉴의 왼쪽 끝이 어긋난다. 자리를 다시 본다.", this);
            }

            PlaceTopLeft(_logo, _layout.LogoPosition, _layout.LogoSize);

            if (_versionLabel != null)
            {
                PlaceTopLeftRect(_versionLabel.rectTransform, _layout.VersionPosition, _layout.VersionSize);
            }

            PlaceMenuItems();
            ApplyStyle();
        }

        /// <summary>색과 글자 크기를 설정대로 맞춘다.</summary>
        public void ApplyStyle()
        {
            if (_visual == null)
            {
                return;
            }

            if (_versionLabel != null)
            {
                _versionLabel.fontSize = _visual.VersionFontSize;
                _versionLabel.color = _visual.VersionColor;
                _versionLabel.alignment = TextAlignmentOptions.Right;
            }
        }

        /// <summary>메뉴를 만든다. 설정에 적힌 차례대로 놓는다.</summary>
        public void ShowMenu()
        {
            if (_visual == null || _layout == null || _menuLayer == null || _menuPrefab == null)
            {
                return;
            }

            int count = _visual.MenuCount;

            while (_items.Count < count)
            {
                TitleMenuItemView item = Instantiate(_menuPrefab, _menuLayer);
                item.Clicked += HandleMenuClicked;
                item.HoverChanged += HandleMenuHoverChanged;
                _items.Add(item);
            }

            for (int i = 0; i < _items.Count; i++)
            {
                TitleMenuItemView item = _items[i];
                if (item == null)
                {
                    continue;
                }

                if (i >= count)
                {
                    item.gameObject.SetActive(false);
                    continue;
                }

                item.gameObject.SetActive(true);
                item.Bind(_visual.GetEntry(i), i, _layout.MenuItemSize, _visual);
            }

            PlaceMenuItems();

            if (!_layout.MenuFitsScreen(count))
            {
                Debug.LogWarning("메뉴가 화면 아래로 넘친다. 간격을 더 줄이거나 시작 자리를 올린다.", this);
            }
        }

        /// <summary>메뉴 항목의 자리를 다시 잡는다. 항목 수에 따라 간격이 달라진다.</summary>
        public void PlaceMenuItems()
        {
            if (_layout == null || _visual == null)
            {
                return;
            }

            int count = _visual.MenuCount;

            for (int i = 0; i < _items.Count && i < count; i++)
            {
                if (_items[i] == null)
                {
                    continue;
                }

                PlaceTopLeftRect(
                    _items[i].RectTransform,
                    _layout.GetMenuItemPosition(i, count),
                    _layout.MenuItemSize);
            }
        }

        /// <summary>그 종류의 메뉴 항목. 없으면 null.</summary>
        public TitleMenuItemView GetItem(TitleMenuKind kind)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null && _items[i].Kind == kind)
                {
                    return _items[i];
                }
            }

            return null;
        }

        /// <summary>버전 글을 적는다.</summary>
        public void SetVersion(string buildNumber)
        {
            if (_versionLabel != null && _visual != null)
            {
                _versionLabel.text = _visual.FormatVersion(buildNumber);
            }
        }

        /// <summary>배경 그림을 바꾼다.</summary>
        public void SetBackground(Sprite sprite)
        {
            if (_background != null)
            {
                _background.sprite = sprite;
            }
        }

        /// <summary>화면을 보이거나 감춘다.</summary>
        public void SetVisible(bool value)
        {
            gameObject.SetActive(value);
        }

        private static void PlaceTopLeft(Image image, Vector2 position, Vector2 size)
        {
            if (image != null)
            {
                PlaceTopLeftRect(image.rectTransform, position, size);
            }
        }

        /// <summary>화면 왼쪽 위를 기준으로 자리를 잡는다. 기획서 좌표를 그대로 넣을 수 있다.</summary>
        private static void PlaceTopLeftRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(position.x, -position.y);
        }

        private void HandleMenuClicked(TitleMenuKind kind)
        {
            if (MenuClicked != null)
            {
                MenuClicked(kind);
            }
        }

        private void HandleMenuHoverChanged(TitleMenuKind kind, bool hovering, string reason)
        {
            if (MenuHoverChanged != null)
            {
                MenuHoverChanged(kind, hovering, reason);
            }
        }
    }
}
