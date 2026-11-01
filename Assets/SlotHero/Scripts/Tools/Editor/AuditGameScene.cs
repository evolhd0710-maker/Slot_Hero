using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 게임 씬의 구조를 훑어 **켜도 안 보이는 칸**을 찾는다.
    ///
    /// 조종기가 켜고 끄는 칸마다 부모를 거슬러 올라가 본다.
    /// 부모 가운데 하나라도 꺼져 있고 그 부모를 켜 줄 조종기가 없으면
    /// 그 칸은 아무리 켜도 화면에 나오지 않는다. 성소와 팝업이 그랬다.
    ///
    /// 함께 켜져야 할 형제가 따로 노는 것도 본다.
    /// 팝업의 어두운 막이 팝업 칸의 형제라 영영 꺼져 있었다.
    ///
    /// 게임 씬 검사가 이걸 불러 쓴다. 메뉴에서 따로 돌려 볼 수도 있다.
    /// </summary>
    public static class AuditGameScene
    {
        /// 켜고 끄는 칸을 담는 필드 이름. 이 말로 끝나면 켜고 끄는 대상으로 본다.
        private static readonly string[] RootSuffixes = { "Root" };

        /// 제 오브젝트를 통째로 켜고 끄는 화면들. 필드가 아니라 `gameObject` 를 끈다.
        private static readonly string[] SelfToggling =
        {
            "ProfileSelectScreenView", "RewardScreenView", "RunResultScreenView",
            "TitleScreenView", "TopBarController", "RunHudController",
        };

        /// 켜고 끄는 칸과 함께 있어야 하는 형제의 이름. 떨어져 있으면 따로 논다.
        private static readonly string[] CompanionNames = { "Dim", "Blocker", "Backdrop" };

        /// <summary>
        /// 조종기마다 붙어 있는 오브젝트와 그 오브젝트가 켜진 채 시작하는지를 찍는다.
        /// 꺼진 채 시작하면 처음 켜질 때까지 Awake 가 돌지 않는다.
        /// </summary>
        [MenuItem("Slot Hero/조종기 자리 찍기", priority = 5)]
        public static void ListControllers()
        {
            string path = UiSceneBuilder.SceneFolder + "/게임.unity";
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            MonoBehaviour[] all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);

            for (int i = 0; i < all.Length; i++)
            {
                System.Type type = all[i].GetType();
                if (type.Namespace == null || !type.Namespace.StartsWith("SlotHero"))
                {
                    continue;
                }

                if (!type.Name.EndsWith("Controller") && !type.Name.EndsWith("Presenter"))
                {
                    continue;
                }

                Debug.Log("  자리  " + type.Name + " → " + PathOf(all[i].gameObject) +
                    (all[i].gameObject.activeInHierarchy ? "  켜진 채 시작" : "  **꺼진 채 시작**"));
            }
        }

        [MenuItem("Slot Hero/게임 씬 구조 감사", priority = 3)]
        public static void Run()
        {
            string path = UiSceneBuilder.SceneFolder + "/게임.unity";
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // 무엇을 봤는지부터 찍는다. 하나도 못 찾고 0건이 나오면 헛돈 것이다.
            Dictionary<GameObject, string> toggled = CollectToggled();
            foreach (KeyValuePair<GameObject, string> pair in toggled)
            {
                Debug.Log("  대상  " + pair.Value + " → " + PathOf(pair.Key) +
                    (pair.Key.activeSelf ? "  (켜짐)" : "  (꺼짐)"));
            }

            List<string> problems = Find();

            for (int i = 0; i < problems.Count; i++)
            {
                Debug.LogWarning("  감사  " + problems[i]);
            }

            Debug.Log("게임 씬 구조 감사: 대상 " + toggled.Count + "개, 문제 " + problems.Count + "건");
        }

        /// <summary>
        /// 확인용 씬을 모두 열어 펼친 칸에 크기가 들어간 곳을 찾는다.
        /// 확인용 씬은 만들 때 예시 자료로 자리를 한 번 잡아 저장해 두었으므로
        /// 열기만 해도 화면이 자리를 잡은 상태다.
        /// </summary>
        [MenuItem("Slot Hero/펼친 칸 크기 감사", priority = 4)]
        public static void RunStretchAudit()
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { UiSceneBuilder.SceneFolder });
            int total = 0;

            for (int g = 0; g < guids.Length; g++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[g]);
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                List<string> problems = FindStretchedWithSize();
                for (int i = 0; i < problems.Count; i++)
                {
                    Debug.LogWarning("  펼침  [" + System.IO.Path.GetFileNameWithoutExtension(path) +
                        "] " + problems[i]);
                }

                total += problems.Count;
            }

            Debug.Log("펼친 칸 크기 감사: 씬 " + guids.Length + "개, 문제 " + total + "건");
        }

        /// <summary>지금 열린 씬에서 문제를 찾아 한 줄씩 돌려준다.</summary>
        public static List<string> Find()
        {
            List<string> problems = new List<string>();
            Dictionary<GameObject, string> toggled = CollectToggled();

            // 켜 줄 조종기가 있는 오브젝트들. 이것들이 꺼져 있는 건 괜찮다.
            HashSet<GameObject> reenabled = new HashSet<GameObject>(toggled.Keys);

            foreach (KeyValuePair<GameObject, string> pair in toggled)
            {
                GameObject target = pair.Key;
                Transform parent = target.transform.parent;

                while (parent != null && parent.GetComponent<Canvas>() == null)
                {
                    if (!parent.gameObject.activeSelf && !reenabled.Contains(parent.gameObject))
                    {
                        problems.Add(pair.Value + " 가 켜는 " + PathOf(target) +
                            " 의 부모 " + PathOf(parent.gameObject) +
                            " 가 꺼져 있고 켜 주는 쪽이 없다. 켜도 안 보인다");
                        break;
                    }

                    parent = parent.parent;
                }

                FindLooseCompanions(target, pair.Value, problems);
            }

            return problems;
        }

        /// <summary>
        /// 켜고 끄는 칸의 형제 가운데 어두운 막 같은 것이 따로 떨어져 있는지 본다.
        /// 칸만 켜지고 막은 안 켜지면 뒤가 막히지 않는다.
        /// </summary>
        private static void FindLooseCompanions(GameObject target, string owner, List<string> problems)
        {
            Transform parent = target.transform.parent;
            if (parent == null || parent.GetComponent<Canvas>() != null)
            {
                return;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform sibling = parent.GetChild(i);
                if (sibling == target.transform)
                {
                    continue;
                }

                for (int n = 0; n < CompanionNames.Length; n++)
                {
                    if (sibling.name == CompanionNames[n] && sibling.GetComponent<Graphic>() != null)
                    {
                        problems.Add(owner + " 가 켜는 " + PathOf(target) + " 옆에 " + sibling.name +
                            " 가 따로 있다. 칸만 켜지고 이건 안 켜진다");
                    }
                }
            }
        }

        /// <summary>
        /// **펼친 축에 크기를 넣은 칸**을 찾는다.
        ///
        /// 기준점이 네 귀퉁이로 벌어진 축에서 `sizeDelta` 는 크기가 아니라
        /// 부모보다 얼마나 더 크게 할지다. 원래는 0 이거나 안쪽 여백인 음수다.
        /// 여기에 크기를 넣으면 칸이 부모 크기에 그 값을 더한 만큼 커진다.
        /// 맵 내용 칸이 그래서 두 배가 되어 스크롤해야 다 보였다.
        ///
        /// 화면이 자리를 잡은 뒤에 불러야 한다. 씬을 막 열었을 때는 아직 크기를 안 넣었다.
        /// </summary>
        public static List<string> FindStretchedWithSize()
        {
            List<string> problems = new List<string>();
            RectTransform[] all = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include);

            // 화면에서 1 픽셀. 이보다 크면 여백이 아니라 크기를 넣은 것으로 본다.
            float threshold = SlotHero.Ui.UiScale.Px(1f);

            for (int i = 0; i < all.Length; i++)
            {
                RectTransform rect = all[i];
                if (rect.GetComponent<Canvas>() != null)
                {
                    continue;
                }

                bool stretchedX = !Mathf.Approximately(rect.anchorMin.x, rect.anchorMax.x);
                bool stretchedY = !Mathf.Approximately(rect.anchorMin.y, rect.anchorMax.y);

                if (stretchedX && rect.sizeDelta.x > threshold)
                {
                    problems.Add(PathOf(rect.gameObject) + " 가로가 펼쳐져 있는데 크기 " +
                        Mathf.RoundToInt(SlotHero.Ui.UiScale.ToWireframe(rect.sizeDelta.x)) +
                        " 가 들어가 부모보다 그만큼 넓다");
                }

                if (stretchedY && rect.sizeDelta.y > threshold)
                {
                    problems.Add(PathOf(rect.gameObject) + " 세로가 펼쳐져 있는데 크기 " +
                        Mathf.RoundToInt(SlotHero.Ui.UiScale.ToWireframe(rect.sizeDelta.y)) +
                        " 가 들어가 부모보다 그만큼 높다");
                }
            }

            return problems;
        }

        /// <summary>
        /// **아무도 크기를 안 잡아 준 글자 칸**을 찾는다.
        ///
        /// 빌더가 새 칸을 만들면 `UiSceneBuilder.SizeDefault` 가 100 × 100 을 넣는다.
        /// 뷰든 빌더든 그 뒤에 크기를 다시 잡아야 하는데, 안 잡으면 그대로 남는다.
        /// 가운데 맞춤 글자가 칸 절반만큼 밀리고 긴 글은 한 자씩 줄바꿈된다.
        /// 현재 빌드의 줄 이름과 칸 제목이 그랬다.
        ///
        /// 네 귀퉁이에 펼친 칸은 부모를 따라가므로 뺀다.
        /// </summary>
        public static List<string> FindUnsizedLabels()
        {
            List<string> problems = new List<string>();
            TMPro.TMP_Text[] all = Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Include);

            Vector2 unsized = SlotHero.Ui.UiScale.V(100f, 100f);

            for (int i = 0; i < all.Length; i++)
            {
                RectTransform rect = all[i].rectTransform;

                bool stretched = !Mathf.Approximately(rect.anchorMin.x, rect.anchorMax.x)
                    || !Mathf.Approximately(rect.anchorMin.y, rect.anchorMax.y);

                if (stretched)
                {
                    continue;
                }

                if (Mathf.Approximately(rect.sizeDelta.x, unsized.x)
                    && Mathf.Approximately(rect.sizeDelta.y, unsized.y))
                {
                    problems.Add(PathOf(rect.gameObject) + " 글자 칸이 기본 크기 100 × 100 그대로다. 아무도 크기를 안 잡았다");
                }
            }

            return problems;
        }

        /// <summary>조종기가 켜고 끄는 오브젝트와 그걸 켜는 쪽의 이름을 모은다.</summary>
        private static Dictionary<GameObject, string> CollectToggled()
        {
            Dictionary<GameObject, string> result = new Dictionary<GameObject, string>();
            MonoBehaviour[] all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);

            for (int i = 0; i < all.Length; i++)
            {
                MonoBehaviour behaviour = all[i];
                if (behaviour == null)
                {
                    continue;
                }

                System.Type type = behaviour.GetType();
                if (type.Namespace == null || !type.Namespace.StartsWith("SlotHero"))
                {
                    continue;
                }

                string owner = type.Name + " (" + behaviour.gameObject.name + ")";

                for (int s = 0; s < SelfToggling.Length; s++)
                {
                    if (type.Name == SelfToggling[s] && !result.ContainsKey(behaviour.gameObject))
                    {
                        result.Add(behaviour.gameObject, owner);
                    }
                }

                FieldInfo[] fields = type.GetFields(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                for (int f = 0; f < fields.Length; f++)
                {
                    if (!EndsWithAny(fields[f].Name, RootSuffixes))
                    {
                        continue;
                    }

                    GameObject target = AsGameObject(fields[f].GetValue(behaviour));
                    if (target != null && !result.ContainsKey(target))
                    {
                        result.Add(target, owner + "." + fields[f].Name);
                    }
                }
            }

            return result;
        }

        private static GameObject AsGameObject(object value)
        {
            GameObject go = value as GameObject;
            if (go != null)
            {
                return go;
            }

            Component component = value as Component;
            return component != null ? component.gameObject : null;
        }

        private static bool EndsWithAny(string name, string[] suffixes)
        {
            // `_root` 처럼 소문자로 끝나는 것도 있어 대소문자를 가리지 않는다.
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (name.EndsWith(suffixes[i], System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string PathOf(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;

            while (parent != null && parent.GetComponent<Canvas>() == null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
