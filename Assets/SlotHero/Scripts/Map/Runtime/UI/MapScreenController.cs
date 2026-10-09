using System;
using UnityEngine;

namespace SlotHero.Map.UI
{
    /// <summary>
    /// 맵 화면의 진행 처리.
    /// 맵을 만들고, 표시에 넘기고, 노드 선택을 받아 진행 상태를 갱신한다.
    ///
    /// 이 스크립트는 화면을 여닫고 어떤 노드를 골랐는지 알리는 데까지만 책임진다.
    /// 고른 방 안에서 무엇이 벌어지는지는 각 방의 기획서를 따르는 쪽에서 처리한다.
    /// </summary>
    public class MapScreenController : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private MapGenerationConfig _generationConfig;
        [SerializeField] private MapScreenView _view;
        [SerializeField] private GameObject _screenRoot;

        [Tooltip("지도를 잠깐 띄울 때 지도 뒤에 까는 반투명 검은 막. 아래 방 화면을 가리고 누르는 것을 막는다.")]
        [SerializeField] private GameObject _peekDim;

        [Header("시작 설정")]
        [Tooltip("켜면 시작할 때 아래 시드로 맵을 만든다. 실제 런에서는 런 시드를 넘겨 호출한다.")]
        [SerializeField] private bool _generateOnStart;
        [SerializeField] private int _debugSeed = 12345;
        [SerializeField] private int _stageIndex = 1;

        private StageMap _map;
        private MapProgress _progress = new MapProgress();

        /// 잠깐 띄운 지도인지. 이때는 노드를 고를 수 없다.
        private bool _peeking;

        /// <summary>노드를 골라 그 방으로 들어갈 때.</summary>
        public event Action<MapNode> RoomEntered;

        /// <summary>맵 화면이 열리거나 닫힐 때. 상단 표시줄의 지도 버튼 활성 처리에 쓴다.</summary>
        public event Action<bool> ScreenVisibilityChanged;

        public StageMap Map
        {
            get { return _map; }
        }

        public MapProgress Progress
        {
            get { return _progress; }
        }

        /// <summary>맵 화면이 열려 있는지.</summary>
        public bool IsOpen
        {
            get { return _screenRoot != null && _screenRoot.activeInHierarchy; }
        }

        private void Awake()
        {
            if (_view != null)
            {
                _view.NodeClicked += HandleNodeClicked;
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.NodeClicked -= HandleNodeClicked;
            }
        }

        private void Start()
        {
            if (_generateOnStart)
            {
                StartNewStage(_debugSeed, _stageIndex);
                Open();
            }
        }

        /// <summary>시드로 스테이지 맵을 새로 만들고 진행 상태를 처음으로 되돌린다.</summary>
        public void StartNewStage(int runSeed, int stageIndex)
        {
            _stageIndex = stageIndex;
            _map = MapGenerator.Generate(runSeed, stageIndex, _generationConfig);
            _progress.Reset();

            if (_view != null)
            {
                _view.Build(_map, _progress);
            }
        }

        /// <summary>저장에서 불러온 맵과 진행 상태를 그대로 붙인다.</summary>
        public void Load(StageMap map, MapProgress progress)
        {
            _map = map;
            if (_map != null)
            {
                _map.RebuildIndex();
            }

            _progress = progress ?? new MapProgress();

            if (_view != null)
            {
                _view.Build(_map, _progress);
                _view.FocusOnCurrentNode();
            }
        }

        /// <summary>잠깐 띄운 지도인지.</summary>
        public bool IsPeeking
        {
            get { return _peeking && IsOpen; }
        }

        /// <summary>맵 화면을 연다. 다음 방을 고르는 맵이다.</summary>
        public void Open()
        {
            SetPeeking(false);
            Show();
        }

        /// <summary>
        /// 방 진행 중에 지도를 잠깐 띄운다.
        /// 지금 화면 위에 반투명 검은 막을 깔고 그 위에 지도를 얹는다. 방 화면은 그대로 아래에 있다.
        /// 진행 상태는 건드리지 않는다. 지금 방을 깬 것으로 치지 않고, 노드도 고를 수 없다.
        /// </summary>
        public void OpenPeek()
        {
            SetPeeking(true);
            Show();
        }

        private void SetPeeking(bool value)
        {
            _peeking = value;

            if (_peekDim != null)
            {
                _peekDim.SetActive(value);
            }
        }

        private void Show()
        {
            if (_screenRoot != null)
            {
                _screenRoot.SetActive(true);
            }

            if (_view != null)
            {
                _view.Refresh();
                _view.FocusOnCurrentNode();
            }

            if (ScreenVisibilityChanged != null)
            {
                ScreenVisibilityChanged(true);
            }
        }

        /// <summary>맵 화면을 닫는다. 잠깐 띄운 지도도 이것으로 걷는다.</summary>
        public void Close()
        {
            SetPeeking(false);

            if (_screenRoot != null)
            {
                _screenRoot.SetActive(false);
            }

            if (ScreenVisibilityChanged != null)
            {
                ScreenVisibilityChanged(false);
            }
        }

        /// <summary>
        /// 방을 마치고 맵으로 돌아온다.
        /// 지금 방을 깬 것으로 치고 맵 화면을 다시 연다.
        /// **방을 끝냈을 때만 부른다.** 지도 버튼으로 잠깐 볼 때는 `OpenPeek` 이다.
        /// </summary>
        public void ReturnToMap()
        {
            ClearCurrentRoom();
            Open();
        }

        /// <summary>현재 방을 완료 처리한다. 방을 끝낸 쪽에서 호출한다.</summary>
        public void ClearCurrentRoom()
        {
            _progress.ClearCurrentRoom();

            if (_view != null)
            {
                _view.Refresh();
            }
        }

        private void HandleNodeClicked(MapNode node)
        {
            if (node == null || _map == null)
            {
                return;
            }

            // 잠깐 띄운 지도는 보기만 한다. 방 진행 중이라 고를 수 있는 노드가 원래 없지만,
            // 보상 화면 위에서 띄우면 방은 이미 깬 상태라 다음 방이 눌릴 수 있어 여기서도 막는다.
            if (_peeking)
            {
                return;
            }

            if (!_progress.EnterNode(_map, node.Id))
            {
                return;
            }

            if (_view != null)
            {
                _view.Refresh();
            }

            if (RoomEntered != null)
            {
                RoomEntered(node);
            }
        }
    }
}
