# Slot Hero 새 코드

기획서를 옮겨 새로 쓴 코드다. 저장소에 원래 있던 `Assets/Scripts` 의 코드와 따로 두었다.

## 폴더

```
Assets/SlotHero/
  Scripts/<영역>/     영역마다 Runtime/{Data, Config, Generation, UI} 와 Editor
                      영역마다 README.md 에 기획서와 코드의 대응을 적었다
  문서/
    작업 기록.md       무엇을 언제 왜 했는지
    Slot Hero 코드 설명.docx   형식과 공개 멤버를 영역별로 모은 문서
```

영역은 Flow, Save, Map, Title, Profile, TopBar, Hud, Sanctum, Events, Reward, CurrentBuild, Popup, Settings, RunResult, Ui, Tools 열여섯이다.
화면 영역끼리는 서로를 모르고 Flow 의 `GameFlowController` 가 잇는다.

## 구조 설명 보드

피그마(FigJam) 보드에 모듈 구조, 상속, 영역 속 파일, 클래스 카드, 구간별 순서도를 그려 두었다.

https://www.figma.com/board/AVi8pGxY7BdquMy4gM0a5c

## 씬과 설정 에셋

이 폴더에는 스크립트와 문서만 있다. 씬, 프리팹, 설정 에셋, 그림은 올리지 않았다.

- 설정 에셋은 `Slot Hero > 설정 에셋 한 번에 만들기` 로 `Assets/SlotHeroConfigs` 에 만든다
- `Slot Hero > 한 바퀴 다 돌리기` 는 설정 에셋, 그림 가져오기, 씬 짓기, 검사를 차례로 돈다
- 그림을 가져오는 `ImportArt` 는 작성자 컴퓨터의 폴더 경로(`SourceRoot`, `RepoRoot`)를 읽는다. 다른 컴퓨터에서는 그 경로를 고쳐야 그림이 들어온다

## 코드 규칙

- 네임스페이스는 `SlotHero.<영역>`, 에디터 전용은 `SlotHero.<영역>Editor`
- 수치는 ScriptableObject 설정 에셋으로 뺀다
- 난수는 `UnityEngine.Random` 을 쓰지 않고 시드에서 나오는 전용 난수를 쓴다. 같은 시드면 어느 기기에서든 같은 결과가 나온다
- 저장 대상 자료는 `[Serializable]` 로 두어 `JsonUtility` 로 바로 직렬화한다
