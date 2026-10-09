using System.Collections.Generic;
using System.Text;
using SlotHero.Sanctum;
using UnityEditor;
using UnityEngine;

namespace SlotHero.SanctumEditor
{
    /// <summary>
    /// 에디터에서 행상 진열과 가격을 눈으로 확인하고 규칙을 한 번에 검증하는 창.
    /// 메뉴 Slot Hero / 성소 미리보기 에서 연다.
    ///
    /// 문양, 코인, 유물 기획서의 코드가 아직 없으므로
    /// 검증용 <see cref="SampleMerchantCatalog"/>를 써서 돌려 본다.
    /// </summary>
    public class SanctumPreviewWindow : EditorWindow
    {
        private SanctumConfig _config;
        private SanctumPriceConfig _priceConfig;

        private int _runSeed = 12345;
        private int _sanctumIndex = 1;
        private int _startGold = 137;
        private int _health = 48;
        private int _maxHealth = 99;
        private int _batchCount = 500;

        private SanctumState _state;
        private SampleMerchantCatalog _catalog;
        private SamplePlayerVitals _vitals;
        private RunGoldState _gold;
        private RunRelicPool _relicPool;

        private Vector2 _scroll;
        private string _log;
        private string _batchReport;
        private bool _batchPassed;

        [MenuItem("Slot Hero/성소 미리보기")]
        public static void Open()
        {
            SanctumPreviewWindow window = GetWindow<SanctumPreviewWindow>();
            window.titleContent = new GUIContent("성소 미리보기");
            window.minSize = new Vector2(620f, 620f);
            window.Show();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawSettings();
            EditorGUILayout.Space();
            DrawSanctum();
            EditorGUILayout.Space();
            DrawStock();
            EditorGUILayout.Space();
            DrawBatch();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("설정", EditorStyles.boldLabel);
            _config = (SanctumConfig)EditorGUILayout.ObjectField(
                "성소 규칙 설정", _config, typeof(SanctumConfig), false);
            _priceConfig = (SanctumPriceConfig)EditorGUILayout.ObjectField(
                "가격 설정", _priceConfig, typeof(SanctumPriceConfig), false);

            _runSeed = EditorGUILayout.IntField("런 시드", _runSeed);
            _sanctumIndex = EditorGUILayout.IntField("몇 번째 성소", _sanctumIndex);
            _startGold = EditorGUILayout.IntField("보유 골드", _startGold);
            _health = EditorGUILayout.IntField("체력", _health);
            _maxHealth = EditorGUILayout.IntField("최대 체력", _maxHealth);

            using (new EditorGUI.DisabledScope(_config == null || _priceConfig == null))
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("성소 들어가기"))
                {
                    EnterSanctum(_runSeed);
                }

                if (GUILayout.Button("시드 바꿔 들어가기"))
                {
                    _runSeed = Random.Range(int.MinValue, int.MaxValue);
                    EnterSanctum(_runSeed);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawSanctum()
        {
            if (_state == null)
            {
                EditorGUILayout.HelpBox("설정 두 개를 넣고 성소에 들어가면 진열이 나온다.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("성소", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                string.Format("골드 {0}    체력 {1} / {2}", _gold.Gold, _vitals.Health, _vitals.MaxHealth));

            EditorGUILayout.BeginHorizontal();

            using (new EditorGUI.DisabledScope(!_state.CanUseCamp(_config)))
            {
                if (GUILayout.Button(_state.CanUseCamp(_config) ? "야영" : "야영 (사용함)"))
                {
                    int healed;
                    PurchaseResult result = _state.TryUseCamp(_config, _vitals, _gold, out healed);
                    Log(result == PurchaseResult.Success
                        ? string.Format("야영으로 체력 {0} 회복", healed)
                        : string.Format("야영 실패: {0}", result));
                }
            }

            if (GUILayout.Button(string.Format("유물 새로고침 ({0})", _state.GetRelicRefreshPrice(_priceConfig))))
            {
                PurchaseResult result = _state.TryRefreshRelics(_config, _priceConfig, _gold, _catalog, _relicPool);
                Log(result == PurchaseResult.Success
                    ? string.Format("유물 새로고침. 다음 가격 {0}", _state.GetRelicRefreshPrice(_priceConfig))
                    : string.Format("유물 새로고침 실패: {0}", result));
            }

            if (GUILayout.Button(string.Format("문양 변경 ({0})", _state.GetSymbolChangePrice(_priceConfig))))
            {
                ChangeFirstOwnedSymbol();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("가진 문양", DescribeOwnedSymbols());

            if (!string.IsNullOrEmpty(_log))
            {
                EditorGUILayout.HelpBox(_log, MessageType.Info);
            }
        }

        private void DrawStock()
        {
            if (_state == null || _state.Stock == null)
            {
                return;
            }

            EditorGUILayout.LabelField("행상 진열", EditorStyles.boldLabel);
            DrawSlotGroup("문양", _state.Stock.SymbolSlots);
            DrawSlotGroup("코인", _state.Stock.CoinSlots);
            DrawSlotGroup("유물", _state.Stock.RelicSlots);
        }

        private void DrawSlotGroup(string title, List<MerchantSlot> slots)
        {
            EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;

            for (int i = 0; i < slots.Count; i++)
            {
                MerchantSlot slot = slots[i];

                if (slot == null || !slot.HasItem)
                {
                    EditorGUILayout.LabelField("빈 자리");
                    continue;
                }

                EditorGUILayout.BeginHorizontal();

                string label = slot.Sold
                    ? string.Format("{0}  (팔림)", slot.Item.DisplayName)
                    : string.Format("{0}  {1} 골드", slot.Item.DisplayName, slot.Price);
                EditorGUILayout.LabelField(label);

                using (new EditorGUI.DisabledScope(slot.Sold || !_gold.CanSpend(slot.Price)))
                {
                    if (GUILayout.Button("사기", GUILayout.Width(60f)))
                    {
                        PurchaseResult result = _state.TryBuy(slot, _gold, _catalog);
                        Log(result == PurchaseResult.Success
                            ? string.Format("{0} 구매", slot.Item.DisplayName)
                            : string.Format("구매 실패: {0}", result));
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        private void DrawBatch()
        {
            EditorGUILayout.LabelField("일괄 검증", EditorStyles.boldLabel);
            _batchCount = EditorGUILayout.IntSlider("시드 수", _batchCount, 10, 5000);

            using (new EditorGUI.DisabledScope(_config == null || _priceConfig == null))
            {
                if (GUILayout.Button("검증 돌리기"))
                {
                    RunBatch(_batchCount);
                }
            }

            if (!string.IsNullOrEmpty(_batchReport))
            {
                EditorGUILayout.HelpBox(_batchReport, _batchPassed ? MessageType.Info : MessageType.Error);
            }
        }

        private void EnterSanctum(int runSeed)
        {
            _catalog = new SampleMerchantCatalog();
            _catalog.AddStartingSymbol("symbol_mercury");
            _catalog.AddStartingSymbol("symbol_lotus");
            _catalog.AddStartingSymbol("symbol_crow");

            _vitals = new SamplePlayerVitals(_health, _maxHealth);

            _gold = new RunGoldState();
            _gold.ResetForNewRun(_startGold);

            _state = new SanctumState();
            _relicPool = RunRelicPool.Build(runSeed, _catalog);
            _state.Begin(runSeed, _sanctumIndex, _config, _priceConfig, _catalog, _relicPool);

            _log = string.Format("시드 {0} 의 {1}번째 성소를 열었다.", runSeed, _sanctumIndex);
        }

        private void ChangeFirstOwnedSymbol()
        {
            List<MerchantItem> owned = new List<MerchantItem>();
            _catalog.CollectOwnedSymbols(owned);

            if (owned.Count == 0)
            {
                Log("가진 문양이 없어 바꿀 것이 없다.");
                return;
            }

            string targetId = owned[0].Id;
            string targetName = owned[0].DisplayName;

            MerchantItem picked;
            PurchaseResult result = _state.TryChangeSymbol(
                targetId, _config, _priceConfig, _gold, _catalog, out picked);

            Log(result == PurchaseResult.Success
                ? string.Format("{0} 을 {1} 로 바꿨다. 다음 가격 {2}",
                    targetName, picked.DisplayName, _state.GetSymbolChangePrice(_priceConfig))
                : string.Format("문양 변경 실패: {0}", result));
        }

        private string DescribeOwnedSymbols()
        {
            if (_catalog == null)
            {
                return "없음";
            }

            List<MerchantItem> owned = new List<MerchantItem>();
            _catalog.CollectOwnedSymbols(owned);

            if (owned.Count == 0)
            {
                return "없음";
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < owned.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(owned[i].DisplayName);
            }

            return builder.ToString();
        }

        /// <summary>
        /// 시드를 바꿔 가며 진열 규칙과 가격 규칙을 확인한다.
        /// 확인 항목은 자리 수, 한 진열 안의 중복, 같은 시드의 재현,
        /// 기준가 일치, 쓸 때마다 오르는 가격의 수열이다.
        /// </summary>
        private void RunBatch(int count)
        {
            StringBuilder report = new StringBuilder();
            int failures = 0;

            for (int i = 0; i < count; i++)
            {
                int seed = 1000 + i * 7919;

                SampleMerchantCatalog catalog = new SampleMerchantCatalog();
                SanctumState state = new SanctumState();
                state.Begin(seed, 1, _config, _priceConfig, catalog, RunRelicPool.Build(seed, catalog));

                string problem = Validate(state);
                if (problem != null)
                {
                    failures++;
                    if (failures <= 5)
                    {
                        report.AppendLine(string.Format("시드 {0}: {1}", seed, problem));
                    }

                    continue;
                }

                // 같은 시드는 같은 진열이 나와야 한다.
                SanctumState again = new SanctumState();
                SampleMerchantCatalog twinCatalog = new SampleMerchantCatalog();
                again.Begin(seed, 1, _config, _priceConfig, twinCatalog, RunRelicPool.Build(seed, twinCatalog));
                if (!SameStock(state.Stock, again.Stock))
                {
                    failures++;
                    if (failures <= 5)
                    {
                        report.AppendLine(string.Format("시드 {0}: 같은 시드인데 진열이 다르다", seed));
                    }
                }
            }

            string priceProblem = ValidatePriceTable();
            if (priceProblem != null)
            {
                failures++;
                report.AppendLine(priceProblem);
            }

            _batchPassed = failures == 0;
            _batchReport = _batchPassed
                ? string.Format("시드 {0}개 전부 통과. 가격 규칙도 기획서와 같다.", count)
                : string.Format("실패 {0}건\n{1}", failures, report);
        }

        private string Validate(SanctumState state)
        {
            if (state.Stock.SymbolSlots.Count != _config.SymbolSlotCount)
            {
                return "문양 자리 수가 다르다";
            }

            if (state.Stock.CoinSlots.Count != _config.CoinSlotCount)
            {
                return "코인 자리 수가 다르다";
            }

            if (state.Stock.RelicSlots.Count != _config.RelicSlotCount)
            {
                return "유물 자리 수가 다르다";
            }

            string duplicated = FindDuplicate(state.Stock.SymbolSlots);
            if (duplicated != null)
            {
                return "문양이 겹친다: " + duplicated;
            }

            duplicated = FindDuplicate(state.Stock.CoinSlots);
            if (duplicated != null)
            {
                return "코인이 겹친다: " + duplicated;
            }

            duplicated = FindDuplicate(state.Stock.RelicSlots);
            if (duplicated != null)
            {
                return "유물이 겹친다: " + duplicated;
            }

            foreach (MerchantSlot slot in state.Stock.AllSlots())
            {
                if (!slot.HasItem)
                {
                    continue;
                }

                int expected = _priceConfig.GetItemPrice(slot.Item);
                if (slot.Price != expected)
                {
                    return string.Format("{0} 의 가격이 {1}인데 기준가는 {2}다",
                        slot.Item.DisplayName, slot.Price, expected);
                }
            }

            return null;
        }

        /// <summary>07 가격 의 예시 수치를 그대로 검산한다.</summary>
        private string ValidatePriceTable()
        {
            int[] symbolTagSums = { 2, 4, 7, 9 };
            int[] symbolPrices = { 30, 40, 55, 65 };
            for (int i = 0; i < symbolTagSums.Length; i++)
            {
                int actual = _priceConfig.GetSymbolPrice(symbolTagSums[i]);
                if (actual != symbolPrices[i])
                {
                    return string.Format("태그 가치 합 {0}의 문양 가격이 {1}인데 기획서는 {2}다",
                        symbolTagSums[i], actual, symbolPrices[i]);
                }
            }

            int[] symbolChange = { 10, 30, 60, 100, 150 };
            for (int i = 0; i < symbolChange.Length; i++)
            {
                int actual = _priceConfig.GetSymbolChangePrice(i);
                if (actual != symbolChange[i])
                {
                    return string.Format("{0}번째 문양 변경 가격이 {1}인데 기획서는 {2}다",
                        i + 1, actual, symbolChange[i]);
                }
            }

            int[] relicRefresh = { 20, 30, 40, 50, 60 };
            for (int i = 0; i < relicRefresh.Length; i++)
            {
                int actual = _priceConfig.GetRelicRefreshPrice(i);
                if (actual != relicRefresh[i])
                {
                    return string.Format("{0}번째 유물 새로고침 가격이 {1}인데 기획서는 {2}다",
                        i + 1, actual, relicRefresh[i]);
                }
            }

            return null;
        }

        private static string FindDuplicate(List<MerchantSlot> slots)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null || !slots[i].HasItem)
                {
                    continue;
                }

                for (int j = i + 1; j < slots.Count; j++)
                {
                    if (slots[j] == null || !slots[j].HasItem)
                    {
                        continue;
                    }

                    if (slots[i].Item.Id == slots[j].Item.Id)
                    {
                        return slots[i].Item.DisplayName;
                    }
                }
            }

            return null;
        }

        private static bool SameStock(MerchantStock left, MerchantStock right)
        {
            return SameSlots(left.SymbolSlots, right.SymbolSlots)
                   && SameSlots(left.CoinSlots, right.CoinSlots)
                   && SameSlots(left.RelicSlots, right.RelicSlots);
        }

        private static bool SameSlots(List<MerchantSlot> left, List<MerchantSlot> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; i++)
            {
                if (left[i].Item.Id != right[i].Item.Id || left[i].Price != right[i].Price)
                {
                    return false;
                }
            }

            return true;
        }

        private void Log(string message)
        {
            _log = message;
        }
    }
}

