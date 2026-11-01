# Slot Hero 태그 공격 이펙트와 소리

전투에서 태그로 공격할 때 레이저가 적에게 닿는 순간 태그마다 이펙트를 터뜨리고 소리를 낸다.
2026년 11월 1일 원재가 태그마다 연상되는 것을 정하고, 확인용 씬(`Scenes/태그 공격 확인`)에서 보고 골랐다.
고른 과정과 후보는 `효과음과 이펙트 목록.md` 7장에 있다.

| 태그 | 연상 | 이펙트 | 소리 |
| --- | --- | --- | --- |
| 수성 | 물 | Vefects 물 튀김을 위아래로 뒤집어 1.47배. 머리 위에서 쏟아진다 | A.M.P. `AMP_Water_Impact_02` 를 30% 키움 |
| 금성 | 금속 | Vefects `Impact_01` 3배 | Free Pack `Metal Impact 7` 첫 0.3초 |
| 지구 | 생명 | VFX Fantasy `FX_NotificationPulse_White` 초록 | A.M.P. `AMP_Buff_04` 앞 0.7초 |
| 화성 | 불 | Vefects 불꽃 셋(앞 하나, 뒤에 조금 높게 둘). 1.5배, 두 배 느리게 | Vefects `Fire_Burst_01` |
| 목성 | 나무, 묶고 조이기 | Unity AI 뿌리 그림. 발밑에서 0.4초 솟고 0.8초 동안 가로 80% 까지 조인 뒤 0.4초에 흐려진다 | 나무 삐걱임 + 찢김 |
| 토성 | 흙, 황사 | Vefects 흙먼지 둘을 누런 흙색 2배로 원판 쪽에서 몰아친다. 1초에서 자른다 | 바람 + 흙 터짐, 1초 |
| 천왕성 | 바람 | VFX Fantasy 베기 빛 하늘색 | Free Pack `Magic Spell_Simple Swoosh_6` |
| 해왕성 | 바다, 파도 | 큰 물 튀김 넷(짙은 파랑 2.2배)이 원판 쪽에서 차례로 덮치고 VFX Fantasy 물살 줄기 셋(1.6배)이 지난다 | 해일 소리 2.5초 |
| 명왕성 | 검은색, 죽음 | Vefects 피 구름 보라 1.5배 + VFX Fantasy 줄기 보라. 둘 다 두 배 느리게 | A.M.P. `AMP_DeBuff_09` 앞 1.5초 |

## 파일 구성

```
TagAttack/
  Runtime/
    Data/
      TagAttackFxEntry.cs     태그 하나의 이펙트(파티클 프리팹, 그림)와 소리, 그림 움직임
    Config/
      TagAttackFxConfig.cs    태그 아홉의 설정. ScriptableObject
    UI/
      TagAttackFxPlayer.cs    이펙트를 적 자리에 터뜨리고 소리를 낸다
  Art/
    Sound/태그공격_*.wav      고른 구간만 잘라 구운 소리 아홉
    목성_뿌리.png             Unity AI(Gemini 3.0 Pro)로 만든 뿌리 그림
  소리 굽기.py                받아 둔 묶음의 원본 소리를 잘라 Art/Sound 를 다시 굽는다
```

`Slot Hero/태그 공격 이펙트 가져오기`(`Tools/Editor/ImportTagAttack.cs`)가 셋을 한다.

1. `Art` 의 소리와 그림을 `Assets/SlotHero/Art/TagAttack` 으로 옮긴다
2. 받아 둔 묶음(유니티 프로젝트 `Assets` 맨 위의 Vefects, VFX Fantasy DEMO)에서 고른 파티클을 태그마다 프리팹 하나(`태그공격_수성.prefab` 등)로 묶는다.
   자리, 크기, 뒤집기, 빠르기, 늦음을 파티클에 굳히고, 재질과 그림과 셰이더를 `Assets/SlotHero/Art/TagAttack` 아래로 복사한다.
   그래서 묶음을 지워도 이펙트는 그대로 돈다. 묶음이 없는 컴퓨터에서는 이 단계를 건너뛰고 이미 만든 프리팹을 쓴다
3. 설정의 태그마다 프리팹, 소리, 그림을 물린다

한 바퀴 다 돌리기가 소리 가져오기 바로 뒤에 이것을 부른다.

## 전투 화면에 잇기

- `CombatScreenView.FireLaser(color, target, tag)` 가 레이저가 다 뻗은 순간 `TagAttackFxPlayer.Play` 를 부른다
- 태그 소리가 있으면 레이저 발사음(`LaserSound`)은 내지 않는다
- 소리 크기는 설정의 효과음 크기(전체 × 효과음)에 태그별 크기와 설정 전체 크기를 곱한다
- 재생기는 전투 화면의 효과음 자리(`AudioSource`)를 함께 쓴다. 확인용 씬 도구(`BuildPreviewScenes.Combat`)가 붙이고 게임 씬이 그대로 가져간다

## 내가 정한 것

1. **소리는 잘라서 굽는다.** 재생할 때 자르면 파일 뒤에 붙은 다른 소리가 줄이는 동안 새어 나온다. 금성이 그랬다.
   그래서 고른 구간만 48kHz 스테레오 16비트로 잘라 구웠다. 겹치는 소리(목성, 토성)는 섞어 한 파일로 만들었다
2. **수성 소리는 30% 키우되 넘치는 부분만 부드럽게 누른다.** 원본이 이미 거의 최대 크기라 그대로 키우면 깨진다
3. **파티클은 캔버스 앞에 그린다.** 게임은 캔버스 하나에 전투, 상단 표시줄, 팝업이 함께 있다.
   파티클은 캔버스보다 높은 차례(`SortingOffset` 10)로 그려 상단 표시줄과 팝업보다도 앞에 나온다. 이펙트가 적 주변에만 터져 겹치는 일은 드물다
4. **Vefects 셰이더는 단계 이름만 바꾼 사본을 쓴다.** 프로젝트는 URP 2D 렌더러를 쓰고, 2D 렌더러는 `Universal2D` 와 이름 없는 단계만 그린다.
   Vefects Lite 셰이더는 `UniversalForwardOnly` 단계만 있어 그대로는 화면에 나오지 않는다
5. **물들이기.** Vefects 는 파티클 색을 쓰지 않고 재질의 색 칸(`_R`, `_G`, `_B`, `_Outline`)으로 칠해 재질 사본에서 물들인다. VFX Fantasy 는 파티클 시작 색에 곱한다
