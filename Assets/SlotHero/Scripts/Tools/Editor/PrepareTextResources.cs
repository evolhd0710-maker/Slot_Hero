using System.IO;
using System.Text.RegularExpressions;
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
        private const string FontFolder = "Assets/SlotHero/Fonts";
        private const string SourceFontPath = FontFolder + "/malgun.ttf";
        private const string FontAssetPath = FontFolder + "/MalgunGothic SDF.asset";

        /// <summary>
        /// 글꼴 에셋의 GUID. 모든 컴퓨터에서 이 값으로 맞춘다.
        ///
        /// 글꼴 폴더는 올리지 않아 에셋을 컴퓨터마다 만든다. 그대로 두면 유니티가 매번 새 GUID 를 붙여
        /// 남이 지어 올린 씬과 프리팹이 내 컴퓨터에서는 없는 글꼴을 가리킨다(2026년 10월 27일).
        /// 값은 그때 저장소의 씬들이 가리키던 GUID 다.
        /// </summary>
        public const string FontGuid = "8d4fd947dd69ff748aeb4b5961191166";

        /// <summary>
        /// 글꼴 에셋 안 머티리얼의 fileID. GUID 처럼 모든 컴퓨터에서 이 값으로 맞춘다.
        ///
        /// 글자마다 머티리얼을 {fileID, GUID} 로 따로 물어 둔다. GUID 만 맞추면 글꼴은 찾아도
        /// 머티리얼이 비어 런 결과 화면에서 MissingReferenceException 이 났다(2026년 10월 27일).
        /// 값은 그때 저장소의 씬들이 가리키던 fileID 다.
        /// </summary>
        public const long FontMaterialFileId = -2575924145907383745;

        /// <summary>
        /// 글꼴 에셋의 첫 아틀라스 텍스처 fileID. 위와 같은 까닭으로 맞춘다.
        ///
        /// 테두리를 두른 글자(`TextOutline`)는 머티리얼 사본이 씬에 함께 저장되고, 그 사본이 첫 아틀라스를 가리킨다.
        /// 이 값이 어긋나 첫 아틀라스에 든 글자(플레이 시간의 0 ~ 4, 9)가 흰 네모로 나왔다(2026년 10월 27일).
        /// 둘째 아틀라스부터는 TMP 가 실행 중에 글꼴에서 머티리얼을 새로 만들어 괜찮았다.
        /// </summary>
        public const long FontAtlasFileId = 5156188376248204739;

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
                PinFontGuid();
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

            // 유니티 6.6 에서 `AssetDatabase.ImportPackage` 가 낡은 것이 되어 새 창구로 바꿨다. 2026년 10월 18일 검토.
            UnityEditor.AssetPackage.Package.Import(package, false);
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

        /// <summary>
        /// 글꼴 에셋의 GUID 를 `FontGuid` 로, 안의 머티리얼 fileID 를 `FontMaterialFileId` 로 바꾼다.
        /// 이미 같으면 아무것도 하지 않는다.
        /// .meta 의 guid 줄과 에셋 글의 fileID 를 고치고 다시 가져오게 하면 유니티가 새 값으로 읽는다.
        /// </summary>
        private static void PinFontGuid()
        {
            string metaPath = FontAssetPath + ".meta";
            if (!File.Exists(FontAssetPath) || !File.Exists(metaPath))
            {
                return;
            }

            bool changed = false;

            if (AssetDatabase.AssetPathToGUID(FontAssetPath) != FontGuid)
            {
                string meta = File.ReadAllText(metaPath);
                File.WriteAllText(metaPath, Regex.Replace(meta, @"guid: [0-9a-f]{32}", "guid: " + FontGuid));
                changed = true;
            }

            // 머티리얼과 첫 아틀라스는 에셋 안의 하위 오브젝트다.
            // 그 fileID 가 쓰인 곳(오브젝트 머리, 글꼴의 m_Material 과 m_AtlasTextures, 머티리얼의 _MainTex)을 모두 바꾼다.
            string text = File.ReadAllText(FontAssetPath);
            bool textChanged = false;
            textChanged |= PinFileId(ref text, @"m_Material: \{fileID: (-?\d+)\}", FontMaterialFileId);
            textChanged |= PinFileId(ref text, @"m_AtlasTextures:\s*\r?\n\s*- \{fileID: (-?\d+)\}", FontAtlasFileId);
            if (textChanged)
            {
                File.WriteAllText(FontAssetPath, text);
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(FontAssetPath, ImportAssetOptions.ForceUpdate);

            string guid = AssetDatabase.AssetPathToGUID(FontAssetPath);
            TMP_FontAsset font = LoadKoreanFont();
            long materialId = 0;
            long atlasId = 0;
            string ignored;
            bool materialPinned = font != null && font.material != null
                && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(font.material, out ignored, out materialId)
                && materialId == FontMaterialFileId;
            bool atlasPinned = font != null && font.atlasTexture != null
                && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(font.atlasTexture, out ignored, out atlasId)
                && atlasId == FontAtlasFileId;

            if (guid == FontGuid && materialPinned && atlasPinned)
            {
                Debug.Log("한글 글꼴 에셋의 GUID 와 머티리얼 · 첫 아틀라스 fileID 를 맞췄다: " + FontGuid);
            }
            else
            {
                Debug.LogWarning("한글 글꼴 에셋을 맞추지 못했다. 지금 GUID " + guid + ", 머티리얼 " + materialId +
                                 ", 첫 아틀라스 " + atlasId + ". 에디터를 다시 열고 한 번 더 한다.");
            }
        }

        /// <summary>
        /// 패턴이 처음 찾은 fileID 를 pinned 로 바꾼다. 그 fileID 가 쓰인 곳을 모두 바꾼다. 바꿨으면 true 다.
        /// </summary>
        private static bool PinFileId(ref string text, string pattern, long pinned)
        {
            Match found = Regex.Match(text, pattern);
            string value = pinned.ToString();
            if (!found.Success || found.Groups[1].Value == value)
            {
                return false;
            }

            string old = found.Groups[1].Value;
            text = Regex.Replace(text, @"(?<![\d-])" + Regex.Escape(old) + @"(?!\d)", value);
            return true;
        }

        /// <summary>만들어 둔 한글 글꼴 에셋. 없으면 null.</summary>
        public static TMP_FontAsset LoadKoreanFont()
        {
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        }
    }
}
