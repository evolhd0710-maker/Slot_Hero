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
        public const string SkinFolder = "Assets/SlotHeroArt/Skin";

        /// 한 변 길이. 9조각 가장자리가 16이라 가운데가 32 남는다
        private const int Size = 64;

        /// 9조각 가장자리
        private const int Border = 16;

        /// 모서리 둥글기
        private const float Radius = 12f;

        [MenuItem("Slot Hero/UI 스킨 만들기", priority = 3)]
        public static void Make()
        {
            UiSceneBuilder.EnsureFolder(SkinFolder);

            // 칸. 아래쪽만 살짝 어둡다.
            Write("패널", MakePanel(0.94f, 0.76f, 3, 0.55f));

            // 버튼. 칸보다 세게 들어가 도드라져 보인다.
            Write("버튼", MakePanel(0.88f, 0.58f, 4, 0.42f));

            // 눌린 버튼. 위아래를 뒤집어 움푹 들어가 보인다.
            Write("버튼_눌림", MakeSunken());

            // 그림자. 칸 뒤에 조금 내려 깔면 떠 있는 것처럼 보인다.
            Write("그림자", MakeShadow());

            AssetDatabase.Refresh();
            ApplyImportSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("UI 스킨 그림 네 장을 만들었다: " + SkinFolder);
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

        private static void Write(string name, Color[] pixels)
        {
            Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
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
            string[] names = { "패널", "버튼", "버튼_눌림", "그림자" };

            for (int i = 0; i < names.Length; i++)
            {
                string path = SkinFolder + "/" + names[i] + ".png";
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = new Vector4(Border, Border, Border, Border);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                importer.SaveAndReimport();
            }
        }
    }
}
