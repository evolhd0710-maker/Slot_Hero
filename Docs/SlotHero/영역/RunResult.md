# Slot Hero 런 종료 결과 화면 코드

인게임 화면 기획서 v0.2 / 12 런 종료 결과 화면 을 옮긴 코드다.

런이 끝났을 때 결과와 주요 정보를 요약해 보여 주는 화면이다.
화면에 들어오는 순간 런 데이터를 지우고, 타이틀로를 누르면 타이틀 화면으로 나간다.

저장소를 받으면 이 `RunResult` 폴더를 통째로 `Assets/Scripts/RunResult`로 옮기면 된다.

## 파일 구성

```
RunResult/
  Runtime/
    Data/
      RunOutcome.cs                 패배인지 클리어인지
      ResultLine.cs                 칸에 적는 줄 하나
      RunResultData.cs              화면에 한 번에 그릴 것 전부
      RunResultText.cs              줄과 글을 만드는 도우미
    Config/
      RunResultLayoutConfig.cs      화면 자리와 크기. ScriptableObject
      RunResultVisualConfig.cs      색과 글자, 문구. ScriptableObject
    UI/
      RunResultPanelView.cs         줄을 쌓아 보여 주는 칸 하나
      RunResultScreenView.cs        화면 배치와 표시
      RunResultScreenController.cs  화면 열기, 런 데이터 삭제 알림, 타이틀로
```

## 기획서 대응표

| 기획서 | 코드 |
| --- | --- |
| 결과 · 사망 혹은 클리어 | `RunOutcome` |
| 결과 · 화면 위쪽에 크게 표시한다 | `OutcomePosition`, `OutcomeFontSize` 64픽셀 |
| 도달 지점 · 스테이지와 단계, 마지막으로 상대한 몬스터 | `RunResultText.Location` |
| 최종 구성 · 마지막 시점에서 보유한 아이템을 간략하게 | `RunResultText.BuildFinalBuild` |
| 이번 런 기록 · 요약과 주요 기록 | `RunResultData.RunRecord` |
| 해금 및 도전과제 · 새로 해금된 항목 | `RunResultData.Unlocks` |
| 타이틀로 · 우측 하단에 배치한다 | `TitleButtonPosition` x 1343, y 929 |
| 타이틀로 · 타이틀 화면으로 이동한다 | `TitleRequested` 이벤트 |
| 이 화면에 진입하는 시점에 런 데이터를 삭제한다 | `Open` 이 `RunDataDeleteRequested` 를 부른다 |

## 자리와 크기

와이어프레임에서 잰 값을 10 단위로 옮긴 것이다. 괄호 안이 실측값이다.

| 항목 | 자리와 크기 |
| --- | --- |
| 결과 | x 580, y 80, 760 × 120 (576 · 76 · 768 × 119) |
| 도달 지점 | x 130, y 250, 790 × 110 (134 · 248 · 787 × 108) |
| 최종 구성 | x 130, y 390, 790 × 500 (134 · 389 · 787 × 497) |
| 이번 런 기록 | x 1000, y 250, 790 × 370 (999 · 248 · 787 × 367) |
| 해금 및 도전과제 | x 1000, y 650, 790 × 240 (999 · 648 · 787 × 238) |
| 타이틀로 | x 1350, y 930, 440 × 100 (1343 · 929 · 442 × 97) |

자리가 잘 맞물린다.

- 결과 칸은 화면 가로 가운데다. 좌우로 580씩 남는다
- 왼쪽 열의 바깥 여백과 오른쪽 열의 바깥 여백이 둘 다 130이다
- 두 열의 폭이 790으로 같고 사이 간격이 80이다
- 두 열의 아래 끝이 890으로 같다
- 위아래로 이어지는 칸 사이가 둘 다 30이다
- 타이틀로의 세로 자리 930이 설정 화면의 닫기 버튼과 같다

칸 안쪽은 이렇다.

| 항목 | 값 |
| --- | --- |
| 좌우 여백 | 20 |
| 위아래 여백 | 30 |
| 줄 간격 | 30 |

최종 구성 칸의 첫 줄이 x 150, y 420에 놓이고 다음 줄이 450, 그다음이 480이다.
오른쪽 두 칸의 첫 줄은 x 1020, y 280과 y 680이다.

색은 다음과 같다.

| 자리 | 색 |
| --- | --- |
| 여섯 칸과 타이틀로 | `#D9D9D9` |
| 칸 안의 글자 | `#1F1B16` |
| 패배 | `#9E2B25` |

글자 크기는 04 화면 공통 규칙 의 텍스트 표를 따른다.
결과 64픽셀 굵게, 도달 지점 30픽셀 굵게, 소제목 26픽셀 굵게, 본문 24픽셀,
타이틀로 30픽셀 굵게다. 도달 지점만 표에 없어 와이어프레임에서 재 왔다.

## 칸 셋을 한 가지로 만든 까닭

최종 구성, 이번 런 기록, 해금 및 도전과제 셋은 와이어프레임에서 생김새가 같다.
굵은 소제목과 보통 본문이 줄 간격 31로 이어지고 문단 사이에 빈 줄이 하나 들어간다.

그래서 칸마다 다른 구조를 두지 않고 `ResultLine` 목록 하나로 다룬다.

```csharp
data.Unlocks.Add(ResultLine.Heading("해금"));
data.Unlocks.Add(ResultLine.Body("유물 곡괭이"));
data.Unlocks.Add(ResultLine.Blank());
```

칸 하나는 글상자 하나로 그린다. 줄마다 글상자를 두지 않은 것은
와이어프레임에서 긴 줄이 저절로 접혀 두 줄이 되면서도 줄 간격을 그대로 지키기 때문이다.
접는 일은 TextMeshPro 에 맡기고 `RunResultText.ToRichText` 가 줄을 글 하나로 잇는다.
소제목에는 `<b>` 와 `<size>` 가 붙는다.

## 최종 구성은 현재 빌드 자료를 그대로 쓴다

와이어프레임의 최종 구성 칸은 현재 빌드 화면과 같은 내용이다.
그래서 `CurrentBuildSnapshot` 을 그대로 받아 줄로 바꾼다.

```csharp
data.FinalBuild = RunResultText.BuildFinalBuild(snapshot, currentBuildVisual, runResultVisual);
```

와이어프레임 예시와 같은 빌드를 넣으면 이렇게 나온다.

```
문양 24장
불꽃 8 · 진흙 6 · 세계수 4 · 전차 3 · 새 1 · 폭풍 1 · 벼락 1
태그 정보
수성 6 · 금성 3 · 지구 10 · 화성 13 · 목성 5 · 토성 7 · 천왕성 2 · 해왕성 1 · 명왕성 1

유물
낡은 책 · 날개 · 곡괭이

코인
금화 · 은화 · 가죽 원반
```

태그 이름은 현재 빌드 화면의 표시 설정에서 가져온다. 이름을 두 곳에 두지 않으려는 것이다.
0장인 태그는 적지 않는다.

## 붙이는 법

### 1. 설정 에셋을 만든다

프로젝트 창에서 우클릭 후 `Create > Slot Hero > 런 종료 결과`에서 두 개를 만든다.

- 런 종료 결과 화면 자리 설정 (`RunResultLayoutConfig`)
- 런 종료 결과 화면 표시 설정 (`RunResultVisualConfig`)

`Slot Hero > 설정 에셋 한 번에 만들기` 메뉴로도 만들 수 있다.

### 2. 씬을 만든다

```
RunResultScreen            RunResultScreenController, 화면 루트
  Background               Image · 배경. 배경 그림이 생기면 그 위에 깐다
  OutcomePanel             Image
    OutcomeLabel           TMP_Text · 가운데 정렬
  LocationPanel            Image
    LocationLabel          TMP_Text · 가운데 정렬
  FinalBuildPanel          RunResultPanelView · 화면 전체에 펼친 빈 RectTransform
    Panel                  Image · 칸 배경
    Content                RectTransform + RectMask2D + ScrollRect · 보이는 칸
      Label                TMP_Text · 왼쪽 위 정렬, 자동 줄바꿈 켬. ScrollRect 의 Content
  RunRecordPanel           위와 같은 구성
  UnlockPanel              위와 같은 구성
  TitleButtonPanel         Image + Button
    TitleButtonLabel       TMP_Text
```

자리는 `RunResultScreenView` 를 우클릭해 `기획서 자리로 맞추기` 를 누르면 잡힌다.

**배경과 보이는 칸은 둘 다 화면 좌표로 자리를 잡는다.** 그래서 한 겹 더 싸지 않고
펼쳐 놓은 빈 RectTransform 아래에 나란히 둔다. 배경 안에 넣으면 자리가 두 번 밀린다.

보이는 칸의 높이는 칸 높이에서 위아래 여백을 뺀 값이고
`GetLinesInPanel` 이 세는 높이와 같다. 늘어나는 것은 그 안의 글뿐이다.

글상자의 줄 간격은 폰트에 따라 달라진다.
기획서의 31에 맞추려면 폰트를 넣은 뒤 TextMeshPro 의 Line Spacing 을 보고 맞춘다.
본문 24픽셀에 줄 간격 31이면 대략 29퍼센트다.

### 3. 호출

```csharp
RunResultData data = new RunResultData();
data.Outcome = RunOutcome.Defeat;
data.LocationText = RunResultText.Location(runResultVisual, 1, 9, "엘리트에게 쓰러짐");
data.FinalBuild = RunResultText.BuildFinalBuild(snapshot, currentBuildVisual, runResultVisual);

data.RunRecord.Add(ResultLine.Body("플레이 시간 59분 40초"));
data.RunRecord.Add(ResultLine.Body("지나온 방 9개"));
data.RunRecord.Add(ResultLine.Body("처치한 몬스터 14마리 · 엘리트 2마리"));
data.RunRecord.Add(ResultLine.Blank());
data.RunRecord.Add(ResultLine.Body("획득한 골드 412 · 소모한 골드 275"));

data.Unlocks.Add(ResultLine.Body("새로 해금 · 유물 곡괭이"));
data.Unlocks.Add(ResultLine.Body("도전과제 · 엘리트 2마리 처치 (2 / 2)"));

// 여는 순간 런 데이터를 지우라고 알린다
result.RunDataDeleteRequested += () => save.DeleteRunData(profileIndex);

// 타이틀로
result.TitleRequested += () => titleScreen.Open();

result.Open(data);
```

`RunDataDeleteRequested` 는 화면을 한 번 여는 동안 한 번만 부른다.
`Close` 로 감췄다가 다시 열어도 또 부르지 않는다.

## 검증 결과

유니티 없이 순수 로직만 따로 빌드해 82건을 확인했고 전부 통과했다.

- 여섯 칸의 자리와 크기가 전부 10 단위로 떨어진다
- 결과 칸이 화면 가로 가운데에 있고 좌우 여백이 130으로 대칭이며 두 열의 아래 끝이 890으로 같다
- 칸 안의 첫 줄이 x 150 · y 420에 놓이고 줄 간격이 30이다. 오른쪽 칸도 x 1020 · y 280이다
- 칸마다 들어가는 줄이 14줄, 10줄, 6줄이고 와이어프레임 예시는 모두 스크롤 없이 들어간다
- 도달 지점이 "스테이지 1 · 9번째 방 · 엘리트에게 쓰러짐"으로 나온다
- 최종 구성이 와이어프레임 예시와 같은 10줄로 나오고 문단 사이에 빈 줄이 하나씩 들어간다
- 0장인 태그가 빠지고 빈 빌드는 줄이 하나도 나오지 않는다
- 소제목에만 `<b><size=26>` 이 붙고 빈 줄은 줄바꿈만 낸다
- 색과 글자 크기가 04 화면 공통 규칙 의 표와 같다

## 기획서와 와이어프레임이 어긋나는 곳

**결과 문구가 다르다.**
기획서 본문은 "사망 혹은 클리어"이고 와이어프레임은 "패배"다.
와이어프레임을 따라 "패배"로 두었고 `RunResultVisualConfig.DefeatText` 에서 바꿀 수 있다.

**와이어프레임에서 타이틀로의 오른쪽 끝이 1픽셀 어긋났다.**
실측은 1785이고 오른쪽 칸의 오른쪽 끝은 1786이었다.
10 단위로 옮기면서 둘 다 1790으로 맞췄다.

## 기획서에 없어 내가 정한 부분

확인이 필요하면 알려 주면 고친다.

1. **클리어 색** 04 화면 공통 규칙 은 패배 색만 `9E2B25` 로 정했다.
   클리어 색이 없어 색을 새로 만들지 않고 기본 글자색 `1F1B16` 을 그대로 두었다.
   정해지면 `ClearColor` 에 넣는다.
2. **줄이 칸보다 많을 때** 그 칸 안에서만 세로로 스크롤한다.
   최종 구성 14줄, 이번 런 기록 10줄, 해금 6줄까지 스크롤 없이 들어간다.
3. **최종 구성을 적는 차례** 문양과 태그를 한 묶음으로 두고 유물, 코인 순으로 이었다.
   와이어프레임과 같은 차례다.
4. **태그 차례** 현재 빌드 화면과 같은 태양계 순으로 둔다.
   와이어프레임 예시는 차례가 뒤섞여 있어 따르지 않았다.
5. **0장인 태그** 적지 않는다. 간략하게 표시하라는 기획서에 맞춘 것이다.
6. **단축키** 넣지 않았다. 결과는 반드시 보고 넘어가는 화면이라
   ESC 로 건너뛸 수 있게 하면 안 된다고 보았다.
7. **배경** 배경 그림은 배경 요청 기획서 소관이라 검정 한 겹만 두었다.

## 이 코드에 없는 것

- 이번 런 기록에 무엇을 적을지. 플레이 시간과 처치 수, 골드 같은 값은 그 시스템이 모아 넘긴다
- 해금과 도전과제의 판정. 도전과제 기획서가 아직 없다
- 런 데이터를 실제로 지우는 일. 저장 시스템 기획서 소관이고 여기서는 알리기만 한다
- 타이틀 화면. 05장이고 아직 코드가 없다
- 결과에 이르는 판정. 사망인지 클리어인지는 전투와 맵이 정해 넘긴다
