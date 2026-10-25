# Slot Hero 새 코드

기획서를 옮겨 새로 쓴 코드와 에셋, 그 문서다. 저장소에 원래 있던 `Assets/Scripts` 의 코드와 따로 두었다.

## 자리

```
Assets/SlotHero/          유니티가 읽는 것은 모두 여기 있다
  Scripts/<영역>/          코드. 영역마다 Runtime/{Data, Config, Generation, UI} 와 Editor
  Configs/                 설정 에셋. 기획서 수치, 물건 목록, 이벤트 목록, 문구
  Scenes/                  게임 씬(게임.unity)과 화면마다 확인용 씬
  Prefabs/                 프리팹
  Art/                     그림. 메뉴로 가져온 그림과 코드로 만든 UI 스킨
  Fonts/                   올리지 않는다. 아래 글꼴 참고
Docs/SlotHero/            문서. Assets 밖이라 유니티가 임포트하지 않는다
  README.md                이 글
  영역/<영역>.md            영역마다 기획서와 코드의 대응
  작업 기록.md              무엇을 언제 왜 했는지
  Slot Hero 코드 설명.docx  형식과 공개 멤버를 영역별로 모은 문서
```

영역은 Flow, Save, Map, Title, Profile, TopBar, Hud, Sanctum, Events, Reward, CurrentBuild, Popup, Settings, RunResult, Combat, Ui, Tools 열일곱이다.
저장소에 원래 있던 코드(`Assets/Scripts`)와 씬, 프리팹은 2026년 10월 25일 폐기했다. 전투는 `Combat` 으로 새로 썼다.
화면 영역끼리는 서로를 모르고 Flow 의 `GameFlowController` 가 잇는다.

에디터 도구가 이 자리들을 경로로 적어 쓴다(`UiSceneBuilder`, `CreateDefaultConfigs`, `ImportArt`, `MakeUiSkin`, `PrepareTextResources`).
폴더를 옮기면 그 상수도 함께 고친다.

## 구조 설명 보드

피그마(FigJam) 보드에 모듈 구조, 상속, 영역 속 파일, 클래스 카드, 구간별 순서도를 그려 두었다.

https://www.figma.com/board/AVi8pGxY7BdquMy4gM0a5c

## 처음 받았을 때

1. 메뉴 `Slot Hero > 글자 준비하기 > 모두 하기` 로 한글 글꼴 에셋을 만든다
2. 메뉴 `Slot Hero > 확인용 씬 만들기 > 모두 다시 만들기` 로 확인용 씬과 프리팹을 다시 지어 새 글꼴을 물린다
3. 메뉴 `Slot Hero > 게임 씬 만들기` 로 게임 씬을 다시 짓는다. 씬이 빌드 설정의 첫 자리에 들어간다
4. `Assets/SlotHero/Scenes/게임.unity` 를 열고 플레이한다

## 글꼴

`Assets/SlotHero/Fonts` 는 올리지 않는다(`Assets/SlotHero/.gitignore`).
지금 쓰는 맑은 고딕은 윈도우에 딸려 오는 글꼴이라 다시 배포할 수 없다.
그래서 받은 씬과 프리팹에서는 글꼴이 비어 있다. 위 1 ~ 3 을 하면 각자 컴퓨터의 맑은 고딕으로 채워진다.
배포할 글꼴은 따로 정해야 한다.

## 그림 다시 가져오기

`Slot Hero > 그림 가져오기` 는 작성자 컴퓨터의 폴더 경로(`ImportArt` 의 `SourceRoot`, `RepoRoot`)에서 그림을 읽는다.
가져온 결과는 `Assets/SlotHero/Art` 에 올라가 있다.
원본 폴더가 없는 컴퓨터에서는 복사만 건너뛰고, 올라가 있는 `Art` 의 그림으로 설정 에셋에 연결을 다시 잇는다.
새 그림을 원본 폴더에서 가져오려면 그 경로를 고친다.

## 코드 규칙

- 네임스페이스는 `SlotHero.<영역>`, 에디터 전용은 `SlotHero.<영역>Editor`
- 수치는 ScriptableObject 설정 에셋으로 뺀다
- 난수는 `UnityEngine.Random` 을 쓰지 않고 시드에서 나오는 전용 난수를 쓴다. 같은 시드면 어느 기기에서든 같은 결과가 나온다
- 저장 대상 자료는 `[Serializable]` 로 두어 `JsonUtility` 로 바로 직렬화한다
