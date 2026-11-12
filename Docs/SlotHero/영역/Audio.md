# Slot Hero 효과음 코드

`효과음과 이펙트 목록.md` 의 효과음을 내는 코드다.
1장의 효과음 표와 1.6 겹칠 때 규칙을 맡고, 소리 크기는 인게임 화면 기획서 07 설정 화면의 사운드 탭 값을 따른다.

지금 붙인 소리는 열셋이다. S23 행상 구매와 S24 야영(2026년 10월 29일 원재), S12 체력 잃음(2026년 10월 30일 원재),
S07 노드 고르기(2026년 10월 31일 원재), 그리고 2026년 11월 13일 원재가 효과음 후보 듣기 씬에서 고른
S01 버튼 누름, S06 현재 빌드 열고 닫기, S09 골드 얻음, S15 코인 받음, S19 런 시작, S20 스테이지 전환과 원재가 따로 고른 S25 · S26 설정 값 올림 내림이다.
같은 날 S12 와 S07 의 파일도 골랐다.
S12 는 피격음이다. 전투에서 맞을 때와 이벤트에서 체력을 잃을 때 함께 쓴다. 파일은 Shapeforms 의 `PUNCH_PERCUSSIVE_HEAVY_08` 이다.
S07 은 맵에서 다음 방 노드를 누를 때 낸다. 파일은 Kenney RPG Audio(CC0)의 한 걸음 소리 다섯(`footstep05` ~ `09`)을 0.3초 간격으로 이은 `Kenney 다섯 걸음 빠름` 이다.

**버튼 누름(S01)** 은 게임 씬을 지을 때 버튼마다 `ButtonClickSound` 를 붙여 낸다(`BuildGameScene.AddButtonSounds`). 5등급이라 같은 때 난 다른 소리가 이긴다.
복제해 쓰는 칸의 프리팹에도 붙인다(`UiSceneBuilder.SavePrefab`). 프리팹은 씬의 소리 창구를 물 수 없어 지금 소리 창구(`SoundPlayer.Current`)를 쓴다.
**골드 얻음(S09)과 코인 받음(S15)** 은 흐름이 매 프레임 런의 골드와 코인 수를 앞 프레임과 견주어 늘었을 때 낸다(`GameFlowController.WatchResources`).
골드와 코인은 여러 길로 늘어 결과를 본다. 런이 바뀐 프레임은 기준만 잡아 시작 골드와 시작 코인에는 소리가 나지 않는다.
코인은 개수가 아니라 넣은 횟수(`RunOwnedData.CoinsAdded`, 저장하지 않는다)를 본다. 한도가 차 하나를 버리고 받으면 개수가 같기 때문이다.
**현재 빌드 열고 닫기(S06)** 는 현재 빌드 화면이 열리고 닫힐 때와 문양 고르기 화면을 열고 고르거나 닫을 때 낸다.
**설정 값 올림 내림(S25 · S26)** 은 설정의 수치 항목(막대)이 한 단계 오르거나 내릴 때 낸다(`SettingsScreenController.ValueStepped`). 막대를 끌면 단계마다 난다. 선택지 넘기기, 음소거, 드롭다운, 탭은 버튼이라 S01 이 난다.
파일은 원재가 받아 둔 Atelier Magicae Fantasy UI Sound Effects 의 `Fantasy_UI (10)`, `(11)` 이다. 이 묶음은 상업용에 쓸 수 있고 파일을 다시 나눠 주는 것은 막는다.
**런 시작(S19)** 은 새 게임과 이어하기, **스테이지 전환(S20)** 은 다음 스테이지 맵을 만들 때(`AdvanceStage`) 낸다. 지금은 스테이지가 하나라 S20 은 나지 않는다.
목록에서 소리를 하나 정할 때마다 `SoundCue` 에 이름을 더하고 설정 에셋에 파일을 물린다.

## 파일 구성

```
Audio/
  Runtime/
    Data/
      SoundCue.cs            효과음 이름. 값이 목록의 번호다(S23 이면 23)
      SoundCueEntry.cs       효과음 하나의 등급, 파일, 크기, 길이
    Config/
      SoundConfig.cs         효과음 목록과 겹칠 때 규칙의 수치. ScriptableObject
    Generation/
      SoundArbiter.cs        겹친 소리 가운데 낼 것을 고른다. 규칙 1 ~ 3
      SoundRandom.cs         같은 소리의 파일 여럿 가운데 하나를 고르는 xorshift32
    SoundPlayer.cs           소리 자리(AudioSource)를 두고 소리를 낸다
  Sound/
    Coin Dropped on Wood Rattle.wav             0.55초. S23 의 30 골드 미만
    Coins in Sack Dropped on Soft Surface.wav   0.55초. S23 의 30 ~ 79 골드
    Coins In Sack Dropped on Wood.wav           0.56초. S23 의 80 골드 이상
    CampfireEcho.wav                            8.75초. S24
```

`Sound` 의 파일은 에셋 스토어 무료 묶음에서 원재가 들어 보고 고른 것이다.
동전 소리 셋은 Shapeforms Audio Free Sound Effects, 모닥불 소리는 Timbeaux's 2D Fantasy SFX Pack 에서 왔다.
`Slot Hero/소리 가져오기`(`ImportSounds`)가 이 폴더를 `Assets/SlotHero/Art/Audio` 로 옮기고 설정 에셋에 물린다.

## 목록 대응표

### 1장 효과음

| 목록 | 코드 |
| --- | --- |
| S23 행상 구매 · 2등급 | `SoundCue.MerchantPurchase`, `SanctumController.ItemPurchased` 를 받은 `GameFlowController.PlayPurchaseSound` 가 `SoundPlayer.PlayPurchase` 에 치른 골드를 넘긴다 |
| S23 의 파일은 치른 골드로 고른다. 30 미만 동전 한 닢, 80 미만 부드러운 바닥의 자루, 80 이상 나무 바닥의 자루 | `SoundConfig.PurchaseGoldSteps`(30, 80), `PurchaseClipIndex`. 파일 차례는 `ImportSounds.Files` 가 구간 차례대로 물린다 |
| S12 체력 잃음 · 3등급 · 야영보다 위 | `SoundCue.HealthLost`, `RunContext.HealthLost` 를 `RunSession.HealthLost` 로 받은 `GameFlowController.PlayHitSound`. 전투는 `CombatEntry` 가, 이벤트는 `RunEventWorld` 가 `TakeDamage` 와 `ChangeMaxHealth` 로 체력을 깎는다 |
| S07 노드 고르기 · 4등급 | `SoundCue.NodeSelect`, `MapScreenController.NodeChosen` 을 받은 `GameFlowController.PlayNodeSound`. 노드가 반짝이기 시작할 때 낸다 |
| S24 야영 · 3등급 · 모닥불 2초 낸 뒤 0.5초 동안 줄인다 | `SoundCue.Camp`, `SanctumController.CampUsed` 를 받은 `GameFlowController.PlayCampSound` |
| 새로 붙인 소리의 칸 | `SoundConfig.AddMissingDefaults`. `ImportSounds` 가 이미 만든 설정 에셋에도 기본 차례대로 더한다 |
| 재생 등급 | `SoundCueEntry.Grade` |
| 같은 등급이면 표에서 위에 있는 소리가 높다 | `SoundConfig.Cues` 의 차례 |
| 낼 시간과 줄이는 시간 | `SoundCueEntry.PlaySeconds`, `FadeSeconds`, `SoundPlayer.UpdateFades` |

### 1.6 겹칠 때 규칙

| 목록 | 코드 |
| --- | --- |
| 1 한 묶음에서 하나만 낸다. 0.1초 | `SoundConfig.BundleWindowSeconds`, `SoundArbiter.Resolve` |
| 2 같은 소리는 0.1초 안에 다시 내지 않는다 | `SoundConfig.RepeatGapSeconds`, `SoundArbiter.Resolve` |
| 3 동시에 울리는 효과음은 둘까지다 | `SoundConfig.MaxVoiceCount`, `SoundArbiter.ChooseVoice` |
| 4 1등급은 혼자 울리고 배경음을 줄인다 | 1등급 소리와 배경음을 붙일 때 만든다 |
| 5 호버는 다른 효과음이 울리는 동안 내지 않는다 | 호버 소리를 붙일 때 만든다 |

### 07 설정 화면 · 사운드 탭

| 기획서 | 코드 |
| --- | --- |
| 전체 0 ~ 100, 음소거 | `masterVolume`, `masterVolumeMute` |
| 효과음 0 ~ 100, 음소거 | `sfxVolume`, `sfxVolumeMute` |
| 두 값을 곱해 효과음 크기로 쓴다 | `SoundPlayer.SetVolume`. 흐름이 `ApplySoundSettings` 로 넘긴다 |

흐름은 게임을 켤 때, 프로필에 들어갈 때, 설정 값을 바꿀 때, 기본값을 되돌릴 때 소리 크기를 넘긴다.
울리고 있는 소리에도 바로 적용한다.
전투의 레이저 발사음은 전투 화면이 따로 내므로 같은 크기를 `CombatScreenView.SoundVolume` 으로 넘겨 곱한다(2026년 10월 31일 원재).
배경음과 환경음 막대는 값만 저장한다. 그 소리를 붙일 때 같은 길로 크기를 넘긴다.

## 부르는 법

```csharp
_sound.Play(SoundCue.MerchantPurchase);
```

`Play` 는 요청을 받아 두기만 한다. 그 프레임의 `LateUpdate` 에서 받아 둔 요청을 한꺼번에 보고 하나를 낸다.
한 번 누른 것에서 나는 알림(구매, 골드 변화, 물건 받기)은 같은 프레임에 함께 오기 때문이다.

파일을 정해 내려면 `Play(cue, clipIndex)` 로 자리를 넘긴다. 행상 구매는 `PlayPurchase(gold)` 가 구간을 찾아 넘긴다.
정해 받은 자리는 그 묶음에만 쓰고, 자리를 넘기지 않은 소리는 무작위로 고른다.

게임 씬 검사는 프레임이 돌지 않아 `AdvanceClock` 으로 시간을 흘리고 `Flush` 로 요청을 처리한 뒤 `LastChosen` 과 `LastClipIndex` 를 본다.

## 씬 구성

게임 씬의 `GameFlow` 오브젝트에 `SoundPlayer` 가 붙고 `SoundConfig` 를 문다. 흐름의 `_sound` 칸이 이것을 가리킨다.
소리 자리(`AudioSource`)는 처음 소리를 낼 때 같은 오브젝트에 `MaxVoiceCount` 만큼 만든다.
듣는 쪽은 카메라의 `AudioListener` 다.

## 내가 정한 것

1. **묶음은 한 프레임에서 시작한다.** 목록은 "0.1초 안에 함께 생긴 소리" 를 한 묶음으로 본다.
   첫 소리를 0.1초 기다렸다 내면 누를 때마다 소리가 늦어, 같은 프레임의 요청을 묶고
   소리를 낸 뒤 0.1초 안에 늦게 온 요청은 등급이 더 높을 때만 내게 했다.
2. **무작위로 고를 때는 같은 파일이 두 번 이어 나지 않는다.** 바로 앞에 낸 파일을 뺀다.
   행상 구매처럼 파일을 정해 받은 소리는 같은 구간이면 같은 파일이 이어 난다.
3. **소리를 고르는 난수는 고정 시드다.** 소리는 게임 결과에 닿지 않아 런 시드를 쓰지 않는다.
   코드 규칙대로 `UnityEngine.Random` 은 쓰지 않는다.
4. **소리 크기는 1 로 둔다.** 두 소리 모두 원본 크기 그대로다. 들어 보고 `SoundCueEntry.Volume` 으로 맞춘다.
5. **음소거여도 고르기는 한다.** 규칙의 묶음 시간과 같은 소리 간격은 실제로 울렸는지와 상관없이 센다.

## 검사

- 순수 로직 검사 `검증/AudioTests.cs` 54건. 기본값, 행상 구매의 골드 구간, 규칙 1 ~ 3, 파일 고르기, 소리 크기를 본다
- 게임 씬 검사(`CheckGameScene`). 흐름에 효과음 창구와 설정이 물렸는지, 행상 구매 파일 셋이 구간 차례대로 물렸는지, 야영 파일 하나가 물렸는지,
  행상에서 사면 S23 을 고르고 치른 골드 구간의 파일을 쓰는지, 야영을 쓰면 S24 를 고르는지 본다

## 효과음 후보 듣기 씬

2026년 11월 13일 원재가 후보만 정하고 붙이지 않은 효과음을 들어 볼 씬을 만들라고 했다.

- **씬.** `Assets/SlotHero/Scenes/시험 효과음 후보`. 플레이를 누르고 칸을 누르면 그 파일을 처음부터 낸다. 앞 소리는 끊는다. "멈춤" 은 소리를 끊는다.
  위에 지금 낸 소리의 번호, 파일 이름, 길이를 적는다. 줄 왼쪽 아래에 받지 않아 넣지 못한 후보를 적는다.
- **줄.** 아직 고를 소리만 한 줄씩 둔다. 지금은 S08-1 지도 펼치기와 S08-2 지도 접기 둘이다. 원재가 보류했다.
  - S08-1 지도 펼치기: Kenney 의 책 펼침, 책장 넘김 둘, 천 스침 둘, 원재의 `asset` 폴더의 `bookFlip2`, Timbeaux 의 메뉴 열림
  - S08-2 지도 접기: Kenney 의 책 덮음, 책 내려놓음 둘, 천 스침 둘, `asset` 폴더의 `bookPlace1`, Timbeaux 의 메뉴 닫힘
- **정리.** 처음에는 S01 ~ S22 스물한 줄(받아 둔 일곱 묶음에서 꺼낸 후보 60 개)이었다.
  같은 날 원재가 들어 보고 S01 · S06 · S09 · S12 · S15 · S19 · S20 의 파일을 골랐고, 나머지는 후보를 쓰지 않기로 하거나(X) 받은 후보가 없었다.
  원재가 씬을 정리하라고 해 고른 파일은 `코드/Audio/Sound` 에 두고, 끝난 줄의 후보 폴더와 `후보 피격음 20261030` 은 휴지통으로 보냈다.
  S08 은 같은 날 펼치기와 접기를 따로 찾으라고 해 두 줄로 나눴다.
- **후보 파일.** `후보 효과음 20261113` 의 소리별 폴더다. S07 은 2번을 고른 뒤 후보 폴더를 휴지통으로 보냈다.
  `소리 가져오기`(`ImportSounds.CopyCandidates`)가 `Assets/SlotHero/Art/Audio/후보/<소리>` 로 옮긴다. Kenney 소리는 OGG 라 WAV 와 OGG 를 함께 옮긴다.
  원본에서 빠진 파일과 소리 폴더는 프로젝트에서도 지운다.
- **코드.** 씬은 `BuildPreviewScenes.BuildSoundTest` 가 확인용 씬과 함께 짓고, 조종기는 `Tools/Runtime/SoundAuditionDriver`, 검사는 `CheckSoundTest` 다.
  고른 소리는 `ImportSounds.Files` 표에 넣고 `코드/Audio/Sound` 에 파일을 두어 붙인다.
