using System.Collections.Generic;
using System.Text;
using SlotHero.Combat;
using SlotHero.CurrentBuild;
using SlotHero.Events;
using SlotHero.Events.UI;
using SlotHero.Flow;
using SlotHero.Sanctum;
using SlotHero.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Tools
{
    /// <summary>
    /// 이벤트 하나만 되풀이해 굴려 보는 시험 씬의 조종기. 지금은 07 기록 말살 을 본다(2026년 11월 13일 원재).
    /// 그 전에는 36 혼돈 을 봤고 같은 날 원재가 36 시험 씬을 지우고 07 로 바꾸라고 했다.
    ///
    /// 플레이를 누르면 시작 덱을 가진 빈 런을 만들고 그 이벤트를 연다. 각본은 게임과 같은 이벤트 목록 에셋에서 꺼내고,
    /// 작업은 게임과 같은 `RunEventWorld` 가 건다. 태그를 고르는 선택지는 게임과 같은 태그 선택 창(`EventTagPicker`)을 연다.
    ///
    /// **물건 목록에 전설 코인이 없어** 07 은 게임에서 화면 2 에서 막힌다. 그래서 이 씬은 물건 목록 사본에
    /// 시험용 전설 코인 하나를 더해 런에 쥐여 준다. 설정 에셋은 건드리지 않는다.
    /// "전설 코인 없이" 는 그 코인 없이 열어 화면 2 에서 막히는 모습을 본다.
    ///
    /// 고르기 전과 뒤의 가진 코인과 문양을 견주어 왼쪽 칸에 적고, 고른 태그를 가진 문양이 남았는지 센다.
    /// 마무리 화면의 떠난다 를 누르면 처음부터 다시 연다.
    ///
    /// **게임 코드가 아니다.** 게임 흐름(`GameFlowController`)은 이것을 쓰지 않고, 저장도 하지 않는다.
    /// </summary>
    public class EventTestDriver : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private EventScreenController _screen;
        [SerializeField] private TMP_Text _infoLabel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _noLegendButton;

        [Tooltip("태그 선택 창. 게임과 같은 `EventTagPicker` 다.")]
        [SerializeField] private MonoBehaviour _tagPickerBehaviour;

        [Header("설정 에셋")]
        [SerializeField] private RunCatalogConfig _catalog;
        [SerializeField] private RunEventConfig _events;
        [SerializeField] private EventIllustrationConfig _illustrations;
        [SerializeField] private CurrentBuildVisualConfig _buildVisual;

        [Header("시험")]
        [Tooltip("굴려 볼 이벤트 식별자.")]
        [SerializeField] private string _eventId = "record_erasure";

        [Tooltip("첫 시드. 다시 할 때마다 하나씩 오른다.")]
        [SerializeField] private int _seed = 1;

        [Tooltip("물건 목록 사본에 더하는 시험용 전설 코인의 식별자.")]
        [SerializeField] private string _testCoinId = "test_legend_coin";

        [Tooltip("시험용 전설 코인의 이름.")]
        [SerializeField] private string _testCoinName = "시험 전설 코인";

        private RunCatalogConfig _catalogCopy;
        private RunContext _context;
        private EventRunner _runner;
        private Dictionary<string, int> _beforeSymbols = new Dictionary<string, int>();
        private Dictionary<string, int> _beforeCoins = new Dictionary<string, int>();
        private bool _withoutLegend;
        private string _pickedTag = string.Empty;

        private IEventTagPicker TagPicker
        {
            get { return _tagPickerBehaviour as IEventTagPicker; }
        }

        private void Start()
        {
            if (_screen != null)
            {
                _screen.ChoiceSelected += HandleChoice;
                _screen.Finished += HandleFinished;
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(() => Restart(false));
            }

            if (_noLegendButton != null)
            {
                _noLegendButton.onClick.AddListener(() => Restart(true));
            }

            Restart(false);
        }

        private void OnDestroy()
        {
            if (_catalogCopy != null)
            {
                Destroy(_catalogCopy);
            }
        }

        /// <summary>새 런으로 처음부터 연다. withoutLegend 면 시험용 전설 코인을 쥐여 주지 않는다.</summary>
        public void Restart(bool withoutLegend)
        {
            _withoutLegend = withoutLegend;
            _pickedTag = string.Empty;
            _seed++;

            if (TagPicker != null && TagPicker.IsOpen)
            {
                TagPicker.Close();
            }

            if (_catalog == null)
            {
                SetInfo("물건 목록 설정이 없다.");
                return;
            }

            if (_catalogCopy != null)
            {
                DestroyImmediate(_catalogCopy);
            }

            _catalogCopy = Instantiate(_catalog);
            _catalogCopy.Coins.Add(MakeTestCoin());

            RunCatalog catalog = new RunCatalog(_catalogCopy);
            RunSaveData run = RunSaveData.StartNew(1, _seed, 250);
            catalog.Attach(run);
            _context = new RunContext(run, catalog);
            catalog.GiveStartingItems(_context);

            if (!withoutLegend)
            {
                run.Owned.AddCoin(_testCoinId);
            }

            EventScript script = _events != null ? _events.Find(_eventId) : null;
            if (script == null)
            {
                SetInfo("이벤트 목록에 " + _eventId + " 가 없다.");
                return;
            }

            RunEventWorld world = new RunEventWorld(_context, _catalogCopy, _buildVisual);
            world.EventId = script.EventId;
            _runner = new EventRunner(script, world, new EventRandom(_seed));
            _beforeSymbols = CountSymbols();
            _beforeCoins = CountCoins();

            EventPage page = _runner.Begin();
            if (_screen != null && page != null)
            {
                _screen.Open(script.EventId, page, _illustrations);
            }

            SetInfo(Header() + "\n\n고르면 여기에 바뀐 것이 나온다.");
        }

        /// <summary>
        /// 시험용 전설 코인. 그림은 목록에서 가장 높은 등급 코인의 것을 빌린다.
        /// 07 은 등급만 보므로 효과는 없다.
        /// </summary>
        private ItemDefinition MakeTestCoin()
        {
            ItemDefinition coin = new ItemDefinition();
            coin.Id = _testCoinId;
            coin.DisplayName = _testCoinName;
            coin.Rarity = ItemRarity.Legend;

            int best = -1;
            for (int i = 0; i < _catalog.Coins.Count; i++)
            {
                if ((int)_catalog.Coins[i].Rarity > best && _catalog.Coins[i].Icon != null)
                {
                    best = (int)_catalog.Coins[i].Rarity;
                    coin.Icon = _catalog.Coins[i].Icon;
                }
            }

            return coin;
        }

        private void HandleChoice(string eventId, string choiceId)
        {
            if (_runner == null)
            {
                return;
            }

            // 07 의 "지울 태그를 고른다" 는 태그 선택 창을 연다. 닫으면 화면에 그대로 남는다. 게임 흐름과 같다.
            if (_runner.GetPickSource(choiceId) == EventPickSource.AllTags && TagPicker != null)
            {
                EventRunner runner = _runner;
                TagPicker.Open(
                    tag =>
                    {
                        if (_runner == runner)
                        {
                            _pickedTag = tag.ToString();
                            Advance(choiceId, _pickedTag);
                        }
                    },
                    null);
                return;
            }

            Advance(choiceId, string.Empty);
        }

        private void Advance(string choiceId, string pickedId)
        {
            EventStep step = _runner.Choose(choiceId, pickedId);
            if (step.Finished || step.Result == null)
            {
                Restart(_withoutLegend);
                return;
            }

            _screen.ApplyResult(step.Result);
            SetInfo(Header() + "\n\n" + Describe());
        }

        private void HandleFinished(string eventId)
        {
            Restart(_withoutLegend);
        }

        /// <summary>시드와 런 이야기.</summary>
        private string Header()
        {
            return "07 기록 말살 시험\n시드 " + _seed + " · " + (_withoutLegend ? "전설 코인 없이" : "시험 전설 코인 1개")
                + "\n가진 문양 " + _context.Run.Owned.TotalSymbolCount + "장 · 코인 " + _context.Run.Owned.CoinIds.Count + "개";
        }

        /// <summary>가진 문양을 식별자마다 센다.</summary>
        private Dictionary<string, int> CountSymbols()
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();
            List<OwnedSymbol> symbols = _context.Run.Owned.Symbols;

            for (int i = 0; i < symbols.Count; i++)
            {
                int had;
                counts.TryGetValue(symbols[i].SymbolId, out had);
                counts[symbols[i].SymbolId] = had + symbols[i].Count;
            }

            return counts;
        }

        /// <summary>가진 코인을 식별자마다 센다.</summary>
        private Dictionary<string, int> CountCoins()
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();
            List<string> coins = _context.Run.Owned.CoinIds;

            for (int i = 0; i < coins.Count; i++)
            {
                int had;
                counts.TryGetValue(coins[i], out had);
                counts[coins[i]] = had + 1;
            }

            return counts;
        }

        /// <summary>고른 태그, 잃은 코인, 잃은 문양과 얻은 문양, 그 태그를 가진 문양이 남았는지 적는다.</summary>
        private string Describe()
        {
            StringBuilder text = new StringBuilder();

            if (!string.IsNullOrEmpty(_pickedTag))
            {
                SymbolTagType tag;
                string tagName = System.Enum.TryParse(_pickedTag, out tag) ? SymbolTags.GetName(tag, _buildVisual) : _pickedTag;
                int left = System.Enum.TryParse(_pickedTag, out tag) ? CountWithTag(tag) : -1;
                text.Append("고른 태그 ").Append(tagName)
                    .Append("\n남은 ").Append(tagName).Append(" 문양 ").Append(left).Append("장")
                    .Append(left == 0 ? " <color=#70C080>(다 지웠다)</color>" : " <color=#E07060>(남았다)</color>")
                    .Append("\n\n");
            }

            AppendDiff(text, "코인", _beforeCoins, CountCoins());
            text.Append("\n\n");
            AppendDiff(text, "문양", _beforeSymbols, CountSymbols());
            return text.ToString();
        }

        /// <summary>그 태그를 가진 문양 장수.</summary>
        private int CountWithTag(SymbolTagType tag)
        {
            int count = 0;
            List<OwnedSymbol> symbols = _context.Run.Owned.Symbols;

            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolTagType first;
                SymbolTagType second;
                if (_catalogCopy.TryGetSymbolTags(symbols[i].SymbolId, out first, out second) && (first == tag || second == tag))
                {
                    count += symbols[i].Count;
                }
            }

            return count;
        }

        /// <summary>앞뒤를 견주어 잃은 것과 얻은 것을 적는다.</summary>
        private void AppendDiff(StringBuilder text, string kind, Dictionary<string, int> before, Dictionary<string, int> after)
        {
            StringBuilder lost = new StringBuilder();
            StringBuilder gained = new StringBuilder();
            int lostCount = 0;
            int gainedCount = 0;

            HashSet<string> ids = new HashSet<string>(before.Keys);
            ids.UnionWith(after.Keys);

            foreach (string id in ids)
            {
                int had;
                int has;
                before.TryGetValue(id, out had);
                after.TryGetValue(id, out has);

                string name = _catalogCopy.GetDisplayName(id);
                if (has < had)
                {
                    lost.Append("\n  ").Append(name).Append(" × ").Append(had - has);
                    lostCount += had - has;
                }
                else if (has > had)
                {
                    gained.Append("\n  ").Append(name).Append(" × ").Append(has - had);
                    gainedCount += has - had;
                }
            }

            text.Append("<color=#E07060>잃은 ").Append(kind).Append(" ").Append(lostCount).Append("</color>").Append(lost)
                .Append("\n<color=#70C080>얻은 ").Append(kind).Append(" ").Append(gainedCount).Append("</color>").Append(gained);
        }

        private void SetInfo(string text)
        {
            if (_infoLabel != null)
            {
                _infoLabel.text = text;
            }
        }
    }
}
