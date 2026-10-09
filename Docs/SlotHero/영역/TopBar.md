# Slot Hero 상단 표시줄 코드

상단 UI 바 기획서 v0.2 를 그대로 옮긴 코드다.
화면 상단 1920 × 64 자리에 런 요약 넷과 버튼 셋을 놓는다.

현재 빌드 버튼은 같은 고정 자리지만 기획서가 다르므로 `Hud` 폴더에 따로 있다.
저장소를 받으면 이 `TopBar` 폴더를 통째로 `Assets/Scripts/TopBar`로 옮기면 된다.

## 파일 구성

```
TopBar/
  Runtime/
    Data/
      TopBarButtonKind.cs     지도 · 설정 · 런 종료
      PlayTimeFormat.cs       분과 초로만 적는 시간 표기
    Config/
      TopBarLayoutConfig.cs   표시줄 규격과 각 항목의 자리. ScriptableObject
      TopBarVisualConfig.cs   색, 글자, 갱신 연출 수치. ScriptableObject
    UI/
      TopBarField.cs          아이콘 하나와 숫자 하나로 된 한 칸
      TopBarHoverArea.cs      누를 수는 없고 마우스만 받는 영역
      TopBarButton.cs         우측 버튼 하나
      TopBarTooltip.cs        마우스 호버 팝업
      TopBarView.cs           표시줄 자리 잡기와 표시
      TopBarController.cs     값 갱신과 상호작용
  Art/
    스테이지_깃발.png          43 × 54
    아이콘_체력.png            52 × 46
    아이콘_골드.png            41 × 37
    버튼_지도.png              80 × 58
    버튼_설정.png              60 × 60
    버튼_런종료.png            56 × 56
```

방 아이콘은 여기에 두지 않는다. 맵 노드 아이콘을 그대로 쓴다.
예시 이미지의 악마 얼굴과 향로도 `Map/Art` 의 `노드_일반`, `노드_성소` 와 같은 그림이다.

## 기획서 대응표

### 03 표시줄 규격

| 기획서 | 코드 |
| --- | --- |
| 기준 해상도 1920 × 1080 | `TopBarLayoutConfig.ReferenceResolution` |
| 표시줄 1920 × 64, 좌우 여백 없음 | `BarHeight`, `TopBarView.ApplyBar` 가 가로로 늘린다 |
| 아이콘 60 × 60 이내, 상하 2픽셀 이상 여유 | `IconMaxSize`, `IconVerticalPadding`, `GetIconSize` |
| 배경 단색 | `TopBarVisualConfig.BackgroundColor` |

### 04 표시 항목

| 기획서 | 코드 |
| --- | --- |
| ① 플레이 시간 | `PlayTimeX`, `TopBarView.SetPlayTime` |
| ② 위치 · 스테이지 아이콘 + 단계 + 방 아이콘 | `LocationX`, `TopBarView.SetLocation` |
| ③ 체력 · 현재와 최대를 한 칸에 | `HealthX`, `TopBarController.SetHealth` |
| ④ 골드 | `GoldX`, `TopBarController.SetGold` |
| ⑤ ~ ⑦ 버튼 | `TopBarButton`, `GetButtonPosition` |
| 아이콘 60 × 60, 누르는 영역 64 × 64 | `ButtonHitSize` 와 `TopBarButton.ApplySize` |
| 분, 초 단위만 표기하고 60분 이상도 시간 단위를 쓰지 않음 | `PlayTimeFormat.Format` |
| 골드를 노란색으로 표기 | `TopBarVisualConfig.GoldColor` |

### 05 정보 갱신 시점

| 기획서 | 코드 |
| --- | --- |
| 플레이 시간 · 매 초마다 연출 없이 숫자만 | `TopBarController.RefreshPlayTime` 이 초가 바뀔 때만 다시 적는다 |
| 위치 · 새로운 방 진입 시 즉시 | `SetLocation` |
| 체력 · 즉시 값 변경 및 짧은 시간 색상 변경 | `SetHealth` 와 `TopBarField.Flash` |
| 골드 · 짧은 시간 변화 연출 후 최종 값 | `SetGold` 와 `UpdateGold` 의 숫자 흘리기 |

### 06 상호작용

| 기획서 | 코드 |
| --- | --- |
| 위치 · 호버 시 스테이지 이름과 방 종류 | `TopBarHoverArea`, `SetLocation` 의 tooltipText |
| 지도 · 맵 화면을 연다 | `ButtonClicked` 이벤트의 `TopBarButtonKind.Map` |
| 설정 · 설정 화면을 연다 | 같은 이벤트의 `Settings` |
| 런 종료 · 런 종료 확인 팝업을 띄운다 | 같은 이벤트의 `EndRun` |
| 버튼 호버 시 텍스트 팝업 | `TopBarButton.TooltipText`, `TopBarTooltip` |
| 각 버튼의 비활성 조건 | `SetButtonInteractable` 로 바깥에서 정한다 |

## 예시 이미지에서 잰 값

기획서가 수치를 적지 않은 자리와 색은 `상단 표시줄 예시2.png` 에서 재 왔다.
`.mdp` 파일은 메디방 페인트 전용 형식이라 레이어를 열 수 없어
같은 그림인 PNG 에서 아이콘을 잘라냈다.

| 항목 | 예시에서 잰 값 | 설정 기본값 |
| --- | --- | --- |
| ① 플레이 시간 | x 29 ~ 182, 숫자 높이 48 | `PlayTimeX` 30 |
| ② 위치 | x 272 ~ 399, 깃발 39 × 50, 숫자 높이 29, 방 아이콘 40 × 46 | `LocationX` 270 |
| ③ 체력 | x 836 ~ 969, 하트 48 × 42, 숫자 높이 32 | `HealthX` 840 |
| ④ 골드 | x 1279 ~ 1381, 코인 37 × 33, 숫자 높이 26 | `GoldX` 1280 |

네 자리는 예시 실측을 10 단위로 옮긴 값이다. 실측은 29 · 272 · 836 · 1279 였다.
| ⑤ 지도 | x 1695 ~ 1770, 중심 1732.5 | 중심 1736 |
| ⑥ 설정 | x 1788 ~ 1843, 중심 1815.5 | 중심 1808 |
| ⑦ 런 종료 | x 1860 ~ 1911, 중심 1885.5 | 중심 1880 |

색은 다음과 같다.

| 자리 | 색 |
| --- | --- |
| 배경 | `#BFBFBF` |
| 플레이 시간 | `#FFFFFF` 에 검은 외곽선 |
| 단계 숫자 | `#FFFFFF` |
| 체력 | `#FF0000` |
| 골드 | `#F7BD11` |

글자 크기는 기획서가 포인트로 적어 화면 픽셀과 바로 맞지 않는다.
예시에서 잰 숫자 높이로 되짚은 값을 기본으로 넣어 뒀다.

| 항목 | 기획서 | 예시 숫자 높이 | 설정 기본값 |
| --- | --- | --- | --- |
| 플레이 시간 | 12pt | 48 | 66 |
| 위치 | 7pt | 29 | 40 |
| 체력 | 6pt | 32 | 44 |
| 골드 | 6pt | 26 | 36 |

폰트마다 숫자 높이가 다르므로 실제 폰트를 넣은 뒤 다시 맞춘다.

## 붙이는 법

### 1. 설정 에셋을 만든다

프로젝트 창에서 우클릭 후 `Create > Slot Hero > 상단 표시줄`에서 두 개를 만든다.

- 표시줄 자리 설정 (`TopBarLayoutConfig`)
- 표시줄 표시 설정 (`TopBarVisualConfig`)

### 2. 아이콘을 넣는다

유니티 임포트 설정은 맵 아이콘과 같게 맞춘다.

- Texture Type: Sprite (2D and UI)
- Alpha Is Transparency 켜기
- Filter Mode: Bilinear, Compression: None 또는 High Quality

`버튼_지도.png` 는 가로가 세로보다 길다.
버튼 아이콘 `Image` 의 Preserve Aspect 는 코드가 켜 주므로 60 × 60 안에 알아서 들어간다.

### 3. 씬을 만든다

```
TopBar                     TopBarView + TopBarController, Image(배경)
  PlayTime                 TMP_Text
  Location                 TopBarHoverArea + Image(투명, Raycast Target 켬), 가로 배치
    스테이지아이콘           Image
    단계                    TMP_Text
    방아이콘                 Image
  Health                   TopBarField, 가로 배치
    아이콘                   Image
    숫자                    TMP_Text
  Gold                     TopBarField, 가로 배치
    아이콘                   Image
    숫자                    TMP_Text
  MapButton                TopBarButton + Button, 자식에 아이콘 Image
  SettingsButton           TopBarButton + Button, 자식에 아이콘 Image
  EndRunButton             TopBarButton + Button, 자식에 아이콘 Image
Tooltip                    TopBarTooltip
  Root                     피벗 (0.5, 1), Content Size Fitter
    배경                    Image
    글자                    TMP_Text
```

세 버튼은 `TopBarView` 의 버튼 목록에 넣고 각자의 `Kind` 를 맞춘다.
팝업은 표시줄 밖에 두어야 표시줄 아래로 나올 수 있으므로 캔버스 바로 아래에 둔다.

칸 안의 아이콘과 숫자 사이 간격은 Horizontal Layout Group 으로 잡는다.
코드는 칸 전체의 자리와 아이콘 크기만 맞추고 안쪽 간격은 건드리지 않는다.

자리는 `TopBarView` 를 우클릭해 `기획서 자리로 맞추기` 를 누르면 바로 잡힌다.

### 4. 호출

```csharp
// 플레이 시간. 표시줄은 시간을 재지 않는다. 흐름의 RunClock 값을 매 프레임 넘긴다
// 초가 바뀔 때만 글자를 고치므로 매 프레임 불러도 된다
topBar.SetElapsedSeconds(clock.Seconds);

// 런 시작
topBar.SetHealth(99, 99, animate: false);
topBar.SetGold(0, animate: false);

// 방에 들어갈 때. 방 아이콘은 맵 화면 표시 설정의 것을 그대로 넘긴다
topBar.SetLocation(
    stageIcon,
    node.Stage,
    mapVisual.GetVisual(node.RoomType).Icon,
    "제국 · 성소");

// 값이 바뀔 때. 성소 코드의 골드를 그대로 이을 수 있다
gold.Changed += (previous, current) => topBar.SetGold(current, animate: true);

// 버튼
topBar.ButtonClicked += kind =>
{
    switch (kind)
    {
        case TopBarButtonKind.Map: mapScreen.Open(); break;
        case TopBarButtonKind.Settings: settingsScreen.Open(); break;
        case TopBarButtonKind.EndRun: endRunPopup.Open(); break;
    }
};

// 비활성 조건. 맵 화면은 이미 알림을 보낸다
mapScreen.ScreenVisibilityChanged += open =>
    topBar.SetButtonInteractable(TopBarButtonKind.Map, !open);

```

스테이지 아이콘과 방 아이콘은 그림을 그대로 받는다.
방 타입마다 어떤 그림을 쓸지는 `MapVisualConfig` 가 이미 들고 있으므로
이 코드가 방 타입을 알 필요가 없고 표시줄이 맵에 매이지도 않는다.

## 검증 결과

유니티 없이 순수 로직만 따로 빌드해 45건을 확인했고 전부 통과했다.

- 시간 표기가 기획서 예시 123:45 와 화면 예시 59:40 대로 나오고,
  60분과 10시간을 넘겨도 시간 단위로 접지 않는다
- 표시줄 64, 아이콘 60, 상하 여유 2, 누르는 영역 64 가 기획서 그대로다
- 런 요약 네 칸의 x 가 예시 실측과 같고 왼쪽에서 순서대로 놓인다
- 버튼 세 개의 중심이 예시와 8픽셀 안에서 맞고 서로 겹치지 않으며 화면 안에 들어간다
- 골드 칸과 가장 왼쪽 버튼 사이에 자리가 남는다
- 배경, 체력, 골드, 글자 색이 예시에서 뽑은 값과 같다
- 체력 표기가 48/99, 골드 표기가 137 과 1,234 로 나온다
- 버튼은 호버에 10퍼센트 밝아지고 누르면 10퍼센트 어두워지며 비활성 투명도가 40퍼센트가 된다
- 체력이 늘 때는 초록, 줄 때는 빨강으로 잠시 바뀌고 골드는 0.3초 동안 흘러간다

## 기획서에 없어 내가 정한 부분

확인이 필요하면 알려 주면 고친다.

1. **배경색** 기획서는 단색이라고만 하고 색값과 불투명도를 미정으로 뒀다.
   예시의 `#BFBFBF` 를 넣어 뒀다.
2. **글자 크기** 기획서의 포인트 값이 화면 픽셀과 맞지 않아 예시에서 되짚었다.
   기획서의 12 : 7 : 6 : 6 비율은 예시의 48 : 29 : 32 : 26 과 대체로 맞고 체력만 예시가 크다.
3. **버튼 사이 간격** 예시는 중심 간격이 83 과 70 으로 들쭉날쭉해서
   기획서의 누르는 영역 64 를 기준으로 간격 10, 우측 여백 10 으로 고르게 폈다.
   결과는 예시 중심과 12픽셀 안에서 맞는다.
   **누르는 영역 64 가 기획서 확정값이라 버튼 자리 자체는 10 으로 떨어지지 않는다.**
   간격과 여백만 10 단위로 두었다.
4. **분 자리 채우기** 물어서 정했다. 런을 시작할 때 0:00 으로 보인다.
   두 자리로 맞추려면 `PadPlayTimeMinutes` 를 켠다.
5. **체력 색 변경** 물어서 정했다. 늘 때는 초록, 줄 때는 빨강이다.
   기본 체력 글자가 이미 `#FF0000` 이라 줄 때의 강조는 그보다 밝은 빨강을 쓴다.
   같은 빨강으로 두면 바뀌는 것이 보이지 않기 때문이다.
6. **골드 변화 연출** "짧은 시간 동안 변화 연출"을 숫자가 흘러가는 방식으로 했다.
   시간은 물어서 0.3초로 정했다.
7. **버튼 상태 표현** 상단 UI 바 기획서에는 없어서
   인게임 화면 기획서 04 화면 공통 규칙 의 버튼 상태를 가져왔다.
   밝기 한 단계는 물어서 10퍼센트로 정했고,
   아이콘이 작아 누를 때 내려가는 연출은 넣지 않는다.
8. **호버 팝업의 생김새** 기획서가 정하지 않아 글자와 배경만 둔 최소 구성이다.
   화면 가장자리를 벗어나지 않게 하는 처리는
   인게임 화면 기획서 13장 아이템 상세 팝업 의 규칙을 가져왔다.
9. **비활성 버튼의 팝업** 눌리지 않더라도 이름 팝업은 띄운다.
10. **아이콘을 잘라낸 것** `.mdp` 를 열 수 없어 같은 그림인 PNG 에서 잘라내고
    배경만 투명으로 만들었다. 원본이 작아 크게 키우면 흐려진다.
    더 큰 원본이 있으면 같은 이름으로 덮어쓰면 코드는 그대로 둬도 된다.
## 방 아이콘

맵 노드 아이콘 다섯 가지를 그대로 쓴다.
`MapVisualConfig` 에 방 타입마다의 그림이 이미 들어 있으므로
`SetLocation` 에 그 그림을 넘기면 된다.

표시줄 배경이 회색이고 노드 아이콘은 검은 실루엣이라 대비는 충분하다.
아이콘 자리가 60 × 60 이고 노드 아이콘 원본이 64 × 64 라 크기도 맞는다.

## 호버 풍선

06 상호작용 의 텍스트 팝업이다. 버튼과 위치 칸에 마우스를 올리면 그 아래에 뜬다.

**풍선은 커서를 받지 않는다.** `TopBarTooltip.ApplyStyle` 이 배경과 글자의
`raycastTarget` 을 끈다. 켜져 있으면 버튼 바로 아래에서 커서를 가로채
버튼이 벗어난 것으로 읽히고, 풍선이 사라지면 다시 올라간 것이 되어 또 뜬다.
그렇게 점멸하면서 그 사이에 누른 클릭이 버튼에 닿지 않는다.

**크기는 글에 맞춰 코드가 잡는다.** `Resize` 가 글자 크기에 `TooltipPadding` 을 더한다.
안 잡으면 유니티가 새 칸에 넣어 주는 100 × 100 이 남아 글과 상관없는 칸이 버튼을 덮는다.
좌표 배율이 12 였을 때 실제로 표시줄 전체를 덮었다.

껍데기와 켜고 끄는 칸을 나눠 둔다. `Tooltip` 은 늘 켜져 있고 그 아래 `Root` 만 켜고 끈다.
게임 씬을 만들 때 `BuildGameScene.Shells` 에 `Tooltip` 이 들어 있어야
`CloseAll` 이 껍데기까지 끄지 않는다.

기준점은 부모 한가운데, 피벗은 위쪽 가운데다.
`Place` 가 `InverseTransformPoint` 로 자리를 구하는데 그 값이 부모 한가운데를 0 으로 보기 때문이다.

## 생존 자원

체력이다. 2026년 9월 30일에 기획서 본문의 남은 "칩"을 체력으로 고쳤다.
03 구성요소 요약 의 런 요약 넷과 05 정보 갱신 시점 의 표 두 곳이었다.
수정 이력의 v0.2 "칩 > 체력 명칭 변경" 은 기록이라 그대로 뒀다.

전투에서 거는 **코인(칩)** 은 이것과 다른 것이다. 상단 표시줄은 코인을 다루지 않는다.

## 이 코드에 없는 것

기획서에서 다른 문서로 넘긴 부분이라 손대지 않았다.

- 세 버튼이 여는 맵 화면, 설정 화면, 런 종료 확인 팝업
- 체력과 골드의 출처. 바뀐 값을 받기만 한다
- 지금 어느 방에 있는지 판단하는 일. 그림과 단계를 받기만 한다
- 플레이 시간을 런 데이터에 저장하는 일. 값을 읽어 갈 창구만 열어 뒀다
- 현재 빌드 버튼과 자동 저장 표시 (`Hud` 폴더)

