using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SlotHero.Ui;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 화면 씬을 코드로 짜는 데 쓰는 도우미.
    /// 각 영역 README 의 씬 구성을 그대로 옮기는 데 쓴다.
    ///
    /// 화면을 눈으로 확인하려고 만든 것이다.
    /// 실제로 쓸 씬은 여기서 만든 것을 손질해 쓰거나 새로 만든다.
    /// </summary>
    public static class UiSceneBuilder
    {
        /// 만든 씬을 두는 자리
        public const string SceneFolder = "Assets/SlotHero/Scenes";

        /// 만든 프리팹을 두는 자리
        public const string PrefabFolder = "Assets/SlotHero/Prefabs";

        /// 설정 에셋이 있는 자리
        public const string ConfigFolder = "Assets/SlotHero/Configs";

        private static TMP_FontAsset _font;

        /// <summary>한글 글꼴. 한 번 찾아 두고 다시 쓴다.</summary>
        public static TMP_FontAsset Font
        {
            get
            {
                if (_font == null)
                {
                    _font = PrepareTextResources.LoadKoreanFont();
                }

                return _font;
            }
        }

        /// <summary>화면 전체를 덮는 캔버스를 만든다. 기준 해상도는 1920 × 1080 이다.</summary>
        public static Canvas CreateCanvas(string name)
        {
            GameObject go = new GameObject(
                name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // 9조각 그림의 가장자리도 좌표 배율을 따라야 한다.
            // 배율이 12 일 때 100 으로 두면 64픽셀 그림의 16픽셀 모서리가 캔버스에서 16칸밖에 안 되어
            // 화면에서는 1.3픽셀로 뭉개졌다. 지금 배율 1 에서는 유니티 기본값 100 과 같다.
            scaler.referencePixelsPerUnit = 100f * UiScale.Current;

            return canvas;
        }

        /// <summary>화면 한가운데의 기준 해상도. 캔버스와 카메라가 함께 쓴다.</summary>
        public static readonly Vector2 ReferenceResolution = UiScale.V(1920f, 1080f);

        /// <summary>
        /// 한 유니티 단위에 몇 픽셀인지.
        /// 100 으로 두면 1080 픽셀이 10.8 단위가 되고 직교 크기는 그 절반인 5.4 다.
        /// 2D 프로젝트의 관례값이고 스프라이트 기본 Pixels Per Unit 과도 같다.
        /// </summary>
        public const float PixelsPerUnit = 100f;

        /// <summary>
        /// 카메라 한 벌을 놓는다.
        ///
        /// 앞 카메라가 실제로 그리고, 뒤 카메라는 화면 전체를 검게 지우기만 한다.
        /// 16:9 를 지키느라 앞 카메라가 창 일부만 그리므로
        /// 뒤 카메라가 없으면 띠 자리에 지난 프레임이 번진다.
        /// </summary>
        public static Camera CreateCamera(string name, Color background, bool keepAspect)
        {
            // 뒤에서 화면 전체를 지우는 카메라. 아무것도 그리지 않는다.
            GameObject letterboxGo = new GameObject("LetterboxCamera", typeof(Camera));
            Camera letterbox = letterboxGo.GetComponent<Camera>();
            letterbox.orthographic = true;
            letterbox.clearFlags = CameraClearFlags.SolidColor;
            letterbox.backgroundColor = Color.black;
            letterbox.cullingMask = 0;
            letterbox.depth = -100f;
            letterbox.rect = new Rect(0f, 0f, 1f, 1f);

            GameObject go = new GameObject(name, typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, -10f);

            Camera camera = go.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = ReferenceResolution.y * 0.5f / PixelsPerUnit;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            camera.depth = 0f;

            CameraFrame frame = go.AddComponent<CameraFrame>();
            frame.SetReference(ReferenceResolution, keepAspect);

            return camera;
        }

        /// <summary>
        /// 캔버스를 그 카메라로 그리게 한다.
        ///
        /// 화면 위에 그냥 덮는 방식(Overlay)은 카메라를 아예 거치지 않는다.
        /// 그러면 16:9 틀이 UI 에는 듣지 않고, 나중에 전투를 뒤에 얹을 때
        /// UI 와 세계가 같은 카메라를 보지 않아 앞뒤를 맞추기 어렵다.
        /// </summary>
        public static void AttachCanvasToCamera(Canvas canvas, Camera camera)
        {
            if (canvas == null || camera == null)
            {
                return;
            }

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 100f;
        }

        /// <summary>입력을 받는 데 필요한 EventSystem 을 놓는다.</summary>
        public static void CreateEventSystem()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static ScrollVisualConfig _scrollConfig;

        /// <summary>
        /// 휠 거리와 스크롤 바 생김새를 담은 설정 에셋(`Configs/Ui/ScrollVisualConfig`).
        /// 예전에는 여기 상수였다(2026년 10월 25일 원재가 설정 에셋으로 옮기라고 했다). 에셋이 없으면 기본값으로 만든다.
        ///
        /// **스크롤을 만들면 반드시 휠 거리를 넣는다.** 유니티 기본값은 1 이라 휠 한 칸에 1 픽셀만 움직인다.
        /// 현재 빌드 화면에서 문양이 세 줄을 넘으면 아래를 볼 수 없었던 것이 이것이었다.
        /// </summary>
        public static ScrollVisualConfig ScrollConfig
        {
            get
            {
                if (_scrollConfig == null)
                {
                    _scrollConfig = LoadConfig<ScrollVisualConfig>("Ui", "ScrollVisualConfig");
                }

                return _scrollConfig != null ? _scrollConfig : ScriptableObject.CreateInstance<ScrollVisualConfig>();
            }
        }

        /// <summary>
        /// 문양 칸에 태그 테두리 둘을 얹을 자리를 만든다. 부모 칸을 꽉 채운다.
        /// 그림은 화면이 문양의 태그를 보고 넣는다. 처음에는 감춰 둔다. 꾸밈이라 커서를 받지 않는다(2026년 10월 19일 원재).
        /// symbol 은 같은 부모 아래의 문양 그림이다. 문양을 보일 때 테두리와 같은 넓이로 맞춘다(2026년 10월 20일 원재).
        /// </summary>
        public static TagCorners NewTagCorners(Transform parent, RectTransform symbol)
        {
            RectTransform root = NewRect("TagCorners", parent);
            Stretch(root);

            Image topRight = NewFlatImage("TopRight", root, Color.white);
            Stretch(topRight.rectTransform);
            topRight.raycastTarget = false;
            topRight.enabled = false;

            Image bottomLeft = NewFlatImage("BottomLeft", root, Color.white);
            Stretch(bottomLeft.rectTransform);
            bottomLeft.raycastTarget = false;
            bottomLeft.enabled = false;

            // 장식 모서리 끝의 태그 기호. 자리와 크기는 화면이 그릴 때 비율로 잡는다(2026년 10월 26일 원재).
            Image topRightGlyph = NewFlatImage("TopRightGlyph", root, Color.white);
            Stretch(topRightGlyph.rectTransform);
            topRightGlyph.raycastTarget = false;
            topRightGlyph.enabled = false;

            Image bottomLeftGlyph = NewFlatImage("BottomLeftGlyph", root, Color.white);
            Stretch(bottomLeftGlyph.rectTransform);
            bottomLeftGlyph.raycastTarget = false;
            bottomLeftGlyph.enabled = false;

            TagCorners corners = root.gameObject.AddComponent<TagCorners>();
            SetField(corners, "_topRight", topRight);
            SetField(corners, "_bottomLeft", bottomLeft);
            SetField(corners, "_topRightGlyph", topRightGlyph);
            SetField(corners, "_bottomLeftGlyph", bottomLeftGlyph);
            SetField(corners, "_symbol", symbol);
            return corners;
        }

        /// <summary>
        /// 문양 뒤에 까는 흑요석 판 자리를 만들어 테두리에 물린다(2026년 10월 20일 원재).
        /// 판은 부모를 꽉 채운다. 문양 그림보다 먼저 그려져야 하므로 부르는 쪽이 문양 그림 앞에 오게 자리를 잡는다.
        /// 그림은 화면이 문양을 보고 넣는다. 처음에는 감춰 둔다. 꾸밈이라 커서를 받지 않는다.
        /// </summary>
        public static Image NewSymbolPlate(Transform parent, TagCorners corners)
        {
            Image plate = NewFlatImage("SymbolPlate", parent, Color.white);
            Stretch(plate.rectTransform);
            plate.raycastTarget = false;
            plate.enabled = false;
            SetField(corners, "_plate", plate);
            return plate;
        }

        /// <summary>스크롤을 붙이고 휠 거리를 맞춘다.</summary>
        public static ScrollRect AddScroll(RectTransform viewport, RectTransform content)
        {
            ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = ScrollConfig.WheelStep;

            // 휠은 창 어디서나 받아야 한다. 글자는 커서를 받지 않으므로 그림이 없는 창에는 투명한 바탕을 깐다.
            // 예전에는 글자 위에서만 휠이 들던 창이 다섯 있었다. 런 결과 칸 셋, 설정 목록, 팝업 본문이다. 2026년 10월 18일 검토.
            if (viewport.GetComponent<Graphic>() == null)
            {
                Image catcher = viewport.gameObject.AddComponent<Image>();
                catcher.color = new Color(0f, 0f, 0f, 0f);
            }

            AddScrollBar(scroll, viewport);
            return scroll;
        }

        /// <summary>
        /// 스크롤 창의 오른쪽 끝에 세로 스크롤 바를 단다. 스크롤할 만큼 분량이 있을 때만 보인다(`ScrollBarVisibility`).
        /// 스크롤할 분량이 있으면 오른쪽에 바가 있어야 한다(2026년 10월 22일 원재).
        ///
        /// 바는 창의 자식이라 창 크기를 화면이 나중에 바꿔도 따라간다. 창 안쪽에 두므로 가림막에 잘리지 않는다.
        /// 내용 쪽은 바 자리(`ScrollBarSpace`)만큼 오른쪽을 비워 두어야 한다. 씬 검사가 겹치는지 본다.
        /// </summary>
        private static void AddScrollBar(ScrollRect scroll, RectTransform viewport)
        {
            // 칸 그림(쇠 테)은 폭 10 에 맞지 않아 둥근 막대를 쓴다. 현재 빌드의 태그 막대와 같다.
            ScrollVisualConfig config = ScrollConfig;
            Image track = NewScrollBarImage("ScrollBar", viewport, config.TrackColor);
            RectTransform barRect = track.rectTransform;
            barRect.anchorMin = new Vector2(1f, 0f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(1f, 0.5f);
            barRect.sizeDelta = new Vector2(config.BarWidth, -config.BarInset * 2f);
            barRect.anchoredPosition = new Vector2(-config.BarInset, 0f);

            RectTransform slidingArea = NewRect("SlidingArea", barRect);
            Stretch(slidingArea);

            Image handle = NewScrollBarImage("Handle", slidingArea, config.HandleColor);
            Stretch(handle.rectTransform);

            Scrollbar bar = track.gameObject.AddComponent<Scrollbar>();
            bar.direction = Scrollbar.Direction.BottomToTop;
            bar.handleRect = handle.rectTransform;
            bar.targetGraphic = handle;

            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            ScrollBarVisibility visibility = viewport.gameObject.AddComponent<ScrollBarVisibility>();
            SetField(visibility, "_scroll", scroll);
            visibility.Refresh();
        }

        /// <summary>
        /// 스크롤 바의 윗끝을 창 위에서 top 만큼 내린다. 화면을 꽉 채우는 창은 맨 위가 상단 표시줄 밑이라
        /// 그대로 두면 바가 표시줄을 가로지른다. 현재 빌드와 고르기 화면이 쓴다.
        /// </summary>
        public static void SetScrollBarTop(ScrollRect scroll, float top)
        {
            if (scroll == null || scroll.verticalScrollbar == null)
            {
                return;
            }

            RectTransform bar = (RectTransform)scroll.verticalScrollbar.transform;
            bar.offsetMin = new Vector2(bar.offsetMin.x, ScrollConfig.BarInset);
            bar.offsetMax = new Vector2(bar.offsetMax.x, -top);
        }

        /// <summary>스크롤 바의 둥근 막대 그림. 손잡이를 끌 수 있게 커서를 받는다.</summary>
        private static Image NewScrollBarImage(string name, Transform parent, Color color)
        {
            Image image = NewFlatImage(name, parent, color);
            image.sprite = RoundedSprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 0.1f;
            return image;
        }

        /// <summary>내용이 오른쪽에 비워 둘 폭. 바와 바 오른쪽 여백, 바 왼쪽 여백 하나를 더한 것이다.</summary>
        public static float ScrollBarSpace
        {
            get { return ScrollConfig.BarSpace; }
        }

        /// <summary>빈 RectTransform 하나를 만든다.</summary>
        public static RectTransform NewRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            SizeDefault(rect);
            return rect;
        }

        /// <summary>
        /// 새 칸의 기본 크기를 배율에 맞춘다.
        ///
        /// 유니티가 새로 만든 RectTransform 에 100 × 100 을 넣는다.
        /// 좌표 배율을 올리면 글자만 커지고 이 칸은 100 그대로라
        /// 뷰가 크기를 따로 잡아 주지 않는 글자가 한 자씩 줄바꿈된다. 배율이 12 였을 때 그랬다.
        /// `Stretch` 나 `PlaceTopLeft` 를 부르면 이 값은 덮어써지므로 걸리적거리지 않는다.
        /// </summary>
        public static void SizeDefault(RectTransform rect)
        {
            rect.sizeDelta = UiScale.V(100f, 100f);
        }

        private static Sprite _roundedSprite;

        /// <summary>
        /// 모서리가 둥근 기본 스프라이트.
        /// 04 화면 공통 규칙 이 버튼과 카드를 둥근 모서리로 정했는데 아직 그림이 없어
        /// 유니티에 딸려 오는 것을 빌려 쓴다.
        /// </summary>
        public static Sprite RoundedSprite
        {
            get
            {
                if (_roundedSprite == null)
                {
                    _roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                }

                return _roundedSprite;
            }
        }

        /// <summary>
        /// 스킨 그림 하나. 없으면 유니티 기본 그림으로 넘어간다.
        /// `Slot Hero / UI 스킨 만들기` 를 한 번 누르면 만들어진다.
        /// </summary>
        public static Sprite LoadSkin(string name)
        {
            Sprite found = AssetDatabase.LoadAssetAtPath<Sprite>(
                MakeUiSkin.SkinFolder + "/" + name + ".png");

            return found != null ? found : RoundedSprite;
        }

        /// <summary>그림 칸 하나를 만든다. 모서리는 둥글고 아래쪽이 살짝 어두워 도드라져 보인다.</summary>
        public static Image NewImage(string name, Transform parent, Color color)
        {
            return NewSkinned(name, parent, color, LoadSkin("패널"));
        }

        /// <summary>누르는 칸. 칸보다 입체감이 세다.</summary>
        public static Image NewButtonImage(string name, Transform parent, Color color)
        {
            return NewSkinned(name, parent, color, LoadSkin("버튼"));
        }

        private static Image NewSkinned(string name, Transform parent, Color color, Sprite sprite)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            SizeDefault((RectTransform)go.transform);

            Image image = go.GetComponent<Image>();
            image.color = color;

            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
            }

            return image;
        }

        /// <summary>
        /// 칸 뒤에 그림자를 깐다. 칸이 떠 있는 것처럼 보인다.
        ///
        /// 그림자는 칸의 **형제**로 바로 앞자리에 넣는다.
        /// 자식으로 넣으면 부모가 먼저 그려져 그림자가 칸 위에 올라온다.
        ///
        /// 칸의 자리와 크기는 뷰가 나중에 다시 잡으므로 여기서 맞춰 두지 않는다.
        /// 대신 `ShadowFollower` 가 붙어 매번 따라간다.
        /// </summary>
        public static Image AddShadowPx(Image panel, float wireframeSpread, float wireframeDrop)
        {
            if (panel == null)
            {
                return null;
            }

            Image shadow = NewSkinned(
                panel.name + "Shadow", panel.transform.parent, Color.white, LoadSkin("그림자"));

            shadow.raycastTarget = false;

            // 칸 바로 앞에 두어 칸보다 먼저 그려지게 한다.
            shadow.transform.SetSiblingIndex(panel.transform.GetSiblingIndex());

            ShadowFollower follower = shadow.gameObject.AddComponent<ShadowFollower>();
            SetField(follower, "_target", panel.rectTransform);
            follower.SetOffsets(UiScale.Px(wireframeSpread), UiScale.Px(wireframeDrop));
            follower.Follow();

            return shadow;
        }

        /// <summary>모서리가 각진 칸. 화면을 덮는 바탕처럼 모서리가 없어야 할 때 쓴다.</summary>
        public static Image NewFlatImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            SizeDefault((RectTransform)go.transform);

            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        /// <summary>
        /// 글상자 하나를 만든다. 한글 글꼴을 함께 물린다.
        /// 04 화면 공통 규칙 의 텍스트 표가 크기와 굵기를 짝으로 정하므로 굵기도 함께 받는다.
        /// </summary>
        public static TMP_Text NewText(
            string name,
            Transform parent,
            string text,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment,
            bool bold = false,
            bool noWrap = false)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            SizeDefault((RectTransform)go.transform);

            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();

            // 글자는 커서를 받지 않는다. 누르는 칸은 글자가 아닌 그림이 받는다(uGUI 규칙, 2026년 10월 18일 검토).
            // 커서를 받는 그림이 적을수록 매 프레임 훑는 수가 준다.
            label.raycastTarget = false;
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;

            if (bold)
            {
                label.fontStyle = FontStyles.Bold;
            }

            if (noWrap)
            {
                label.textWrappingMode = TextWrappingModes.NoWrap;
            }

            if (Font != null)
            {
                label.font = Font;
            }

            return label;
        }

        /// <summary>
        /// 글자를 치는 칸 하나를 만든다.
        ///
        /// TextMeshPro 의 입력칸은 글상자와 자리표시 글상자, 그리고 그것을 감싸는
        /// 잘라 내기 칸이 함께 있어야 돈다. 손으로 다 이어 준다.
        /// </summary>
        public static TMP_InputField NewInputField(
            string name, Transform parent, float fontSize, Color color)
        {
            Image background = NewImage(name, parent, Color.white);
            RectTransform root = background.rectTransform;

            RectTransform area = NewRect("TextArea", root);
            Stretch(area);
            area.offsetMin = UiScale.V(10f, 6f);
            area.offsetMax = UiScale.V(-10f, -6f);
            area.gameObject.AddComponent<RectMask2D>();

            TMP_Text placeholder = NewText(
                "Placeholder", area, "이름을 입력하세요",
                fontSize, new Color(color.r, color.g, color.b, 0.4f),
                TextAlignmentOptions.Left);
            Stretch(placeholder.rectTransform);

            TMP_Text text = NewText(
                "Text", area, string.Empty, fontSize, color, TextAlignmentOptions.Left);
            Stretch(text.rectTransform);

            TMP_InputField field = background.gameObject.AddComponent<TMP_InputField>();
            field.textViewport = area;
            field.textComponent = (TMP_Text)text;
            field.placeholder = placeholder;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.characterLimit = 16;

            // 프리팹에서는 꺼 둔다. 이름을 고칠 때만 화면이 켠다.
            background.gameObject.SetActive(false);

            return field;
        }

        /// <summary>
        /// 누를 수 있게 만든다.
        /// 누르는 동안 그림을 움푹 들어간 것으로 바꿔 눌린 느낌을 준다.
        ///
        /// 색은 각 화면 스크립트가 직접 다룬다.
        /// 유니티의 색 전환을 함께 켜면 둘이 서로 덮어써 깜빡이므로 그림만 바꾼다.
        /// </summary>
        public static Button AddButton(GameObject go)
        {
            return AddButton(go, go.GetComponent<Image>());
        }

        /// <summary>
        /// 누를 수 있게 만든다. 그림이 자식에 있을 때 그것을 지정한다.
        /// 프리팹은 뿌리에 그림이 없고 `Background` 자식이 그림을 갖는다.
        /// </summary>
        public static Button AddButton(GameObject go, Image target)
        {
            Button button = go.GetComponent<Button>();
            if (button == null)
            {
                button = go.AddComponent<Button>();
            }

            // **어느 버튼에나 호버 확대를 붙인다.**
            // 밝기만 바꾸면 그림이 밝은 버튼은 올렸는지 티가 안 난다.
            // 성소의 모닥불과 행상인, 행상의 버튼들이 그랬다.
            // 크기만 건드리므로 밝기를 스스로 다루는 버튼과도 겹치지 않는다.
            if (go.GetComponent<HoverScale>() == null)
            {
                go.AddComponent<HoverScale>();
            }

            if (target == null)
            {
                button.transition = Selectable.Transition.None;
                return button;
            }

            Sprite pressed = LoadSkin("버튼_눌림");
            Sprite normal = target.sprite;

            // **버튼 스킨일 때만 눌림 그림으로 바꾼다.**
            // 그림 자체가 버튼인 칸에 눌림 그림을 넣으면 누르는 동안 그림 대신 상자가 나온다.
            // 성소의 모닥불과 행상인을 누를 때 흰 상자가 잠깐 보였다 사라진 것이 이것이었다.
            // 어두운 칸 그림을 버튼으로 쓰는 곳(현재 빌드와 고르기 화면)도 바꾸지 않는다.
            // 눌림 그림은 베이지라 누르는 동안 크림색 글자가 읽히지 않았다(2026년 10월 21일 코덱스 검토).
            if (pressed == null || pressed == normal || !IsButtonSkinSprite(normal))
            {
                button.transition = Selectable.Transition.None;
                return button;
            }

            button.targetGraphic = target;
            button.transition = Selectable.Transition.SpriteSwap;

            // **눌림만 채운다. 나머지는 비워 둔다.**
            // 비워 두면 `Image.overrideSprite` 가 null 이 되어 그 칸의 `sprite` 가 그대로 나온다.
            // 거기에 그림을 채우면 뷰가 `sprite` 를 갈아 끼워도 덮여 버려 화면에 나오지 않는다.
            // 야영을 쓴 뒤 꺼진 모닥불로 바뀌지 않은 것이 이것이었다.
            // 뷰가 `interactable` 을 끄는 순간 버튼이 비활성 그림으로 되돌려 놓고 있었다.
            SpriteState state = new SpriteState();
            state.pressedSprite = pressed;
            button.spriteState = state;

            return button;
        }

        /// <summary>
        /// 베이지 버튼 스킨 그림인지. 눌림 그림은 이것과 모양과 색이 같아 바꿔 끼워도 어색하지 않다.
        /// 어두운 칸 그림, 모닥불, 행상인, 아이콘 같은 그림은 아니다.
        /// </summary>
        public static bool IsButtonSkinSprite(Sprite sprite)
        {
            if (sprite == null)
            {
                return false;
            }

            return sprite == LoadSkin("버튼");
        }

        /// <summary>돌벽 위에 얹는 어둡게 하기. 와이어프레임의 `배경 · 검정 50%` 다.</summary>
        public const float StoneBackdropShade = 0.5f;

        /// <summary>
        /// 전투 배경 위에 얹는 어둡게 하기. 그림이 이미 어두워 돌벽보다 옅게 둔다.
        /// 가운데 슬롯 칸과 주인공, 몬스터가 배경보다 앞으로 나와 보이게 하는 정도다(2026년 10월 25일).
        /// </summary>
        public const float BattleBackdropShade = 0.25f;

        /// <summary>
        /// 전투 방의 배경. 무너진 제국의 폐허 그림(`배경_전투`)이다(2026년 10월 25일 원재가 코덱스로 그리게 했다).
        /// 공용 배경 바로 위에 깔고 흐름이 전투 방에서만 켠다. 그림이 없으면 만들지 않고 null 을 돌려준다.
        /// </summary>
        public static Image NewBattleBackdrop(string name, Transform parent)
        {
            Sprite battle = ImportArt.LoadItemArt("배경_전투");
            if (battle == null)
            {
                return null;
            }

            Image image = NewFlatImage(name, parent, Color.white);
            Stretch(image.rectTransform);
            image.raycastTarget = false;
            image.sprite = battle;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;

            Image shade = NewFlatImage("Shade", image.transform, new Color(0f, 0f, 0f, BattleBackdropShade));
            Stretch(shade.rectTransform);
            shade.raycastTarget = false;

            return image;
        }

        /// <summary>
        /// 런 중 화면 뒤에 까는 돌벽.
        ///
        /// 와이어프레임 `10 맵 화면`, `12 이벤트`, `13 런 종료 결과` 가 모두
        /// `배경 · 돌벽.png (임시)` 위에 `배경 · 검정 50%` 를 얹어 쓴다. 그것을 그대로 짓는다.
        /// **임시 그림이다.** 배경 요청 기획서의 1스테이지 전투 배경이 오면 갈아 끼운다.
        ///
        /// 게임 씬은 이것을 맨 뒤에 하나만 깔고, 확인용 씬은 화면마다 `BehindScreen` 으로 깐다.
        /// 확인용 씬의 것은 게임 씬으로 옮길 때 지워진다.
        /// 그래야 게임 씬에서 화면마다 깐 바탕이 공용 배경을 덮지 않는다.
        /// </summary>
        public static Image NewStoneBackdrop(string name, Transform parent)
        {
            Image wall = NewFlatImage(name, parent, Color.white);
            Stretch(wall.rectTransform);
            wall.raycastTarget = false;

            Sprite stone = ImportArt.LoadItemArt("배경_돌벽");

            if (stone != null)
            {
                wall.sprite = stone;
                wall.type = Image.Type.Simple;

                // 그림이 16:9 라 늘려도 비율이 깨지지 않는다.
                wall.preserveAspect = false;
            }
            else
            {
                // 그림이 없으면 예전의 어두운 한 색으로 둔다.
                wall.color = new Color(0.125f, 0.114f, 0.102f, 1f);
            }

            // 팝업의 어두운 막과 헷갈리지 않게 이름을 `Dim` 으로 두지 않는다.
            // 구조 감사가 `Dim` 이라는 이름을 팝업 막으로 읽는다.
            Image shade = NewFlatImage("Shade", wall.transform, new Color(0f, 0f, 0f, StoneBackdropShade));
            Stretch(shade.rectTransform);
            shade.raycastTarget = false;

            return wall;
        }

        /// <summary>부모를 가득 채우게 늘린다.</summary>
        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>화면 왼쪽 위를 기준으로 자리를 잡는다. 기획서 좌표를 그대로 넣을 수 있다.</summary>
        public static void PlaceTopLeft(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(position.x, -position.y);
        }

        /// <summary>
        /// 감춰진 직렬화 필드에 값을 넣는다.
        /// 각 화면 스크립트의 참조가 `[SerializeField] private` 라 이 길로만 넣을 수 있다.
        /// </summary>
        public static void SetField(Object target, string fieldName, Object value)
        {
            if (target == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(fieldName);

            if (property == null)
            {
                Debug.LogWarning(target.GetType().Name + " 에 " + fieldName + " 필드가 없다.");
                return;
            }

            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// 감춰진 배열 필드를 통째로 채운다.
        /// `MerchantScreenView` 의 진열 자리 배열처럼 개수가 설정에서 오는 것에 쓴다.
        /// </summary>
        public static void SetArrayField(Object target, string fieldName, Object[] values)
        {
            if (target == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(fieldName);

            if (property == null)
            {
                Debug.LogWarning(target.GetType().Name + " 에 " + fieldName + " 필드가 없다.");
                return;
            }

            if (!property.isArray)
            {
                Debug.LogWarning(target.GetType().Name + " 의 " + fieldName + " 은 배열이 아니다.");
                return;
            }

            property.arraySize = values != null ? values.Length : 0;

            for (int i = 0; i < property.arraySize; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>참 거짓 필드에 값을 넣는다.</summary>
        public static void SetBoolField(Object target, string fieldName, bool value)
        {
            if (target == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(fieldName);

            if (property == null)
            {
                Debug.LogWarning(target.GetType().Name + " 에 " + fieldName + " 필드가 없다.");
                return;
            }

            property.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>설정 에셋을 읽어 온다.</summary>
        public static T LoadConfig<T>(string area, string name) where T : ScriptableObject
        {
            return AssetDatabase.LoadAssetAtPath<T>(ConfigFolder + "/" + area + "/" + name + ".asset");
        }

        /// <summary>프리팹으로 저장하고 씬에 둔 원본은 지운다.</summary>
        public static GameObject SavePrefab(GameObject go, string name)
        {
            EnsureFolder(PrefabFolder);

            string path = PrefabFolder + "/" + name + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            return prefab;
        }

        /// <summary>
        /// 씬 안의 그림자를 모두 칸 자리에 다시 맞춘다.
        ///
        /// 그림자를 붙이는 때는 칸이 아직 기본 크기일 때다.
        /// 자리는 그 뒤에 `ApplyLayout` 이 잡으므로 다 만든 뒤 한 번 더 맞춰야 한다.
        /// 플레이 중에는 `ShadowFollower` 가 매 프레임 따라가지만
        /// 배치 모드에서는 `LateUpdate` 가 돌지 않아 여기서 직접 불러 준다.
        /// </summary>
        public static void SyncShadows()
        {
            ShadowFollower[] followers =
                Object.FindObjectsByType<ShadowFollower>();

            for (int i = 0; i < followers.Length; i++)
            {
                followers[i].Follow();
            }
        }

        /// <summary>폴더가 없으면 위에서부터 차례로 만든다.</summary>
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
