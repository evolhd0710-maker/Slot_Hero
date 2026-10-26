using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SlotHero.Ui;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 확인용 씬을 1920 × 1080 그림으로 뽑는다.
    /// 메뉴 Slot Hero / 확인용 씬 그림으로 뽑기 에서 실행한다.
    ///
    /// 유니티를 켜지 않고도 화면이 어떻게 나오는지 보려고 만든 것이다.
    /// 캔버스를 잠깐 카메라에 물려 그리고 원래대로 돌려 둔다.
    /// </summary>
    public static class CapturePreviewScenes
    {
        /// 뽑은 그림을 두는 자리. Assets 밖이라 유니티가 가져가지 않는다.
        private const string OutputFolder = "화면 확인";

        [MenuItem("Slot Hero/확인용 씬 그림으로 뽑기")]
        public static void CaptureAll()
        {
            string folder = Path.Combine(Directory.GetCurrentDirectory(), OutputFolder);
            Directory.CreateDirectory(folder);

            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { UiSceneBuilder.SceneFolder });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Capture(path, folder);
            }

            CaptureCombatStates(folder);

            Debug.Log("씬 " + guids.Length + "개를 그림으로 뽑았다: " + folder);
        }

        /// <summary>
        /// 전투 화면은 한 장으로는 코인 고르기만 보인다. 피그마 Frame 2 ~ 5 에 대응하는 모습을 더 뽑는다.
        /// 코인 호버(고름 강조와 설명 풍선), 태그 UI, 태그 칸 호버, 공격 예정과 피해 숫자다.
        /// 편집 모드라 Awake 가 돌지 않아 마우스 알림 대신 처리기를 직접 부른다. 씬은 저장하지 않는다.
        /// </summary>
        private static void CaptureCombatStates(string folder)
        {
            string scenePath = UiSceneBuilder.SceneFolder + "/" + BuildPreviewScenes.CombatSceneName + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                return;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            SlotHero.Combat.UI.CombatScreenController screen = Object.FindFirstObjectByType<SlotHero.Combat.UI.CombatScreenController>();
            if (screen == null || screen.View == null)
            {
                return;
            }

            SlotHero.Combat.UI.CombatScreenView view = screen.View;
            BuildPreviewScenes.OpenCombatSample(screen);

            // Frame 2. 첫 코인에 마우스를 올린 모습.
            CallPrivate(view.CoinDial, "HandleCoinHover", 0, true);
            CallPrivate(screen, "HandleCoinHover", 0, true);
            Shoot(BuildPreviewScenes.CombatSceneName + " 코인 고름", folder);
            CallPrivate(view.CoinDial, "HandleCoinHover", 0, false);
            CallPrivate(screen, "HandleCoinHover", 0, false);

            // Frame 3. 코인을 골라 태그 UI 로 바뀐 모습.
            screen.ChooseCoin(0);
            Shoot(BuildPreviewScenes.CombatSceneName + " 태그", folder);

            // Frame 4. 태그 칸 하나에 마우스를 올린 모습. 고를 수 있는 첫 태그 칸을 쓴다(소용돌이는 건너뛴다).
            // 그 태그 문양이 있는 릴에 강조 테두리가 둘러지도록 조종기의 호버 처리기도 부른다.
            SlotHero.Combat.UI.TagSectorView hovered = null;
            SlotHero.Combat.SymbolTagType tag = SlotHero.Combat.SymbolTagType.Mercury;
            int hoveredIndex = -1;
            for (int i = 0; i < view.TagDial.Sectors.Count && hovered == null; i++)
            {
                if (view.TagDial.Sectors[i].Interactable && SlotHero.Combat.UI.TagDialView.TryGetTag(i, out tag))
                {
                    hovered = view.TagDial.Sectors[i];
                    hoveredIndex = i;
                }
            }

            if (hovered != null)
            {
                hovered.OnPointerEnter(null);
                CallPrivate(screen, "HandleTagHover", tag, true);
                Shoot(BuildPreviewScenes.CombatSceneName + " 태그 고름", folder);
                hovered.OnPointerExit(null);
                CallPrivate(screen, "HandleTagHover", tag, false);

                // Frame 5. 그 칸을 공격 예정으로 채우고 몬스터 위에 피해 숫자를 띄운 모습.
                view.TagDial.MarkChosen(hoveredIndex);
                view.TagDial.SetCharge(true, view.GetTagVisual(tag).BarColor);
            }

            SlotHero.Combat.UI.DamageNumberView number = Object.FindFirstObjectByType<SlotHero.Combat.UI.DamageNumberView>(FindObjectsInactive.Include);
            if (number != null && view.Layout != null)
            {
                Vector2 center = view.GetEnemyCenter(null);
                number.gameObject.SetActive(true);
                number.transform.SetAsLastSibling();
                number.RectTransform.anchoredPosition = new Vector2(center.x, -(center.y - 40f));
                number.Show(1280, string.Empty);
                if (number.Group != null)
                {
                    number.Group.alpha = 1f;
                }
            }

            Shoot(BuildPreviewScenes.CombatSceneName + " 피해 숫자", folder);

            // 건드린 씬을 버리고 저장된 상태로 다시 연다.
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        private static void CallPrivate(object target, string method, params object[] arguments)
        {
            if (target == null)
            {
                return;
            }

            System.Reflection.MethodInfo info = target.GetType().GetMethod(
                method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (info == null)
            {
                Debug.LogWarning("처리기를 찾지 못했다: " + target.GetType().Name + "." + method);
                return;
            }

            info.Invoke(target, arguments);
        }

        /// <summary>
        /// 지금 열려 있는 씬을 그대로 그림으로 뽑는다.
        /// 게임 씬을 굴려 보면서 중간 모습을 남기는 데 쓴다.
        /// </summary>
        public static void CaptureOpenScene(string name)
        {
            string folder = Path.Combine(Directory.GetCurrentDirectory(), OutputFolder);
            Directory.CreateDirectory(folder);
            Shoot(name, folder);
        }

        private static void Capture(string scenePath, string outputFolder)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Shoot(Path.GetFileNameWithoutExtension(scenePath), outputFolder);
        }

        private static void Shoot(string name, string outputFolder)
        {
            // 씬에 있는 카메라를 그대로 쓴다.
            // 실제로 게임이 쓰는 카메라로 찍어야 그림이 게임과 같다.
            Camera camera = FindSceneCamera();
            GameObject cameraObject = null;

            if (camera == null)
            {
                // 카메라가 없는 옛 씬을 위해 하나 만들어 쓴다.
                cameraObject = new GameObject("CaptureCamera");
                camera = cameraObject.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.orthographic = true;
            }

            // 화면을 덮는 캔버스는 카메라에 잡히지 않으므로 잠깐 카메라에 물린다.
            Canvas[] canvases = Object.FindObjectsByType<Canvas>();
            RenderMode[] modes = new RenderMode[canvases.Length];

            for (int i = 0; i < canvases.Length; i++)
            {
                modes[i] = canvases[i].renderMode;

                if (modes[i] == RenderMode.ScreenSpaceCamera && canvases[i].worldCamera != null)
                {
                    // 이미 카메라에 물려 있으면 건드리지 않는다.
                    continue;
                }

                canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                canvases[i].worldCamera = camera;
                canvases[i].planeDistance = 10f;
            }

            Canvas.ForceUpdateCanvases();

            RenderTexture texture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            texture.antiAliasing = 2;

            camera.targetTexture = texture;

            // 16:9 틀은 창 크기를 보고 잡힌다.
            // 그림으로 뽑을 때는 창이 아니라 이 그림 크기를 봐야 하므로 다시 잡게 한다.
            // 편집 모드에서는 Update 가 돌지 않아 여기서 직접 불러 준다.
            CameraFrame frame = camera.GetComponent<CameraFrame>();
            if (frame != null)
            {
                frame.Apply();
            }

            camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = texture;

            Texture2D shot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            shot.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0);
            shot.Apply();

            RenderTexture.active = previous;
            camera.targetTexture = null;

            File.WriteAllBytes(Path.Combine(outputFolder, name + ".png"), shot.EncodeToPNG());

            // 씬을 건드린 것을 되돌린다.
            for (int i = 0; i < canvases.Length; i++)
            {
                canvases[i].renderMode = modes[i];
            }

            Object.DestroyImmediate(shot);

            if (cameraObject != null)
            {
                Object.DestroyImmediate(cameraObject);
            }

            texture.Release();
            Object.DestroyImmediate(texture);
        }

        /// <summary>
        /// 씬의 카메라를 찾는다. 띠를 지우기만 하는 뒤 카메라는 건너뛴다.
        /// 그리는 것은 앞 카메라다.
        /// </summary>
        private static Camera FindSceneCamera()
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>();
            Camera best = null;

            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i].cullingMask == 0)
                {
                    continue;
                }

                if (best == null || cameras[i].depth > best.depth)
                {
                    best = cameras[i];
                }
            }

            return best;
        }
    }
}
