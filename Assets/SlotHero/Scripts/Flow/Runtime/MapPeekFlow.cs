using System;
using SlotHero.CurrentBuild.UI;
using SlotHero.Map.UI;
using SlotHero.TopBar;
using SlotHero.TopBar.UI;

namespace SlotHero.Flow
{
    /// <summary>
    /// 상단 표시줄의 지도 버튼과 지도 잠깐 보기.
    ///
    /// 2026년 10월 5일에 원재가 정한 흐름이다.
    ///
    ///   방 진행 중(성소, 이벤트, 전투, 보상)      켜짐. 누르면 지도를 잠깐 띄운다
    ///   지도를 잠깐 띄운 중                      켜짐. 누르면 방으로 돌아간다. ESC 도 같다
    ///   방 진행 중이 아님(다음 방을 고르는 맵)     꺼짐
    ///   방 진행 중이 아님 + 현재 빌드 열림        켜짐. 누르면 현재 빌드만 닫는다
    ///   방 진행 중 + 현재 빌드 열림              켜짐. 현재 빌드를 닫고 지도를 잠깐 띄운다
    ///   설정, 팝업, 고르기 화면이 떠 있음         꺼짐
    ///
    /// 현재 빌드 버튼도 거울처럼 같다. 지도를 띄운 중에 누르면 지도를 걷고 현재 빌드를 연다(흐름 조종기가 한다).
    /// 둘 다 플레이 시간은 흐른다. 2026년 10월 6일 원재가 정했다.
    ///
    /// 잠깐 띄우는 지도는 방 화면을 닫지 않는다. 지금 화면 위에 반투명 검은 막을 깔고 그 위에 지도를 얹는다.
    /// 그래서 방 안에서 무엇이 벌어지고 있든 상관없이 띄우고 걷을 수 있다. 방을 깬 것으로 치지도 않는다.
    ///
    /// 2026년 10월 8일에 `GameFlowController` 에서 떼어 냈다. 원재가 "지도 버튼과 팝업 처리도 나눠 줘" 라고 했다.
    /// 방 진행 중인지와 무엇이 겹쳐 떠 있는지는 흐름 조종기가 알려 준다.
    /// </summary>
    public class MapPeekFlow
    {
        private readonly MapScreenController _map;
        private readonly TopBarController _topBar;
        private readonly CurrentBuildScreenController _currentBuild;

        /// 진행 중인 런이 있는지.
        private readonly Func<bool> _hasRun;

        /// 방 진행 중인지. 방에 들어가 아직 맵으로 돌아오지 않았다. 보상 화면도 방 진행 중이다.
        private readonly Func<bool> _roomInProgress;

        /// 설정, 팝업, 고르기 화면처럼 다른 조작을 막는 것이 떠 있는지.
        private readonly Func<bool> _blocked;

        private bool _peeking;

        /// 지도 버튼에 마지막으로 넣은 상태. 바뀔 때만 다시 넣는다.
        private bool _buttonShown = true;
        private bool _buttonKnown;

        /// <summary>화면들과 흐름의 상태를 묻는 길을 물려 만든다.</summary>
        public MapPeekFlow(
            MapScreenController map,
            TopBarController topBar,
            CurrentBuildScreenController currentBuild,
            Func<bool> hasRun,
            Func<bool> roomInProgress,
            Func<bool> blocked)
        {
            _map = map;
            _topBar = topBar;
            _currentBuild = currentBuild;
            _hasRun = hasRun;
            _roomInProgress = roomInProgress;
            _blocked = blocked;
        }

        /// <summary>지도를 잠깐 띄워 보고 있는지.</summary>
        public bool IsPeeking
        {
            get { return _peeking; }
        }

        /// <summary>지금 지도 버튼을 누를 수 있는지. 위의 표 그대로다.</summary>
        public bool CanUseButton()
        {
            if (_hasRun == null || !_hasRun())
            {
                return false;
            }

            // 고르기 화면은 팝업처럼 다른 조작을 막는다. 고르거나 취소해야 닫힌다.
            if (_blocked != null && _blocked())
            {
                return false;
            }

            return _peeking || IsBuildOpen() || IsRoomInProgress();
        }

        /// <summary>지도 버튼을 눌렀다.</summary>
        public void HandleButton()
        {
            if (!CanUseButton())
            {
                return;
            }

            if (IsBuildOpen())
            {
                _currentBuild.Close();

                // 방 진행 중이면 이어서 지도를 띄운다. 아니면 뒤에 이미 맵이 있으므로 현재 빌드만 닫는다.
                // 현재 빌드를 열면 띄운 지도는 걷히므로 둘이 함께 떠 있는 일은 없다.
                if (IsRoomInProgress() && !_peeking)
                {
                    Start();
                }

                return;
            }

            if (_peeking)
            {
                End();
                return;
            }

            if (IsRoomInProgress())
            {
                Start();
            }
        }

        /// <summary>지도를 잠깐 띄운다. 노드는 고를 수 없다. 보기만 한다.</summary>
        public void Start()
        {
            if (_map == null)
            {
                return;
            }

            _peeking = true;
            _map.OpenPeek();
        }

        /// <summary>잠깐 띄운 지도를 걷고 방으로 돌아간다. 방은 보던 자리 그대로다.</summary>
        public void End()
        {
            _peeking = false;

            if (_map != null)
            {
                _map.Close();
            }
        }

        /// <summary>
        /// 잠깐 보기를 잊는다. 지도는 건드리지 않는다.
        /// 흐름이 화면을 다 닫거나 방에 들어가거나 맵으로 돌아가며 지도를 따로 여닫을 때 쓴다.
        /// </summary>
        public void Forget()
        {
            _peeking = false;
        }

        /// <summary>
        /// 지도 버튼을 지금 상태에 맞게 켜고 끈다. 흐름이 매 프레임 부른다.
        ///
        /// 설정, 팝업, 현재 빌드가 여닫히는 자리가 여럿이라 자리마다 부르면 빠뜨리기 쉽다.
        /// 지도 버튼을 끄는 함수가 있었는데 아무 데서도 부르지 않아 늘 켜져 있던 것이 그 예다.
        /// 값이 바뀔 때만 버튼에 넣는다.
        /// </summary>
        public void RefreshButton()
        {
            if (_topBar == null)
            {
                return;
            }

            bool usable = CanUseButton();
            if (_buttonKnown && usable == _buttonShown)
            {
                return;
            }

            _buttonKnown = true;
            _buttonShown = usable;
            _topBar.SetButtonInteractable(TopBarButtonKind.Map, usable);
        }

        private bool IsBuildOpen()
        {
            return _currentBuild != null && _currentBuild.IsOpen;
        }

        private bool IsRoomInProgress()
        {
            return _roomInProgress != null && _roomInProgress();
        }
    }
}
