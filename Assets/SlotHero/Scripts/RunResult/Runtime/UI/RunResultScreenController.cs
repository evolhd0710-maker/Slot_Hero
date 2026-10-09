using System;
using UnityEngine;

namespace SlotHero.RunResult.UI
{
    /// <summary>
    /// 런 종료 결과 화면의 흐름.
    /// 인게임 화면 기획서 v0.2 / 12 런 종료 결과 화면 을 맡는다.
    ///
    /// 화면을 열 때 런 데이터를 지우라고 알리고, 타이틀로를 누르면 타이틀로 가자고 알린다.
    /// 실제로 지우고 옮기는 일은 저장 시스템과 화면 전환이 맡는다.
    /// </summary>
    public class RunResultScreenController : MonoBehaviour
    {
        [Header("화면")]
        [SerializeField] private RunResultScreenView _view;

        [Tooltip("켜 두면 시작할 때 화면을 감춘다. Open 을 부르면 나타난다.")]
        // 기본은 끈다. 이 조종기는 제가 숨기는 화면 위에 붙어 있고, 게임 씬은 화면을 꺼 둔 채 시작한다.
        // 꺼진 오브젝트는 처음 켜질 때 Awake 가 돌므로, 켜 두면 흐름이 처음 열 때
        // 켜지는 순간 도로 숨겨 화면이 안 나온다. 켜진 채 시작하는 씬에서만 켠다.
        [SerializeField] private bool _hideOnAwake;

        private RunResultData _data;
        private bool _deleteRequested;

        /// <summary>
        /// 런 데이터를 지워야 할 때.
        /// 기획서의 "이 화면에 진입하는 시점에 런 데이터를 삭제한다"에 해당한다.
        /// 화면을 한 번 여는 동안 한 번만 부른다.
        /// </summary>
        public event Action RunDataDeleteRequested;

        /// <summary>타이틀 화면으로 가야 할 때.</summary>
        public event Action TitleRequested;

        /// <summary>지금 화면에 올라간 결과.</summary>
        public RunResultData Data
        {
            get { return _data; }
        }

        private void Awake()
        {
            if (_view != null)
            {
                _view.TitleClicked += HandleTitleClicked;

                if (_hideOnAwake)
                {
                    _view.SetVisible(false);
                }
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.TitleClicked -= HandleTitleClicked;
            }
        }

        /// <summary>결과를 받아 화면을 연다. 여는 순간 런 데이터를 지우라고 알린다.</summary>
        public void Open(RunResultData data)
        {
            if (data == null)
            {
                return;
            }

            _data = data;

            if (_view != null)
            {
                _view.Show(data);
                _view.SetVisible(true);
            }

            if (!_deleteRequested)
            {
                _deleteRequested = true;

                if (RunDataDeleteRequested != null)
                {
                    RunDataDeleteRequested();
                }
            }
        }

        /// <summary>화면을 감춘다. 다시 열면 런 데이터 삭제를 또 알리지 않는다.</summary>
        public void Close()
        {
            if (_view != null)
            {
                _view.SetVisible(false);
            }
        }

        private void HandleTitleClicked()
        {
            if (TitleRequested != null)
            {
                TitleRequested();
            }
        }
    }
}
