using System.Collections.Generic;
using SlotHero.Events;

namespace SlotHero.Flow
{
    /// <summary>
    /// 이벤트 각본을 짓는다. `이벤트 기획서 초안` 6장 이벤트 상세 를 옮긴 것이다.
    ///
    /// **본문은 `이벤트 기획서.pdf` 의 스토리를 옮겼다.** 맞춤법이 틀린 곳만 고쳤다.
    /// PDF 에 없는 것은 넣지 않는다. 초안에만 있던 30 쉼터 는 2026년 10월 5일에 원재의 말대로 지웠다.
    /// 선택지 뒤에 붙는 얻는 것(초록)과 잃는 것(빨강) 줄, 고른 뒤 본문 위에 붙는 결과 한 줄만 내가 적었다.
    /// 무엇이 바뀌었는지 보이게 하는 줄이라 이야기 글은 아니다.
    /// <see cref="EventScreen.Mood"/> 에는 PDF 의 장면을 삽화 발주용으로 한 줄에 줄여 두었다.
    ///
    /// 삽화 식별자는 `이벤트식별자_화면식별자` 다. `BuildAll` 이 비어 있는 화면에 채운다.
    /// 그 이름의 그림을 `Events/Art` 에 넣으면 붙는다.
    ///
    /// **수치도 임시다.** 기획서가 `00` 으로 비워 둔 자리는 내가 채웠다.
    /// 어느 값인지는 `Events/README.md` 의 표에 모아 두었다.
    ///
    /// 각본 번호는 기획서 5장 이벤트 목록 의 번호를 그대로 쓴다.
    /// 아직 옮기지 않은 이벤트는 그 번호가 비어 있다.
    /// </summary>
    public static class EventScripts
    {
        /// <summary>옮겨 둔 이벤트를 모두 만든다.</summary>
        public static List<EventScript> BuildAll()
        {
            List<EventScript> all = new List<EventScript>();

            all.Add(SuspiciousAura());
            all.Add(CarvePower());
            all.Add(PandorasBox());
            all.Add(Reflection());
            all.Add(Purification());
            all.Add(SymbolTrade());
            all.Add(BloodDeal());
            all.Add(Corrosion());
            all.Add(MindCollapse());
            all.Add(SleepingTreasure());
            all.Add(RelicTrade());
            all.Add(OwnerlessRelic());
            all.Add(TwoBoxes());
            all.Add(MoneyChange());
            all.Add(CoinSale());
            all.Add(MadSmith());
            all.Add(SomethingShiny());
            all.Add(Blessing());
            all.Add(Recovery());
            all.Add(LifeTrade());
            all.Add(Plunder());
            all.Add(Erosion());
            all.Add(LeftBehind());
            all.Add(Gamble());
            all.Add(Challenger());
            all.Add(Ambush());

            for (int i = 0; i < all.Count; i++)
            {
                NameIllustrations(all[i]);
            }

            return all;
        }

        /// <summary>삽화 식별자 이름. `이벤트식별자_화면식별자` 다.</summary>
        public static string IllustrationIdOf(string eventId, string screenId)
        {
            return eventId + "_" + screenId;
        }

        /// <summary>
        /// 비어 있는 화면에 삽화 식별자를 채운다.
        /// 화면마다 그림 하나라는 이벤트 기획서 1장 구성 방식 을 따른다.
        /// </summary>
        private static void NameIllustrations(EventScript script)
        {
            for (int i = 0; i < script.Screens.Count; i++)
            {
                EventScreen screen = script.Screens[i];
                if (screen != null && string.IsNullOrEmpty(screen.IllustrationId))
                {
                    screen.IllustrationId = IllustrationIdOf(script.EventId, screen.ScreenId);
                }
            }
        }

        // ── 6.1 문양 이벤트 ──────────────────────────────────────

        /// <summary>
        /// 01 수상한 기운. 긍정, 화면 3개.
        /// 화면 2 의 선택지는 차가운 연기, 뜨거운 연기, 어둠만 있는 통로 셋이고
        /// 각 통로에 무작위 문양 하나씩을 지목한다. 비고대로 선택지에 문양 이름을 붙인다.
        /// </summary>
        private static EventScript SuspiciousAura()
        {
            EventScript script = Begin("suspicious_aura", "수상한 기운", EventNature.Good);

            script.Add(new EventScreen("1", "탐험 중 덩굴 사이로 희미한 빛이 새어 나오는 불길한 분위기",
                    "탐험 중 덩굴 속에서 희미한 빛이 새어 나온다.\n"
                    + "불길한 예감이 들지만, 어쩌면 이 모든 것은 신의 뜻.\n"
                    + "운에 맡겨볼 것인가, 예감을 믿어볼 것인가.")
                .Add(EventScreenChoice.Advance("search", "탐색한다", "2"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2",
                    "덩굴 속 세 갈래 통로. 연하늘색 얼음 같은 연기, 검붉은 강렬한 연기, 검보라색 암울한 연기가 각각 피어오르는 분위기",
                    "덩굴 속으로 들어가보니 세 갈래의 통로가 있다.")
                .Add(Gain("pick", EventActionKind.GainSymbol, "끝")
                    .FilledWith(EventChoiceFill.RandomSymbols, 3, "문양")
                    .EachLabeled(
                        "차가운 연기가 나는 통로",
                        "뜨거운 연기가 나는 통로",
                        "오직 어둠만이 존재하는 통로")
                    .With(EventEffectLine.Gain(Named()))));

            return script.Add(Ending("얻은 문양에서 피어오른 연기가 온몸을 감싸는 분위기",
                "얻은 문양으로부터 피어오른 연기가 온 몸을 감싼다. 문양의 힘이 느껴진다."));
        }

        /// <summary>
        /// 02 힘을 새기는 것. 긍정, 화면 4개.
        /// 화면 2 에서 고른 태그를 기억했다가 화면 3 의 태그와 합쳐 그 둘을 가진 문양을 준다.
        /// 화면 3 의 후보에서는 화면 2 에서 고른 태그를 뺀다. 기획서 비고 그대로다.
        /// </summary>
        private static EventScript CarvePower()
        {
            EventScript script = Begin("carve_power", "힘을 새기는 것", EventNature.Good);

            script.Add(new EventScreen("1",
                    "숲길의 허름한 탁자에서 후드를 쓴 점쟁이 노파가 비릿하게 웃으며 손짓하는 분위기",
                    "길을 지나가다 점쟁이를 마주한다.\n"
                    + "\"힘이 필요하지 않는가?\"\n"
                    + "평소라면 지나쳤겠지만, 어쩐지 그 날은 걸음을 멈추게 된다.\n"
                    + "\"카드를 뽑아봐. 힘이 깃들 것이야.\"\n"
                    + "범상치 않은 느낌의 카드가 눈 앞에서 아른거린다. 무슨 카드를 뽑을까. 선택을 주저한다.")
                .Add(EventScreenChoice.Advance("go", "계속한다", "2")));

            script.Add(new EventScreen("2", "허름한 탁자 위에 화려한 카드 다섯 장이 펼쳐진 분위기",
                    "카드 5장을 고르고, 뒤집을 카드 1장을 고른다.")
                .Add(EventScreenChoice.Advance("card", "{0}", "3")
                    .FilledWith(EventChoiceFill.RandomTags, 5, "{0}")
                    .Remember("first")));

            script.Add(new EventScreen("3", "고른 카드가 빛나는 가운데 다시 다섯 장 중 하나를 고르는 분위기",
                    "다시 카드 5장을 고르고, 뒤집을 카드 1장을 고른다.")
                .Add(Act("card", "{0}",
                        EventAction.OnTarget(EventActionKind.GainSymbolByTags).Recalling("first"), "끝")
                    .FilledWith(EventChoiceFill.RandomTags, 5, "{0}")
                    .Excluding("first")
                    .Revealing()
                    .With(EventEffectLine.Gain(Decided(0)))));

            return script.Add(Ending("노파가 두 카드의 문양이 새겨진 사탕을 건네고 흔적 없이 사라지는 분위기",
                "카드를 본 점쟁이가 기괴하게 웃으며 사탕 한 알을 건넨다. "
                + "아까 고른 카드 두 장의 문양이 모두 새겨져 있는 사탕이다. "
                + "사탕을 삼키니 알 수 없는 힘이 느껴진다.\n"
                + "뒤를 돌아보니 흔적도 없이 사라져 있는 점쟁이. 남아있는 것은 입 안의 기묘한 맛 뿐이다. "
                + "어째선지 찜찜하지만 가야할 길이 멀다."));
        }

        /// <summary>03 판도라의 상자. 무작위, 화면 2개.</summary>
        private static EventScript PandorasBox()
        {
            EventScript script = Begin("pandora", "판도라의 상자", EventNature.Random);

            script.Add(new EventScreen("1",
                    "금박과 뱀의 눈 같은 빨간 보석으로 장식된 화려한 상자. 틈에서 신비로운 파스텔톤 연기가 새어 나오는 분위기",
                    "빨간 보석으로 장식된 상자가 보인다. 마치 뱀의 눈을 연상시키는 보석이다.\n"
                    + "열어보고 싶은 욕망에 휩싸이지만, 결과는 감히 예측할 수 없다.")
                .Add(Act("open", "연다", new EventAction(EventActionKind.ChangeAllSymbols), "끝")
                    .With(EventEffectLine.Cost("보유 문양 전체"))
                    .With(EventEffectLine.Gain("무작위 문양")))
                .Add(EventScreenChoice.Finish("leave", "건드리지 않는다")));

            return script.Add(Ending("열린 상자에서 연기가 피어오르고 믿기지 않는 결과에 눈을 깜빡이는 분위기",
                "빨간 보석은 행운의 보석이었을까, 불행의 보석이었을까.\n"
                + "믿을 수 없는 결과에 여러 번 눈을 깜빡여본다."));
        }

        /// <summary>
        /// 04 닮은 모습. 긍정, 화면 4개.
        /// 화면 1 의 문양과 화면 2 의 태그를 기억했다가, 화면 3 에서 고른 태그로 그 태그를 갈아 끼운다.
        /// 화면 3 의 후보에서는 원래 문양의 두 태그를 모두 뺀다. 원래 문양으로 되돌아가지 않게 하려는 기획서 비고다.
        /// </summary>
        private static EventScript Reflection()
        {
            EventScript script = Begin("reflection", "닮은 모습", EventNature.Good);

            script.Add(new EventScreen("1", "숲속 이끼 낀 우물에서 물을 떠먹고, 수면에 확신에 찬 내 얼굴이 비치는 분위기",
                    "참을 수 없는 갈증에 숲속 우물에 가까이 간다. "
                    + "급하게 물을 떠먹고 나니, 우물에는 새로운 인상의 내가 비춰진다.\n"
                    + "어째선지 확신에 찬 표정. 저런 나라면 이 모든걸 해낼 수 있지 않을까.\n"
                    + "우물 속으로 손을 뻗고 싶은 욕망에 휩싸인다.")
                .Add(EventScreenChoice.Advance("symbol", "문양 선택", "2")
                    .FilledWith(EventChoiceFill.OwnedSymbols, 3, "문양 선택")
                    .With(EventEffectLine.Cost(Named()))
                    .Remember("symbol"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "우물을 휘젓는 손 아래로 수면이 일렁이는 분위기",
                    "우물을 휘젓자 기묘한 분위기의 돌이 떠오른다. "
                    + "하나를 낚아채면, 하나는 금방 가라앉을 것 같다. 무엇을 골라야할까.")
                .Add(EventScreenChoice.Advance("drop", "{0}", "3")
                    .FilledWith(EventChoiceFill.TagsOfRemembered, 2, "{0}")
                    .FilledFrom("symbol")
                    .Remember("drop")
                    .With(EventEffectLine.Cost("이 태그를 바꾼다"))));

            script.Add(new EventScreen("3", "물속에서 빛깔이 다른 보석 같은 돌들이 떠오르는 분위기",
                    "돌이 세 개 떠오른다. 아까처럼 기묘한 분위기의 돌이다. 하나를 골라야만 할 것 같다.")
                .Add(Act("stone", "{0}",
                        EventAction.OnTarget(EventActionKind.ReshapeSymbol).Recalling("symbol", "drop"), "끝")
                    .FilledWith(EventChoiceFill.RandomTags, 3, "{0}")
                    .Excluding("symbol")
                    .Revealing()
                    .With(EventEffectLine.Gain(Decided(0)))));

            return script.Add(Ending("수면에 자신감에 가득 찬 얼굴이 비치고 미소 짓는 분위기",
                "다시 우물에 자신을 비춰보자, 처음에 보았던 자신감에 가득 찬 얼굴이 비춰진다.\n"
                + "미소를 지은 채, 이만 자리에서 일어난다."));
        }

        /// <summary>
        /// 05 정화. 긍정, 화면 3개.
        /// 고른 태그를 가진 문양이 모두 그 태그가 없는 무작위 문양으로 바뀐다.
        /// 보유 문양이 모두 같은 문양이면 태그가 둘뿐이라 선택지도 둘만 나온다. 기획서 비고 그대로다.
        /// </summary>
        private static EventScript Purification()
        {
            EventScript script = Begin("purification", "정화", EventNature.Good);

            script.Add(new EventScreen("1", "숲속 야영지에서 잠을 깨우는 장난스러운 도깨비불 셋이 아른거리는 분위기",
                    "이상한 느낌에 잠에서 깨자 도깨비불이 눈 앞에서 아른거린다. "
                    + "무시하고 잠을 자려 해보아도 일부러 눈 앞으로 와 잠을 깨운다.\n"
                    + "무시할까, 본보기로 하나를 잡아볼까.")
                .Add(EventScreenChoice.Advance("near", "다가간다", "2"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "밝게 타는 불, 깜빡이는 불, 희미하게 맴도는 불이 가까이서 장난치는 분위기",
                    "유독 3개의 불이 장난끼가 많은 듯 하다. "
                    + "자세히 관찰해보니 하나는 아주 밝게 타오르고 있고, 하나는 타오르다가 꺼졌다가 하며 놀래키곤 한다. "
                    + "하나는 희미한 불이지만 가까이서 맴돌곤 한다.\n"
                    + "무엇을 잡아볼까.")
                .Add(Act("catch", "{0}", EventAction.OnTarget(EventActionKind.PurgeTag), "끝")
                    .FilledWith(EventChoiceFill.OwnedTags, 3, "{0}")
                    .With(EventEffectLine.Cost("이 태그 문양 전부"))
                    .With(EventEffectLine.Gain("무작위 문양")))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            return script.Add(Ending("억센 손에 붙잡힌 불들이 움찔거리며 사그라드는 분위기",
                "억센 손길로 하나를 잡아 뒤흔드니 불들이 움찔거리며 사그라든다.\n"
                + "이제야 잠에 들 수 있을 것 같다."));
        }

        /// <summary>06 문양 거래. 긍정, 화면 3개.</summary>
        private static EventScript SymbolTrade()
        {
            EventScript script = Begin("symbol_trade", "문양 거래", EventNature.Good);

            script.Add(new EventScreen("1", "뼈만 남은 기묘한 아이가 내 식량을 바라보며 손을 내미는 분위기",
                    "얼마 남지 않은 식량이다. 비장한 마음으로 입에 쑤셔넣던 도중, "
                    + "기묘한 분위기의 아이가 식량을 쳐다본다. 무시하려 했건만, 손까지 내민다. "
                    + "하지만 내게도 소중한 식량이다.\n"
                    + "\"먹을 것을 주시면, 제게도 중요한걸 건네드릴게요. 정말, 배고파요.\"\n"
                    + "지금 식량을 먹지 않으면 언제 식량을 얻어서 먹을 수 있을지 모른다. 무시할까, 거래할까.")
                .Add(EventScreenChoice.Advance("talk", "거래한다", "2"))
                .Add(EventScreenChoice.Finish("leave", "거절한다")));

            script.Add(new EventScreen("2", "아이의 손바닥 위에 붉고 파랗고 흰 행운석 셋이 빛나는 분위기",
                    "\"행운석이에요. ...세상이 이렇게 되기 전, 값이 꽤 나갔던 돌이에요. "
                    + "노파한테 팔면 식량을 얻을 수 있을텐데, 지금 너무 배가 고파서...\"\n"
                    + "하나는 피처럼 붉고, 하나는 바다처럼 파랗다. 하나는 구름처럼 희다. "
                    + "아이를 자세히 보니 뼈만 드러나있다. 고르더라도 하나만 고르도록 할까.")
                .Add(Act("trade", "{0}", EventAction.OnTarget(EventActionKind.ChangeSymbol), "끝")
                    .FilledWith(EventChoiceFill.OwnedSymbols, 3, "문양 선택")
                    .Revealing()
                    .With(EventEffectLine.Cost(Named()))
                    .With(EventEffectLine.Gain(Decided(0))))
                .Add(EventScreenChoice.Finish("cancel", "마음을 바꾼다")));

            return script.Add(Ending("아이가 식량을 허겁지겁 먹고 나는 행운석을 쥐고 웃는 분위기",
                "\"...!감사해요.\"\n"
                + "아이가 식량을 허겁지겁 먹는다.\n"
                + "나는 웃으며 행운석을 손에 꼭 쥔다. 어째선지 발걸음이 가벼운 듯한 느낌이다."));
        }

        /// <summary>
        /// 08 피의 거래. 긍정, 화면 3개.
        /// 화면 2 의 "전체 문양 목록에서 원하는 문양 1개를 획득한다" 는 고르기 화면에서 고른다.
        /// 2026년 10월 5일에 고르기 화면을 만들며 옮겼다. 기획서가 `00` 으로 비워 둔 체력은 최대 체력의 15% 로 두었다.
        /// </summary>
        private static EventScript BloodDeal()
        {
            EventScript script = Begin("blood_deal", "피의 거래", EventNature.Good);

            script.Add(new EventScreen("1", "붓을 든 기묘한 노인이 무거운 짐과 지게를 가리키며 집요하게 따라오는 분위기",
                    "붓을 든 기묘한 노인과 눈을 마주친다. 눈을 피하려 했건만. 집요하게 따라온다.\n"
                    + "\"저 짐을 이 지게에 올려주면, 원하는 힘을 새겨주겠네.\"\n"
                    + "짐이, 어마무시하게 무거워보인다. 피곤할대로 피곤한데 지나칠까, 노인의 말을 믿어볼까.")
                .Add(Act("pay", "체력으로 치른다",
                        EventAction.ByMaxHealth(EventActionKind.LoseHealth, 0.15f), "2")
                    .With(EventEffectLine.Cost("체력 {hp}")))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "짐을 다 올린 나에게 노인이 붓을 들고 원하는 힘을 묻는 분위기",
                    "\"이걸 해내다니. 원하는 힘을 말해보게. 몸에 새겨줄테니.\"")
                .Add(Act("pick", "문양을 고른다", EventAction.OnTarget(EventActionKind.GainSymbol), "끝")
                    .PickingFrom(EventPickSource.AllSymbols)
                    .With(EventEffectLine.Gain("원하는 문양"))));

            return script.Add(Ending("힘이 깃든 몸으로 뒤돌아보니 노인이 흔적 없이 사라진 분위기",
                "정말로 힘이 깃든 듯 하다. 뒤를 돌아보니 노인은 흔적도 없이 사라져 있다. 정체가 뭐였을까."));
        }

        /// <summary>09 부식. 부정, 화면 2개.</summary>
        private static EventScript Corrosion()
        {
            EventScript script = Begin("corrosion", "부식", EventNature.Bad);

            script.Add(new EventScreen("1", "진흙탕에 빠져 발버둥치는 나를 지나가던 상인이 내려다보는 분위기",
                    "지나가던 중 진흙탕에 빠졌다. 발버둥치던 중 지나가던 상인과 눈이 마주친다.\n"
                    + "\"돈을 주면 구해주지.\"\n"
                    + "혼자 빠져나가려고 시도해볼까, 돈을 주고 나올까.")
                .Add(Act("health", "체력을 잃는다",
                        EventAction.ByMaxHealth(EventActionKind.LoseHealth, 0.15f), "끝")
                    .With(EventEffectLine.Cost("체력 {hp}")))
                .Add(Act("gold", "골드를 잃는다",
                        EventAction.Gold(EventActionKind.LoseGold, 50), "끝")
                    .With(EventEffectLine.Cost("골드 50"))));

            return script.Add(Ending("진흙을 털고 다시 걸음을 옮기는 분위기",
                "잃어버린 것이 있어도, 삶은 계속되는 법. 다시 발걸음을 옮긴다."));
        }

        /// <summary>
        /// 10 정신 붕괴. 부정, 화면 2개.
        /// 후보는 "가장 많이 보유한 태그를 가진 문양 중 무작위 3종" 이다.
        /// 태그를 장수대로 세어 가장 많은 것을 고르고 그 태그를 가진 보유 문양에서 뽑는다.
        /// </summary>
        private static EventScript MindCollapse()
        {
            EventScript script = Begin("mind_collapse", "정신 붕괴", EventNature.Bad);

            script.Add(new EventScreen("1", "깊은 어둠이 자욱한 악몽 속에서 손끝에 모아둔 값진 돌들이 만져지는 분위기",
                    "악몽. 필시, 악몽이다. 깊은 어둠만이 눈 앞에 자욱하다.\n"
                    + "무엇이라도 잡아야한다. 손을 휘저으니 이전에 모아둔 값진 돌들이 만져진다.\n"
                    + "무엇을 잡을까.")
                .Add(Act("lose", "{0}", EventAction.OnTarget(EventActionKind.LoseSymbol), "끝")
                    .FilledWith(EventChoiceFill.MostCommonTagSymbols, 3, "문양 선택")
                    .With(EventEffectLine.Cost(Named()))));

            return script.Add(Ending("힘을 너무 준 손 안에서 돌이 깨져 있는 분위기",
                "너무 힘을 준 나머지 돌이 깨져버린 것을 마주한다.\n"
                + "이곳은 영 꿈자리가 좋지 않군, 다시 길을 떠난다."));
        }

        // ── 6.2 유물 이벤트 ──────────────────────────────────────

        /// <summary>
        /// 12 잠든 보물. 긍정, 화면 4개.
        /// 기획서 화면 3 은 둘 다 가졌을 때의 마무리, 화면 4 는 하나만 가졌을 때의 마무리다.
        /// 화면 3 의 글은 `끝2` 에, 화면 4 의 글은 `끝` 에 들어간다.
        /// "둘 다 가진다" 는 유물 둘을 받으며 최대 체력을 함께 깎는다 (`CostingMaxHealth`).
        /// </summary>
        private static EventScript SleepingTreasure()
        {
            EventScript script = Begin("sleeping_treasure", "잠든 보물", EventNature.Good);

            script.Add(new EventScreen("1", "동굴 속 고급스럽지만 어딘가 수상한 거대한 상자의 분위기",
                    "동굴 탐험 중 거대한 상자를 발견한다.\n"
                    + "꽤나 고급스러워 보이는 상자지만, 어째선지 수상한 낌새이다. 열어볼까, 그냥 떠날까.")
                .Add(EventScreenChoice.Advance("search", "탐색한다", "2"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "크고 깊은 상자 속 서로 다른 방향 깊숙이 유물 둘이 박혀 있는 분위기",
                    "상자는 꽤나 크고 깊었다. 둘 다 다른 방향으로 깊이 있어 꺼내기 쉽지 않아보인다.\n"
                    + "어떤 걸 꺼내야할까.")
                .Add(Gain("one", EventActionKind.GainRelic, "끝")
                    .FilledWith(EventChoiceFill.RandomRelics, 2, "유물")
                    .With(EventEffectLine.Gain(Named())))
                .Add(Act("both", "둘 다 가진다",
                        new EventAction(EventActionKind.GainRelic).Times(2).CostingMaxHealth(0.10f), "끝2")
                    .TakingAllFrom("one")
                    .With(EventEffectLine.Gain(AllDecided()))
                    .With(EventEffectLine.Cost("최대 체력 {hpCost}"))));

            script.Add(Ending("유용해 보이는 유물을 챙기며 앞으로의 여정을 기대하는 분위기",
                "꽤 유용할 듯 하다. 앞으로의 여정이 기대된다."));

            return script.Add(Ending2("욕심을 부린 끝에 한숨을 쉬며 무거운 발걸음을 옮기는 분위기",
                "...이렇게까지 필요한 것은 아니었는데.\n"
                + "어째선지 한숨이 절로 나온다. 발걸음이 무겁다."));
        }

        /// <summary>13 유물 교환. 긍정, 화면 3개.</summary>
        private static EventScript RelicTrade()
        {
            EventScript script = Begin("relic_trade", "유물 교환", EventNature.Good);

            script.Add(new EventScreen("1", "짐을 정리하는 내게 행상인이 유물을 보고 눈을 빛내며 다가오는 분위기",
                    "짐을 정리하던 중 행상인이 눈을 빛내며 다가온다. "
                    + "그동안 모은 유물에 관심을 보이는 듯 하다.\n"
                    + "\"저기...\"")
                .Add(EventScreenChoice.Advance("talk", "이야기를 듣는다", "2"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "행상인이 자기 유물을 내밀며 교환을 권하는 분위기",
                    "\"훌륭한 것들을 모으셨군요. 나이도 보아하니 젊어보이는데, 대단합니다. "
                    + "내 것과 바꾸겠습니까?\n"
                    + "저도 안목이 꽤나 훌륭하답니다. 젊은이한테 좋은 교환이 될겁니다.\"")
                .Add(Act("swap", "교환한다", new EventAction(EventActionKind.ChangeRelic).StepUpRarity(50, 40, 10), "끝")
                    .Revealing()
                    .With(EventEffectLine.Cost(Decided(0)))
                    .With(EventEffectLine.Gain(Decided(1)))));

            return script.Add(Ending("수상한 분위기와 달리 괜찮은 유물을 받아 들고 길을 떠나는 분위기",
                "수상한 분위기와는 다르게 그의 말처럼 꽤 괜찮은 유물이다. 마저 길을 떠나볼까."));
        }

        /// <summary>18 주인 없는 유물. 긍정, 화면 3개.</summary>
        private static EventScript OwnerlessRelic()
        {
            EventScript script = Begin("ownerless_relic", "주인 없는 유물", EventNature.Good);

            script.Add(new EventScreen("1", "나무 아래 누군가 흘리고 간 듯한 유물이 놓인 분위기",
                    "나무 아래에 유물이 보인다. 설마, 누군가 흘리고 간 유물일까.")
                .Add(EventScreenChoice.Advance("search", "탐색한다", "2"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "한쪽 유물에 손을 대자 다른 쪽이 땅속으로 파고드는 분위기",
                    "한 쪽에 손을 대자 한 쪽은 땅 속으로 파고든다. 유물의 힘을 땅이 흡수하려는 듯 하다.\n"
                    + "무엇을 선택해야 좋을까.")
                .Add(Gain("pick", EventActionKind.GainRelic, "끝")
                    .FilledWith(EventChoiceFill.RandomRelics, 2, "유물")
                    .RarityBetween(1, -1)
                    .With(EventEffectLine.Gain(Named()))));

            return script.Add(Ending("대가 없이 유물을 얻어 예감 좋게 길을 나서는 분위기",
                "유물을 아무 대가 없이 얻다니, 오늘은 운이 정말 좋은 것 같다. 왠지, 예감이 좋다."));
        }

        /// <summary>19 두 개의 상자. 무작위, 화면 4개.</summary>
        private static EventScript TwoBoxes()
        {
            EventScript script = Begin("two_boxes", "두 개의 상자", EventNature.Random);

            script.Add(new EventScreen("1", "비릿하게 웃는 노파가 큼직한 상자 둘을 가리키는 분위기",
                    "길을 가던 중 웬 노파가 불러세운다.\n"
                    + "\"자네, 행운을 시험해보지 않겠는가?\"\n"
                    + "노파가 비릿한 웃음을 지으며 두 상자를 가리킨다. 꽤나 큰 크기의 상자다.\n"
                    + "\"하나는 함정이고, 하나는 귀한 보물일세. 젊은이는 운이 꽤 좋을 것 같은데 말이야.\"\n"
                    + "노파의 말이 맞을까?")
                .Add(FiftyFifty("first", "첫 번째를 연다"))
                .Add(FiftyFifty("second", "두 번째를 연다"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2A", "보물 상자를 열고 입꼬리가 올라가는 나와 아쉬워하는 노파의 분위기",
                    "...다행히도 결과는, 보물 상자였다. 뜻하지 않은 행운에 입꼬리가 계속해서 올라간다.\n"
                    + "노파는 어째선지 아쉬운 듯한 표정이다.")
                .Add(Act("take", "챙긴다", new EventAction(EventActionKind.GainRelic), "끝")
                    .Revealing()
                    .With(EventEffectLine.Gain(Decided(0)))));

            script.Add(new EventScreen("2B", "상자에 몸이 반쯤 빠진 나를 두고 노파가 깔깔 웃는 분위기",
                    "상자에 몸의 절반이 빠진다. 아무리 애를 써도 빠져나가지질 않는다.\n"
                    + "노파가 깔깔거리며 웃는다.\n"
                    + "\"행운 따윈 누구에게도 없단다, 애송아!\"\n"
                    + "얼마 지나지 않아 노파의 기척이 사라진다. 설마 버리고 간걸까, 그렇겠지. 한숨부터 절로 나온다.")
                .Add(Act("endure", "버틴다",
                        EventAction.ByMaxHealth(EventActionKind.LoseHealth, 0.20f), "끝")
                    .With(EventEffectLine.Cost("체력 {hp}"))));

            return script.Add(Ending("결과가 어떻든 다시 갈 길을 가는 분위기",
                "그 노파 덕에 오늘의 행운을 확실히 알 수 있었다.\n"
                + "결과가 어찌됐든, 가야할 길을 가야겠지."));
        }

        // ── 6.3 코인 이벤트 ──────────────────────────────────────

        /// <summary>20 환전. 무작위, 화면 3개.</summary>
        private static EventScript MoneyChange()
        {
            EventScript script = Begin("money_change", "환전", EventNature.Random);

            script.Add(new EventScreen("1", "길가의 상인 무리가 코인을 바꿔 주겠다며 손을 내미는 분위기",
                    "지나가던 중 상인 무리를 만났다.\n"
                    + "상인은 코인을 무작위로 바꿔주겠다고 제안한다.")
                .Add(Act("swap", "{0}", EventAction.OnTarget(EventActionKind.ChangeCoin), "2")
                    .FilledWith(EventChoiceFill.OwnedCoins, 3, "코인 선택")
                    .With(EventEffectLine.Cost(Named()))
                    .Revealing()
                    .With(EventEffectLine.Gain(Decided(0))))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "묘한 표정의 나에게 상인의 친구가 자기도 바꿔 주겠다며 나서는 분위기",
                    "나의 묘한 표정을 보자 옆에 있던 그의 친구가 자신도 바꿔주겠다며 제안한다.")
                .Add(Act("swap2", "{0}",
                        EventAction.OnTarget(EventActionKind.ChangeCoin).Costing(10), "끝")
                    .FilledWith(EventChoiceFill.OwnedCoins, 3, "코인 선택")
                    .With(EventEffectLine.Cost("골드 10"))
                    .With(EventEffectLine.Cost(Named()))
                    .Revealing()
                    .With(EventEffectLine.Gain(Decided(0))))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            return script.Add(Ending("상인 무리가 유유히 떠나가는 분위기",
                "상인 무리가 유유히 떠나간다. 나도 가야할 길을 가야겠지."));
        }

        /// <summary>23 코인 매각. 긍정, 화면 3개.</summary>
        private static EventScript CoinSale()
        {
            EventScript script = Begin("coin_sale", "코인 매각", EventNature.Good);

            script.Add(new EventScreen("1", "행상인 차림인데 짐이 가벼운 기묘한 남자가 반갑게 웃으며 다가오는 분위기",
                    "...정말이지, 기묘한 차림의 남자다. 차림은 행상인이 분명한데, 짐은 가벼워보인다.\n"
                    + "그를 쳐다보던 도중 눈이 마주친다. 그가 반갑게 웃으며 다가온다.")
                .Add(EventScreenChoice.Advance("talk", "이야기를 듣는다", "2"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "남자가 골드 주머니를 흔들며 물건을 사겠다고 하는 분위기",
                    "\"내게 물건을 팔게. 그러면, 골드를 주겠네. 정직한 값으로 주도록 하지.\"\n"
                    + "그의 말을 믿어도 되는걸까. 일단 골드가 들어있는 주머니를 점검해보도록 하자. "
                    + "돈이 부족한 상황이었나?")
                .Add(Act("sell", "{0}", EventAction.OnTarget(EventActionKind.SellCoin), "끝")
                    .FilledWith(EventChoiceFill.OwnedCoins, 3, "코인 선택")
                    .With(EventEffectLine.Cost(Named()))
                    .With(EventEffectLine.Gain("등급별 골드")))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            return script.Add(Ending("두둑해진 주머니를 들고 가벼운 발걸음으로 떠나는 분위기",
                "두둑해진 주머니가 어째선지 기분이 좋다. 가벼운 발걸음으로 길을 떠난다."));
        }

        /// <summary>
        /// 25 미치광이 대장장이. 부정, 화면 2개.
        /// 초안에서는 `빛 바랜 동전` 이었다. 이벤트 기획서 PDF 가 이 이름과 이야기로 바꿨다.
        /// </summary>
        private static EventScript MadSmith()
        {
            EventScript script = Begin("mad_smith", "미치광이 대장장이", EventNature.Bad);

            script.Add(new EventScreen("1", "대장장이라 자칭하는 노인이 내 코인을 낚아채 가는 분위기",
                    "웬 노인이 자신을 대장장이라고 하며, 코인을 더 희귀하게 만들어주겠다고 한다.\n"
                    + "거절하려 했지만 빼앗기고 만다.")
                .Add(Act("accept", "받아들인다",
                        new EventAction(EventActionKind.DowngradeCoin).TargetingRarityAtLeast(1), "끝")
                    .With(EventEffectLine.Cost("무작위 보유 코인 한 등급 아래로"))));

            return script.Add(Ending("대장장이가 멋쩍게 웃으며 도망치고 한숨을 쉬는 분위기",
                "코인이 더 흔해지고 말았다. 대장장이가 멋쩍게 웃으며 도망친다.\n"
                + "한숨이 절로 나온다. 갈 길이 멀게만 느껴진다."));
        }

        /// <summary>26 반짝이는 것. 긍정, 화면 3개.</summary>
        private static EventScript SomethingShiny()
        {
            EventScript script = Begin("something_shiny", "반짝이는 것", EventNature.Good);

            script.Add(new EventScreen("1", "커다란 바위 아래에서 무언가 반짝이는 분위기",
                    "커다란 바위 아래로 반짝이는 것들이 보인다. 다가가볼까?")
                .Add(EventScreenChoice.Advance("look", "살펴본다", "2"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "바위에 박힌 보석 셋 가운데 하나에 손을 대자 나머지 둘의 빛이 꺼지는 분위기",
                    "자세히 보니 보석 세 개가 박혀있다. "
                    + "모두 챙겨가고 싶지만, 하나에 손을 대면 나머지 두 개의 보석의 빛이 꺼지고 만다.\n"
                    + "어쩔 수 없이 하나만 골라야할 듯 하다.")
                .Add(Gain("pick", EventActionKind.GainCoin, "끝")
                    .FilledWith(EventChoiceFill.RandomCoins, 3, "코인")
                    .RarityWeights(70, 30)
                    .With(EventEffectLine.Gain(Named()))));

            return script.Add(Ending("화려하게 빛나는 보석을 가방에 넣고 떠나는 분위기",
                "보석이 화려하게 빛이 난다. 기분좋게 보석을 가방에 넣고 다시 길을 떠난다."));
        }

        /// <summary>27 축복. 긍정, 화면 2개.</summary>
        private static EventScript Blessing()
        {
            EventScript script = Begin("blessing", "축복", EventNature.Good);

            script.Add(new EventScreen("1", "성직자와 함께 기도하고 그가 감격해 축복을 내리는 분위기",
                    "지나가다 성직자와 마주친다. 그는 이런 세상에서도 분명 신이 세상을 구원해줄 것이라 하였다.\n"
                    + "\"자, 다함께 기도합시다.\"\n"
                    + "함께 기도를 하자 성직자는 감격하여 축복을 내려주겠다고 한다.")
                .Add(Act("bless", "{0}", EventAction.OnTarget(EventActionKind.UpgradeCoin), "끝")
                    .FilledWith(EventChoiceFill.OwnedCoins, 3, "코인 선택")
                    .RarityBetween(0, 1)
                    .With(EventEffectLine.Cost(Named()))
                    .With(EventEffectLine.Gain("한 등급 위 무작위 코인")))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            return script.Add(Ending("따뜻한 온기를 느끼며 다시 길을 떠나는 분위기",
                "따뜻함이 온 몸을 맴도는 듯하다. 온기를 느끼며 다시 길을 떠난다."));
        }

        // ── 6.4 회복과 골드 이벤트 ───────────────────────────────

        /// <summary>28 회복. 긍정, 화면 3개.</summary>
        private static EventScript Recovery()
        {
            EventScript script = Begin("recovery", "회복", EventNature.Good);

            script.Add(new EventScreen("1", "햇빛이 드는 작고 평화로운 강가에서 지친 몸을 쉬게 하는 분위기",
                    "끝을 알 수 없는 여정에 몸이 지칠대로 지친 상태. 그때, 햇빛이 들어오는 작은 강을 만난다.\n"
                    + "물 온도도 그닥 차갑지 않고, 분위기도 평화롭다. 오랜만에 피로를 풀고 갈까.")
                .Add(Act("heal", "회복한다",
                        EventAction.ByMaxHealth(EventActionKind.Heal, 0.30f), "끝")
                    .With(EventEffectLine.Gain("체력 {hp}")))
                .Add(EventScreenChoice.Advance("look", "살펴본다", "2"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "강가에 탐스럽게 열린 커다란 열매의 분위기",
                    "자세히 살펴보니 강가에 커다란 열매가 있다. 저걸 먹으면 체력이 금방 회복될 듯 하다.")
                .Add(Act("eat", "먹는다",
                        EventAction.ByMaxHealth(EventActionKind.GainMaxHealth, 0.08f), "끝")
                    .With(EventEffectLine.Gain("최대 체력 {hp}")))
                .Add(EventScreenChoice.Advance("back", "돌아간다", "1")));

            return script.Add(Ending("가벼워진 몸으로 강가를 떠나는 분위기",
                "가벼워진 몸으로 다시 길을 떠난다."));
        }

        /// <summary>29 생명력 거래. 긍정, 화면 3개.</summary>
        private static EventScript LifeTrade()
        {
            EventScript script = Begin("life_trade", "생명력 거래", EventNature.Good);

            script.Add(new EventScreen("1", "행상인이라기엔 헐거운 차림의 남성이 말을 걸어오는 분위기",
                    "행상인이라기엔 차림이 헐거운 남성이 다가온다.\n"
                    + "대화를 하고자 하는 듯한데... 얘기를 들어볼까.")
                .Add(EventScreenChoice.Advance("talk", "이야기를 듣는다", "2"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "남성이 골드를 내보이며 젊음을 달라고 섬뜩하게 제안하는 분위기",
                    "그가 위험한 제안을 해온다.\n"
                    + "\"내겐 젊음이 필요해. 대신 골드를 주겠어. 어떻게 하겠나?\"")
                .Add(Trade("small", "체력 {hp} 을 내준다", 0.10f, 60))
                .Add(Trade("mid", "체력 {hp} 을 내준다", 0.20f, 130))
                .Add(Trade("big", "체력 {hp} 을 내준다", 0.30f, 210))
                .Add(EventScreenChoice.Finish("leave", "거절한다")));

            return script.Add(Ending("흐려지는 시야 속에서 정신을 붙잡고 길을 나서는 분위기",
                "시야가 흐려진다. 정신을 붙잡고, 다시 길을 나선다."));
        }

        /// <summary>31 약탈. 부정, 화면 3개.</summary>
        private static EventScript Plunder()
        {
            EventScript script = Begin("plunder", "약탈", EventNature.Bad);

            script.Add(new EventScreen("1", "잠에서 깨자마자 목에 도적의 칼날이 닿아 있는 위협적인 분위기",
                    "잠에서 깨자마자, 목에 칼날이 느껴진다. 도적이다.\n"
                    + "\"난 생각보다 자비로운 도적이라서, 골드를 내어주면 살려줄게. 아니면 나랑 싸워야겠지.\"")
                .Add(EventScreenChoice.Advance("go", "계속한다", "2")));

            script.Add(new EventScreen("2", "피로가 쌓인 몸으로 칼을 쥐고 골드와 싸움 사이에서 망설이는 분위기",
                    "칼이 있기야 해서 어떻게든 싸울 수는 있겠지만, 피로가 누적된 몸이라 쉽지 않아보인다.\n"
                    + "골드를 내어주고 순순히 물러갈까, 싸워볼까.")
                .Add(Act("gold", "골드를 내준다",
                        EventAction.Gold(EventActionKind.LoseGold, 60), "끝")
                    .With(EventEffectLine.Cost("골드 60")))
                .Add(Act("health", "싸운다",
                        EventAction.ByMaxHealth(EventActionKind.LoseHealth, 0.20f), "끝")
                    .With(EventEffectLine.Cost("체력 {hp}"))));

            return script.Add(Ending("손해를 본 채 스스로를 다독이며 떠나는 분위기",
                "어떤 선택이었든, 손해는 있었을 것이라며 스스로를 다독인다."));
        }

        /// <summary>32 침식. 부정, 화면 3개.</summary>
        private static EventScript Erosion()
        {
            EventScript script = Begin("erosion", "침식", EventNature.Bad);

            script.Add(new EventScreen("1", "도깨비불이 몸속으로 스며드는 불길한 분위기",
                    "도깨비불이 몸에 스며든다. 멈춰보려 하지만, 이미 늦은 상황.")
                .Add(EventScreenChoice.Advance("go", "계속한다", "2")));

            script.Add(new EventScreen("2", "몸속의 도깨비불이 들뜬 기괴한 목소리로 장난을 거는 분위기",
                    "\"난 장난이 좋아, 난 장난이 좋아! 너에게 두가지 선택지를 주겠어. 무엇을 선택할래?\"\n"
                    + "기괴한 목소리가 들뜬 듯이 신나게 물어본다.")
                .Add(Act("now", "현재 체력을 잃는다",
                        EventAction.ByMaxHealth(EventActionKind.LoseHealth, 0.25f), "끝")
                    .With(EventEffectLine.Cost("체력 {hp}")))
                .Add(Act("max", "최대 체력을 잃는다",
                        EventAction.ByMaxHealth(EventActionKind.LoseMaxHealth, 0.10f), "끝")
                    .With(EventEffectLine.Cost("최대 체력 {hp}"))));

            return script.Add(Ending("몸을 맴도는 불길한 기운을 떨쳐 내고 걸음을 옮기는 분위기",
                "불길한 기운이 몸을 맴돌지만 떨쳐낸다. 갈 길이 멀다."));
        }

        /// <summary>33 남겨진 것. 긍정, 화면 3개.</summary>
        private static EventScript LeftBehind()
        {
            EventScript script = Begin("left_behind", "남겨진 것", EventNature.Good);

            script.Add(new EventScreen("1", "아직 온기가 남은 모닥불 잔해, 누군가 급히 떠난 흔적의 분위기",
                    "모닥불 잔해... 아직 온기가 남아있다. 누군가 급히 떠나간 것일까?")
                .Add(EventScreenChoice.Advance("look", "살펴본다", "2"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "모닥불 주위에 남겨진 골드를 신나게 주머니에 담는 분위기",
                    "주위를 탐색하니 골드가 남겨져 있었다. 신나게 골드를 주머니 안에 넣는다.")
                .Add(Act("take", "챙긴다", EventAction.Gold(EventActionKind.GainGold, 80), "끝")
                    .With(EventEffectLine.Gain("골드 80"))));

            return script.Add(Ending("무거워진 주머니를 차고 가벼운 발걸음으로 떠나는 분위기",
                "무거워진 주머니, 가벼운 발걸음. 정말이지 좋은 조합이다."));
        }

        /// <summary>34 도박. 무작위, 화면 5개.</summary>
        private static EventScript Gamble()
        {
            EventScript script = Begin("gamble", "도박", EventNature.Random);

            script.Add(new EventScreen("1", "동굴 안 노파들의 작은 도박장, 한 노파가 슬그머니 다가오는 분위기",
                    "동굴 안에 들어가자 노파들끼리의 작은 도박장이 보인다. 한 노파가 슬그머니 다가온다.")
                .Add(EventScreenChoice.Advance("talk", "이야기를 듣는다", "2"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "두 노파 중 누구에게 걸지 부추기는 도박장의 분위기",
                    "\"자네도 걸어봐! 젊음은 행운의 여신의 증표지.\"\n"
                    + "그날따라 어째선지 솔깃하다.\n"
                    + "\"저쪽 할멈한테 걸래, 이쪽 할멈에게 걸래?\"")
                .Add(Bet("small", "30G를 건다", 30))
                .Add(Bet("big", "60G를 건다", 60))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(GambleWin(30));
            script.Add(GambleWin(60));

            script.Add(new EventScreen("짐", "이긴 쪽 노파가 신나게 웃고 담배 연기가 매캐한 분위기",
                    "\"하하, 젊은이. 큰 코 다쳤구만.\"\n"
                    + "이긴 쪽의 노파가 신나게 웃는다. 담배연기가 어째선지 매캐하게 느껴진다. 속이 쓰리다.")
                .Add(EventScreenChoice.Advance("accept", "받아들인다", "끝")));

            return script.Add(Ending("한 노파의 기도를 뒤로하고 도박장을 떠나는 분위기",
                "행운의 여신이 당신께 깃들길.\n"
                + "한 노파의 기도를 뒤로 하고 길을 떠난다."));
        }

        // ── 6.5 도전과 특수 이벤트 ───────────────────────────────

        /// <summary>35 도전자. 중립, 화면 4개.</summary>
        private static EventScript Challenger()
        {
            EventScript script = Begin("challenger", "도전자", EventNature.Neutral);

            script.Add(new EventScreen("1", "동굴 안쪽에서 함성이 터지는 몸싸움 도박장의 분위기",
                    "동굴 안쪽에서 함성이 들린다.\n"
                    + "알고보니 몸싸움의 승부에 돈을 거는 도박장의 일종이었다.")
                .Add(EventScreenChoice.Advance("step", "다가간다", "2"))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("2", "준비된 상대가 시작 신호와 함께 창을 휘두르는 분위기",
                    "\"자네도 해보겠는가? 꽤 잘할 것 같은데.\"\n"
                    + "고개를 끄덕이자 곧바로 상대가 준비되었다.\n"
                    + "\"시작!\"\n"
                    + "상대가 창을 휘두른다.")
                // 비고 · 부정 효과 A와 B는 3장의 부정 효과 목록에서 무작위로 정한다.
                // 고른 것을 기억해 두었다가 화면 3 의 전투에 건다. 2026년 10월 8일 외부 검토가 이어지지 않았다고 짚었다.
                .Add(EventScreenChoice.Advance("penalty", "부정 효과를 받는다", "3")
                    .FilledWith(EventChoiceFill.CombatPenalties, 2, "부정 효과를 받는다")
                    .EachLabeled("부정 효과 A를 받는다", "부정 효과 B를 받는다")
                    .Remember("penalty")
                    .With(EventEffectLine.Cost(Named())))
                .Add(EventScreenChoice.Finish("leave", "떠난다")));

            script.Add(new EventScreen("3", "기합과 함께 검으로 상대를 가르는 분위기",
                    "기합을 지르며 검으로 상대를 가른다.")
                .Add(Act("fight", "싸운다", EventAction.Combat(true, true).Recalling("penalty"), "끝")
                    .With(EventEffectLine.Gain("이기면 보상 두 배"))
                    .With(EventEffectLine.Cost("부정 효과"))));

            return script.Add(Ending("두둑한 보상을 손에 쥐고 기분 좋게 동굴을 떠나는 분위기",
                "\"역시, 잘 싸울 줄 알았다니까.\"\n"
                + "손에 두둑한 보상이 담긴다. 기분좋게 동굴을 떠난다."));
        }

        /// <summary>38 매복. 부정, 화면 2개.</summary>
        private static EventScript Ambush()
        {
            EventScript script = Begin("ambush", "매복", EventNature.Bad);

            script.Add(new EventScreen("1", "함정에 빠진 순간 매복한 도적들이 칼을 빼 들고 둘러싸는 분위기",
                    "아차! 함정에 빠졌다. 도적들이 매복하고 있었다.\n"
                    + "칼을 빼들고 싸워볼까, 아니면 도망쳐볼까.")
                // "무작위 부정 효과 1개를 받은 채 전투를 시작한다." 기억이 없으니 싸울 때 목록에서 하나를 뽑는다.
                .Add(Act("fight", "싸운다", EventAction.Combat(false, true), "끝")
                    .With(EventEffectLine.Cost("부정 효과")))
                .Add(Act("flee", "도망친다",
                        EventAction.ByMaxHealth(EventActionKind.LoseHealth, 0.15f), "끝")
                    .With(EventEffectLine.Cost("체력 {hp}"))));

            return script.Add(Ending("겨우 포위를 벗어나 숨이 턱끝까지 찬 분위기",
                "겨우 포위를 벗어났다. 숨이 턱끝까지 찬다. 가야할 길이 먼데도..."));
        }

        // ── 짓는 데 쓰는 것들 ────────────────────────────────────

        private static EventScript Begin(string eventId, string title, EventNature nature)
        {
            EventScript script = new EventScript();
            script.EventId = eventId;
            script.Title = title;
            script.Nature = nature;
            return script;
        }

        /// <summary>마무리 화면. 4장 공통 규칙 의 "마무리 화면의 종료 선택지로 이벤트를 마친다".</summary>
        private static EventScreen Ending(string mood, string body)
        {
            return new EventScreen("끝", mood, body)
                .Add(EventScreenChoice.Finish("leave", "떠난다"));
        }

        /// <summary>마무리 화면이 둘인 이벤트의 두 번째 것.</summary>
        private static EventScreen Ending2(string mood, string body)
        {
            return new EventScreen("끝2", mood, body)
                .Add(EventScreenChoice.Finish("leave", "떠난다"));
        }

        private static EventScreenChoice Act(
            string id, string label, EventAction action, string next)
        {
            return EventScreenChoice.Act(id, label, action, next);
        }

        /// <summary>
        /// 얻고 잃는 줄에 적는 지목된 물건 이름. `[곡괭이]` 처럼 괄호로 감싼다.
        /// 2026년 10월 5일 원재가 "곡괭이 — 유물 1개" 가 아니라 "유물 — [곡괭이]" 로 적으라고 했다.
        /// 선택지 이름에는 종류(유물, 문양, 코인)를 적고, 가진 것 중에서 내주는 것이면 "코인 선택" 처럼 적는다.
        /// "고른 코인 1개" 같은 말은 쓰지 않는다.
        /// </summary>
        private static string Named()
        {
            return "[" + EventRunner.NameToken + "]";
        }

        /// <summary>
        /// 미리 정한 결과 하나의 이름. `[곡괭이]` 처럼 괄호로 감싼다. 선택지에 `Revealing` 을 붙여야 정해진다.
        /// 2026년 10월 5일 원재가 의도적으로 숨기는 경우만 "무작위" 로 적고 나머지는 정확한 이름을 적으라고 했다.
        /// 숨기기로 한 것은 03 판도라의 상자, 05 정화, 25 미치광이 대장장이, 27 축복 이다.
        /// 20 환전 은 처음에 숨기기로 했다가 문양 거래(06), 유물 교환(13) 과 같은 방식이어야 한다고 해 공개로 바꿨다.
        /// </summary>
        private static string Decided(int index)
        {
            return "[" + EventRunner.ResultToken(index) + "]";
        }

        /// <summary>미리 정한 결과를 모두. `[곡괭이], [날개]` 로 적힌다.</summary>
        private static string AllDecided()
        {
            return EventRunner.ResultsToken;
        }

        /// <summary>선택지에 지목된 것을 그대로 받는 실행 선택지.</summary>
        private static EventScreenChoice Gain(string id, EventActionKind kind, string next)
        {
            return EventScreenChoice.Act(id, "{0}", EventAction.OnTarget(kind), next);
        }

        /// <summary>반반으로 갈리는 진행 선택지. 19 두 개의 상자 가 쓴다.</summary>
        private static EventScreenChoice FiftyFifty(string id, string label)
        {
            return EventScreenChoice.Advance(id, label, string.Empty)
                .Branch(50, "2A")
                .Branch(50, "2B");
        }

        /// <summary>
        /// 체력을 내주고 골드를 받는 선택지. 29 생명력 거래 가 쓴다.
        ///
        /// 기획서가 "체력을 잃고 00G를 획득한다"로 한 줄에 적어 두었으므로
        /// 체력을 잃는 것을 작업으로 두고 골드는 `GoldGain` 으로 함께 받는다.
        /// </summary>
        private static EventScreenChoice Trade(string id, string label, float ratio, int gold)
        {
            EventAction action = EventAction
                .ByMaxHealth(EventActionKind.LoseHealth, ratio)
                .Giving(gold);

            return EventScreenChoice.Act(id, label, action, "끝")
                .With(EventEffectLine.Cost("체력 {hp}"))
                .With(EventEffectLine.Gain("골드 " + gold));
        }

        /// <summary>
        /// 골드를 걸고 반반으로 갈리는 선택지. 34 도박 이 쓴다.
        ///
        /// 건 금액마다 이기는 화면을 따로 둔다.
        /// 기획서가 "화면 2에서 건 금액의 2배를 획득한다"로 정했는데
        /// 각본이 앞 화면에서 고른 값을 기억하지 않기 때문이다.
        /// 거는 금액이 둘뿐이라 화면을 둘로 나누는 쪽이 간단하다.
        /// </summary>
        private static EventScreenChoice Bet(string id, string label, int amount)
        {
            return EventScreenChoice.Act(
                    id, label, EventAction.Gold(EventActionKind.LoseGold, amount), string.Empty)
                .Branch(50, "이김" + amount)
                .Branch(50, "짐")
                .With(EventEffectLine.Cost("골드 " + amount))
                .With(EventEffectLine.Gain("이기면 골드 " + (amount * 2)));
        }

        /// <summary>도박에서 이긴 화면. 건 금액의 두 배를 받는다.</summary>
        private static EventScreen GambleWin(int bet)
        {
            return new EventScreen("이김" + bet, "탄성과 탄식이 뒤섞인 도박장에서 머쓱하게 골드를 챙기는 분위기",
                    "\"역시, 젊은 사람들이 감이 좋아.\"\n"
                    + "도박장에서 갖가지 탄성과 탄식이 모두 흘러나온다. 나는 머쓱하게 웃으며 골드를 챙긴다.")
                .Add(EventScreenChoice
                    .Act("take", "챙긴다",
                        EventAction.Gold(EventActionKind.GainGold, bet * 2), "끝")
                    .With(EventEffectLine.Gain("골드 " + (bet * 2))));
        }
    }
}
