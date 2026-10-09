# Slot Hero 프로필 코드

인게임 화면 기획서 v0.2 / 06 프로필 선택 화면 과
05 타이틀 화면 의 현재 프로필 버튼을 옮긴 코드다.

타이틀 오른쪽 위의 현재 프로필 버튼을 누르면 프로필 선택 화면으로 들어가고,
거기서 프로필을 고르거나 만들거나 이름을 고치거나 지운다.

저장소를 받으면 이 `Profile` 폴더를 통째로 `Assets/Scripts/Profile`로 옮기면 된다.

## 파일 구성

```
Profile/
  Runtime/
    Data/
      ProfileSummary.cs                 자리 하나의 요약
      ProfileList.cs                    자리 전부와 지금 쓰는 자리
      ProfileText.cs                    카드에 적을 글을 만든다
    Config/
      ProfileLayoutConfig.cs            화면 자리와 크기. ScriptableObject
      ProfileVisualConfig.cs            색과 글자, 문구. ScriptableObject
    UI/
      CurrentProfileButton.cs           타이틀 오른쪽 위의 현재 프로필 버튼
      ProfileCardView.cs                프로필 카드 하나
      ProfileSelectScreenView.cs        화면 배치와 표시
      ProfileSelectScreenController.cs  고르기, 만들기, 이름 고치기, 지우기
```

## 기획서 대응표

### 05 타이틀 화면 · 현재 프로필

| 기획서 | 코드 |
| --- | --- |
| 현재 프로필명을 표시한다 | `ProfileText.CurrentProfile`, `CurrentProfileButton.Show` |
| 입력 시 프로필 선택 화면으로 이동한다 | `CurrentProfileButton.Clicked` 이벤트 |

### 06 프로필 선택 화면

| 기획서 | 코드 |
| --- | --- |
| 제목 · 화면 왼쪽 위에 프로필 선택 | `TitlePosition`, `TitleText` |
| 프로필 카드 · 검은색으로 프로필명 | `NameColor` |
| 프로필 카드 · 흰색으로 플레이 시간과 마지막 플레이 날짜 | `InfoColor` |
| 카드마다 다른 배경색을 사용한다 | `CardColors` 세 색 |
| 입력 시 해당 프로필로 전환한다 | `ProfileChosen` 이벤트 |
| 빈 자리 · 가운데에 비어 있음을 작성한다 | `EmptyText`, 카드 한가운데 |
| 빈 자리 · 입력 시 프로필을 생성하고 프로필명 입력 상태로 전환한다 | `BeginEditName`, `ProfileCreated` |
| 수정 · 버튼을 카드 오른쪽 위에 배치한다 | `GetActionButtonPosition` |
| 수정 · 해당 카드의 프로필명 입력 상태로 전환한다 | `ProfileRenamed` |
| 삭제 · 수정 오른쪽 | `GetActionButtonPosition(card, 0)` 이 맨 오른쪽 |
| 삭제 · 입력 시 프로필 삭제 확인 팝업을 출력한다 | `DeleteRequested` 이벤트 |
| 뒤로 · 타이틀 화면으로 이동한다. 우측 하단에 배치한다 | `BackPosition`, `BackRequested` |
| 저장 파일을 읽을 수 없는 카드 입력 시 저장 데이터 오류 팝업 | `BrokenProfileOpened` 이벤트 |

## 자리와 크기

와이어프레임에서 잰 값을 10 단위로 옮긴 것이다. 괄호 안이 실측값이다.

| 항목 | 자리와 크기 |
| --- | --- |
| 제목 | x 120, y 80, 770 × 100 (115 · 76 · 768 × 97) |
| 카드 | x 120부터 580 간격, y 420, 520 × 260 (115 · 416 · 518 × 260) |
| 뒤로 | x 1510, y 920, 290 × 90 (1517 · 918 · 288 × 86) |
| 현재 프로필 버튼 | x 1360, y 50, 440 × 100 (1363 · 54 · 442 × 97) |

자리가 잘 맞물린다.

- 카드 셋과 좌우 여백을 더하면 120 + 3 × 520 + 2 × 60 + 120 이 되어 화면 폭 1920 이 딱 떨어진다
- 카드 셋의 오른쪽 끝이 1800이고 좌우 여백이 둘 다 120이다
- 뒤로 버튼과 현재 프로필 버튼의 오른쪽 끝도 1800이다
- 현재 프로필 버튼의 크기 440 × 100은 런 종료 결과의 타이틀로 버튼과 같다

카드 안쪽은 이렇다.

| 항목 | 자리 |
| --- | --- |
| 좌우 여백 | 20 |
| 프로필명 | 카드 위에서 70, 높이 40 |
| 플레이 시간 | 카드 위에서 130, 높이 30 |
| 마지막 플레이 | 그 아래 40 간격 |
| 수정 · 삭제 | 카드 오른쪽 위에서 20 안쪽, 60 × 60, 사이 간격 10 |

첫 카드로 재면 프로필명이 x 140 · y 490, 플레이 시간이 y 550, 마지막 플레이가 y 590이다.
수정이 x 490, 삭제가 x 560이고 둘 다 y 440이다.

색은 다음과 같다.

| 자리 | 색 |
| --- | --- |
| 첫 카드 | `#C0392B` |
| 둘째 카드 | `#27853C` |
| 셋째 카드 | `#2E5FBF` |
| 제목 칸과 뒤로 | `#D9D9D9` |
| 프로필명 | `#1F1B16` |
| 플레이 정보와 비어 있음 | 흰색 |
| 삭제 글자 | `#9E2B25` |

글자 크기는 04 화면 공통 규칙 의 텍스트 표를 따른다.
제목 40픽셀 굵게, 프로필명 32픽셀 굵게, 플레이 정보 24픽셀,
비어 있음 30픽셀 굵게, 수정과 삭제 20픽셀, 뒤로 30픽셀 굵게다.
현재 프로필만 표에 없어 와이어프레임에서 잰 26픽셀 굵게로 두었다.

04장 표가 프로필 카드를 그대로 다룬다.
"프로필 카드의 비어 있음 색상 2"와 "프로필 카드의 플레이 정보 색상 2"가 둘 다 흰색이고
와이어프레임과 맞는다.

## 카드 하나가 세 모습을 맡는다

`ProfileCardView` 하나가 채워진 자리, 빈 자리, 읽을 수 없는 자리를 모두 그린다.

- **채워진 자리** 프로필명과 플레이 정보 두 줄, 수정과 삭제 버튼
- **빈 자리** 가운데에 "비어 있음"만. 고칠 것도 지울 것도 없으니 버튼을 감춘다
- **읽을 수 없는 자리** 채도를 빼고 플레이 정보 자리에 "불러올 수 없음"을 적는다.
  손상이 아니라 지금 잠시 읽지 못한 자리도 같이 그리되(ProfileSummary.IsReadFailed), 누르면 삭제를 권하지 않고 기다렸다 다시 읽어 들어간다(2026년 10월 9일. Save README 의 "기다렸다 다시 읽는다")

마우스를 올리면 04 화면 공통 규칙 대로 5퍼센트 커진다.
커지는 것은 카드 안의 `Content` 뿐이라 카드의 자리 계산과 어긋나지 않는다.

## 붙이는 법

### 1. 설정 에셋을 만든다

프로젝트 창에서 우클릭 후 `Create > Slot Hero > 프로필`에서 두 개를 만든다.

- 프로필 화면 자리 설정 (`ProfileLayoutConfig`)
- 프로필 화면 표시 설정 (`ProfileVisualConfig`)

`Slot Hero > 설정 에셋 한 번에 만들기` 메뉴로도 만들 수 있다.

### 2. 씬을 만든다

타이틀 화면 쪽에는 버튼 하나만 둔다.

```
CurrentProfileButton       CurrentProfileButton, Button
  Fill                     Image · 안쪽 바탕. 기본은 투명이다
  Border                   Image · 테두리만 그리는 스프라이트
  Label                    TMP_Text · 왼쪽 정렬
```

프로필 선택 화면은 이렇게 둔다.

```
ProfileSelectScreen        ProfileSelectScreenController, 화면 루트
  Background               Image
  TitlePanel               Image
    TitleLabel             TMP_Text
  CardLayer                빈 RectTransform. 카드가 여기 아래에 만들어진다
  BackPanel                Image + Button
    BackLabel              TMP_Text
```

카드 프리팹은 이렇게 둔다.

```
ProfileCard                ProfileCardView, Button
  Content                  RectTransform · 마우스를 올리면 이것만 커진다
    Background             Image
    NameLabel              TMP_Text
    PlayTimeLabel          TMP_Text
    LastPlayedLabel        TMP_Text
    EmptyLabel             TMP_Text · 카드 한가운데
    NameInput              TMP_InputField · 이름을 고칠 때만 나온다
    EditPanel              Image + Button
      EditLabel            TMP_Text
    DeletePanel            Image + Button
      DeleteLabel          TMP_Text
```

자리는 `ProfileSelectScreenView` 를 우클릭해 `기획서 자리로 맞추기` 를 누르면 잡힌다.
카드 안쪽은 `Bind` 를 부를 때 저절로 잡힌다.

### 3. 호출

```csharp
// 타이틀에서 현재 프로필을 보여 준다
currentProfileButton.Show(profiles);
currentProfileButton.Clicked += () => profileSelect.Open(profiles);

// 프로필을 고르면 그 프로필로 전환한다
profileSelect.ProfileChosen += index => save.SwitchProfile(index);

// 빈 자리에 새로 만들거나 이름을 고치면 저장한다
profileSelect.ProfileCreated += (index, name) => save.CreateProfile(index, name);
profileSelect.ProfileRenamed += (index, name) => save.RenameProfile(index, name);

// 삭제는 확인 팝업을 거친다
profileSelect.DeleteRequested += index =>
{
    PopupSpec spec = PopupSpec.Confirm(
        "프로필을 삭제하시겠습니까?",
        "이 프로필의 기록은 되돌릴 수 없습니다.",
        "아니오",
        "예",
        confirmIsDangerous: true);

    popup.Show(spec, buttonId =>
    {
        if (buttonId == "confirm")
        {
            profileSelect.ConfirmDelete(index);
            save.DeleteProfile(index);
        }
    });
};

// 읽을 수 없는 자리를 누르면 저장 데이터 오류 팝업을 띄운다
profileSelect.BrokenProfileOpened += index => popup.Show(saveErrorSpec, OnSaveErrorChosen);

// 뒤로는 타이틀로 돌아간다
profileSelect.BackRequested += () =>
{
    profileSelect.Close();
    currentProfileButton.Show(profiles);
    titleScreen.Open();
};
```

삭제 확인 팝업은 `Popup` 폴더의 공통 틀을 그대로 쓴다.
기획서가 "두 버튼을 폭 450, 간격 80으로 배치한다. 긍정에만 강조색을 사용한다"로 정했고
공통 틀의 두 버튼 배치와 같다.

`ConfirmDelete` 가 자리를 빈 자리로 되돌리고 화면을 다시 그린다.
지우는 자리가 쓰고 있던 프로필이면 고른 자리도 함께 풀린다.

## 내보내는 사건을 흐름이 다 받아야 한다

이 화면은 **스스로 고치지 않고 알리기만 하는 것**이 여럿이다.
흐름이 그 가운데 하나라도 안 받으면 그 버튼은 눌려도 아무 일이 없다.

| 사건 | 흐름이 할 일 |
| --- | --- |
| `ProfileChosen` | 그 프로필을 고르고 타이틀로 보낸다 |
| `DeleteRequested` | **삭제 확인 팝업을 띄운다.** 확인을 받으면 `ConfirmDelete` 를 부른다 |
| `ProfileDeleted` | 저장 파일을 지운다. 화면이 자리를 비운 뒤에 온다 |
| `ProfileCreated` · `ProfileRenamed` | 이름을 저장한다 |
| `BrokenProfileOpened` | 저장 데이터 오류 팝업을 띄운다 |
| `BackRequested` | 타이틀로 보낸다 |

`DeleteRequested` 와 `BackRequested` 와 `BrokenProfileOpened` 를 안 받고 있어
삭제 버튼과 뒤로 버튼이 오래 죽어 있었다.

## 게임을 켤 때 들어갈 프로필

`ProfileList.PickStarting` 이 고른다. **마지막으로 논 자리**다.
마지막 플레이 날짜는 런 데이터를 저장할 때마다 오늘로 맞춰지므로 런을 하다 나간 프로필도 그날 논 것으로 친다.
날짜가 같거나 아직 논 적이 없으면 번호가 작은 쪽이고,
빈 자리와 읽을 수 없는 자리는 고르지 않는다.

흐름은 이것이 자리를 찾아내면 **타이틀부터** 연다.
저장 쪽이 첫 프로필을 미리 만들어 두므로 처음 켠 사람도 타이틀로 간다.
하나도 못 찾을 때만 이 화면에서 시작한다. 이름을 받지 않으면 런을 시작할 수 없기 때문이다.

## 현재 프로필 버튼

타이틀 오른쪽 위에 있고 `Clicked` 하나만 내보낸다.
흐름이 그것을 받아 이 화면을 연다. **이 길 말고 프로필을 바꿀 방법이 없다.**
`GameFlowController` 의 `_currentProfileButton` 이 그 버튼이고
게임 씬을 만들 때 타이틀 화면 안에서 찾아 물린다.

## 검증 결과

유니티 없이 순수 로직만 따로 빌드해 90건을 확인했고 전부 통과했다.

- 제목, 카드 셋, 뒤로, 현재 프로필 버튼의 자리가 와이어프레임 실측과 같다
- 카드 셋의 오른쪽 끝이 1805이고 좌우 여백이 115로 대칭이다.
  뒤로와 현재 프로필 버튼의 오른쪽 끝도 거기에 맞는다
- 카드 안 프로필명이 x 140 · y 484, 플레이 정보가 y 542와 y 586이다
- 삭제가 x 557, 수정이 x 489이고 삭제 오른쪽에 여백 20이 남는다
- 둘째 카드 안쪽도 그 카드를 따라 x 726으로 옮겨진다
- 45600초가 12시간 40분, 3540초가 0시간 59분으로 나온다
- 와이어프레임 예시 네 줄이 그대로 나온다
  (플레이 시간 12시간 40분 / 마지막 플레이 2026-09-03 / 플레이 시간 0시간 0분 / 마지막 플레이 없음)
- 쓰던 자리를 지우면 고른 자리가 풀리고 다른 자리를 지우면 그대로 남는다
- 카드 색 셋이 와이어프레임과 같고 자리가 색보다 많으면 처음부터 다시 쓴다
- 읽을 수 없는 카드는 채도가 빠져 세 채널이 같아지고 멀쩡한 카드는 색이 남는다
- 마우스를 올리면 5퍼센트 커지고 카드가 밝아지며 누르면 어두워진다

## 기획서와 와이어프레임이 어긋나는 곳

**프로필명 색이 04장 표와 06장 본문에서 다르다.**
04 화면 공통 규칙 의 텍스트 표는 "프로필 카드의 프로필명 색상 2"라 흰색인데
06 프로필 선택 화면 본문은 "검은색으로 프로필명"이고 와이어프레임도 검은색이다.
06장과 와이어프레임을 따라 검은색으로 두었다.

**제목 글자가 다른 화면보다 작다.**
설정 화면의 "설정"은 실측 높이 38인데 프로필 선택의 "프로필 선택"은 32다.
04장 표의 화면 주요 텍스트 40픽셀을 따랐으므로 와이어프레임보다 크게 나온다.

**수정 글자 색이 표에 없는 색이다.**
와이어프레임에서 "수정" 글자가 남색 계열로 찍힌다.
04장 표에 없는 색이라 기본 글자색 `1F1B16` 으로 두었다.
삭제는 붉은색으로 찍혀 강조색 `9E2B25` 와 맞으므로 그대로 따랐다.

## 기획서에 없어 내가 정한 부분

확인이 필요하면 알려 주면 고친다.

1. **프로필명 입력 상태의 생김새** 기획서에 화면이 없다.
   카드 안 프로필명 자리에 입력칸을 띄우고 나머지를 감췄다.
2. **이름을 비우고 넘길 때** 만들지도 고치지도 않는다.
   빈 자리는 빈 자리로 남는다. 취소로 본 것이다.
3. **읽을 수 없는 자리의 생김새** 04 화면 공통 규칙 의 채도 제거를 가져와
   카드 색의 채도를 빼고 플레이 정보 자리에 "불러올 수 없음"을 적었다.
   누를 수는 있다. 기획서가 누르면 팝업을 띄우게 했기 때문이다.
4. **빈 자리의 수정과 삭제** 감췄다. 고칠 것도 지울 것도 없다.
5. **지금 쓰는 프로필 표시** 카드에 따로 표시하지 않는다. 기획서에 없다.
6. **자리 수** 저장 시스템 기획서의 프로필 3개를 기본값으로 두었다.
   `ProfileList.EnsureSlots` 로 바꿀 수 있다.
7. **현재 프로필이 없을 때** 버튼에 "프로필 없음"을 적는다.

## 이 코드에 없는 것

- 프로필을 실제로 만들고 지우고 바꾸는 일. 저장 시스템 기획서 소관이고
  여기서는 화면에 보이는 목록만 고치고 알린다
- 프로필 삭제 확인 팝업과 저장 데이터 오류 팝업. `Popup` 폴더의 공통 틀로 띄운다
- 타이틀 화면. 05장이고 현재 프로필 버튼 말고는 아직 코드가 없다
- 프로필명에 쓸 수 있는 글자와 길이 제한. 정해지면 입력칸에 걸면 된다
