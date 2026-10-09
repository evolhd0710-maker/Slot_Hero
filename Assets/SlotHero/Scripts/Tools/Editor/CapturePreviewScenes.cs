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

            Debug.Log("씬 " + guids.Length + "개를 그림으로 뽑았다: " + folder);
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
