using System.Collections.Generic;

namespace SlotHero.Events
{
    /// <summary>
    /// 선택을 하나 처리한 결과.
    /// 화면을 어떻게 바꿀지와 이벤트가 끝났는지를 담는다.
    /// </summary>
    public struct EventStep
    {
        /// <summary>화면에 넘길 것. 끝났으면 null 일 수 있다.</summary>
        public EventResult Result;

        /// <summary>이벤트가 끝났는지.</summary>
        public bool Finished;

        /// <summary>전투를 시작해야 하는지. 전투가 끝나면 <see cref="EventRunner.ResumeAfterCombat"/> 를 부른다.</summary>
        public bool StartsCombat;

        /// <summary>전투 보상을 두 배로 줄지.</summary>
        public bool DoubleCombatReward;

        /// <summary>전투에 부정 효과를 걸고 시작할지.</summary>
        public bool WithCombatPenalty;

        /// <summary>
        /// 걸 부정 효과의 식별자(D1 ~ D6). 3장 도전 이벤트 공통 규칙 의 목록이다.
        /// 35 도전자 는 화면 2 에서 고른 것, 38 매복 은 목록에서 무작위로 정한 것이다. 걸지 않으면 빈 글이다.
        /// </summary>
        public string CombatPenaltyId;

        /// <summary>걸 부정 효과의 글. 전투 화면에 적는다.</summary>
        public string CombatPenaltyText;
    }

    /// <summary>
    /// 이벤트 각본을 실제로 굴린다.
    ///
    /// 각본은 화면 여럿을 오가는 꼴이고, 화면을 그리는 쪽은 쪽 하나가 자라는 꼴이다.
    /// 인게임 화면 기획서 11 이벤트 화면 의 "선택 후" 가 고른 선택지를 위에 남기고
    /// 이어지는 선택지를 그 아래에 쌓으라고 정했기 때문이다.
    /// 그 사이를 여기서 옮긴다. 화면 하나가 곧 `EventResult` 하나다.
    ///
    /// 선택지를 무엇으로 채울지와 작업을 실제로 하는 것은 <see cref="IEventWorld"/> 가 맡는다.
    /// </summary>
    public class EventRunner
    {
        private readonly EventScript _script;
        private readonly IEventWorld _world;
        private readonly EventRandom _random;

        /// 지금 화면의 선택지마다 지목해 둔 대상. 선택지 식별자로 찾는다.
        private readonly Dictionary<string, EventItem> _targets = new Dictionary<string, EventItem>();

        /// 화면에 나간 선택지가 각본의 어느 줄에서 나온 것인지.
        ///
        /// **각본의 선택지 하나가 화면에서는 여럿이 된다.**
        /// "보유 문양 중 무작위 3종" 한 줄이 선택지 셋으로 불어나기 때문이다.
        /// 그래서 화면에서 돌아온 식별자로 각본 줄을 되찾을 길이 따로 있어야 한다.
        private readonly Dictionary<string, EventScreenChoice> _made =
            new Dictionary<string, EventScreenChoice>();

        /// 앞 화면에서 고른 것을 기억해 둔다. 선택지의 `RememberAs` 이름으로 찾는다.
        /// 02 힘을 새기는 것 이 화면 2 의 태그를 화면 3 에서 쓰는 것이 이것이다.
        /// 이벤트 하나 안에서만 산다. 이벤트 방에 들어갈 때마다 러너를 새로 만든다.
        private readonly Dictionary<string, string> _memory = new Dictionary<string, string>();

        /// 기억해 둔 것의 이름. 35 도전자 가 화면 2 에서 고른 부정 효과의 글을 전투에 넘기는 데 쓴다.
        private readonly Dictionary<string, string> _memoryNames = new Dictionary<string, string>();

        /// 공개하는 선택지마다 미리 정해 둔 결과. 선택지 식별자로 찾는다.
        /// 칸에 적은 이름과 실제로 받는 것이 같도록 고를 때 그대로 작업에 넘긴다.
        private readonly Dictionary<string, List<EventItem>> _decided = new Dictionary<string, List<EventItem>>();

        /// 지금 화면에서 각본 줄마다 채운 대상. 각본 줄 식별자로 찾는다.
        /// TakeAllFromId 가 다른 선택지가 지목한 것을 모두 받는 데 쓴다.
        private readonly Dictionary<string, List<EventItem>> _filledBySpec = new Dictionary<string, List<EventItem>>();

        /// 지금 화면에서 체력이 모자라서만 막힌 강제 손실 선택지. 다른 선택지가 모두 막히면 다시 연다.
        /// 4장 공통 규칙 · 부정 이벤트와 무작위 이벤트의 체력 손실 선택지가 그렇다.
        private readonly HashSet<string> _forcedLoss = new HashSet<string>();

        /// 지금 화면에서 받을 코인이나 유물이 소지 한도를 넘는 선택지. 고르면 흐름이 버릴 것을 고르게 한다.
        /// 이런 선택지뿐인 화면에는 떠나는 선택지를 끼운다. 획득을 포기할 길이다(2026년 10월 9일 원재).
        private readonly HashSet<string> _needsRoom = new HashSet<string>();

        private EventScreen _screen;

        /// 전투가 끝난 뒤 갈 화면. 전투를 여는 선택지가 적어 둔다.
        private string _afterCombatScreenId;

        public EventRunner(EventScript script, IEventWorld world, EventRandom random)
        {
            _script = script;
            _world = world;
            _random = random ?? new EventRandom(1);
        }

        /// <summary>
        /// 화면 하나에 나오는 선택지의 최대 수. 0 이하면 따지지 않는다.
        /// 흐름이 이벤트 목록의 화면당 최대 선택지 수(5)를 넣는다. 저절로 끼우는 떠나는 선택지도 이 안에 넣는다(`EnsureWayOut`).
        /// </summary>
        public int MaxChoices { get; set; }

        /// <summary>지금 보고 있는 화면. 아직 시작하지 않았으면 null 이다.</summary>
        public EventScreen Screen
        {
            get { return _screen; }
        }

        /// <summary>이 각본의 이벤트 식별자.</summary>
        public string EventId
        {
            get { return _script != null ? _script.EventId : string.Empty; }
        }

        /// <summary>첫 화면을 연다. 화면에 그대로 넘길 쪽을 돌려준다.</summary>
        public EventPage Begin()
        {
            if (_script == null || _script.StartScreen == null)
            {
                return null;
            }

            return Enter(_script.StartScreen);
        }

        /// <summary>
        /// 선택지를 하나 골랐다.
        ///
        /// 작업을 하고 다음 화면을 정해 `EventResult` 로 만든다.
        /// 종료 선택지면 끝났다고 알린다.
        /// </summary>
        public EventStep Choose(string choiceId)
        {
            return Choose(choiceId, string.Empty);
        }

        /// <summary>
        /// 그 선택지가 고르기 화면을 여는지. 연다면 무엇에서 고르는지.
        /// 흐름이 이것을 보고 고르기 화면을 연 뒤 고른 것을 <see cref="Choose(string, string)"/> 에 넘긴다.
        /// </summary>
        public EventPickSource GetPickSource(string choiceId)
        {
            EventScreenChoice choice;
            if (string.IsNullOrEmpty(choiceId) || !_made.TryGetValue(choiceId, out choice) || choice == null)
            {
                return EventPickSource.None;
            }

            return choice.PickSource;
        }

        /// <summary>
        /// 선택지를 하나 골랐다. 고르기 화면에서 고른 것이 있으면 그것을 대상으로 삼는다.
        /// 비워 두면 선택지가 지목해 둔 것이 대상이다.
        /// </summary>
        public EventStep Choose(string choiceId, string pickedId)
        {
            EventStep step = new EventStep();

            if (_screen == null)
            {
                step.Finished = true;
                return step;
            }

            EventScreenChoice choice;
            if (!_made.TryGetValue(choiceId, out choice) || choice == null)
            {
                return step;
            }

            if (choice.Kind == EventChoiceKind.Finish)
            {
                step.Finished = true;
                return step;
            }

            string targetId = !string.IsNullOrEmpty(pickedId) ? pickedId : GetTargetId(choiceId);
            string done = string.Empty;

            // 작업보다 먼저 기억한다. 기억해 둔 것을 쓰는 작업은 앞 화면의 것만 보므로 순서가 바뀌어도 같다.
            if (!string.IsNullOrEmpty(choice.RememberAs))
            {
                _memory[choice.RememberAs] = targetId;
                _memoryNames[choice.RememberAs] = GetTargetName(choiceId);
            }

            if (choice.Kind == EventChoiceKind.Act && choice.Action != null)
            {
                if (choice.Action.OpensCombat)
                {
                    // 전투는 여기서 화면을 넘기지 않는다.
                    // 전투가 끝난 뒤 `ResumeAfterCombat` 이 다음 화면을 연다.
                    _afterCombatScreenId = PickNextScreenId(choice);
                    step.StartsCombat = true;
                    step.DoubleCombatReward = choice.Action.DoubleCombatReward;
                    step.WithCombatPenalty = choice.Action.WithCombatPenalty;
                    step.CombatPenaltyId = string.Empty;
                    step.CombatPenaltyText = string.Empty;

                    if (choice.Action.WithCombatPenalty)
                    {
                        PickCombatPenalty(choice.Action, out step.CombatPenaltyId, out step.CombatPenaltyText);
                    }

                    return step;
                }

                done = _world != null
                    ? _world.Apply(choice.Action, MakeTarget(choice.Action, targetId, choiceId), _random)
                    : string.Empty;
            }

            EventScreen next = _script.Find(PickNextScreenId(choice));

            if (next == null)
            {
                // 갈 화면이 없으면 그 자리에서 끝난 것으로 본다.
                // 이렇게 받아 주지 않으면 방을 나갈 수가 없어 런이 막힌다.
                step.Finished = true;
                return step;
            }

            step.Result = MakeResult(choiceId, next, done);
            return step;
        }

        /// <summary>전투가 끝났다. 전투를 연 선택지가 적어 둔 화면을 연다.</summary>
        public EventStep ResumeAfterCombat(string choiceId)
        {
            EventStep step = new EventStep();

            EventScreen next = _script != null ? _script.Find(_afterCombatScreenId) : null;
            _afterCombatScreenId = null;

            if (next == null)
            {
                step.Finished = true;
                return step;
            }

            step.Result = MakeResult(choiceId, next, string.Empty);
            return step;
        }

        /// <summary>
        /// 화면 하나를 열어 쪽으로 만든다.
        /// 선택지를 채우고 할 수 없는 것을 비활성으로 돌린다.
        /// </summary>
        private EventPage Enter(EventScreen screen)
        {
            _screen = screen;
            ClearScreenState();

            List<EventChoice> choices = BuildChoices(screen);
            EnsureWayOut(choices, screen);

            EventPage page = new EventPage();
            page.IllustrationId = screen.IllustrationId;
            page.BodyText = screen.BodyText;
            page.Choices.AddRange(choices);
            return page;
        }

        /// <summary>
        /// 다음 화면으로 넘어가는 결과를 만든다.
        /// 화면이 통째로 바뀌므로 본문을 갈아 끼우고 선택지를 새로 얹는다.
        /// </summary>
        private EventResult MakeResult(string chosenChoiceId, EventScreen next, string done)
        {
            string body = next.BodyText;

            // 작업으로 무엇이 바뀌었는지 다음 화면 본문 위에 한 줄로 붙인다.
            // 기획서가 "보상이나 손실이 적용된 뒤에는 마무리 화면으로 넘어간다"로 정했을 뿐
            // 무엇을 받았는지는 적지 않아, 보고 알 수 있게 여기서 적는다.
            if (!string.IsNullOrEmpty(done))
            {
                body = done + "\n\n" + body;
            }

            EventResult result = new EventResult(chosenChoiceId, body);
            result.IllustrationId = next.IllustrationId;

            _screen = next;
            ClearScreenState();

            List<EventChoice> choices = BuildChoices(next);
            EnsureWayOut(choices, next);

            result.NextChoices.AddRange(choices);
            return result;
        }

        /// <summary>그 화면의 선택지를 실제로 만든다.</summary>
        private List<EventChoice> BuildChoices(EventScreen screen)
        {
            List<EventChoice> made = new List<EventChoice>();
            List<EventItem> items = new List<EventItem>();

            for (int i = 0; i < screen.Choices.Count; i++)
            {
                EventScreenChoice spec = screen.Choices[i];
                if (spec == null)
                {
                    continue;
                }

                if (spec.Fill == EventChoiceFill.None)
                {
                    made.Add(MakeChoice(spec, spec.Id, spec.Label, string.Empty, string.Empty));
                    continue;
                }

                // 대상으로 채우는 선택지. 모을 것이 없으면 선택지 자체가 없다.
                items.Clear();

                if (_world != null)
                {
                    _world.Collect(MakeFillRequest(spec), _random, items);
                }

                _filledBySpec[spec.Id] = new List<EventItem>(items);

                for (int j = 0; j < items.Count; j++)
                {
                    string label = string.Format(spec.GetFillLabelFormat(j), items[j].Label);
                    string id = spec.Id + "_" + j;

                    made.Add(MakeChoice(spec, id, label, items[j].Id, items[j].Label));
                    _targets[id] = items[j];
                }
            }

            return made;
        }

        /// <summary>
        /// 고를 수 있는 선택지가 없는 화면을 막는다.
        ///
        /// **먼저 강제 손실 선택지를 연다.** 4장 공통 규칙 이
        /// "부정 이벤트와 무작위 이벤트에서 체력 손실로 현재 체력이 1 미만이 되는 선택지는 비활성으로 표시한다.
        /// 다른 선택지가 모두 비활성이면 그 선택지를 활성으로 표시한다" 고 정했다.
        /// 2장 도 부정 이벤트의 "손해는 피할 수 없다" 고 정했다.
        /// 예전에는 여기서 바로 공짜 "떠난다" 를 끼워, 체력 10 · 골드 0 으로 09 부식 을 만나면 아무 손실 없이 끝났다.
        /// 19 두 개의 상자 의 함정도 그렇게 피했다. 2026년 10월 8일 외부 검토가 짚어 고쳤다.
        /// 열린 선택지로 체력이 0 이 되면 흐름이 런을 끝낸다.
        ///
        /// 플레이어가 스스로 체력을 내주는 선택지(긍정 이벤트의 29 생명력 거래 같은 것)는 다시 열지 않는다.
        ///
        /// 그래도 고를 것이 없을 때만 떠나는 선택지를 하나 끼워 넣는다.
        /// 채울 것이 하나도 없어 선택지가 통째로 사라진 화면이 그렇다. 그대로 두면 방을 나갈 수가 없다.
        /// </summary>
        private void EnsureWayOut(List<EventChoice> made, EventScreen screen)
        {
            if (!HasSelectable(made))
            {
                for (int i = 0; i < made.Count; i++)
                {
                    if (made[i] == null || !_forcedLoss.Contains(made[i].Id))
                    {
                        continue;
                    }

                    // 체력 부족 줄만 걷어 다시 연다. 다른 조건으로도 막혔으면 강제 손실 후보에 들지 않는다.
                    made[i].Lines.RemoveAll(line => line.Kind == EventEffectKind.Requirement && !line.Met);
                    made[i].ResolveState();
                }
            }

            // 고를 수 있는 것이 소지 한도를 넘는 획득뿐이어도 떠나는 선택지를 끼운다.
            // 버릴 것을 고르지 않고 획득을 포기하면 이 화면으로 돌아오는데, 그때 나갈 길이 있어야 한다.
            // 26 반짝이는 것 의 화면 2 처럼 보석 고르기뿐인 화면이 그렇다.
            if (HasSelectableWithoutRoomNeed(made))
            {
                return;
            }

            // 화면당 최대 선택지 수를 넘기지 않는다. 칸이 다 찼으면 고를 수 없는 선택지를 뒤에서부터 하나 빼고,
            // 그런 것이 없으면 소지 한도를 넘는 획득 선택지를 하나 뺀다. 떠나는 선택지는 꼭 있어야 하기 때문이다.
            // 2026년 10월 9일 외부 검토가 저절로 끼우는 선택지까지 세라고 짚었다.
            if (MaxChoices > 0 && made.Count >= MaxChoices)
            {
                int drop = LastIndex(made, choice => choice != null && !choice.IsSelectable);
                if (drop < 0)
                {
                    drop = LastIndex(made, choice => choice != null && _needsRoom.Contains(choice.Id));
                }

                if (drop >= 0)
                {
                    made.RemoveAt(drop);
                }
            }

            EventScreenChoice leave = EventScreenChoice.Finish("나가기", "떠난다");
            made.Add(MakeChoice(leave, leave.Id, leave.Label, string.Empty, string.Empty));
        }

        private static int LastIndex(List<EventChoice> made, System.Predicate<EventChoice> match)
        {
            for (int i = made.Count - 1; i >= 0; i--)
            {
                if (match(made[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>선택지 하나를 화면이 쓰는 꼴로 만든다.</summary>
        private EventChoice MakeChoice(
            EventScreenChoice spec, string id, string label, string targetId, string targetName)
        {
            // 화면에서 이 식별자로 돌아오면 어느 각본 줄이었는지 되찾을 수 있어야 한다.
            _made[id] = spec;

            List<EventItem> decided = Decide(spec, id, targetId);

            EventChoice choice = new EventChoice(id, FillAmounts(label, spec.Action, targetName, decided));
            choice.ResultHidden = spec.ResultHidden;

            for (int i = 0; i < spec.Lines.Count; i++)
            {
                EventEffectLine line = spec.Lines[i];
                choice.Lines.Add(new EventEffectLine(line.Kind, FillAmounts(line.Text, spec.Action, targetName, decided), line.Met));
            }

            // 4장 공통 규칙 · 종료 선택지는 항상 비용 없이 고를 수 있다.
            if (spec.Kind == EventChoiceKind.Finish || _world == null)
            {
                choice.ResolveState();
                return choice;
            }

            string reason;
            if (spec.Action != null && !_world.CanDo(spec.Action, MakeTarget(spec.Action, targetId, id), out reason))
            {
                // 숨기지 않고 비활성으로 두며 모자란 것을 함께 적는다.
                choice.Lines.Add(EventEffectLine.Requirement(reason, false));

                if (IsForcedHealthLoss(spec.Action, reason))
                {
                    _forcedLoss.Add(id);
                }
            }

            if (spec.Action != null && _world.NeedsRoom(spec.Action))
            {
                _needsRoom.Add(id);
            }

            choice.ResolveState();
            return choice;
        }

        /// <summary>선택지 글에서 이 작업이 다루는 체력 수로 바뀌는 자리.</summary>
        public const string HealthToken = "{hp}";

        /// <summary>선택지 글에서 이 작업이 함께 깎는 최대 체력 수로 바뀌는 자리.</summary>
        public const string MaxHealthCostToken = "{hpCost}";

        /// <summary>선택지 글에서 그 선택지가 지목한 물건 이름으로 바뀌는 자리.</summary>
        public const string NameToken = "{name}";

        /// <summary>선택지 글에서 미리 정한 결과를 모두 `[이름], [이름]` 으로 적는 자리.</summary>
        public const string ResultsToken = "{results}";

        /// <summary>미리 정한 결과가 없을 때 그 자리에 적는 글.</summary>
        public const string NoResultText = "없음";

        /// <summary>선택지 글에서 미리 정한 결과 하나의 이름으로 바뀌는 자리. 0 부터 센다.</summary>
        public static string ResultToken(int index)
        {
            return "{result" + index + "}";
        }

        /// <summary>화면이 바뀔 때 그 화면에만 쓰던 것을 비운다.</summary>
        private void ClearScreenState()
        {
            _targets.Clear();
            _made.Clear();
            _decided.Clear();
            _filledBySpec.Clear();
            _forcedLoss.Clear();
            _needsRoom.Clear();
        }

        /// <summary>
        /// 체력이 모자라서만 막힌 강제 손실 선택지인지.
        /// 부정 이벤트와 무작위 이벤트의 체력 손실만 든다. 4장 공통 규칙 이다.
        /// 최대 체력 손실은 지금 체력을 1 아래로 떨어뜨리지 않아 이 규칙에 들지 않는다.
        /// 최대 체력이 1 아래로 내려가 막힌 것(`EventAction.NotEnoughMaxHealth`)은 다시 열지 않는다.
        /// </summary>
        private bool IsForcedHealthLoss(EventAction action, string reason)
        {
            if (_script == null || action == null || reason != EventAction.NotEnoughHealth)
            {
                return false;
            }

            if (_script.Nature != EventNature.Bad && _script.Nature != EventNature.Random)
            {
                return false;
            }

            return action.Kind == EventActionKind.LoseHealth;
        }

        private bool HasSelectableWithoutRoomNeed(List<EventChoice> made)
        {
            for (int i = 0; i < made.Count; i++)
            {
                if (made[i] != null && made[i].IsSelectable && !_needsRoom.Contains(made[i].Id))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>그 선택지가 받을 코인이나 유물이 소지 한도를 넘는지. 넘으면 흐름이 고르기 전에 버릴 것을 고르게 한다.</summary>
        public bool NeedsRoom(string choiceId)
        {
            return !string.IsNullOrEmpty(choiceId) && _needsRoom.Contains(choiceId);
        }

        /// <summary>그 선택지가 하는 작업. 없으면 null 이다.</summary>
        public EventAction GetAction(string choiceId)
        {
            EventScreenChoice choice;
            if (string.IsNullOrEmpty(choiceId) || !_made.TryGetValue(choiceId, out choice) || choice == null)
            {
                return null;
            }

            return choice.Action;
        }

        /// <summary>
        /// 그 선택지로 받을 것의 이름. 버릴 것 고르기 창에 "[이름] 을 받는다" 로 적는다.
        /// 지목한 것이 있으면 그 이름, 미리 정해 둔 결과가 있으면 그 이름들, 둘 다 없으면 빈 글이다.
        /// </summary>
        public string DescribeGain(string choiceId)
        {
            string target = GetTargetName(choiceId);
            if (!string.IsNullOrEmpty(target))
            {
                return target;
            }

            List<EventItem> decided;
            if (string.IsNullOrEmpty(choiceId) || !_decided.TryGetValue(choiceId, out decided) || decided == null)
            {
                return string.Empty;
            }

            string names = string.Empty;
            for (int i = 0; i < decided.Count; i++)
            {
                names += (i > 0 ? ", " : string.Empty) + decided[i].Label;
            }

            return names;
        }

        private static bool HasSelectable(List<EventChoice> made)
        {
            for (int i = 0; i < made.Count; i++)
            {
                if (made[i] != null && made[i].IsSelectable)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 공개하는 선택지면 결과를 미리 정해 둔다. 공개하지 않으면 null 이다.
        /// 다른 선택지가 지목한 것을 모두 받는 선택지는 그것을 그대로 결과로 삼는다.
        /// </summary>
        private List<EventItem> Decide(EventScreenChoice spec, string choiceId, string targetId)
        {
            if (spec == null || !spec.RevealOutcome || spec.Kind != EventChoiceKind.Act || spec.Action == null)
            {
                return null;
            }

            List<EventItem> decided = new List<EventItem>();
            List<EventItem> taken;

            if (!string.IsNullOrEmpty(spec.TakeAllFromId) && _filledBySpec.TryGetValue(spec.TakeAllFromId, out taken))
            {
                decided.AddRange(taken);
            }
            else if (_world != null)
            {
                _world.Decide(spec.Action, MakeTarget(spec.Action, targetId, string.Empty), _random, decided);
            }

            _decided[choiceId] = decided;
            return decided;
        }

        /// <summary>미리 정한 결과 자리를 이름으로 바꾼다.</summary>
        private static string FillResults(string text, List<EventItem> decided)
        {
            if (text.IndexOf("{result", System.StringComparison.Ordinal) < 0)
            {
                return text;
            }

            string all = string.Empty;
            int count = decided != null ? decided.Count : 0;

            for (int i = 0; i < count; i++)
            {
                all += (i > 0 ? ", " : string.Empty) + "[" + decided[i].Label + "]";
                text = text.Replace(ResultToken(i), decided[i].Label);
            }

            text = text.Replace(ResultsToken, count > 0 ? all : NoResultText);

            // 정하지 못한 자리. 받을 것이 남지 않은 경우다.
            for (int i = count; i < 4; i++)
            {
                text = text.Replace(ResultToken(i), NoResultText);
            }

            return text;
        }

        /// <summary>
        /// 체력 자리를 지금 최대 체력으로 잰 숫자로, 이름 자리를 지목한 물건 이름으로 바꾼다.
        /// 2026년 10월 5일 원재가 "최대 체력의 10%" 처럼 적지 말고 정확히 얼마인지만 보이라고 했다.
        /// 최대 체력이 100 이고 10% 면 10 이다. 실제로 깎이는 수와 같은 규칙(`EventAction.HealthAmount`)을 쓴다.
        /// 같은 날 받는 물건은 "유물 — [곡괭이]" 처럼 종류를 앞에, 이름을 괄호에 넣어 얻고 잃는 줄에 적기로 했다.
        /// </summary>
        private string FillAmounts(string text, EventAction action, string targetName, List<EventItem> decided)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('{') < 0)
            {
                return text;
            }

            text = text.Replace(NameToken, targetName ?? string.Empty);
            text = FillResults(text, decided);

            if (action == null)
            {
                return text;
            }

            int maxHealth = _world != null ? _world.MaxHealth : 0;
            return text
                .Replace(HealthToken, action.HealthAmount(maxHealth).ToString())
                .Replace(MaxHealthCostToken, action.MaxHealthCostAmount(maxHealth).ToString());
        }

        /// <summary>그 선택지가 갈 화면. 확률로 갈리면 여기서 굴린다.</summary>
        private string PickNextScreenId(EventScreenChoice choice)
        {
            if (choice.Branches.Count == 0)
            {
                return choice.NextScreenId;
            }

            int total = 0;
            for (int i = 0; i < choice.Branches.Count; i++)
            {
                if (choice.Branches[i] != null && choice.Branches[i].Weight > 0)
                {
                    total += choice.Branches[i].Weight;
                }
            }

            if (total <= 0)
            {
                return choice.NextScreenId;
            }

            int roll = _random.Range(0, total);

            for (int i = 0; i < choice.Branches.Count; i++)
            {
                EventBranch branch = choice.Branches[i];
                if (branch == null || branch.Weight <= 0)
                {
                    continue;
                }

                roll -= branch.Weight;
                if (roll < 0)
                {
                    return branch.ScreenId;
                }
            }

            return choice.NextScreenId;
        }

        private string GetTargetId(string choiceId)
        {
            EventItem item;
            return _targets.TryGetValue(choiceId, out item) ? item.Id : string.Empty;
        }

        private string GetTargetName(string choiceId)
        {
            EventItem item;
            return _targets.TryGetValue(choiceId, out item) ? item.Label : string.Empty;
        }

        /// <summary>
        /// 전투에 걸 부정 효과를 정한다. 3장 도전 이벤트 공통 규칙.
        /// 작업이 기억을 적었으면 앞 화면에서 고른 것이다(35 도전자 의 A 와 B).
        /// 적지 않았으면 목록에서 무작위로 하나 뽑는다(38 매복 의 "무작위 부정 효과 1개").
        /// </summary>
        private void PickCombatPenalty(EventAction action, out string id, out string text)
        {
            id = string.Empty;
            text = string.Empty;

            if (action.RecallKeys != null && action.RecallKeys.Count > 0)
            {
                string key = action.RecallKeys[0];
                id = Recall(key);

                string name;
                text = _memoryNames.TryGetValue(key, out name) && name != null ? name : string.Empty;

                if (!string.IsNullOrEmpty(id))
                {
                    return;
                }
            }

            if (_world == null)
            {
                return;
            }

            List<EventItem> picked = new List<EventItem>();
            _world.Collect(new EventFillRequest(EventChoiceFill.CombatPenalties, 1), _random, picked);

            if (picked.Count > 0)
            {
                id = picked[0].Id;
                text = picked[0].Label;
            }
        }

        /// <summary>기억해 둔 것. 없으면 빈 글이다.</summary>
        public string Recall(string key)
        {
            string value;
            if (string.IsNullOrEmpty(key) || !_memory.TryGetValue(key, out value))
            {
                return string.Empty;
            }

            return value ?? string.Empty;
        }

        /// <summary>작업이 쓸 대상. 작업이 적은 기억을 차례대로 풀어 넣는다.</summary>
        private EventTarget MakeTarget(EventAction action, string targetId, string choiceId)
        {
            EventTarget target = new EventTarget(targetId);

            List<EventItem> decided;
            if (!string.IsNullOrEmpty(choiceId) && _decided.TryGetValue(choiceId, out decided) && decided != null)
            {
                target.Decided = new List<string>();
                for (int i = 0; i < decided.Count; i++)
                {
                    target.Decided.Add(decided[i].Id);
                }
            }

            if (action != null && action.RecallKeys != null && action.RecallKeys.Count > 0)
            {
                target.Recalled = new List<string>();
                for (int i = 0; i < action.RecallKeys.Count; i++)
                {
                    target.Recalled.Add(Recall(action.RecallKeys[i]));
                }
            }

            return target;
        }

        /// <summary>채울 것을 묻는 글. 기억 이름을 실제 식별자로 풀어 넣는다.</summary>
        private EventFillRequest MakeFillRequest(EventScreenChoice spec)
        {
            EventFillRequest request = new EventFillRequest(spec.Fill, spec.FillCount);
            request.SourceId = Recall(spec.FillSourceKey);
            request.MinRarity = spec.FillMinRarity;
            request.MaxRarity = spec.FillMaxRarity;
            request.RarityWeights = spec.FillRarityWeights;

            if (spec.FillExcludeKeys != null && spec.FillExcludeKeys.Count > 0)
            {
                request.ExcludeIds = new List<string>();
                for (int i = 0; i < spec.FillExcludeKeys.Count; i++)
                {
                    string value = Recall(spec.FillExcludeKeys[i]);
                    if (!string.IsNullOrEmpty(value))
                    {
                        request.ExcludeIds.Add(value);
                    }
                }
            }

            return request;
        }
    }
}
