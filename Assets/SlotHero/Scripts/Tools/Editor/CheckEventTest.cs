using System.Collections.Generic;
using System.Reflection;
using SlotHero.Combat;
using SlotHero.Combat.UI;
using SlotHero.Events;
using SlotHero.Events.UI;
using SlotHero.Flow;
using SlotHero.Flow.UI;
using SlotHero.Save;
using SlotHero.Tools;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 07 기록 말살 시험 씬을 굴려 본다(2026년 11월 13일 원재).
    /// 플레이를 누른 것처럼 시험 조종기를 시작하고 다가간다 → 전설 코인을 건넨다 → 지울 태그를 고른다 를 차례로 누른다.
    /// 태그 선택 창에서 시작 덱에 가장 많은 태그를 골라, 그 태그 문양이 다 바뀌고 장수는 그대로인지 본다.
    /// 전설 코인 없이 열면 화면 2 의 건넨다 가 막히는지도 본다.
    /// </summary>
    public static class CheckEventTest
    {
        private const string EventId = "record_erasure";

        private static int _checked;
        private static int _failed;

        [MenuItem("Slot Hero/검사/07 기록 말살 시험 씬")]
        public static void Check()
        {
            _checked = 0;
            _failed = 0;

            string path = UiSceneBuilder.SceneFolder + "/" + BuildPreviewScenes.RecordErasureTestSceneName + ".unity";
            if (!System.IO.File.Exists(path))
            {
                Report("07 기록 말살 시험 씬이 있다", false, path);
                Finish();
                return;
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            HashSet<MonoBehaviour> awakened = new HashSet<MonoBehaviour>();
            CheckPreviewScenes.CallAwake(awakened);

            EventTestDriver driver = Object.FindAnyObjectByType<EventTestDriver>();
            EventScreenController screen = Object.FindAnyObjectByType<EventScreenController>(FindObjectsInactive.Include);
            EventTagPicker picker = Object.FindAnyObjectByType<EventTagPicker>(FindObjectsInactive.Include);
            Report("시험 조종기, 이벤트 화면, 태그 선택 창이 있다", driver != null && screen != null && picker != null);
            if (driver == null || screen == null || picker == null)
            {
                Finish();
                return;
            }

            Invoke(driver, "Start");
            Report("시작하면 이벤트 화면이 열린다", screen.IsOpen);

            RunContext context = Field<RunContext>(driver, "_context");
            RunCatalogConfig catalog = Field<RunCatalogConfig>(driver, "_catalogCopy");
            string coinId = Field<string>(driver, "_testCoinId");
            Report("시험 전설 코인을 쥐고 시작한다", context != null && context.Run.Owned.CoinIds.Contains(coinId));
            if (context == null || catalog == null)
            {
                Finish();
                return;
            }

            int before = context.Run.Owned.TotalSymbolCount;
            SymbolTagType tag = MostCommonTag(context, catalog);
            int carriers = CountWithTag(context, catalog, tag);
            Report("시작 덱에 지울 태그 문양이 있다", carriers > 0, carriers.ToString());

            Invoke(driver, "HandleChoice", EventId, "near");
            Report("화면 2 의 전설 코인을 건넨다 가 눌린다", IsSelectable(screen, "give"));

            Invoke(driver, "HandleChoice", EventId, "give");
            Report("건네면 시험 전설 코인이 없어진다", !context.Run.Owned.CoinIds.Contains(coinId));

            Invoke(driver, "HandleChoice", EventId, "erase");
            CheckPreviewScenes.CallAwake(awakened);
            Report("지울 태그를 고른다 가 태그 선택 창을 연다", picker.IsOpen);

            TagDialView dial = picker.Dial;
            if (dial != null)
            {
                dial.Sectors[TagDialView.GetSectorIndex(tag)].OnPointerClick(null);
            }

            Report("태그를 누르면 창이 닫힌다", !picker.IsOpen);
            Report("고른 태그 문양이 하나도 남지 않는다", CountWithTag(context, catalog, tag) == 0,
                CountWithTag(context, catalog, tag).ToString());
            Report("지워도 문양 장수는 그대로다", context.Run.Owned.TotalSymbolCount == before,
                context.Run.Owned.TotalSymbolCount.ToString());

            string info = Field<TMP_Text>(driver, "_infoLabel").text;
            Report("시험 칸에 고른 태그와 바뀐 문양이 적힌다", info.Contains("고른 태그") && info.Contains("다 지웠다") && info.Contains("얻은 문양"));

            Canvas.ForceUpdateCanvases();
            // 확인용 씬 그림 뽑기가 플레이 전 모습을 같은 이름으로 뽑으므로 이름을 달리한다.
            CapturePreviewScenes.CaptureOpenScene(BuildPreviewScenes.RecordErasureTestSceneName + " 지운 뒤");

            driver.Restart(true);
            context = Field<RunContext>(driver, "_context");
            Invoke(driver, "HandleChoice", EventId, "near");
            Report("전설 코인 없이 열면 건넨다 가 막힌다", !IsSelectable(screen, "give"));
            Report("전설 코인 없이 열면 거절한다 로 떠날 수 있다", IsSelectable(screen, "refuse"));

            Finish();
        }

        /// <summary>가진 문양에서 가장 많은 태그.</summary>
        private static SymbolTagType MostCommonTag(RunContext context, RunCatalogConfig catalog)
        {
            SymbolTagType best = SymbolTagType.Saturn;
            int bestCount = -1;

            foreach (SymbolTagType tag in System.Enum.GetValues(typeof(SymbolTagType)))
            {
                int count = CountWithTag(context, catalog, tag);
                if (count > bestCount)
                {
                    best = tag;
                    bestCount = count;
                }
            }

            return best;
        }

        private static int CountWithTag(RunContext context, RunCatalogConfig catalog, SymbolTagType tag)
        {
            int count = 0;
            List<OwnedSymbol> symbols = context.Run.Owned.Symbols;

            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolTagType first;
                SymbolTagType second;
                if (catalog.TryGetSymbolTags(symbols[i].SymbolId, out first, out second) && (first == tag || second == tag))
                {
                    count += symbols[i].Count;
                }
            }

            return count;
        }

        /// <summary>지금 화면에 그 선택지가 있고 누를 수 있는지.</summary>
        private static bool IsSelectable(EventScreenController screen, string choiceId)
        {
            EventPage page = Field<EventPage>(screen, "_page");
            EventChoice choice = page != null ? page.Find(choiceId) : null;
            return choice != null && choice.IsSelectable;
        }

        private static void Finish()
        {
            Debug.Log("07 기록 말살 시험 씬 검사 " + _checked + "건, 실패 " + _failed + "건");
        }

        private static void Report(string name, bool ok)
        {
            Report(name, ok, string.Empty);
        }

        private static void Report(string name, bool ok, string detail)
        {
            _checked++;
            if (!ok)
            {
                _failed++;
            }

            Debug.Log("  " + (ok ? "통과" : "실패") + "  " + name + (ok || string.IsNullOrEmpty(detail) ? string.Empty : "   (" + detail + ")"));
        }

        private static void Invoke(object target, string method, params object[] args)
        {
            MethodInfo info = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (info != null)
            {
                info.Invoke(target, args.Length == 0 ? null : args);
            }
        }

        private static T Field<T>(object target, string name) where T : class
        {
            FieldInfo info = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return info != null ? info.GetValue(target) as T : null;
        }
    }
}
