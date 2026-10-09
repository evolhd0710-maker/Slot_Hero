using System.Collections.Generic;
using SlotHero.Tools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 확인용 씬이 실제로 눌리는지 본다.
    /// 메뉴 Slot Hero / 확인용 씬 눌러 보기 에서 실행한다.
    ///
    /// 씬마다 입력을 받을 채비가 되어 있는지 살피고,
    /// 화면 조종기를 연 뒤 버튼을 하나씩 눌러 무슨 일이 일어나는지 기록에 남긴다.
    /// 플레이 모드로 들어가지 않고도 눌린 결과를 볼 수 있다.
    /// </summary>
    public static class CheckPreviewScenes
    {
        [MenuItem("Slot Hero/확인용 씬 눌러 보기")]
        public static void CheckAll()
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { UiSceneBuilder.SceneFolder });

            for (int i = 0; i < guids.Length; i++)
            {
                Check(AssetDatabase.GUIDToAssetPath(guids[i]));
            }
        }

        private static void Check(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            Debug.Log("===== " + name + " =====");

            // 입력을 받으려면 이 둘이 있어야 한다.
            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            GraphicRaycaster raycaster = Object.FindAnyObjectByType<GraphicRaycaster>();

            Debug.Log("  EventSystem " + (eventSystem != null ? "있음" : "없음")
                + " · GraphicRaycaster " + (raycaster != null ? "있음" : "없음"));

            // 편집 모드에서는 Awake 가 돌지 않아 화면이 버튼에 귀를 대지 않는다.
            // 플레이 모드와 같은 상태를 만들려고 직접 불러 준다.
            HashSet<MonoBehaviour> awakened = new HashSet<MonoBehaviour>();
            int first = CallAwake(awakened);

            // 조종기를 열어 둬야 눌렀을 때 무언가 일어난다.
            PreviewDriver driver = Object.FindAnyObjectByType<PreviewDriver>();
            if (driver != null)
            {
                driver.Run();
            }
            else
            {
                Debug.LogWarning("  PreviewDriver 가 없다. 눌러도 아무 일도 일어나지 않는다.");
            }

            // 화면을 열면서 새로 찍어 낸 항목들은 아직 Awake 를 받지 못했다.
            int second = CallAwake(awakened);
            Debug.Log("  Awake 를 부른 스크립트 " + (first + second) + "개 (열고 나서 " + second + "개)");

            // 눌러 본다. 같은 버튼을 여러 번 누르지 않게 한 바퀴만 돈다.
            Button[] buttons = Object.FindObjectsByType<Button>();
            List<Button> usable = new List<Button>();

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null
                    && buttons[i].isActiveAndEnabled
                    && buttons[i].interactable)
                {
                    usable.Add(buttons[i]);
                }
            }

            Debug.Log("  누를 수 있는 버튼 " + usable.Count + "개 / 모두 " + buttons.Length + "개");

            for (int i = 0; i < usable.Count; i++)
            {
                Button button = usable[i];
                if (button == null)
                {
                    continue;
                }

                string path = GetPath(button.transform);
                Debug.Log("  눌러 봄 · " + path);
                button.onClick.Invoke();
            }
        }

        /// <summary>
        /// 씬에 있는 우리 스크립트의 Awake 를 직접 부른다.
        /// 플레이 모드가 아니면 유니티가 불러 주지 않아 화면이 버튼에 귀를 대지 못한다.
        /// 우리가 쓴 것만 부르려고 이름 앞이 SlotHero 인 것만 고른다.
        /// </summary>
        private static int CallAwake(HashSet<MonoBehaviour> already)
        {
            MonoBehaviour[] all = Object.FindObjectsByType<MonoBehaviour>();
            int count = 0;

            for (int i = 0; i < all.Length; i++)
            {
                MonoBehaviour behaviour = all[i];
                if (behaviour == null || already.Contains(behaviour))
                {
                    continue;
                }

                already.Add(behaviour);
                System.Type type = behaviour.GetType();
                if (type.Namespace == null || !type.Namespace.StartsWith("SlotHero"))
                {
                    continue;
                }

                System.Reflection.MethodInfo awake = type.GetMethod(
                    "Awake",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (awake == null)
                {
                    continue;
                }

                awake.Invoke(behaviour, null);
                count++;
            }

            return count;
        }

        /// <summary>어느 오브젝트인지 알아볼 수 있게 위에서부터의 이름을 잇는다.</summary>
        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            Transform parent = transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
