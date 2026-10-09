using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.RunResult.UI
{
    /// <summary>
    /// 런 종료 결과 화면 표시.
    /// 인게임 화면 기획서 v0.2 / 12 런 종료 결과 화면 의 배치를 맡는다.
    ///
    /// 결과가 화면 위쪽 가운데에 크게 들어가고
    /// 왼쪽에 도달 지점과 최종 구성, 오른쪽에 이번 런 기록과 해금이 들어간다.
    /// 타이틀로는 우측 하단이다.
    /// </summary>
    public class RunResultScreenView : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private RunResultLayoutConfig _layout;
        [SerializeField] private RunResultVisualConfig _visual;

        [Header("바탕")]
        [SerializeField] private Image _background;

        [Header("결과")]
        [SerializeField] private Image _outcomePanel;
        [SerializeField] private TMP_Text _outcomeLabel;

        [Header("도달 지점")]
        [SerializeField] private Image _locationPanel;
        [SerializeField] private TMP_Text _locationLabel;

        [Header("세 칸")]
        [SerializeField] private RunResultPanelView _finalBuildPanel;
        [SerializeField] private RunResultPanelView _runRecordPanel;
        [SerializeField] private RunResultPanelView _unlockPanel;

        [Header("타이틀로")]
        [SerializeField] private Button _titleButton;
        [SerializeField] private Image _titleButtonPanel;
        [SerializeField] private TMP_Text _titleButtonLabel;

        /// <summary>타이틀로를 눌렀을 때.</summary>
        public event Action TitleClicked;

        private void Awake()
        {
            if (_titleButton != null)
            {
                _titleButton.onClick.AddListener(HandleTitleClicked);
            }

            ApplyLayout();
        }

        private void OnDestroy()
        {
            if (_titleButton != null)
            {
                _titleButton.onClick.RemoveListener(HandleTitleClicked);
            }
        }

        /// <summary>
        /// 기획서 12장의 자리를 실제 RectTransform 에 적용한다.
        /// 씬에서 자리를 눈으로 확인하려면 컴포넌트를 우클릭해 바로 부르면 된다.
        /// </summary>
        [ContextMenu("기획서 자리로 맞추기")]
        public void ApplyLayout()
        {
            if (_layout == null)
            {
                return;
            }

            if (!_layout.ColumnsDoNotOverlap())
            {
                Debug.LogWarning("왼쪽 칸과 오른쪽 칸이 겹친다. 자리를 다시 본다.", this);
            }

            PlaceTopLeft(_outcomePanel, _layout.OutcomePosition, _layout.OutcomeSize);
            PlaceTopLeft(_locationPanel, _layout.LocationPosition, _layout.LocationSize);
            PlaceTopLeft(_titleButtonPanel, _layout.TitleButtonPosition, _layout.TitleButtonSize);

            PlacePanel(_finalBuildPanel, _layout.FinalBuildPosition, _layout.FinalBuildSize);
            PlacePanel(_runRecordPanel, _layout.RunRecordPosition, _layout.RunRecordSize);
            PlacePanel(_unlockPanel, _layout.UnlockPosition, _layout.UnlockSize);

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

            SetPanelColor(_outcomePanel);
            SetPanelColor(_locationPanel);
            SetPanelColor(_titleButtonPanel);

            if (_outcomeLabel != null)
            {
                _outcomeLabel.fontSize = _visual.OutcomeFontSize;
                _outcomeLabel.alignment = TextAlignmentOptions.Center;
            }

            if (_locationLabel != null)
            {
                _locationLabel.fontSize = _visual.LocationFontSize;
                _locationLabel.color = _visual.TextColor;
                _locationLabel.alignment = TextAlignmentOptions.Center;
            }

            if (_titleButtonLabel != null)
            {
                _titleButtonLabel.text = _visual.TitleButtonText;
                _titleButtonLabel.fontSize = _visual.ButtonFontSize;
                _titleButtonLabel.color = _visual.TextColor;
                _titleButtonLabel.alignment = TextAlignmentOptions.Center;
            }
        }

        /// <summary>결과 한 벌을 화면에 올린다.</summary>
        public void Show(RunResultData data)
        {
            if (data == null || _layout == null)
            {
                return;
            }

            if (_outcomeLabel != null && _visual != null)
            {
                _outcomeLabel.text = _visual.GetOutcomeText(data.Outcome);
                _outcomeLabel.color = _visual.GetOutcomeColor(data.Outcome);
            }

            if (_locationLabel != null)
            {
                _locationLabel.text = data.LocationText;
            }

            SetPanelLines(_finalBuildPanel, data.FinalBuild, _layout.FinalBuildSize);
            SetPanelLines(_runRecordPanel, data.RunRecord, _layout.RunRecordSize);
            SetPanelLines(_unlockPanel, data.Unlocks, _layout.UnlockSize);
        }

        /// <summary>화면을 보이거나 감춘다.</summary>
        public void SetVisible(bool value)
        {
            gameObject.SetActive(value);
        }

        private void SetPanelLines(
            RunResultPanelView panel,
            System.Collections.Generic.List<ResultLine> lines,
            Vector2 panelSize)
        {
            if (panel != null)
            {
                panel.SetLines(lines, _layout, _visual, panelSize);
            }
        }

        private void PlacePanel(RunResultPanelView panel, Vector2 position, Vector2 size)
        {
            if (panel == null)
            {
                return;
            }

            PlaceTopLeft(panel.Panel, position, size);

            if (panel.Content != null && _layout != null)
            {
                // 보이는 칸은 칸 안쪽 여백만큼 사방으로 들어가 앉는다.
                // 높이를 size.y 그대로 두면 아래 여백만큼 칸 밖으로 삐져나간다.
                // 여기서 잡은 높이가 GetLinesInPanel 이 세는 높이와 같아야 한다.
                Vector2 contentPosition = new Vector2(
                    position.x + _layout.PanelPadding,
                    position.y + _layout.PanelTopPadding);
                Vector2 contentSize = new Vector2(
                    size.x - _layout.PanelPadding * 2f,
                    size.y - _layout.PanelTopPadding * 2f);
                PlaceTopLeftRect(panel.Content, contentPosition, contentSize);
            }
        }

        private void SetPanelColor(Image image)
        {
            if (image != null && _visual != null)
            {
                image.color = _visual.PanelColor;
            }
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

        private void HandleTitleClicked()
        {
            if (TitleClicked != null)
            {
                TitleClicked();
            }
        }
    }
}
