using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 글자를 화면에 그리는 데 필요한 것을 갖춘다.
    /// 메뉴 Slot Hero / 글자 준비하기 에서 실행한다.
    ///
    /// TextMeshPro 는 기본 글꼴에 한글이 없어 그대로 두면 네모로 나온다.
    /// 그래서 윈도우의 맑은 고딕에서 글꼴 에셋을 만들어 쓴다.
    ///
    /// **확인용이다.** 실제로 내보낼 때 쓸 글꼴은 따로 정해야 한다.
    /// 맑은 고딕은 윈도우에 딸려 오는 글꼴이라 배포 조건을 따로 확인해야 한다.
    /// </summary>
    public static class PrepareTextResources
    {
        private const string FontFolder = "Assets/SlotHeroFonts";
        private const string SourceFontPath = FontFolder + "/malgun.ttf";
        private const string FontAssetPath = FontFolder + "/MalgunGothic SDF.asset";

        /// 윈도우에 딸려 오는 맑은 고딕
        private const string WindowsFont = @"C:\Windows\Fonts\malgun.ttf";

        [MenuItem("Slot Hero/글자 준비하기/1 · TMP 기본 리소스 가져오기")]
        public static void ImportEssentialsOnly()
        {
            try
            {
                ImportEssentials();
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError("TMP 리소스 가져오기 실패: " + e);
            }
        }

        [MenuItem("Slot Hero/글자 준비하기/2 · 한글 글꼴 에셋 만들기")]
        public static void CreateFontOnly()
        {
            try
            {
                CreateKoreanFontAsset();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError("한글 글꼴 에셋 만들기 실패: " + e);
            }
        }

        /// <summary>둘을 이어서 한다. 에디터에서 손으로 쓸 때 편하라고 둔 것이다.</summary>
        [MenuItem("Slot Hero/글자 준비하기/모두 하기")]
        public static void Prepare()
        {
            ImportEssentialsOnly();
            CreateFontOnly();
        }

        /// <summary>TextMeshPro 가 쓰는 기본 리소스를 가져온다. 이미 있으면 건너뛴다.</summary>
        private static void ImportEssentials()
        {
            if (Directory.Exists("Assets/TextMesh Pro/Resources"))
            {
                Debug.Log("TMP 기본 리소스가 이미 있다.");
                return;
            }

            string package = FindEssentialPackage();
            if (string.IsNullOrEmpty(package))
            {
                Debug.LogWarning("TMP 기본 리소스 꾸러미를 찾지 못했다. 손으로 가져와야 한다.");
                return;
            }

            AssetDatabase.ImportPackage(package, false);
            AssetDatabase.Refresh();
            Debug.Log("TMP 기본 리소스를 가져왔다: " + package);
        }

        /// <summary>패키지 캐시에서 TMP 기본 리소스 꾸러미를 찾는다.</summary>
        private static string FindEssentialPackage()
        {
            string cache = Path.Combine(Directory.GetCurrentDirectory(), "Library/PackageCache");
            if (!Directory.Exists(cache))
            {
                return string.Empty;
            }

            string[] found = Directory.GetFiles(
                cache, "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);

            return found.Length > 0 ? found[0] : string.Empty;
        }

        /// <summary>맑은 고딕에서 한글 글꼴 에셋을 만든다.</summary>
        private static void CreateKoreanFontAsset()
        {
            if (File.Exists(FontAssetPath))
            {
                Debug.Log("한글 글꼴 에셋이 이미 있다: " + FontAssetPath);
                return;
            }

            if (!Directory.Exists(FontFolder))
            {
                Directory.CreateDirectory(FontFolder);
                AssetDatabase.Refresh();
            }

            if (!File.Exists(SourceFontPath))
            {
                if (!File.Exists(WindowsFont))
                {
                    Debug.LogWarning("맑은 고딕을 찾지 못했다: " + WindowsFont);
                    return;
                }

                File.Copy(WindowsFont, SourceFontPath, true);
                AssetDatabase.ImportAsset(SourceFontPath);
            }

            Font font = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (font == null)
            {
                Debug.LogWarning("글꼴을 읽지 못했다: " + SourceFontPath);
                return;
            }

            // 한글은 글자가 많아 미리 굽지 않고 쓰는 만큼만 채우는 방식으로 만든다.
            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(
                font,
                90,
                9,
                UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic);

            if (asset == null)
            {
                Debug.LogWarning("글꼴 에셋을 만들지 못했다.");
                return;
            }

            asset.name = Path.GetFileNameWithoutExtension(FontAssetPath);
            AssetDatabase.CreateAsset(asset, FontAssetPath);

            // 아틀라스 텍스처와 자료를 에셋 안에 함께 담는다.
            if (asset.atlasTextures != null)
            {
                for (int i = 0; i < asset.atlasTextures.Length; i++)
                {
                    if (asset.atlasTextures[i] != null)
                    {
                        asset.atlasTextures[i].name = "Atlas " + i;
                        AssetDatabase.AddObjectToAsset(asset.atlasTextures[i], asset);
                    }
                }
            }

            if (asset.material != null)
            {
                asset.material.name = asset.name + " Material";
                AssetDatabase.AddObjectToAsset(asset.material, asset);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("한글 글꼴 에셋을 만들었다: " + FontAssetPath);
        }

        /// <summary>만들어 둔 한글 글꼴 에셋. 없으면 null.</summary>
        public static TMP_FontAsset LoadKoreanFont()
        {
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        }
    }
}
