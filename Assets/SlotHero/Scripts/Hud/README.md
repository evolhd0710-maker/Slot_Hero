# Slot Hero 현재 빌드 버튼 · 고정 자리 코드

인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 "런 진행 중 고정되는 자리"를 옮긴 코드다.
현재 빌드 버튼과 그 아래 자동 저장 표시까지를 한 덩어리로 다룬다.

상단 UI 바 기획서 v0.2 의 수정 이력이 "빌드 확인 버튼 삭제(별도 구현)"이므로
이 버튼은 상단 표시줄이 아니라 화면 우측 하단의 고정 자리에 따로 둔다.

**성소의 나가기 화살표도 우측 하단이라 둘이 겹친다.**
기획서와 와이어프레임이 이 버튼을 1650 부터로 정해 두었으므로 이쪽을 지키고,
화살표를 왼쪽으로 220 물렸다. `SanctumLayoutConfig.ExitPosition` 이 그 값이다.
반대로 하려면 `HudLayoutConfig.OnLeftCorner` 를 켜 버튼을 왼쪽 아래로 옮기면 된다.
`SanctumLayoutConfig.ExitClearsHudButton` 이 둘이 안 겹치는지 본다.

폴더 이름을 `Hud`로 한 것은 04장의 고정 자리가 특정 화면이 아니라
런 진행 중 모든 화면 위에 늘 같은 자리로 떠 있는 것이기 때문이다.
버튼을 눌러 열리는 현재 빌드 화면은 같은 기획서 13장 소관이라 이 폴더에 넣지 않는다.

저장소를 받으면 이 `Hud` 폴더를 통째로 `Assets/Scripts/Hud`로 옮기면 된다.

## 파일 구성

```
Hud/
  Runtime/
    Config/
      HudLayoutConfig.cs      04장 고정 자리 수치. ScriptableObject
      HudVisualConfig.cs      04장 버튼 상태와 자동 저장 표시의 색. ScriptableObject
    UI/
      CurrentBuildButton.cs   현재 빌드 버튼 하나의 표시와 입력
      AutoSaveNotice.cs       자동 저장 표시
      RunHudController.cs     고정 자리 적용과 바깥 연결
  Art/
    현재빌드_버튼배경.png        400 × 400, 우주 바탕 원에 푸른 테두리
    현재빌드_버튼배경_비활성.png  400 × 400, 채도를 뺀 것
    현재빌드_아이콘.png          300 × 300, 혼천의, 알파
    현재빌드_아이콘_비활성.png    300 × 300, 채도를 뺀 것
    원본/
      우주배경.png             600 × 600, Figma 원본
      혼천의_베이지.png         1254 × 1254, Figma 원본, 알파
```

## 기획서 대응표

### 04 화면 공통 규칙 · 런 진행 중 고정되는 자리

| 기획서 | 코드 |
| --- | --- |
| 기준 1920 × 1080 | `HudLayoutConfig.ReferenceResolution` |
| 현재 빌드 버튼 200 × 200 원형 | `ButtonSize`, 원형은 배경 스프라이트가 맡는다 |
| 우측 하단 모서리에서 가로 세로 70픽셀 안쪽 | `ButtonCornerMargin`, `GetButtonPosition` |
| 자동 저장 표시 200 × 40 | `AutoSaveNoticeSize` |
| 버튼과 아래 모서리 사이 상하 15픽셀 간격 | `AutoSaveNoticeGap`, `GetAutoSaveNoticePosition` |

### 04 화면 공통 규칙 · 버튼과 카드의 상태

| 기획서 | 코드 |
| --- | --- |
| ① 기본 버튼 | `HudVisualConfig.NormalTint` |
| ② 마우스 호버, 밝기 한 단계 상승 | `HoverBrightness` |
| ③ 클릭 중, 밝기 한 단계 하락 | `PressedBrightness` |
| ③ 클릭 중, 2퍼센트 아래로 이동 | `PressedOffsetRatio`, `CurrentBuildButton` 의 Content 이동 |
| ④ 비활성, 채도 제거 | 채도를 뺀 스프라이트로 바꾼다 |
| ④ 비활성, 투명도 40퍼센트 | `DisabledAlpha` |
| ④ 비활성 이유 작성 | `CurrentBuildButton.DisabledReason`, `HoverChanged` 이벤트 |

현재 빌드 화면이 열려 있는 동안에는 비활성 표시 대신 버튼을 아예 감춘다.
`RunHudController.SetCurrentBuildOpen` 이 그 처리를 한다.
다른 이유로 막을 때는 `SetCurrentBuildAvailable` 을 쓰면 감추지 않고 이유를 보여 준다.

### 04 화면 공통 규칙 · 레이어 순서

| 기획서 | 코드 |
| --- | --- |
| 4 현재 빌드 버튼과 그 아래 자동 저장 표시 자리 | `RunHudController` 가 묶어 둔 두 개 |
| 5 오버레이의 자동 저장 표시 | `AutoSaveNotice`. 자리는 4번 영역이고 그리는 것은 오버레이다 |

### 06 오버레이 목록 · 13 현재 빌드 화면

| 기획서 | 코드 |
| --- | --- |
| 자동 저장 직후 저장 중 문구 출력 | `RunHudController.NotifyAutoSaved` |
| 화면 우하단의 현재 빌드 버튼을 통해 연다 | `RunHudController.CurrentBuildRequested` 이벤트 |

## 에셋

Figma 와이어프레임의 `04 배치 공통 규칙 · 고정 배치` 프레임에서 원본 두 장을 받아
`Art/원본`에 그대로 두고, 유니티에서 쓸 네 장을 합성했다.

- 버튼 배경은 우주 그림을 원으로 자르고 `#4D8FCC` 테두리를 둘렀다.
  테두리 두께는 와이어프레임에서 재 온 6픽셀이다.
- 아이콘은 혼천의를 줄인 것이다.
- 비활성용은 같은 그림의 채도를 뺀 것이다.

기획서 크기는 버튼 200 × 200, 아이콘 150 × 150 이지만 에셋은 그 두 배로 만들었다.
1920 × 1080 보다 큰 해상도에서 흐려지지 않게 하기 위해서이고,
화면에 나오는 크기는 `HudLayoutConfig` 의 값으로 코드가 맞춘다.

유니티 임포트 설정은 맵 아이콘과 같게 맞춘다.

- Texture Type: Sprite (2D and UI)
- Alpha Is Transparency 켜기
- Filter Mode: Bilinear, Compression: None 또는 High Quality

버튼을 원 모양 그대로만 누르게 하려면 배경 스프라이트의 Read/Write Enabled 를 켜고
배경 `Image` 의 `alphaHitTestMinimumThreshold` 를 0.5 로 준다.
켜지 않으면 200 × 200 사각형 전체가 눌린다.

## 붙이는 법

### 1. 설정 에셋을 만든다

프로젝트 창에서 우클릭 후 `Create > Slot Hero > 화면 공통`에서 두 개를 만든다.
기본값이 기획서 그대로라 손대지 않아도 된다.

- 고정 자리 설정 (`HudLayoutConfig`)
- 고정 자리 표시 설정 (`HudVisualConfig`)

### 2. 씬을 만든다

```
RunHud                       RunHudController
  CurrentBuildButton         CurrentBuildButton + Button, 200 × 200
    Content                  빈 RectTransform. 누를 때 이것만 내려간다
      배경                    Image · 현재빌드_버튼배경
      아이콘                  Image · 현재빌드_아이콘, 150 × 150
  AutoSaveNotice             AutoSaveNotice + CanvasGroup, 200 × 40
    라벨                     TMP_Text, 가운데 정렬
```

`Button` 의 Target Graphic 은 Content 아래의 배경 `Image` 로 잡는다.
버튼의 Transition 은 코드가 직접 None 으로 되돌리므로 그대로 둬도 된다.

Canvas 는 Canvas Scaler 를 Scale With Screen Size, 기준 해상도 1920 × 1080 으로 맞춘다.
04장 레이어 순서에 따라 이 묶음은 상단 표시줄보다 위, 팝업보다 아래에 둔다.

자리는 `RunHudController` 를 우클릭해 `기획서 자리로 맞추기` 를 누르면 바로 잡힌다.
게임을 켤 때도 `Awake` 에서 같은 계산이 한 번 돈다.

### 3. 호출

```csharp
// 버튼을 눌렀을 때. 현재 빌드 화면은 아직 없으므로 다음 작업에서 잇는다
hud.CurrentBuildRequested += () => currentBuildScreen.Open();

// 현재 빌드 화면이 열리고 닫힐 때. 열려 있는 동안 버튼을 막고 감춘다
hud.SetCurrentBuildOpen(true);

// 저장 시스템이 자동 저장을 마쳤을 때
hud.NotifyAutoSaved();

// 설정 화면의 자동 저장 알림 항목
hud.SetAutoSaveNoticeEnabled(false);

// 버튼에 마우스가 올라갔을 때. 비활성이면 이유가 함께 온다
hud.CurrentBuildHoverChanged += (hovering, reason) => tooltip.Set(hovering, reason);
```

## 검증 결과

유니티 없이 순수 로직만 따로 빌드해 27건을 확인했고 전부 통과했다.

- 기획서 수치 그대로 나온다. 버튼 200 × 200, 모서리 여백 70, 아이콘 150,
  자동 저장 표시 200 × 40, 간격 15
- 기준점과 피벗을 화면 우측 하단에 둔 좌표를 Figma 기준으로 되돌리면
  버튼이 x 1650, y 810 으로 와이어프레임 실측과 같다
- 버튼 아래 간격과 화면 아래 간격이 모두 15 로 떨어진다
- 버튼이 화면 가장자리 40픽셀 안전 영역 안에 들어간다
- 호버는 10퍼센트 밝아지고 누르면 10퍼센트 어두워지며,
  비활성은 호버보다 우선하고 투명도가 40퍼센트가 된다
- 200 크기 버튼은 누를 때 4픽셀 아래로 내려간다
- 자동 저장 표시가 0.5초 떠 있다가 0.5초 동안 사라져 전체 1초가 된다

## 기획서에 없어 내가 정한 부분

확인이 필요하면 알려 주면 고친다.

1. **밝기 한 단계의 폭** 물어서 10퍼센트로 정했다.
   호버는 10퍼센트 밝게, 누름은 10퍼센트 어둡게다.
2. **채도 제거 방법** 셰이더를 새로 쓰지 않고 채도를 뺀 스프라이트로 바꾼다.
   비활성 스프라이트를 비워 두면 투명도만 40퍼센트로 낮아진다.
3. **버튼이 비활성되는 때** 물어서 정했다.
   현재 빌드 화면이 열려 있는 동안 막고 화면에서도 감춘다.
4. **누를 때 내려가는 범위** 버튼 루트가 아니라 안쪽 Content 만 내려간다.
   루트를 움직이면 고정 자리 계산과 어긋나기 때문이다.
5. **자동 저장 문구** 물어서 "자동 저장중..." 으로 정했다.
6. **자동 저장 표시가 떠 있는 시간** 물어서 정했다.
   0.5초 그대로 두고 0.5초 동안 흐려져 전체 1초다.
   연출 속도 설정이나 일시 정지와 무관해야 하므로 실제 시간으로 센다.
7. **에셋을 두 배 크기로 만든 것** 표시 크기는 코드가 기획서 값으로 맞춘다.
8. **자동 저장 표시의 세로 자리** 기획서 문구대로 계산하면 위에서 1025 인데
   와이어프레임 실측은 1026 이다. 1픽셀 차이라 기획서 문구를 따랐다.

## 이 코드에 없는 것

기획서에서 다른 문서나 다음 작업으로 넘긴 부분이라 손대지 않았다.

- 현재 빌드 화면 (인게임 화면 기획서 13장). 버튼을 눌렀다는 신호만 보낸다
- 상단 표시줄 (상단 UI 바 기획서). `TopBar` 폴더에 따로 있다
- 저장을 실제로 하는 일과 저장 시점 (저장 시스템 기획서). 저장이 끝났다는 신호를 받기만 한다
- 설정 화면의 자동 저장 알림 항목 (인게임 화면 기획서 08장). 값을 넣어 주는 창구만 열어 뒀다
- 호버 툴팁을 실제로 그리는 것. 올라갔다는 것과 비활성 이유만 이벤트로 알린다
- 배경과 본문 레이어
