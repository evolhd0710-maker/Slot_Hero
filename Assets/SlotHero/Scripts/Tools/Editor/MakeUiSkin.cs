using System.IO;
using UnityEditor;
using UnityEngine;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 화면에 입체감을 주는 스킨 그림을 코드로 만든다.
    /// 메뉴 Slot Hero / UI 스킨 만들기 에서 실행한다.
    ///
    /// 지금까지는 유니티에 딸려 오는 흰 칸 그림을 색만 입혀 썼다.
    /// 그래서 칸이 전부 평면으로 보였다.
    ///
    /// 여기서 만드는 그림은 **어둡게 하는 쪽만** 담는다.
    /// `Image.color` 가 곱셈으로 들어가므로 흰색보다 밝게는 못 만든다.
    /// 그래서 위쪽은 1.0 그대로 두고 아래로 갈수록 어둡게 해 빛이 위에서 오는 것처럼 만든다.
    /// 설정 에셋에 적힌 색이 곧 칸 윗부분의 색이 되므로 기획서 색이 그대로 살아 있다.
    ///
    /// 9조각으로 늘어나므로 가장자리 16픽셀은 늘어나지 않는다. 모서리가 뭉개지지 않는다.
    /// </summary>
    public static class MakeUiSkin
    {
        /// 스킨 그림이 들어가는 자리
        public const string SkinFolder = "Assets/SlotHero/Art/Skin";

        /// 한 변 길이. 9조각 가장자리가 16이라 가운데가 32 남는다
        private const int Size = 64;

        /// 9조각 가장자리
        private const int Border = 16;

        /// 모서리 둥글기
        private const float Radius = 12f;

        /// <summary>
        /// 손으로 그린 스킨 그림이 있는 자리. 있으면 코드로 만들지 않고 이것을 옮긴다(2026년 10월 20일 원재).
        /// 문양과 같은 화풍(평면 명암, 크림색 선, 쇠 모서리)으로 그린 칸과 버튼이다.
        /// 칸은 어두운 갈회색, 버튼은 베이지라 그림 색이 곧 화면 색이다. 설정 에셋의 색은 흰색이면 그림 그대로, 아니면 그 위에 곱해진다.
        /// </summary>
        private static readonly string DrawnFolder = Path.Combine(ImportArt.SourceRoot, @"Ui\Art");

        /// <summary>그린 칸 그림의 9조각 가장자리. 장식이 모두 이 안에 있다.</summary>
        private const int DrawnPanelBorder = 64;

        /// <summary>그린 버튼 그림의 9조각 가장자리.</summary>
        private const int DrawnButtonBorder = 40;

        /// <summary>
        /// 그린 칸 가장자리가 화면에서 차지하는 두께. 그림 64 픽셀을 이만큼으로 줄여 그린다.
        /// 기준 해상도 1920 × 1080 의 픽셀이다.
        /// </summary>
        private const float DrawnPanelBorderOnScreen = 28f;

        /// <summary>그린 버튼 가장자리가 화면에서 차지하는 두께.</summary>
        private const float DrawnButtonBorderOnScreen = 18f;

        [MenuItem("Slot Hero/UI 스킨 만들기", priority = 3)]
        public static void Make()
        {
            UiSceneBuilder.EnsureFolder(SkinFolder);

            // 칸. 그린 그림이 있으면 그것을, 없으면 아래쪽만 살짝 어두운 칸을 만든다.
            if (!CopyDrawn("패널"))
            {
                Write("패널", MakePanel(0.94f, 0.76f, 3, 0.55f));
            }

            // 버튼. 칸보다 세게 들어가 도드라져 보인다.
            if (!CopyDrawn("버튼"))
            {
                Write("버튼", MakePanel(0.88f, 0.58f, 4, 0.42f));
            }

            // 눌린 버튼. 위아래를 뒤집어 움푹 들어가 보인다.
            if (!CopyDrawn("버튼_눌림"))
            {
                Write("버튼_눌림", MakeSunken());
            }

            // 그림자. 칸 뒤에 조금 내려 깔면 떠 있는 것처럼 보인다.
            Write("그림자", MakeShadow());

            // 점선 테두리. 아직 갖지 않은 빈 칸에 테두리만 남긴다(2026년 10월 21일 원재).
            Write("점선", MakeDashedOutline());

            // 원. 전투의 코인/태그 원판과 코인 자리에 쓴다. 9조각이 아니다.
            Write("원", MakeCircle(), CircleSize);

            AssetDatabase.Refresh();
            ApplyImportSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("UI 스킨 그림 다섯 장을 만들었다: " + SkinFolder);
        }

        /// 원 그림의 한 변. 원판이 600 이라 크게 늘려도 가장자리가 뭉개지지 않게 넉넉히 둔다
        private const int CircleSize = 512;

        /// <summary>
        /// 흰 원. 가장자리 한 픽셀을 부드럽게 걸친다.
        /// 태그 UI 의 칸은 이 원을 부채꼴로 채워 그린다(`Image.Type.Filled`).
        /// </summary>
        private static Color[] MakeCircle()
        {
            Color[] pixels = new Color[CircleSize * CircleSize];
            float half = CircleSize * 0.5f;

            for (int y = 0; y < CircleSize; y++)
            {
                for (int x = 0; x < CircleSize; x++)
                {
                    float dx = x + 0.5f - half;
                    float dy = y + 0.5f - half;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy) - (half - 1f);
                    float alpha = Mathf.Clamp01(0.5f - distance);
                    pixels[y * CircleSize + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            return pixels;
        }

        /// <summary>
        /// 위가 밝고 아래가 어두운 칸.
        /// </summary>
        /// <param name="bottomFill">칸 바닥의 밝기. 위는 늘 1.0 이다.</param>
        /// <param name="bottomEdge">아래쪽 안쪽 테두리의 밝기.</param>
        /// <param name="edgeThickness">그 테두리의 두께.</param>
        /// <param name="outline">바깥 테두리 한 줄의 밝기.</param>
        private static Color[] MakePanel(
            float bottomFill, float bottomEdge, int edgeThickness, float outline)
        {
            Color[] pixels = new Color[Size * Size];

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float distance = RoundedDistance(x, y);

                    // 모서리 바깥은 비워 둔다. 경계는 한 픽셀 걸쳐 부드럽게 만든다.
                    float alpha = Mathf.Clamp01(0.5f - distance);
                    if (alpha <= 0f)
                    {
                        pixels[y * Size + x] = new Color(0f, 0f, 0f, 0f);
                        continue;
                    }

                    // 아래에서 위로 가는 밝기. y 는 아래가 0 이다.
                    float up = y / (float)(Size - 1);
                    float value = Mathf.Lerp(bottomFill, 1f, up);

                    // 아래쪽 안쪽 테두리.
                    float fromBottom = -distance;
                    if (y < Size * 0.5f && fromBottom < edgeThickness)
                    {
                        float t = Mathf.Clamp01(fromBottom / edgeThickness);
                        value = Mathf.Lerp(bottomEdge, value, t);
                    }

                    // 위쪽 안쪽 테두리는 밝게 두어야 하는데 곱셈이라 1.0 을 넘을 수 없다.
                    // 그래서 위쪽은 건드리지 않고 그대로 1.0 로 둔다.

                    // 바깥 테두리 한 줄.
                    if (distance > -1.2f)
                    {
                        float t = Mathf.Clamp01((distance + 1.2f) / 1.2f);
                        value = Mathf.Lerp(value, outline, t);
                    }

                    pixels[y * Size + x] = new Color(value, value, value, alpha);
                }
            }

            return pixels;
        }

        /// <summary>눌린 버튼. 위가 어둡고 아래가 밝아 움푹 들어가 보인다.</summary>
        private static Color[] MakeSunken()
        {
            Color[] pixels = new Color[Size * Size];

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float distance = RoundedDistance(x, y);
                    float alpha = Mathf.Clamp01(0.5f - distance);

                    if (alpha <= 0f)
                    {
                        pixels[y * Size + x] = new Color(0f, 0f, 0f, 0f);
                        continue;
                    }

                    float up = y / (float)(Size - 1);
                    float value = Mathf.Lerp(0.98f, 0.80f, up);

                    // 위쪽 안쪽 테두리를 어둡게 해 눌린 자국을 만든다.
                    float fromEdge = -distance;
                    if (y > Size * 0.5f && fromEdge < 4f)
                    {
                        float t = Mathf.Clamp01(fromEdge / 4f);
                        value = Mathf.Lerp(0.52f, value, t);
                    }

                    if (distance > -1.2f)
                    {
                        float t = Mathf.Clamp01((distance + 1.2f) / 1.2f);
                        value = Mathf.Lerp(value, 0.42f, t);
                    }

                    pixels[y * Size + x] = new Color(value, value, value, alpha);
                }
            }

            return pixels;
        }

        /// <summary>칸 뒤에 까는 그림자. 검고 가장자리로 갈수록 옅어진다.</summary>
        /// <summary>
        /// 둥근 네모의 점선 테두리. 흰색이라 설정 색이 곱해져 색이 정해진다.
        /// 늘이면 점선 간격도 늘어나므로 이 그림은 9조각 늘리기가 아니라 바둑판 깔기(Tiled)로 쓴다.
        /// 점 하나와 빈칸 하나가 DashLength 씩이고 가운데 32 픽셀에 딱 맞게 반복된다.
        /// </summary>
        private static Color[] MakeDashedOutline()
        {
            Color[] pixels = new Color[Size * Size];

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float distance = RoundedDistance(x, y);

                    // 바깥 경계에서 안쪽으로 DashThickness 만큼만 선이다.
                    float alpha = Mathf.Clamp01(0.5f - distance) * Mathf.Clamp01(distance + DashThickness + 0.5f);
                    if (alpha <= 0f)
                    {
                        pixels[y * Size + x] = new Color(1f, 1f, 1f, 0f);
                        continue;
                    }

                    // 모서리 곡선은 이어 그린다. 곧은 변은 그 변을 따라가는 좌표로 점과 빈칸을 번갈아 둔다.
                    bool inCornerX = x < Radius || x >= Size - Radius;
                    bool inCornerY = y < Radius || y >= Size - Radius;
                    bool dash = true;
                    if (!(inCornerX && inCornerY))
                    {
                        int along = inCornerX ? y : x;
                        dash = (along / DashLength) % 2 == 0;
                    }

                    pixels[y * Size + x] = new Color(1f, 1f, 1f, dash ? alpha : 0f);
                }
            }

            return pixels;
        }

        /// <summary>점선 하나와 빈칸 하나의 길이. 가운데 32 픽셀을 나눠 떨어진다.</summary>
        private const int DashLength = 4;

        /// <summary>점선의 두께.</summary>
        private const float DashThickness = 2.5f;

        private static Color[] MakeShadow()
        {
            Color[] pixels = new Color[Size * Size];

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float distance = RoundedDistance(x, y);

                    // 가장자리에서 10픽셀에 걸쳐 옅어진다.
                    float alpha = Mathf.Clamp01(-distance / 10f);
                    alpha = alpha * alpha;

                    pixels[y * Size + x] = new Color(0f, 0f, 0f, alpha * 0.45f);
                }
            }

            return pixels;
        }

        /// <summary>
        /// 모서리가 둥근 네모의 경계까지의 거리.
        /// 안쪽이면 음수, 바깥이면 양수다.
        /// </summary>
        private static float RoundedDistance(int x, int y)
        {
            float half = Size * 0.5f;
            float dx = Mathf.Abs(x + 0.5f - half) - (half - Radius);
            float dy = Mathf.Abs(y + 0.5f - half) - (half - Radius);

            float outsideX = Mathf.Max(dx, 0f);
            float outsideY = Mathf.Max(dy, 0f);
            float outside = Mathf.Sqrt(outsideX * outsideX + outsideY * outsideY);

            float inside = Mathf.Min(Mathf.Max(dx, dy), 0f);

            return outside + inside - Radius;
        }

        /// <summary>그린 스킨 그림이 있으면 스킨 자리로 옮긴다. 옮겼으면 true 다.</summary>
        private static bool CopyDrawn(string name)
        {
            string source = Path.Combine(DrawnFolder, name + ".png");
            if (!File.Exists(source))
            {
                return false;
            }

            File.Copy(source, Path.Combine(Directory.GetCurrentDirectory(), SkinFolder, name + ".png"), true);
            return true;
        }

        /// <summary>그린 그림이면 9조각 가장자리와 화면 두께를 돌려준다. 코드로 만든 그림이면 false 다.</summary>
        private static bool TryGetDrawnBorder(string name, out int border, out float onScreen)
        {
            border = Border;
            onScreen = Border;

            if (name == "그림자" || !File.Exists(Path.Combine(DrawnFolder, name + ".png")))
            {
                return false;
            }

            bool panel = name == "패널";
            border = panel ? DrawnPanelBorder : DrawnButtonBorder;
            onScreen = panel ? DrawnPanelBorderOnScreen : DrawnButtonBorderOnScreen;
            return true;
        }

        private static void Write(string name, Color[] pixels)
        {
            Write(name, pixels, Size);
        }

        private static void Write(string name, Color[] pixels, int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply();

            File.WriteAllBytes(
                Path.Combine(Directory.GetCurrentDirectory(), SkinFolder, name + ".png"),
                texture.EncodeToPNG());

            Object.DestroyImmediate(texture);
        }

        /// <summary>9조각으로 늘어나게 하고 압축을 끈다.</summary>
        private static void ApplyImportSettings()
        {
            string[] names = { "패널", "버튼", "버튼_눌림", "그림자", "점선", "원" };

            for (int i = 0; i < names.Length; i++)
            {
                string path = SkinFolder + "/" + names[i] + ".png";
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null)
                {
                    continue;
                }

                // 원은 9조각으로 늘리지 않는다. 늘리면 가장자리가 네모로 펴진다.
                float border = names[i] == "원" ? 0f : Border;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;

                // 그린 그림은 가장자리가 넓어 그대로 두면 화면에서 너무 두껍다.
                // 한 단위에 담는 픽셀 수를 늘려 가장자리가 화면에서 onScreen 두께로 보이게 한다.
                // 원은 9조각으로 늘리지 않으므로 가장자리를 두지 않는다.
                int drawnBorder;
                float onScreen;
                if (border == 0f)
                {
                    importer.spriteBorder = Vector4.zero;
                    importer.spritePixelsPerUnit = 100f;
                }
                else if (TryGetDrawnBorder(names[i], out drawnBorder, out onScreen))
                {
                    importer.spriteBorder = new Vector4(drawnBorder, drawnBorder, drawnBorder, drawnBorder);
                    importer.spritePixelsPerUnit = 100f * drawnBorder / onScreen;
                }
                else
                {
                    importer.spriteBorder = new Vector4(Border, Border, Border, Border);
                    importer.spritePixelsPerUnit = 100f;
                }
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                // 바둑판 깔기로 쓰는 그림은 투명한 곳까지 사각형 그대로 그려야 경고 없이 깔린다.
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }
        }
    }
}
