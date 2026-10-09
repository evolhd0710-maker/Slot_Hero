# Slot Hero 타이틀 화면 코드

인게임 화면 기획서 v0.2 / 05 타이틀 화면 을 옮긴 코드다.

로고와 세로 메뉴, 버전을 놓고 저장된 런 상태에 따라 메뉴를 켜고 끈다.

화면 오른쪽 위의 현재 프로필 버튼은 `Profile` 폴더의 `CurrentProfileButton` 이 맡는다.
프로필 선택 화면과 짝을 이루는 부품이라 그쪽에 두었다.

저장소를 받으면 이 `Title` 폴더를 통째로 `Assets/Scripts/Title`로 옮기면 된다.

## 파일 구성

```
Title/
  Runtime/
    Data/
      TitleMenuKind.cs             메뉴 항목의 종류 넷
      SavedRunState.cs             저장된 런의 상태와 그에 따른 갈림길
    Config/
      TitleLayoutConfig.cs         화면 자리와 크기. ScriptableObject
      TitleVisualConfig.cs         메뉴 목록과 색, 글자, 문구. ScriptableObject
    UI/
      TitleMenuItemView.cs         메뉴 항목 하나
      TitleScreenView.cs           화면 배치와 표시
      TitleScreenController.cs     메뉴 누름과 저장된 런 상태 처리
```

## 기획서 대응표

| 기획서 | 코드 |
| --- | --- |
| 로고 · 화면 왼쪽 위에 배치한다 | `LogoPosition` |
| 화면 왼쪽에 세로로 메뉴를 배치한다 | `MenuPosition`, `GetMenuItemPosition` |
| 항목이 추가될 경우 간격을 조정하여 배치한다 | `GetMenuSpacing` 이 항목 수에 따라 줄인다 |
| 새 게임 · 새 런을 시작한다 | `NewGameStarted` 이벤트 |
| 새 게임 · 저장된 런이 있을 경우 확인 팝업을 띄운다 | `NewGameConfirmRequested` 이벤트 |
| 이어하기 · 저장된 자리의 런 진행 화면으로 진입한다 | `ContinueRequested` 이벤트 |
| 설정 · 설정 화면으로 이동한다 | `SettingsRequested` 이벤트 |
| 종료 · 게임 종료 확인 팝업을 거쳐 종료한다 | `QuitConfirmRequested` 이벤트 |
| 버전 · 빌드 번호를 표시한다 | `SetVersion`, `VersionFormat` |
| 저장된 런이 없을 경우 이어하기 비활성 | `SavedRunStates.CanContinue` |
| 저장된 런이 없을 경우 새 게임은 확인 없이 즉시 시작 | `SavedRunStates.NeedsNewGameConfirm` |
| 런 데이터가 손상되면 알림을 출력하고 비활성으로 변경 | `RunDataBrokenNotice`, `SavedRunState.Broken` |

## 저장된 런 상태로 갈리는 곳

| 상태 | 이어하기 | 새 게임 | 화면을 열 때 |
| --- | --- | --- | --- |
| 저장된 런 있음 | 누를 수 있다 | 확인 팝업을 거친다 | - |
| 저장된 런 없음 | 비활성 | 바로 시작한다 | - |
| 런 데이터 손상 | 비활성 | 확인 팝업을 거친다 | 손상 알림을 띄운다 |
| 런이나 메타를 지금 읽지 못함 | 비활성 | 확인 팝업을 거친다 | 띄우지 않는다. 이어하기 비활성 까닭에 다시 시도를 적는다(SavedRunState.Unreadable, 2026년 10월 9일). 흐름이 기다렸다 다시 읽고, 이어하기를 눌렀다가 못 읽었으면 읽히는 대로 이어 한다 |

**손상된 런도 저장된 런이라 새 게임이 확인을 거친다.**
기획서가 "저장된 런이 있을 경우 새 게임 확인 팝업을 띄운다"로 정했고
손상되었더라도 지우는 것은 되돌릴 수 없기 때문이다.
확인 없이 바로 시작하는 것은 저장된 런이 아예 없을 때뿐이다.

이어할 수 있는지와 확인을 거치는지는 따로 간다.
손상된 런은 이어하지 못하지만 새 게임은 묻는다.

**검사를 통과하지 못한 런 파일은 프로필을 읽는 순간 지우고 팝업을 띄운다.**
2026년 10월 7일 원재가 정했다. 플레이어의 뜻과 상관없이 지우므로 버튼은 확인 하나다.
제목은 "런 데이터 손상" 이고 띄우는 쪽은 `GameFlowController.HandleSaveNotice` 다.
지운 뒤에는 저장된 런 없음 상태로 타이틀이 열린다.
위 표의 "런 데이터 손상" 상태는 손상된 런 파일을 지우지 못해 남았을 때만 생긴다. 파일이 잠겨 있을 때처럼 드문 경우를 대비해 둔다.
예전에는 지금 게임보다 새 판으로 쓴 런 파일일 때도 이 상태였는데, 2026년 10월 8일부터 판 번호와 상관없이 읽으므로 그 길은 없다.

## 자리와 크기

와이어프레임에서 잰 값을 10 단위로 옮긴 것이다. 괄호 안이 실측값이다.

| 항목 | 자리와 크기 |
| --- | --- |
| 로고 | x 170, y 220, 470 × 70 (172 · 218 · 470 × 66) |
| 메뉴 | x 170, y 600부터 110 간격, 400 × 50 (176 · 595 · 간격 약 114) |
| 버전 | x 1540, y 990, 100 × 30 (1537 · 996) |

로고와 메뉴의 왼쪽 끝이 170으로 같다.
메뉴 네 항목이 y 600 · 710 · 820 · 930에 놓인다.

메뉴 글자는 04 화면 공통 규칙 의 화면 주요 텍스트 40픽셀 굵게다. 와이어프레임 실측도 같다.
버전은 표에 없어 보조 텍스트 20픽셀로 두었다. 와이어프레임은 이보다 작게 그렸다.

## 항목이 늘면 간격이 줄어든다

기획서가 "항목이 추가될 경우 간격을 조정하여 배치한다"로 정했다.
기본 간격 110으로 넣어 화면을 넘치지 않으면 그대로 쓰고, 넘치면 줄인다.
줄인 간격도 10 단위가 되게 내림으로 맞춘다.

메뉴가 쓸 수 있는 세로는 첫 항목 위 600부터 안전 영역 안쪽 끝 1040까지에서
항목 높이 50을 뺀 390이다.

| 항목 수 | 간격 |
| --- | --- |
| 4 (기획서가 정한 수) | 110 |
| 5 | 90 |
| 6 | 70 |
| 7 | 60 |

`MinMenuSpacing` 60 아래로는 줄이지 않는다. 그보다 많으면 화면을 넘치고 경고가 찍힌다.

## 메뉴 차례를 설정에 둔 까닭

**타이틀 메뉴 순서가 아직 확정이 아니다.**
참고 이미지와 새 기획서가 새 게임과 이어하기의 차례를 다르게 적었다는 기록이 있다.

그래서 차례를 코드에 박지 않고 `TitleVisualConfig.MenuEntries` 목록으로 두었다.
목록의 차례가 곧 화면의 차례이고, 항목을 더하거나 빼도 자리와 간격이 따라간다.

기본값은 새 게임 → 이어하기 → 설정 → 종료다.
기획서 본문의 나열 차례와 와이어프레임이 둘 다 이 차례이기 때문이다.

## 붙이는 법

### 1. 설정 에셋을 만든다

프로젝트 창에서 우클릭 후 `Create > Slot Hero > 타이틀`에서 두 개를 만든다.

- 타이틀 화면 자리 설정 (`TitleLayoutConfig`)
- 타이틀 화면 표시 설정 (`TitleVisualConfig`)

`Slot Hero > 설정 에셋 한 번에 만들기` 메뉴로도 만들 수 있다.

### 2. 씬을 만든다

```
TitleScreen                TitleScreenController, 화면 루트
  Background               Image · 배경 그림
  Logo                     Image · SLOT HERO 로고
  MenuLayer                빈 RectTransform. 항목이 여기 아래에 만들어진다
  VersionLabel             TMP_Text · 오른쪽 정렬
  CurrentProfileButton     Profile 폴더의 CurrentProfileButton
```

메뉴 항목 프리팹은 이렇게 둔다.

```
TitleMenuItem              TitleMenuItemView, Button
  Content                  RectTransform · 누를 때 이것만 2퍼센트 내려간다
    Label                  TMP_Text · 왼쪽 정렬
```

자리는 `TitleScreenView` 를 우클릭해 `기획서 자리로 맞추기` 를 누르면 잡힌다.

### 3. 호출

```csharp
// 저장된 런 상태를 저장 시스템에서 받아 화면을 연다
SavedRunState state = save.HasRun(profileIndex)
    ? (save.CanReadRun(profileIndex) ? SavedRunState.Ready : SavedRunState.Broken)
    : SavedRunState.None;

title.Open(state, Application.version);

// 새 게임. 저장된 런이 있으면 확인 팝업을 거친다.
// 손상된 런도 저장된 런이라 여기로 온다. 요약을 읽을 수 없으므로 본문을 달리 적는다.
title.NewGameConfirmRequested += () =>
{
    bool broken = title.SavedRun == SavedRunState.Broken;

    string body = broken
        ? "저장된 런을 불러올 수 없습니다. 새로 시작하면 그 기록이 사라집니다."
        : "진행 중인 런이 사라집니다. 되돌릴 수 없습니다.\n\n" + save.GetRunSummary(profileIndex);

    PopupSpec spec = PopupSpec.Confirm(
        "새 게임을 시작하시겠습니까?",
        body,
        "돌아가기",
        "새 게임 시작",
        confirmIsDangerous: true);

    popup.Show(spec, id =>
    {
        if (id == "confirm")
        {
            title.ConfirmNewGame();
        }
    });
};

title.NewGameStarted += () => game.StartNewRun();
title.ContinueRequested += () => game.LoadSavedRun(profileIndex);
title.SettingsRequested += () => settings.Open(savedSettingsValues);

// 종료도 확인 팝업을 거친다
title.QuitConfirmRequested += () =>
{
    PopupSpec spec = PopupSpec.Confirm("게임을 종료하시겠습니까?", string.Empty, "취소", "종료");
    popup.Show(spec, id =>
    {
        if (id == "confirm")
        {
            Application.Quit();
        }
    });
};

// 런 데이터가 손상됐으면 여는 순간 알림이 온다
title.RunDataBrokenNotice += () => popup.Show(runDataBrokenSpec, OnBrokenNoticeClosed);

// 누를 수 없는 항목에 마우스를 올리면 까닭을 알려 준다
title.MenuHoverChanged += (kind, hovering, reason) =>
{
    if (hovering && !string.IsNullOrEmpty(reason))
    {
        tooltip.Show(reason);
    }
    else
    {
        tooltip.Hide();
    }
};

// 현재 프로필 버튼은 Profile 폴더가 맡는다
currentProfileButton.Show(profiles);
currentProfileButton.Clicked += () => profileSelect.Open(profiles);

// 새 런을 시작했거나 프로필을 바꿔 돌아오면 상태를 다시 알려 준다
title.SetSavedRunState(SavedRunState.None);
```

`RunDataBrokenNotice` 는 화면을 한 번 여는 동안 한 번만 부른다.
`SetSavedRunState` 로 손상이 아닌 상태를 넣으면 다시 알릴 수 있게 풀린다.

## 그림이 아직 없다

**로고 그림도 배경 그림도 한 장도 없다.**
배경은 배경 요청 기획서가 "메인 화면 배경 · 고대 유적 슬롯"으로 발주해 둔 상태다.

그림이 없는 동안 검은 화면으로 두지 않는다.
그러면 그림이 빠진 것인지 화면이 깨진 것인지 가릴 수가 없다.
그 기획서가 정한 색을 그대로 깔아 둔다.

| 자리 | 지금 쓰는 색 | 기획서 |
| --- | --- | --- |
| 배경 | `#4A3A28` | 지구라트 건물의 어두컴컴한 딥 브라운 |
| 로고 글자 | `#E8C87A` | 석판 틈새 빛의 밝은 앰버 골드 |

그림이 오면 `코드/Title/Art` 에 `배경_메인화면.png` 와 `로고.png` 로 넣는다.
`ImportArt` 가 그 폴더를 가져오고 확인용 씬이 알아서 물린다.
코드는 손대지 않아도 된다.

같은 기획서가 1스테이지 전투 배경도 함께 발주해 두었다. 그쪽도 아직 안 왔다.

## 검증 결과

유니티 없이 순수 로직만 따로 빌드해 64건을 확인했고 전부 통과했다.

- 로고, 메뉴, 버전의 자리와 크기가 전부 10 단위로 떨어진다
- 로고와 메뉴의 왼쪽 끝이 170으로 같다
- 메뉴 네 항목이 y 600 · 710 · 820 · 930에 놓인다
- 항목이 다섯이면 간격이 90, 여섯이면 70, 일곱이면 60으로 줄고 모두 10 단위다
- 두 항목부터 열 항목까지 간격이 모두 10으로 떨어진다
- 가장 좁은 간격 60 아래로는 줄지 않고, 그보다 많으면 화면을 넘친다고 알린다
- 메뉴 차례를 뒤집으면 찾는 자리도 따라 바뀐다
- 저장된 런이 있을 때만 이어하기를 누를 수 있다
- 저장된 런이 있거나 손상됐으면 새 게임이 확인을 거치고, 아예 없을 때만 바로 시작한다
- 손상된 런은 이어하지 못하지만 새 게임은 묻는다
- 런이 손상됐을 때만 알림을 띄운다
- 누를 수 없는 항목은 투명도가 40퍼센트가 되고 마우스를 올려도 밝아지지 않는다
- 누르면 어두워지고 2퍼센트 내려간다

## 기획서에 없어 내가 정한 부분

확인이 필요하면 알려 주면 고친다.

1. **비활성 이유 문구** 04 화면 공통 규칙 이 비활성 이유를 적게 하는데
   와이어프레임에는 이유가 없다. "저장된 런이 없습니다"와
   "저장된 런을 불러올 수 없습니다"를 넣고 `MenuHoverChanged` 로 알린다.
   어디에 띄울지는 받는 쪽이 정한다.
2. **메뉴 항목의 누르는 영역** 400 × 50 으로 고정했다.
   글자 폭이 항목마다 달라 그때그때 맞추면 누르는 자리가 들쭉날쭉해진다.
3. **메뉴가 쓸 수 있는 세로** 첫 항목 위부터 안전 영역 안쪽 끝까지로 잡았다.
   04 화면 공통 규칙 의 안전 영역 40을 가져온 것이다.
4. **가장 좁은 간격** 60 으로 두었다. 글자 40픽셀이 서로 붙지 않을 만큼이다.
5. **버전 글자 크기** 04장 표에 버전이 없어 보조 텍스트 20픽셀로 두었다.
   와이어프레임 실측은 이보다 훨씬 작다.
6. **손상된 런의 확인 팝업 본문** 요약을 읽을 수 없으므로 부르는 쪽이 다른 글을 적어야 한다.
   `TitleScreenController.SavedRun` 으로 지금 상태를 알 수 있다.

## 이 코드에 없는 것

- 로고 그림과 배경 그림. 배경은 배경 요청 기획서 소관이다
- 새 게임 확인, 게임 종료 확인, 런 데이터 손상 알림 팝업. `Popup` 폴더의 공통 틀로 띄운다
- 저장된 런을 읽고 쓰는 일. 저장 시스템 기획서 소관이고 여기서는 상태만 받는다
- 빌드 번호. 부르는 쪽이 넘겨 준다
- 현재 프로필 버튼. `Profile` 폴더가 맡는다
- 비활성 이유를 띄우는 자리. `MenuHoverChanged` 를 받는 쪽이 정한다
