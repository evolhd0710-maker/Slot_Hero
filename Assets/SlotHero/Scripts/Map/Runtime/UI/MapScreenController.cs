using System;
using UnityEngine;
using SlotHero.Ui;

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

        [Tooltip("맵 화면을 열 때 두루마리를 펼치는 연출(효과음과 이펙트 목록 E05). 비워 두면 바로 다 펼친 지도가 나온다.")]
        [SerializeField] private MapScrollUnroll _unroll;

        [Header("시작 설정")]
        [Tooltip("켜면 시작할 때 아래 시드로 맵을 만든다. 실제 런에서는 런 시드를 넘겨 호출한다.")]
        [SerializeField] private bool _generateOnStart;
        [SerializeField] private int _debugSeed = 12345;
        [SerializeField] private int _stageIndex = 1;

        private StageMap _map;
        private string _fingerprint;
        private int _fingerprintSeed;
        private int _fingerprintStage;
        private MapProgress _progress = new MapProgress();

        /// 잠깐 띄운 지도인지. 이때는 노드를 고를 수 없다.
        private bool _peeking;

        /// 닫으려고 두루마리를 마는 중인지. 다 말면 화면을 끈다.
        private bool _closing;

        // 고른 노드가 반짝이는 동안 기다리는 노드와 지난 시간. 반짝임이 끝나면 두루마리를 만다.
        private MapNode _enteringNode;
        private float _enterTime;

        /// 두루마리를 다 말기를 기다리는 노드. 다 말면 그 방으로 들어간다.
        private MapNode _rollingNode;

        /// <summary>노드를 골라 그 방으로 들어갈 때. 고른 노드가 반짝이고 두루마리를 다 만 뒤에 울린다.</summary>
        public event Action<MapNode> RoomEntered;

        /// <summary>노드를 누른 바로 그때. 반짝임보다 먼저 울린다. 방 고르기 소리를 여기서 낸다.</summary>
        public event Action<MapNode> NodeChosen;

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

        /// <summary>
        /// 맵 화면이 열려 있는지. 닫으려고 두루마리를 마는 동안에는 이미 닫힌 것으로 친다.
        /// 마는 것은 보이기만 하고 노드를 누를 수 없다.
        /// </summary>
        public bool IsOpen
        {
            get { return _screenRoot != null && _screenRoot.activeInHierarchy && !_closing; }
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

        /// <summary>
        /// 그 시드와 스테이지로 지금 규칙이 만드는 지도의 지문(`MapFingerprint`). 화면에는 그리지 않는다.
        /// 저장된 런의 지도가 업데이트 뒤에도 같은지 볼 때 쓴다. 같은 시드와 스테이지면 앞서 잰 값을 다시 쓴다.
        /// </summary>
        public string GetFingerprint(int runSeed, int stageIndex)
        {
            if (_generationConfig == null)
            {
                return string.Empty;
            }

            if (_fingerprintSeed != runSeed || _fingerprintStage != stageIndex || string.IsNullOrEmpty(_fingerprint))
            {
                _fingerprint = MapFingerprint.Of(MapGenerator.Generate(runSeed, stageIndex, _generationConfig));
                _fingerprintSeed = runSeed;
                _fingerprintStage = stageIndex;
            }

            return _fingerprint;
        }

        // 맵을 붙이는 길은 셋이다. 셋 다 맵 만들기, 진행 붙이기, 화면 만들기를 차례로 한 번씩만 한다.
        // 예전에는 이어하기가 `StartNewStage` 로 화면을 만든 뒤 `Load` 로 같은 맵의 화면을 한 번 더 만들었다(2026년 10월 25일 코덱스 검토).

        /// <summary>시드로 스테이지 맵을 새로 만들고 진행 상태를 처음으로 되돌린다. 런 시작과 스테이지 넘김이 쓴다.</summary>
        public void StartNewStage(int runSeed, int stageIndex)
        {
            _map = GenerateMap(runSeed, stageIndex);
            _progress.Reset();
            BuildView(false);
        }

        /// <summary>
        /// 시드로 그 스테이지 맵을 다시 만들고 저장해 둔 진행 상태를 얹는다. 이어하기가 쓴다.
        /// 맵은 저장하지 않고 시드로 다시 만든다(맵 생성 규칙 기획서 v0.2 / `MapRandom.ForStage`).
        /// </summary>
        public void ResumeStage(int runSeed, int stageIndex, MapProgress progress)
        {
            _map = GenerateMap(runSeed, stageIndex);
            _progress = progress ?? new MapProgress();
            BuildView(true);
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
            BuildView(true);
        }

        /// <summary>지금 규칙으로 그 스테이지 맵을 만든다. 화면은 건드리지 않는다.</summary>
        private StageMap GenerateMap(int runSeed, int stageIndex)
        {
            _stageIndex = stageIndex;
            return MapGenerator.Generate(runSeed, stageIndex, _generationConfig);
        }

        /// <summary>지금 맵과 진행 상태로 화면을 만든다. focusCurrent 면 지금 노드가 보이게 옮긴다.</summary>
        private void BuildView(bool focusCurrent)
        {
            if (_view == null)
            {
                return;
            }

            _view.Build(_map, _progress);
            if (focusCurrent)
            {
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
            // 방으로 들어가려고 마는 중에는 열지 않는다. 열면 마는 것이 멈춰 방에 들어가지 못한다.
            if (_rollingNode != null)
            {
                return;
            }

            // 이미 떠 있는 지도를 다시 열 때는 두루마리를 다시 펼치지 않는다.
            // 마는 도중에 다시 열면 마는 것을 멈추고 처음부터 펼친다.
            bool wasOpen = IsOpen;
            _closing = false;

            if (_screenRoot != null)
            {
                _screenRoot.SetActive(true);
            }

            if (_view != null)
            {
                _view.Refresh();
                _view.FocusOnCurrentNode();
            }

            if (_unroll != null && !wasOpen)
            {
                _unroll.Play();
            }

            if (ScreenVisibilityChanged != null)
            {
                ScreenVisibilityChanged(true);
            }
        }

        /// <summary>
        /// 맵 화면을 닫는다. 잠깐 띄운 지도도 이것으로 걷는다.
        /// 두루마리를 0.3초 동안 다시 만 뒤 화면을 끈다(`MapVisualConfig.RollUpSeconds`, 2026년 10월 31일 원재).
        /// 닫힌 것으로 치는 것은 바로다. 다음 화면은 기다리지 않고 열리고 마는 두루마리가 그 위에서 잠깐 보인다.
        /// </summary>
        public void Close()
        {
            bool wasOpen = IsOpen;
            SetPeeking(false);

            // 이미 마는 중이면 마저 말고 끈다.
            if (!_closing)
            {
                if (wasOpen && _unroll != null && _unroll.PlayRollUp(HideScreen))
                {
                    _closing = true;
                }
                else
                {
                    HideScreen();
                }
            }

            if (ScreenVisibilityChanged != null)
            {
                ScreenVisibilityChanged(false);
            }
        }

        /// <summary>맵 화면을 끈다. 두루마리를 다 말았을 때나 말지 않고 닫을 때 부른다.</summary>
        private void HideScreen()
        {
            _closing = false;

            if (_unroll != null)
            {
                _unroll.CancelRollUp();
            }

            if (_screenRoot != null)
            {
                _screenRoot.SetActive(false);
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

            // 고른 노드가 반짝이는 동안에는 다른 노드를 받지 않는다.
            if (_enteringNode != null)
            {
                return;
            }

            // 두루마리를 펼치는 동안에는 노드를 받지 않는다. 뷰포트가 커서를 막지만 한 번 더 막는다.
            if (_unroll != null && _unroll.IsPlaying)
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

            if (NodeChosen != null)
            {
                NodeChosen(node);
            }

            // 고른 노드가 0.3초 반짝인 뒤 방으로 들어간다(2026년 10월 31일 원재).
            // 게임이 돌지 않으면(편집 모드 검사) 바로 들어간다.
            float seconds = _view != null && _view.Visual != null ? _view.Visual.SelectFlashSeconds : 0f;
            if (!Application.isPlaying || seconds <= 0f || !isActiveAndEnabled)
            {
                EnterRoom(node);
                return;
            }

            _enteringNode = node;
            _enterTime = 0f;

            // 반짝이는 중 화면을 누르면 반짝임과 이어지는 두루마리 말기를 한꺼번에 건너뛰고 방으로 들어간다(2026년 11월 1일 원재).
            EffectSkip.Begin(this, SkipEntering);
        }

        private void Update()
        {
            if (_enteringNode == null)
            {
                return;
            }

            float seconds = _view != null && _view.Visual != null ? _view.Visual.SelectFlashSeconds : 0f;
            _enterTime += Time.unscaledDeltaTime;
            float t = seconds <= 0f ? 1f : Mathf.Clamp01(_enterTime / seconds);

            if (_view != null)
            {
                _view.SetNodeFlash(_enteringNode, MapVisualConfig.GetFlashAmount(t));
            }

            if (t < 1f)
            {
                return;
            }

            FinishEntering(false);
        }

        /// <summary>화면을 눌러 노드 반짝임을 건너뛰었다. 말기까지 바로 끝내고 방으로 들어간다.</summary>
        private void SkipEntering()
        {
            if (_enteringNode != null)
            {
                FinishEntering(true);
            }
        }

        /// <summary>
        /// 노드 반짝임이 끝났다. 두루마리를 말고 다 말면 그 방으로 들어간다.
        /// skipRollUp 이면 말기도 바로 끝낸다.
        /// </summary>
        private void FinishEntering(bool skipRollUp)
        {
            EffectSkip.End(this);
            MapNode node = _enteringNode;
            _enteringNode = null;

            if (_view != null)
            {
                _view.SetNodeFlash(node, 0f);
            }

            // 두루마리를 다 만 뒤에 방으로 들어간다. 예전에는 말기 시작과 함께 방이 열려
            // 지도 연출이 끝나기 전에 다음 방이 나왔다(2026년 10월 31일 원재).
            if (_unroll != null && !_closing && _unroll.PlayRollUp(HandleRolledUpToRoom))
            {
                _closing = true;
                _rollingNode = node;

                if (ScreenVisibilityChanged != null)
                {
                    ScreenVisibilityChanged(false);
                }

                if (skipRollUp)
                {
                    _unroll.SkipToEnd();
                }

                return;
            }

            EnterRoom(node);
        }

        /// <summary>방으로 들어가려고 만 두루마리를 다 말았다. 맵 화면을 끄고 그 방으로 들어간다.</summary>
        private void HandleRolledUpToRoom()
        {
            MapNode node = _rollingNode;
            _rollingNode = null;
            HideScreen();

            if (node != null)
            {
                EnterRoom(node);
            }
        }

        /// <summary>
        /// 반짝이거나 마는 도중에 꺼지면 기다리지 않고 바로 방으로 들어간다. 그러지 않으면 방이 열리지 않아 런이 멈춘다.
        /// </summary>
        private void OnDisable()
        {
            EffectSkip.End(this);
            MapNode node = _enteringNode ?? _rollingNode;
            _enteringNode = null;
            _rollingNode = null;

            if (node != null)
            {
                EnterRoom(node);
            }
        }

        private void EnterRoom(MapNode node)
        {
            if (_view != null)
            {
                _view.SetNodeFlash(node, 0f);
            }

            if (RoomEntered != null)
            {
                RoomEntered(node);
            }
        }
    }
}
