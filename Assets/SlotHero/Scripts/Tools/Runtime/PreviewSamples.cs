using System.Collections.Generic;
using SlotHero.CurrentBuild;
using SlotHero.Events;
using SlotHero.Popup;
using SlotHero.Profile;
using SlotHero.Reward;
using SlotHero.RunResult;
using SlotHero.Combat;

namespace SlotHero.Tools
{
    /// <summary>
    /// 확인용 씬에 채워 넣는 보기 자료.
    /// 기획서 와이어프레임의 예시를 그대로 옮긴 것이다.
    ///
    /// 씬을 만드는 에디터 쪽과 눌러 보는 런타임 쪽이 같은 자료를 써야 해서 여기 모았다.
    /// **게임 자료가 아니다.** 실제 자료가 생기면 이 파일은 지운다.
    /// </summary>
    public static class PreviewSamples
    {
        /// <summary>프로필 셋. 하나는 논 적이 있고 하나는 갓 만들었고 하나는 비었다.</summary>
        public static ProfileList Profiles()
        {
            ProfileList list = ProfileList.CreateEmpty(ProfileList.DefaultSlotCount);
            list.Set(0, ProfileSummary.Filled("프로필1", 45600, "2026-09-03"));
            list.Set(1, ProfileSummary.Filled("프로필2", 0, string.Empty));
            list.CurrentIndex = 0;
            return list;
        }

        /// <summary>현재 빌드 화면과 런 종료 결과가 함께 쓰는 빌드.</summary>
        public static CurrentBuildSnapshot Build()
        {
            CurrentBuildSnapshot snapshot = new CurrentBuildSnapshot();

            snapshot.Relics.Add(BuildEntry.Item("book", "낡은 책"));
            snapshot.Relics.Add(BuildEntry.Item("wing", "날개"));
            snapshot.Relics.Add(BuildEntry.Item("pick", "곡괭이"));

            snapshot.Coins.Add(BuildEntry.Item("gold", "금화"));
            snapshot.Coins.Add(BuildEntry.Item("silver", "은화"));

            // 문양 하나는 서로 다른 태그를 둘 가진다.
            snapshot.Symbols.Add(BuildEntry.Symbol(
                "flame", "불꽃", 8, SymbolTagType.Mars, SymbolTagType.Mercury));
            snapshot.Symbols.Add(BuildEntry.Symbol(
                "mud", "진흙", 6, SymbolTagType.Earth, SymbolTagType.Saturn));
            snapshot.Symbols.Add(BuildEntry.Symbol(
                "tree", "세계수", 4, SymbolTagType.Jupiter, SymbolTagType.Earth));
            snapshot.Symbols.Add(BuildEntry.Symbol(
                "chariot", "전차", 3, SymbolTagType.Mercury, SymbolTagType.Mars));
            snapshot.Symbols.Add(BuildEntry.Symbol(
                "bird", "새", 1, SymbolTagType.Venus, SymbolTagType.Uranus));
            snapshot.Symbols.Add(BuildEntry.Symbol(
                "storm", "폭풍", 1, SymbolTagType.Uranus, SymbolTagType.Neptune));
            snapshot.Symbols.Add(BuildEntry.Symbol(
                "bolt", "벼락", 1, SymbolTagType.Saturn, SymbolTagType.Pluto));

            return snapshot;
        }

        /// <summary>이벤트 한 쪽. 요구 항목이 있는 선택지와 숨은 선택지를 함께 둔다.</summary>
        public static EventPage Event()
        {
            EventPage page = new EventPage();
            page.BodyText =
                "길가에 낡은 상자가 놓여 있다.\n" +
                "자물쇠는 오래전에 삭았고 뚜껑 틈으로 무언가 반짝인다.\n\n" +
                "열어 볼 수도, 그냥 지나칠 수도 있다.";

            page.Choices.Add(EventChoice.Known(
                "open", "열어 본다", EventEffectLine.Gain("유물 획득")));
            page.Choices.Add(EventChoice.Known(
                "pry", "지렛대로 연다",
                EventEffectLine.Requirement("지렛대", false),
                EventEffectLine.Gain("유물 획득")));
            page.Choices.Add(EventChoice.Known(
                "buy", "값을 치른다",
                EventEffectLine.Requirement("골드 20", true),
                EventEffectLine.Cost("골드 20 소모"),
                EventEffectLine.Gain("코인 획득")));
            page.Choices.Add(EventChoice.Hidden("leave", "지나친다"));

            return page;
        }

        /// <summary>선택지를 고른 뒤 이어지는 쪽.</summary>
        public static EventResult EventResultFor(string choiceId)
        {
            EventResult result = new EventResult();
            result.ChosenChoiceId = choiceId;

            if (choiceId == "leave")
            {
                result.BodyText = "그냥 지나쳤다. 등 뒤에서 아무 소리도 나지 않았다.";
                return result;
            }

            result.BodyText = "상자를 열자 먼지가 일었다. 안에는 낡은 무언가가 들어 있었다.";
            result.NextChoices.Add(EventChoice.Known(
                "take", "챙긴다", EventEffectLine.Gain("유물 획득")));
            result.NextChoices.Add(EventChoice.Known("skip", "그냥 둔다"));

            return result;
        }

        /// <summary>런 종료 결과 한 벌.</summary>
        public static RunResultData Result(
            CurrentBuildVisualConfig buildVisual, RunResultVisualConfig visual)
        {
            RunResultData data = new RunResultData();
            data.Outcome = RunOutcome.Defeat;
            data.LocationText = RunResultText.Location(visual, 1, 9, "엘리트에게 쓰러짐");
            data.FinalBuild = RunResultText.BuildFinalBuild(ResultBuild(), buildVisual, visual);

            data.RunRecord.Add(ResultLine.Body("플레이 시간 59분 40초"));
            data.RunRecord.Add(ResultLine.Body("지나온 방 9개"));
            data.RunRecord.Add(ResultLine.Body("처치한 몬스터 14마리 · 엘리트 2마리"));
            data.RunRecord.Add(ResultLine.Blank());
            data.RunRecord.Add(ResultLine.Body("획득한 골드 412 · 소모한 골드 275"));
            data.RunRecord.Add(ResultLine.Body("한 번에 준 가장 큰 피해 186"));
            data.RunRecord.Add(ResultLine.Body("가장 많이 모은 태그 - 수성 11장 · 화성 11장"));

            data.Unlocks.Add(ResultLine.Body("새로 해금 · 유물 곡괭이"));
            data.Unlocks.Add(ResultLine.Body("도전과제 · 엘리트 2마리 처치 (2 / 2)"));
            data.Unlocks.Add(ResultLine.Body("도전과제 · 문양 30장 모으기 (24 / 30)"));

            return data;
        }

        /// <summary>
        /// 런 종료 결과 와이어프레임의 문양 24장짜리 빌드.
        /// 태그 장수는 문양에서 저절로 나오므로 따로 적지 않는다.
        /// </summary>
        public static CurrentBuildSnapshot ResultBuild()
        {
            CurrentBuildSnapshot snapshot = Build();
            snapshot.Coins.Add(BuildEntry.Item("leather", "가죽 원반"));
            return snapshot;
        }

        /// <summary>
        /// 10 보상 화면 와이어프레임의 보상 한 벌.
        /// 기본 보상 12, 오버킬 보상 3, 문양 카드 셋이다.
        /// </summary>
        public static RewardOffer Reward()
        {
            RewardOffer offer = RewardOffer.Gold(12, 3);

            offer.Add(RewardCard.Symbol(
                "Grave", "무덤", SymbolTagType.Saturn, SymbolTagType.Pluto));
            offer.Add(RewardCard.Symbol(
                "Scythe", "낫", SymbolTagType.Venus, SymbolTagType.Pluto));
            offer.Add(RewardCard.Symbol(
                "DarkCloud", "먹구름", SymbolTagType.Mercury, SymbolTagType.Uranus));

            return offer;
        }

        /// <summary>08장 예시 1 · 새 게임 확인.</summary>
        public static PopupSpec NewGamePopup()
        {
            return PopupSpec.Confirm(
                "새 게임을 시작하시겠습니까?",
                "진행 중인 런이 사라집니다. 되돌릴 수 없습니다.\n\n" +
                "스테이지 1 · 9번째 방\n유물 3개 · 코인 3개 · 문양 24장",
                "돌아가기",
                "새 게임 시작",
                true);
        }

        /// <summary>14장 · 런 종료 확인. 가운데가 강조색이다.</summary>
        public static PopupSpec EndRunPopup()
        {
            PopupSpec spec = new PopupSpec("런을 끝내시겠습니까?", "지금까지의 진행이 기록됩니다.");
            spec.Add(PopupButtonSpec.Normal("cancel", "취소"));
            spec.Add(PopupButtonSpec.Danger("abandon", "런 포기"));
            spec.Add(PopupButtonSpec.Normal("save", "저장하고 나가기"));
            return spec;
        }
    }
}
