using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SymbolTagAutoAssigner
{
    private class SymbolSetting
    {
        public string assetName;
        public string displayName;
        public SymbolTagType firstTag;
        public SymbolTagType secondTag;

        public SymbolSetting(
            string assetName,
            string displayName,
            SymbolTagType firstTag,
            SymbolTagType secondTag
        )
        {
            this.assetName = assetName;
            this.displayName = displayName;
            this.firstTag = firstTag;
            this.secondTag = secondTag;
        }
    }

    private static readonly SymbolSetting[] settings =
    {
        // ===== 수성 =====

        new SymbolSetting(
            "QuickSilver",
            "수은",
            SymbolTagType.Mercury,
            SymbolTagType.Venus
        ),

        new SymbolSetting(
            "Fish",
            "물고기",
            SymbolTagType.Mercury,
            SymbolTagType.Earth
        ),

        new SymbolSetting(
            "Geyser",
            "간헐천",
            SymbolTagType.Mercury,
            SymbolTagType.Mars
        ),

        new SymbolSetting(
            "Lotus",
            "연꽃",
            SymbolTagType.Mercury,
            SymbolTagType.Jupiter
        ),

        new SymbolSetting(
            "Mud",
            "진흙",
            SymbolTagType.Mercury,
            SymbolTagType.Saturn
        ),

        new SymbolSetting(
            "DarkCloud",
            "먹구름",
            SymbolTagType.Mercury,
            SymbolTagType.Uranus
        ),

        new SymbolSetting(
            "Tsunami",
            "해일",
            SymbolTagType.Mercury,
            SymbolTagType.Neptune
        ),

        new SymbolSetting(
            "Styx",
            "스틱스강",
            SymbolTagType.Mercury,
            SymbolTagType.Pluto
        ),

        // ===== 금성 =====

        new SymbolSetting(
            "Chariot",
            "전차",
            SymbolTagType.Venus,
            SymbolTagType.Earth
        ),

        new SymbolSetting(
            "Furnace",
            "용광로",
            SymbolTagType.Venus,
            SymbolTagType.Mars
        ),

        new SymbolSetting(
            "GoldLaurel",
            "금빛계관",
            SymbolTagType.Venus,
            SymbolTagType.Jupiter
        ),

        new SymbolSetting(
            "Gem",
            "보석",
            SymbolTagType.Venus,
            SymbolTagType.Saturn
        ),

        new SymbolSetting(
            "Meteorite",
            "운석",
            SymbolTagType.Venus,
            SymbolTagType.Uranus
        ),

        new SymbolSetting(
            "Anchor",
            "닻",
            SymbolTagType.Venus,
            SymbolTagType.Neptune
        ),

        new SymbolSetting(
            "Scythe",
            "낫",
            SymbolTagType.Venus,
            SymbolTagType.Pluto
        ),

        // ===== 지구 =====

        new SymbolSetting(
            "Phoenix",
            "불사조",
            SymbolTagType.Earth,
            SymbolTagType.Mars
        ),

        new SymbolSetting(
            "Druid",
            "드루이드",
            SymbolTagType.Earth,
            SymbolTagType.Jupiter
        ),

        new SymbolSetting(
            "Golem",
            "골렘",
            SymbolTagType.Earth,
            SymbolTagType.Saturn
        ),

        new SymbolSetting(
            "Bird",
            "새",
            SymbolTagType.Earth,
            SymbolTagType.Uranus
        ),

        new SymbolSetting(
            "Whale",
            "고래",
            SymbolTagType.Earth,
            SymbolTagType.Neptune
        ),

        new SymbolSetting(
            "Raven",
            "까마귀",
            SymbolTagType.Earth,
            SymbolTagType.Pluto
        ),

        // ===== 화성 =====

        new SymbolSetting(
            "FireFlower",
            "불의 꽃",
            SymbolTagType.Mars,
            SymbolTagType.Jupiter
        ),

        new SymbolSetting(
            "Volcano",
            "화산",
            SymbolTagType.Mars,
            SymbolTagType.Saturn
        ),

        new SymbolSetting(
            "Sunrise",
            "일출",
            SymbolTagType.Mars,
            SymbolTagType.Uranus
        ),

        new SymbolSetting(
            "LightHouse",
            "등대",
            SymbolTagType.Mars,
            SymbolTagType.Neptune
        ),

        new SymbolSetting(
            "Fullmoon",
            "만월",
            SymbolTagType.Mars,
            SymbolTagType.Pluto
        ),

        // ===== 목성 =====

        new SymbolSetting(
            "Forest",
            "숲",
            SymbolTagType.Jupiter,
            SymbolTagType.Saturn
        ),

        new SymbolSetting(
            "Yggdrasil",
            "세계수",
            SymbolTagType.Jupiter,
            SymbolTagType.Uranus
        ),

        new SymbolSetting(
            "Coral",
            "산호",
            SymbolTagType.Jupiter,
            SymbolTagType.Neptune
        ),

        new SymbolSetting(
            "Coffin",
            "관",
            SymbolTagType.Jupiter,
            SymbolTagType.Pluto
        ),

        // ===== 토성 =====

        new SymbolSetting(
            "Desert",
            "사막",
            SymbolTagType.Saturn,
            SymbolTagType.Uranus
        ),

        new SymbolSetting(
            "Island",
            "섬",
            SymbolTagType.Saturn,
            SymbolTagType.Neptune
        ),

        new SymbolSetting(
            "Grave",
            "무덤",
            SymbolTagType.Saturn,
            SymbolTagType.Pluto
        ),

        // ===== 천왕성 =====

        new SymbolSetting(
            "Hail",
            "우박",
            SymbolTagType.Uranus,
            SymbolTagType.Neptune
        ),

        new SymbolSetting(
            "NightSky",
            "밤하늘",
            SymbolTagType.Uranus,
            SymbolTagType.Pluto
        ),

        // ===== 해왕성 =====

        new SymbolSetting(
            "Shipwreck",
            "난파선",
            SymbolTagType.Neptune,
            SymbolTagType.Pluto
        )
    };

    [MenuItem("Tools/Slot Hero/문양 데이터 일괄 적용")]
    public static void ApplySymbolData()
    {
        Dictionary<SymbolTagType, SymbolTagData> tagDictionary =
            FindSymbolTags();

        if (tagDictionary.Count != 9)
        {
            Debug.LogError(
                $"SymbolTagData가 9개 필요합니다. 현재 발견: {tagDictionary.Count}개"
            );

            return;
        }

        Dictionary<string, SymbolData> symbolDictionary =
            FindSymbols();

        Dictionary<string, Sprite> iconDictionary =
            FindSymbolIcons();

        int successCount = 0;
        int missingSymbolCount = 0;
        int missingIconCount = 0;
        int errorCount = 0;

        foreach (SymbolSetting setting in settings)
        {
            if (!symbolDictionary.TryGetValue(
                    setting.assetName,
                    out SymbolData symbol))
            {
                Debug.LogWarning(
                    $"SymbolData 없음: " +
                    $"{setting.assetName} ({setting.displayName})"
                );

                missingSymbolCount++;
                continue;
            }

            if (!tagDictionary.TryGetValue(
                    setting.firstTag,
                    out SymbolTagData firstTag))
            {
                Debug.LogError(
                    $"{setting.firstTag} SymbolTagData를 찾지 못했습니다."
                );

                errorCount++;
                continue;
            }

            if (!tagDictionary.TryGetValue(
                    setting.secondTag,
                    out SymbolTagData secondTag))
            {
                Debug.LogError(
                    $"{setting.secondTag} SymbolTagData를 찾지 못했습니다."
                );

                errorCount++;
                continue;
            }

            SerializedObject serializedObject =
                new SerializedObject(symbol);

            SerializedProperty symbolNameProperty =
                serializedObject.FindProperty("symbolName");

            SerializedProperty displayNameProperty =
                serializedObject.FindProperty("displayName");

            SerializedProperty iconProperty =
                serializedObject.FindProperty("icon");

            SerializedProperty firstTagProperty =
                serializedObject.FindProperty("firstTag");

            SerializedProperty secondTagProperty =
                serializedObject.FindProperty("secondTag");

            SerializedProperty valueProperty =
                serializedObject.FindProperty("value");

            if (symbolNameProperty == null ||
                displayNameProperty == null ||
                iconProperty == null ||
                firstTagProperty == null ||
                secondTagProperty == null ||
                valueProperty == null)
            {
                Debug.LogError(
                    $"{symbol.name}: SymbolData 필드를 찾지 못했습니다. " +
                    $"symbolName, displayName, icon, firstTag, secondTag, value가 필요합니다."
                );

                errorCount++;
                continue;
            }

            // 문양 내부 이름
            symbolNameProperty.stringValue =
                setting.assetName;

            // 한글 표시 이름
            displayNameProperty.stringValue =
                setting.displayName;

            // 첫 번째 태그
            firstTagProperty.objectReferenceValue =
                firstTag;

            // 두 번째 태그
            secondTagProperty.objectReferenceValue =
                secondTag;

            // 태그 가치의 합을 실제 SymbolData 값으로 저장
            valueProperty.intValue =
                firstTag.Value + secondTag.Value;

            // 같은 영어 이름의 이미지 자동 연결
            if (iconDictionary.TryGetValue(
                    setting.assetName,
                    out Sprite icon))
            {
                iconProperty.objectReferenceValue =
                    icon;
            }
            else
            {
                Debug.LogWarning(
                    $"이미지를 찾지 못했습니다: {setting.assetName}"
                );

                missingIconCount++;
            }

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(symbol);

            successCount++;

            string iconName =
                iconProperty.objectReferenceValue != null
                    ? iconProperty.objectReferenceValue.name
                    : "없음";

            Debug.Log(
                $"{setting.assetName} ({setting.displayName}) | " +
                $"{firstTag.DisplayName} + {secondTag.DisplayName} | " +
                $"Value = {firstTag.Value + secondTag.Value} | " +
                $"Icon = {iconName}"
            );
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "===== 문양 데이터 일괄 적용 완료 =====\n" +
            $"성공: {successCount} / {settings.Length}\n" +
            $"누락 문양: {missingSymbolCount}\n" +
            $"누락 이미지: {missingIconCount}\n" +
            $"오류: {errorCount}"
        );
    }

    private static Dictionary<SymbolTagType, SymbolTagData>
        FindSymbolTags()
    {
        Dictionary<SymbolTagType, SymbolTagData> result =
            new Dictionary<SymbolTagType, SymbolTagData>();

        string[] guids =
            AssetDatabase.FindAssets("t:SymbolTagData");

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            SymbolTagData tag =
                AssetDatabase.LoadAssetAtPath<SymbolTagData>(
                    path
                );

            if (tag == null)
                continue;

            if (result.ContainsKey(tag.TagType))
            {
                Debug.LogError(
                    $"{tag.TagType} SymbolTagData가 중복되어 있습니다. " +
                    $"경로: {path}"
                );

                continue;
            }

            result.Add(
                tag.TagType,
                tag
            );
        }

        return result;
    }

    private static Dictionary<string, SymbolData>
        FindSymbols()
    {
        Dictionary<string, SymbolData> result =
            new Dictionary<string, SymbolData>(
                StringComparer.OrdinalIgnoreCase
            );

        string[] guids =
            AssetDatabase.FindAssets("t:SymbolData");

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            SymbolData symbol =
                AssetDatabase.LoadAssetAtPath<SymbolData>(
                    path
                );

            if (symbol == null)
                continue;

            if (result.ContainsKey(symbol.name))
            {
                Debug.LogWarning(
                    $"같은 이름의 SymbolData가 두 개 이상 있습니다: " +
                    $"{symbol.name}"
                );

                continue;
            }

            result.Add(
                symbol.name,
                symbol
            );
        }

        return result;
    }

    private static Dictionary<string, Sprite>
        FindSymbolIcons()
    {
        Dictionary<string, Sprite> result =
            new Dictionary<string, Sprite>(
                StringComparer.OrdinalIgnoreCase
            );

        HashSet<string> requiredNames =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

        foreach (SymbolSetting setting in settings)
        {
            requiredNames.Add(
                setting.assetName
            );
        }

        string[] textureGuids =
            AssetDatabase.FindAssets("t:Texture2D");

        foreach (string guid in textureGuids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            string fileName =
                Path.GetFileNameWithoutExtension(path);

            // 우리가 사용할 36개 문양 이미지만 검사
            if (!requiredNames.Contains(fileName))
                continue;

            if (result.ContainsKey(fileName))
            {
                Debug.LogWarning(
                    $"같은 이름의 이미지가 두 개 이상 있습니다: " +
                    $"{fileName}\n경로: {path}"
                );

                continue;
            }

            Sprite sprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(
                    path
                );

            // 이미지가 아직 Sprite 형식이 아니라면
            // 자동으로 Sprite (2D and UI)로 변경
            if (sprite == null)
            {
                TextureImporter importer =
                    AssetImporter.GetAtPath(path)
                    as TextureImporter;

                if (importer != null)
                {
                    importer.textureType =
                        TextureImporterType.Sprite;

                    importer.spriteImportMode =
                        SpriteImportMode.Single;

                    importer.alphaIsTransparency =
                        true;

                    importer.SaveAndReimport();
                }

                sprite =
                    AssetDatabase.LoadAssetAtPath<Sprite>(
                        path
                    );
            }

            if (sprite == null)
            {
                Debug.LogWarning(
                    $"Sprite로 불러오지 못했습니다: {path}"
                );

                continue;
            }

            result.Add(
                fileName,
                sprite
            );
        }

        return result;
    }

    [MenuItem("Tools/Slot Hero/문양 누락 확인")]
    public static void CheckMissingSymbols()
    {
        Dictionary<string, SymbolData> symbols =
            FindSymbols();

        Dictionary<string, Sprite> icons =
            FindSymbolIcons();

        int missingSymbolCount = 0;
        int missingIconCount = 0;

        foreach (SymbolSetting setting in settings)
        {
            if (!symbols.ContainsKey(setting.assetName))
            {
                Debug.LogWarning(
                    $"누락 SymbolData: " +
                    $"{setting.assetName} | {setting.displayName}"
                );

                missingSymbolCount++;
            }

            if (!icons.ContainsKey(setting.assetName))
            {
                Debug.LogWarning(
                    $"누락 이미지: {setting.assetName}"
                );

                missingIconCount++;
            }
        }

        Debug.Log(
            "===== 문양 누락 확인 =====\n" +
            $"누락 SymbolData: {missingSymbolCount}\n" +
            $"누락 이미지: {missingIconCount}"
        );
    }
}