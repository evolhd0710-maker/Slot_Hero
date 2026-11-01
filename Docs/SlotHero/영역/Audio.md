# Slot Hero 효과음 코드

`효과음과 이펙트 목록.md` 의 효과음을 내는 코드다.
1장의 효과음 표와 1.6 겹칠 때 규칙을 맡고, 소리 크기는 인게임 화면 기획서 07 설정 화면의 사운드 탭 값을 따른다.

지금 붙인 소리는 넷이다. S23 행상 구매와 S24 야영(2026년 10월 29일 원재), S12 체력 잃음(2026년 10월 30일 원재),
S07 노드 고르기(2026년 10월 31일 원재)다.
S12 는 피격음이다. 전투에서 맞을 때와 이벤트에서 체력을 잃을 때 함께 쓴다. 쓸 파일은 원재가 나중에 정한다.
후보 일곱은 `후보 피격음 20261030` 에 꺼내 두었다. 파일을 정하기 전에는 소리를 고르기만 하고 내지 않는다.
S07 은 맵에서 다음 방 노드를 누를 때 낸다. 후보 일곱은 `후보 노드 고르기 20261031` 에 꺼내 두었다.
Fantasy Menu SFX 의 확인 셋, Timbeaux 의 확인과 퀘스트 수락과 발걸음, Shapeforms 의 돌 돌리기다.
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
