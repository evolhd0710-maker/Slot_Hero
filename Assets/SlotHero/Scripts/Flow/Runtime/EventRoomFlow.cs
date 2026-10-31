using System.Collections.Generic;
using SlotHero.CurrentBuild;
using SlotHero.Events;

namespace SlotHero.Flow
{
    /// <summary>각본이 내놓은 한 걸음을 받아 흐름이 할 일.</summary>
    public enum EventStepAction
    {
        /// <summary>다음 화면을 이벤트 화면에 얹는다.</summary>
        Show = 0,

        /// <summary>전투를 연다. 전투가 끝나면 `EventRoomFlow.ResumeAfterCombat` 으로 돌아온다.</summary>
        StartCombat = 1,

        /// <summary>각본이 끝났거나 갈 화면이 없다. 방을 마친다.</summary>
        Leave = 2,
    }

    /// <summary>
    /// 이벤트 방 하나를 굴린다. 각본을 고르고, 선택지를 받아 작업을 걸고, 전투를 연 선택지면 전투 뒤에 되돌아온다.
    ///
    /// 각본은 화면 여럿을 오가는 꼴이고 화면을 그리는 쪽은 쪽 하나가 자라는 꼴이라
    /// 그 사이를 `EventRunner` 가 옮긴다. 선택지에 무엇을 지목할지와 작업을 실제로 거는 것은 `RunEventWorld` 다.
    /// 같은 시드로 같은 방에 들어가면 같은 각본, 같은 선택지가 나온다.
    ///
    /// 2026년 10월 8일에 `GameFlowController` 에서 떼어 냈다. `RunSession` 참고.
    /// 이벤트 화면과 고르기 화면을 여닫는 일, 전투를 여는 일은 흐름 조종기에 남는다.
    /// </summary>
    public class EventRoomFlow
    {
        private readonly RunEventConfig _config;
        private readonly RunCatalogConfig _catalogConfig;
        private readonly CurrentBuildVisualConfig _buildVisual;

        /// 지금 돌고 있는 각본. 이벤트 방 밖에서는 null 이다.
        private EventRunner _runner;

        /// 그 각본이 런에 대고 작업을 거는 창구.
        private RunEventWorld _world;

        /// 이벤트가 연 전투를 끝내고 돌아갈 선택지. 비어 있으면 이벤트 전투가 아니다.
        private string _combatChoiceId;

        /// 이번 전투 보상을 두 배로 줄지. 도전 이벤트가 켜 둔다.
        private bool _doubleCombatReward;

        /// 이번 전투에 걸 부정 효과. 도전 이벤트(35 도전자, 38 매복)가 정한다. 걸지 않으면 빈 글이다.
        private string _penaltyId = string.Empty;
        private string _penaltyText = string.Empty;

        /// <summary>
        /// 각본 목록과 물건 목록, 태그 이름을 물려 만든다.
        /// 태그 이름은 현재 빌드 화면과 같은 것을 쓴다. 02 · 04 · 05 가 태그를 선택지에 적는다.
        /// </summary>
        public EventRoomFlow(RunEventConfig config, RunCatalogConfig catalogConfig, CurrentBuildVisualConfig buildVisual)
        {
            _config = config;
            _catalogConfig = catalogConfig;
            _buildVisual = buildVisual;
        }

        /// <summary>
        /// 지금 각본을 가리키는 표. 고르기 화면을 연 사이에 방이 바뀌었는지 볼 때 쓴다.
        /// 고르기 화면을 열 때 받아 두고, 골랐을 때 아직 같은지 견준다.
        /// </summary>
        public object Token
        {
            get { return _runner; }
        }

        /// <summary>이번 전투 보상을 두 배로 줄지.</summary>
        public bool DoubleCombatReward
        {
            get { return _doubleCombatReward; }
        }

        /// <summary>
        /// 이벤트가 연 전투에 넘길 조건. 부정 효과와 보상 두 배다. 3장 도전 이벤트 공통 규칙.
        /// 예전에는 각본이 부정 효과를 고르게만 하고 전투에 넘기지 않았다. 2026년 10월 8일 외부 검토가 짚었다.
        /// </summary>
        public CombatOptions CombatOptions
        {
            get
            {
                CombatOptions options = CombatOptions.None;
                options.FromEvent = true;
                options.DoubleReward = _doubleCombatReward;
                options.PenaltyId = _penaltyId;
                options.PenaltyText = _penaltyText;
                return options;
            }
        }

        /// <summary>이벤트가 연 전투가 끝나기를 기다리는지.</summary>
        public bool IsWaitingForCombat
        {
            get { return _runner != null && !string.IsNullOrEmpty(_combatChoiceId); }
        }

        /// <summary>
        /// 이벤트 방에 들어가 첫 쪽을 만든다. 각본을 고를 수 없으면 null 이다. 그때 흐름은 방을 마친다.
        /// roomKey 는 방 하나를 런 전체에서 가리키는 번호다(`GameFlowController.RoomKey`).
        /// </summary>
        public EventPage Begin(RunContext context, int roomKey, out string eventId)
        {
            eventId = string.Empty;
            Forget();

            if (_config == null || context == null)
            {
                return null;
            }

            // 지금 스테이지에 나올 수 있고 이번 런에서 아직 만나지 않은 이벤트만 출현 비중대로 뽑는다.
            // 같은 런에서는 같은 이벤트를 다시 보지 않는다(2026년 10월 9일 원재).
            if (context.Run.SeenEventIds == null)
            {
                context.Run.SeenEventIds = new List<string>();
            }

            EventScript script = _config.Pick(context.Run.Seed, roomKey, context.Run.StageIndex, context.Run.SeenEventIds);
            if (script == null)
            {
                return null;
            }

            _world = new RunEventWorld(context, _catalogConfig, _buildVisual);
            _world.CombatPenalties = _config.CombatPenalties;
            _world.EventId = script.EventId;
            _runner = new EventRunner(script, _world, EventRandom.ForEvent(context.Run.Seed, roomKey));
            _runner.MaxChoices = _config.MaxChoicesPerScreen;

            EventPage page = _runner.Begin();
            if (page == null)
            {
                Forget();
                return null;
            }

            // 만난 것으로 적는다. 저장은 방 완료 저장이 한다. 방 안에서 나갔다 이어 하면 파일에는 아직 없어 같은 이벤트를 다시 뽑는다.
            if (!context.Run.SeenEventIds.Contains(script.EventId))
            {
                context.Run.SeenEventIds.Add(script.EventId);
            }

            eventId = script.EventId;
            return page;
        }

        /// <summary>그 선택지가 고르기 화면을 여는지. 연다면 무엇에서 고르는지.</summary>
        public EventPickSource GetPickSource(string choiceId)
        {
            return _runner != null ? _runner.GetPickSource(choiceId) : EventPickSource.None;
        }

        /// <summary>
        /// 그 선택지가 받을 코인이나 유물이 소지 한도를 넘으면, 무엇을 몇 개 버려야 하는지 알려 준다.
        /// 흐름이 고르기 전에 버릴 것을 고르게 한다. 포기하면 선택지를 고르지 않은 것으로 하고 화면에 남는다.
        /// incomingName 은 버릴 것 고르기 창에 적을 받을 것의 이름이다.
        /// </summary>
        public bool TryGetRoomNeed(string choiceId, out ItemPickKind kind, out int count, out string incomingName)
        {
            kind = ItemPickKind.Coin;
            count = 0;
            incomingName = string.Empty;

            if (_runner == null || _world == null || !_runner.NeedsRoom(choiceId))
            {
                return false;
            }

            EventAction action = _runner.GetAction(choiceId);
            count = _world.RoomShortfall(action);
            if (count <= 0)
            {
                return false;
            }

            kind = action.Kind == EventActionKind.GainCoin ? ItemPickKind.Coin : ItemPickKind.Relic;
            incomingName = _runner.DescribeGain(choiceId);
            if (string.IsNullOrEmpty(incomingName))
            {
                incomingName = kind == ItemPickKind.Coin ? "새 코인" : "새 유물";
            }

            return true;
        }

        /// <summary>선택지를 골랐다. 고르기 화면에서 고른 것이 있으면 pickedId 로 넘긴다.</summary>
        public EventStep Choose(string choiceId, string pickedId)
        {
            if (_runner == null)
            {
                EventStep done = new EventStep();
                done.Finished = true;
                return done;
            }

            return _runner.Choose(choiceId, pickedId ?? string.Empty);
        }

        /// <summary>
        /// 각본이 내놓은 한 걸음을 흐름이 할 일로 바꾼다.
        /// 전투를 여는 걸음이면 돌아올 선택지와 보상 두 배를 적어 둔다.
        /// </summary>
        public EventStepAction Interpret(EventStep step, string choiceId)
        {
            if (step.StartsCombat)
            {
                _combatChoiceId = choiceId;
                _doubleCombatReward = step.DoubleCombatReward;
                _penaltyId = step.WithCombatPenalty && step.CombatPenaltyId != null ? step.CombatPenaltyId : string.Empty;
                _penaltyText = step.WithCombatPenalty && step.CombatPenaltyText != null ? step.CombatPenaltyText : string.Empty;

                if (_world != null)
                {
                    _world.DoubleCombatReward = step.DoubleCombatReward;
                }

                return EventStepAction.StartCombat;
            }

            // 각본이 끝났거나 갈 화면이 없다.
            // 받아 주지 않으면 화면이 멈춰 방을 나갈 수가 없고 런이 막힌다.
            if (step.Finished || step.Result == null)
            {
                return EventStepAction.Leave;
            }

            return EventStepAction.Show;
        }

        /// <summary>
        /// 이벤트가 연 전투가 끝났다. 전투를 연 선택지가 적어 둔 화면으로 돌아간다.
        /// 기획서가 "전투가 끝나면 마무리 화면으로 넘어간다" 로 정했다.
        /// </summary>
        public EventStep ResumeAfterCombat(out string choiceId)
        {
            choiceId = _combatChoiceId;
            _combatChoiceId = null;

            if (_runner == null)
            {
                EventStep done = new EventStep();
                done.Finished = true;
                return done;
            }

            return _runner.ResumeAfterCombat(choiceId);
        }

        /// <summary>
        /// 이벤트가 여는 고르기 화면의 내용. 무엇에서 고르는지에 따라 칸과 글이 달라진다.
        /// 08 피의 거래 는 모든 문양에서, 나머지는 가진 것에서 고른다.
        /// </summary>
        public ItemPickRequest MakePickRequest(EventPickSource source, RunContext context)
        {
            CurrentBuildSnapshot build = context.BuildSnapshot();
            ItemPickRequest request = new ItemPickRequest();
            request.Build = build;

            switch (source)
            {
                case EventPickSource.AllSymbols:
                    request.Kind = ItemPickKind.Symbol;
                    request.Title = "받을 문양 선택";
                    request.ShowCount = false;
                    request.ResultFormat = "[{0}] 을 받는다";
                    request.ConfirmText = "받기";
                    for (int i = 0; i < _catalogConfig.Symbols.Count; i++)
                    {
                        SymbolDefinition symbol = _catalogConfig.Symbols[i];
                        request.Candidates.Add(BuildEntry.Symbol(symbol.Id, symbol.DisplayName, 0, symbol.FirstTag, symbol.SecondTag));
                    }

                    break;

                case EventPickSource.OwnedCoins:
                    request.Kind = ItemPickKind.Coin;
                    request.Title = "코인 선택";
                    request.Candidates.AddRange(build.Coins);
                    break;

                case EventPickSource.OwnedRelics:
                    request.Kind = ItemPickKind.Relic;
                    request.Title = "유물 선택";
                    request.Candidates.AddRange(build.Relics);
                    break;

                default:
                    request.Kind = ItemPickKind.Symbol;
                    request.Title = "문양 선택";
                    request.Candidates.AddRange(build.Symbols);
                    break;
            }

            return request;
        }

        /// <summary>
        /// 돌고 있던 각본을 놓는다. 방을 나갈 때와 화면을 다 닫을 때 부른다.
        /// 안 놓으면 다음 전투가 끝났을 때 지난 이벤트로 돌아가려 하고,
        /// 지난 도전 이벤트의 보상 두 배가 그대로 남는다.
        /// </summary>
        public void Forget()
        {
            _runner = null;
            _world = null;
            _combatChoiceId = null;
            _doubleCombatReward = false;
            _penaltyId = string.Empty;
            _penaltyText = string.Empty;
        }
    }
}
