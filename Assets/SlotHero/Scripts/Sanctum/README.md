# Slot Hero 성소 · 행상 코드

성소 기획서 v0.2 를 그대로 옮긴 코드다.
깃허브 저장소를 아직 못 받았으므로 기존 작업물과 겹치는 부분은 없다고 보고 새로 썼다.
저장소를 받으면 이 `Sanctum` 폴더를 통째로 `Assets/Scripts/Sanctum`으로 옮기면 된다.
`Editor` 폴더는 유니티가 알아서 에디터 전용으로 취급하므로 그대로 두면 된다.

## 파일 구성

```
Sanctum/
  Runtime/
    Data/
      SanctumItemKind.cs      상품 종류. 문양 · 코인 · 유물
      ItemRarity.cs           코인과 유물의 등급. 07 가격의 표와 1:1
      PurchaseResult.cs       값을 치르는 모든 행동의 결과
      MerchantItem.cs         상품 하나. 진열과 가격 산정에 쓰는 값만 담는다
      MerchantSlot.cs         진열 한 자리. 상품, 가격, 팔림 여부
      MerchantStock.cs        진열 전체. 문양 4 · 코인 2 · 유물 3
      SanctumState.cs         성소 한 곳의 상태와 규칙. 저장에 그대로 실린다
      RunGoldState.cs         런 단위 골드. 06 재화
      RunRelicPool.cs         런 하나에서 유물이 나올 순서
      IMerchantCatalog.cs     무엇을 팔 수 있는지 알려 주고 산 것을 넣어 주는 창구
      IPlayerVitals.cs        야영이 회복시킬 체력의 창구
      SampleMerchantCatalog.cs 검증용 임시 카탈로그와 임시 체력
    Config/
      RarityPrice.cs          등급 하나의 기준가
      PriceGrowth.cs          쓸 때마다 값이 오르는 방식
      SanctumConfig.cs        야영과 진열 수치. ScriptableObject
      SanctumPriceConfig.cs   가격 규칙 전부. ScriptableObject
      SanctumLayoutConfig.cs   성소 화면 자리. PSD 실측
      MerchantLayoutConfig.cs  행상 화면 자리. PSD 실측
      SanctumVisualConfig.cs  화면 색과 연출 수치. ScriptableObject
    Generation/
      SanctumRandom.cs        시드 전용 난수. xorshift32
      MerchantStockBuilder.cs 진열 구성과 새로고침, 바꿀 문양 뽑기
    UI/
      IMerchantIconSource.cs  상품 그림을 내주는 창구
      MerchantSlotView.cs     진열 한 자리의 표시
      SanctumScreenView.cs    성소 화면 표시
      MerchantScreenView.cs   행상 화면 표시
      SanctumController.cs    진입 흐름과 구매 처리 전체
  Editor/
    SanctumPreviewWindow.cs   에디터 미리보기와 일괄 검증 창
```

## 기획서 대응표

### 03 진입 흐름

| 기획서 | 코드 |
| --- | --- |
| 맵 → 성소 진입 | `SanctumController.Enter` |
| 성소 화면 | `SanctumController.Open`, `SanctumScreenView` |
| 야영 선택 → 야영 사용 · 체력 회복 | `SanctumController.UseCamp`, `SanctumState.TryUseCamp` |
| 행상 선택 → 행상 화면 · 물건 구매 | `OpenMerchant`, `MerchantScreenView`, `SanctumState.TryBuy` |
| 돌아가기 | `SanctumController.CloseMerchant` |
| 나가기 선택 → 성소 퇴장 → 맵 | `SanctumController.Exit`와 `Exited` 이벤트 |

### 04 야영

| 기획서 | 코드 |
| --- | --- |
| 사용 방식 즉시 | `SanctumScreenView.CampClicked`가 바로 `UseCamp`로 이어진다 |
| 사용 횟수 1회 | `SanctumConfig.CampUseLimit`, `SanctumState.CampUsedCount` |
| 비용 없음 | `SanctumConfig.CampCost` 기본 0 |
| 체력 40 회복, 최대 초과 금지 | `SanctumConfig.CampHealAmount`, `TryUseCamp`의 회복량 자르기 |
| 사용 표시 비활성화 | `SanctumScreenView.Refresh`의 `CampUsedTint`와 버튼 잠그기 |
| 사용 피드백 체력 색상 일시 변경 | `SanctumController.CampUsed` 이벤트와 `SanctumVisualConfig.CampHealFeedback*` |

### 05 행상 구성

| 기획서 | 코드 |
| --- | --- |
| 문양 4, 서로 다르게 | `SanctumConfig.SymbolSlotCount`, `MerchantStockBuilder.FillSlots`의 중복 금지 |
| 코인 2, 보유하지 않은 것 | `CoinSlotCount`, `IMerchantCatalog.CollectCoinCandidates` |
| 유물 3, 보유하지 않은 것 | `RelicSlotCount`, `RunRelicPool.Draw` |
| 문양 변경, 횟수 제한 없음 | `SanctumState.TryChangeSymbol` |
| 유물 새로고침, 3자리 모두 교체 | `SanctumState.TryRefreshRelics`, `MerchantStockBuilder.RefreshRelics` |

### 06 재화

| 기획서 | 코드 |
| --- | --- |
| 명칭 골드 | `RunGoldState` |
| 획득처 전투 보상 · 오버킬 · 이벤트 | `RunGoldState.Add`. 부르는 쪽은 성소 밖이다 |
| 최소 수치 0, 초과 소모 시 보정 | `RunGoldState.Lose` |
| 보유 수치를 넘겨 소모 불가 | `RunGoldState.TrySpend`. 모자라면 아무것도 하지 않는다 |
| 유지 단위 런 | `[Serializable]`로 런 데이터에 실린다 |
| 시작 수치 0 | `SanctumConfig.GoldStartAmount` |

### 07 가격

| 기획서 | 코드 |
| --- | --- |
| 문양 20 + 태그 가치 합 × 5 | `SanctumPriceConfig.GetSymbolPrice` |
| 코인 15 / 25 / 35 / 45 / 60 | `SanctumPriceConfig.CoinPrices` |
| 유물 30 / 45 / 60 / 75 / 100 | `SanctumPriceConfig.RelicPrices` |
| 문양 변경 10 > 30 > 60 > 100 > 150 | `SymbolChangeGrowth = Triangular` |
| 유물 새로고침 20 > 30 > 40 > 50 > 60 | `RelicRefreshGrowth = Linear` |
| 가격은 별도 데이터로 관리 | 수치를 전부 `SanctumPriceConfig` 에셋으로 뺐다 |
| 골드가 부족한 항목은 붉은 색 | `SanctumVisualConfig.PriceNotAffordableColor` |

### 08 화면 구성

| 기획서 | 코드 |
| --- | --- |
| 좌측 야영, 우측 행상, 우측 하단 나가기 | `SanctumScreenView`의 세 참조 |
| 야영과 행상은 순서 제한 없음 | `Refresh`에서 행상 버튼을 늘 열어 둔다 |
| 커서를 올리면 상세 설명 팝업 | `MerchantSlotView`의 포인터 처리와 `ItemDetailRequested` 이벤트 |
| 클릭 시 즉시 구매 | `SanctumController.HandleSlotClicked` |
| 가격은 물건 하단에 골드 아이콘과 함께 | `MerchantSlotView`의 가격 묶음 |
| 하단 중앙에 돌아가기 | `MerchantScreenView.BackClicked` |

## 유물이 나오는 방식

유물은 진열할 때마다 새로 뽑지 않는다.
런을 시작할 때 유물 전체를 시드 난수로 한 번 섞어 `RunRelicPool` 에 테이블을 만들어 두고,
행상에 올릴 때마다 그 순서대로 앞에서부터 꺼낸다.
이미 가지고 있어 나올 수 없는 유물은 건너뛰고 그 자리도 지나간 것으로 친다.

이렇게 하면 같은 유물이 여러 성소에 거듭 나오지 않고,
새로고침해도 방금 봤던 유물이 다시 올라오지 않는다.

테이블은 언제나 이 게임의 유물 전부를 담는다. 보유 여부는 테이블을 만들 때 보지 않고
꺼낼 때만 보므로, 이미 가진 유물도 테이블 안에는 그대로 들어 있고 지나칠 뿐이다.

테이블을 다 쓰면 다음 테이블을 이어서 준비한다.
난수는 런 시드와 테이블 번호를 함께 섞으므로 같은 런이라도 테이블마다 순서가 다르다.
그래서 새로고침을 아무리 많이 해도 진열이 끊기지 않는다.
값을 받지 않고 멈추는 경우는 가지고 있지 않은 유물이 아예 없을 때뿐이다.

테이블이 넘어가면 방금 진열에 올린 유물을 새 테이블에서 다시 만날 수 있다.
그때는 그 유물을 지나간 것으로 치지 않고 테이블 맨 뒤로 미룬다.
한 진열에 같은 유물이 두 번 오르지 않으면서도 그 유물의 차례는 잃지 않는다.
나올 수 있는 유물이 자리 수보다 적으면 그만큼만 채우고 남는 자리는 비운다.

런 시드, 테이블 번호, 순서, 지금까지 꺼낸 자리가 모두 `RunRelicPool` 에 들어 있고
`[Serializable]` 이라 저장 시스템 기획서의 런 데이터에 그대로 실린다.
불러온 뒤 이어서 꺼내면 같은 유물이 이어 나온다.

문양과 코인은 이 순서를 쓰지 않는다.
문양은 보유 여부를 따지지 않고 코인은 자리가 둘뿐이라 성소마다 새로 뽑는다.

## 스테이지별 희귀도 분포

`SanctumConfig.RarityByStage` 에 스테이지 범위마다 일반부터 전설까지 다섯 칸의 비중을 적는다(`StageRarityWeights`).
2026년 10월 9일 외부 검토가 권했고 원재가 틀만 만들기로 정했다. 기획서에 수치가 없어 비워 두었고, 비어 있으면 예전과 같은 진열이 나온다.
지금 스테이지는 물건 목록이 알려 준다(`IMerchantCatalog.Stage`).

- 덮는 줄이 있으면 코인 자리마다 등급부터 굴리고 그 등급 안에서 고르게 뽑는다(`RarityPicker`)
- 유물은 등급만 굴리고, 순서 테이블에 남은 것 가운데 그 등급에서 차례가 가장 앞선 것을 꺼내 지나간 자리로 옮긴다(RarityPicker.PickFirstIndex).
  같은 등급 안에서는 테이블 순서를 그대로 지킨다. 예전에는 등급 안에서 다시 무작위로 골라 순서가 흐트러졌다(2026년 10월 9일 외부 검토).
  건너뛴 다른 등급의 유물은 차례를 잃지 않는다. 난수는 런 시드, 테이블 번호, 꺼낸 자리로 만들어 이어 해도 같다
- 문양은 등급이 없어 쓰지 않는다

물건마다의 등장 스테이지(`ItemDefinition.MinStage`, `MaxStage`)는 물건 목록이 후보를 모을 때 뺀다.
범위 밖의 유물은 순서 테이블에서 가진 유물처럼 지나가고 다음 테이블에서 다시 나온다.

## 붙이는 법

### 1. 창구 두 개를 구현한다

성소 코드는 문양, 코인, 유물의 실제 데이터를 갖지 않는다.
기획서가 그 정의를 다른 문서로 넘겼기 때문이다.
대신 아래 두 창구로만 주고받는다.

- `IMerchantCatalog` 무엇을 팔 수 있는지, 지금 무엇을 가졌는지, 산 것을 어디에 넣을지
- `IPlayerVitals` 야영이 회복시킬 체력

그림까지 함께 내주려면 `IMerchantIconSource`도 같은 클래스에 구현한다.
그러면 `SanctumController.Enter`에 아이콘을 따로 넘기지 않아도 알아서 잡는다.

문양, 코인, 유물 기획서의 코드가 생기기 전까지는
`SampleMerchantCatalog`로 돌려 볼 수 있다. 실제 데이터가 생기면 그 파일은 지운다.

### 2. 설정 에셋을 만든다

프로젝트 창에서 우클릭 후 `Create > Slot Hero > 성소`에서 세 개를 하나씩 만든다.
기본값이 기획서 v0.2 그대로라 손대지 않아도 된다.

- 성소 규칙 설정 (`SanctumConfig`)
- 가격 설정 (`SanctumPriceConfig`)
- 성소 화면 표시 설정 (`SanctumVisualConfig`)

### 3. 씬을 만든다

```
SanctumScreen                SanctumController
  SanctumRoot                성소 화면 루트, SanctumScreenView
    야영                      Button + Image
    행상                      Button
    나가기                    Button
  MerchantRoot               행상 화면 루트, MerchantScreenView
    돗자리
      문양자리 × 4           MerchantSlotView, 2 × 2 배치
      코인자리 × 2           MerchantSlotView, 가로 배치
    양탄자
      문양변경                Button + 가격 TMP_Text
      유물새로고침             Button + 가격 TMP_Text
      유물자리 × 3           MerchantSlotView
    돌아가기                 Button
```

진열 자리 프리팹은 루트에 `MerchantSlotView` + `Button`,
자식에 물건 그림 `Image`와 가격 묶음(골드 아이콘 `Image` + 가격 `TMP_Text`)을 둔다.
문양, 코인, 유물이 모두 같은 프리팹을 쓸 수 있다.

자리 잡기는 1920 × 1080 고정이다.
기획서가 자리를 정하지 않았으므로 `성소 이미지 예시.psd` 와 `행상 예시.psd` 의
레이어 경계에서 재어 `SanctumLayoutConfig` 와 `MerchantLayoutConfig` 에 넣었다.
씬을 만드는 쪽이 그 값을 읽어 놓고, 코드는 상태에 따라 바뀌는 색, 가격, 켜고 끄기만 건드린다.

**성소 화면은 그림 석 장이 전부다.** 모닥불이 야영, 행상인이 행상, 화살표가 나가기다.
누르는 영역이 곧 그림이라 따로 칸을 두지 않는다.
야영을 쓰면 불이 꺼진 모닥불 그림으로 바꾼다. PSD 의 숨긴 레이어가 그것이다.
04 야영 의 "회색으로 바꾼다"를 색 대신 그림으로 한 것이다.
그림이 없으면 예전처럼 색만 흐려진다.

**행상 화면의 문양 변경 칸과 유물 칸의 테두리는 양탄자 그림에 이미 그려져 있다.**
그래서 그 자리에는 누르는 칸과 물건 그림만 얹는다.

### 뿌리를 셋으로 나눈 까닭

```
SanctumScreen            SanctumController. 늘 켜 둔다
  SanctumPanel           _sanctumRoot. SanctumScreenView
  MerchantPanel          _merchantRoot. MerchantScreenView
```

조종기를 성소 칸과 같은 오브젝트에 두면 **행상으로 들어갈 때 조종기까지 꺼져**
돌아올 방법이 없어진다. 그래서 껍데기를 하나 더 두고 그 안에서 둘을 켜고 끈다.
게임 씬을 만드는 쪽도 이 껍데기만은 끄지 않는다.

### 4. 호출

```csharp
// 런을 시작할 때 유물 순서를 한 번 만들어 둔다. 런 데이터와 함께 들고 다닌다
RunRelicPool relicPool = RunRelicPool.Build(runSeed, catalog);

// 맵에서 성소 노드를 골랐을 때
sanctum.Enter(runSeed, sanctumIndex, catalog, vitals, gold, relicPool);

// 나갔을 때 맵으로 돌려보낸다
sanctum.Exited += () =>
{
    mapScreen.ClearCurrentRoom();
    mapScreen.Open();
};

// 저장에서 불러올 때
sanctum.Load(savedSanctumState, catalog, vitals, gold, savedRelicPool);
```

`sanctumIndex`는 같은 런 안에서 성소마다 다른 진열이 나오게 하는 번호다.
몇 번째 성소인지를 세도 되고 `MapNode.Id`를 그대로 넘겨도 된다.

문양 변경은 바꿀 문양을 고르는 화면이 필요하다.
그 화면은 이 코드에 없으므로 이벤트를 받아 띄우고 결과를 돌려준다.
게임에서는 `GameFlowController` 가 고르기 화면을 띄운다. 아래 `문양 변경은 고르기 화면에서 고른다` 를 본다.

```csharp
sanctum.SymbolChangeTargetRequested += owned =>
{
    // owned 목록으로 고르는 화면을 띄운다
    // 골랐으면 sanctum.ConfirmSymbolChange(고른 문양의 Id)
    // 그만두면 sanctum.CancelSymbolChange()
};
```

`SanctumState`는 `[Serializable]`이라 `JsonUtility`로 그대로 직렬화된다.
불러온 뒤에는 `RestoreAfterLoad`를 부르며, `SanctumController.Load`가 알아서 부른다.

### 5. 에디터에서 확인

메뉴 `Slot Hero > 성소 미리보기`를 열고 설정 에셋 두 개를 넣은 뒤
성소에 들어가면 진열과 가격이 나온다.
야영, 구매, 문양 변경, 유물 새로고침을 눌러 볼 수 있고
아래의 일괄 검증은 시드를 바꿔 가며 한꺼번에 돌려 규칙 위반을 알려 준다.

## 검증 결과

유니티 없이 순수 로직만 따로 빌드해 181건을 확인했고 전부 통과했다.
그중 서른다섯 건이 PSD 에서 잰 성소와 행상 화면의 자리다.

- 07 가격의 네 표와 예시 수치가 코드 계산과 모두 일치
  (문양 30 · 40 · 55 · 65, 코인 15 ~ 60, 유물 30 ~ 100,
  문양 변경 10 · 30 · 60 · 100 · 150, 유물 새로고침 20 · 30 · 40 · 50 · 60)
- 시드 3000개에서 자리 수 4 / 2 / 3, 한 진열 안 중복 없음, 가격이 기준가와 일치
- 같은 시드는 같은 진열, 같은 런이라도 성소 번호가 다르면 다른 진열
- 보유한 코인과 유물은 진열에 오르지 않음
- 유물 순서는 같은 시드면 같고 시드가 다르면 다르며, 뽑아 둔 순서대로 나온다
- 같은 런 시드라도 테이블 번호가 바뀌면 순서가 달라지고, 같은 번호면 다시 같아진다
- 유물을 여럿 가진 뒤에 만든 테이블도 크기가 전체 유물 수 그대로고,
  가진 유물은 테이블에 남아 있다가 꺼낼 때만 걸러진다
- 새로고침한 유물이 앞에 나온 것과 겹치지 않고, 가진 유물은 건너뛴다
- 새로고침 20번을 이어 해도 모두 성공하고 유물 자리가 빈 적이 없으며 테이블이 여러 번 넘어간다
- 나올 수 있는 유물이 자리 수보다 적으면 그만큼만 채우고 같은 유물을 두 번 담지 않는다
- 테이블 마지막 자리에서 세 자리를 요청한 시드 200개에서 모두 서로 다른 세 개가 나오고,
  테이블 크기가 그대로이며, 다시 만난 유물이 맨 뒤로 밀린 경우가 실제로 나온다
- 유물을 모두 가지면 값을 받지 않고 멈추며 진열은 그대로 남는다
- 야영은 잃은 체력만큼 최대 40 회복, 가득이면 회복량 0, 성소당 1회, 새 성소에서 되살아남
- 골드는 보유를 넘겨 소모되지 않고 강제 차감은 0으로 보정
- 값을 치르지 못하면 진열도 횟수도 유물 순서 위치도 바뀌지 않음
- 저장에 실리는 값만 옮겨도 가격과 유물 순서가 그대로 이어짐

## 기획서에 없어 내가 정한 부분

확인이 필요하면 알려 주면 고친다.

1. **문양 변경의 대상과 비용 누적** 물어서 정했다.
   문양 변경은 가지고 있는 문양 하나를 다른 문양으로 바꾸는 것이고,
   문양 변경과 유물 새로고침의 사용 횟수는 성소마다 0에서 다시 시작한다.
2. **유물이 나오는 방식** 물어서 정했다.
   런을 시작할 때 테이블을 미리 뽑아 두고 나올 수 없는 것을 건너뛰며 순서대로 내놓으며,
   테이블을 다 쓰면 다음 테이블을 준비한다. 테이블 번호가 바뀌면 순서도 달라진다.
3. **바뀐 문양을 고르는 방법** 기획서에 없어 시드 난수로 뽑게 했다.
   바꾸기 전과 같은 문양은 나오지 않는다. `SanctumConfig.SymbolChangeExcludesSource`.
   이미 가진 문양도 피하게 하려면 `SymbolChangeExcludesOwned`를 켠다.
4. **후보가 모자랄 때** 보유하지 않은 코인이나 유물이 자리 수보다 적으면 남는 자리를 비운다.
   빈 자리는 화면에 아무것도 그리지 않는다.
   유물은 테이블을 넘겨 가며 꺼내지만, 자리를 채우려고 같은 유물을 두 번 올리지는 않는다.
   그런 유물은 테이블 맨 뒤로 미뤄 다음 차례에 나오게 한다.
   남은 자리를 한 바퀴 다 미뤄도 담을 것이 없으면 그 테이블을 접고 다음 테이블로 넘어간다.
5. **새로고침과 팔린 자리** 유물을 사고 새로고침하면 팔린 자리도 새 유물로 채운다.
   "3자리 모두 교체한다"를 글자 그대로 따른 것이다. `SanctumConfig.RefreshRefillsSoldSlots`로 끌 수 있다.
6. **바꿀 것이 없을 때** 가지고 있지 않은 유물이 아예 없으면
   값을 받지 않고 진열도 건드리지 않는다.
   테이블을 다 쓴 것만으로는 멈추지 않고 다음 테이블로 이어 간다.
7. **체력이 가득할 때의 야영** 물어서 정했다.
   잃은 체력이 회복 수치보다 적으면 잃은 만큼만 회복하고, 가득이면 회복량 0으로 알린다.
   화면에서 막고 싶으면 `CampUsed` 이벤트의 회복량이 0인지 보면 된다.
8. **진열을 만드는 시점** 성소에 들어올 때 한 번 만들고 나갈 때까지 유지한다.
   진열은 `SanctumState`에 들어 있어 저장에 그대로 실리므로
   저장 시스템 기획서의 방 진입 자동 저장 뒤에 이어 해도 같은 진열이 나온다.
9. **골드를 여기에 둔 것** 골드는 성소 밖에서도 쓰지만 규칙을 정한 문서가 성소 기획서라 여기에 뒀다.
   저장소를 받으면 런 데이터 쪽으로 옮겨도 된다.
10. **커서를 올렸을 때의 확대** 인게임 화면 기획서 04 화면 공통 규칙의 카드 상태를 가져와 5퍼센트로 했다.

## 문양 변경은 고르기 화면에서 고른다

05 행상 구성 의 문양 변경은 **가진 문양 가운데 하나를 플레이어가 고르는 것**이다.
2026년 10월 5일에 고르기 화면(`CurrentBuild` 의 `ItemPickerScreenController`)을 만들어 그 화면에서 고르게 했다.

`SymbolChangeTargetRequested` 가 오면 `GameFlowController` 가 가진 문양 전부를 고르기 화면에 띄운다.
현재 빌드 화면처럼 정렬 칸과 오른쪽 태그 비중이 함께 나온다.
칸을 눌러 골라 두면 아래 줄에 `[해일] 을 무작위 문양으로 바꾼다` 가 적히고, `바꾸기` 를 누르면 `ConfirmSymbolChange` 로 넘긴다.
`취소` 나 ESC 면 `CancelSymbolChange` 로 넘기고 값은 치르지 않는다.

무엇으로 바뀌는지는 지금처럼 시드 난수가 정한다. 그래서 아래 줄에 `무작위 문양` 으로 적는다.

`SymbolChangePicker` 는 고르기 화면이 씬에 없을 때만 쓴다. 그때는 예전처럼 시드 난수로 하나 뽑아 확인 팝업에 보여 준다.

### 그림이 없는 자리

**문양 변경 칸은 그림이 없다.** 양탄자에 무늬는 그려져 있지만
거기가 누르는 자리라는 표시가 없어, 투명한 누르는 칸만 얹어 두었더니
화면에서 누를 데를 찾을 수가 없었다.

지금은 PSD 에서 잰 보라색 칸과 `문양 무작위 변경` 글자를 얹어 둔다.
`행상_문양변경.png` 를 `Art` 에 넣으면 확인용 씬이 그쪽을 쓰고 글자는 빠진다.

## 가득 찼을 때 사기

코인과 유물에는 소지 한도가 있다. 2026년 10월 9일 원재가 **사기 전에 버릴 것을 고르게** 하라고 정했다.

`SanctumState.TryBuy` 는 골드를 먼저 보고, 모자라지 않으면 `IMerchantCatalog.HasRoomFor` 로 자리를 본다.
자리가 없으면 값을 치르지 않고 `PurchaseResult.NoRoom` 을 돌려준다. 골드가 모자라면 그쪽이 먼저다. 버려도 살 수 없기 때문이다.
`SanctumController` 는 그 칸을 기다리게 두고 `RoomNeeded` 를 알린다.
흐름이 버릴 것 고르기 창을 띄우고, 고르면 `ConfirmPendingPurchase` 로 마저 사고, 포기하면 `CancelPendingPurchase` 로 사지 않는다.
문양은 한도가 없어 늘 산다.

## `Exit` 과 `Close` 는 다르다

| 부르는 때 | 무엇 | `Exited` |
| --- | --- | --- |
| 플레이어가 나가기를 눌렀다 | `Exit` | 울린다. 방이 끝난다 |
| 흐름이 다른 화면으로 넘어간다 | `Close` | 안 울린다. 화면만 치운다 |

**섞으면 깬 적도 없는 방이 완료로 저장된다.**
`GameFlowController` 가 화면을 치울 때 `Exit` 을 불렀더니
`Exited` 가 울려 방 완료가 저장되고 보상 화면까지 열렸다.
성소에서 지도 버튼을 누르면 그렇게 됐고, 런을 끝내는 도중에도 같은 일이 일어났다.

방 안에서 지도 버튼을 눌러 맵을 봤다가 그 방에 다시 들어오면
흐름이 `Enter` 가 아니라 `Open` 을 불러 **보던 진열을 그대로 이어 간다.**
새로 만들면 진열이 다시 굴려져 마음에 드는 물건이 나올 때까지
지도를 들락거리면 되고, 쓴 야영도 되살아난다.

## 생존 자원

체력으로 확정했다. 성소 기획서 v0.2 본문과 화면 예시에 칩이 한 군데도 없다.
상단 UI 바 기획서도 2026년 9월 30일에 남은 두 곳을 체력으로 고쳤다.

전투에서 거는 **코인(칩)** 은 이것과 다른 것이다. 행상은 코인도 팔지만
그 값을 치르는 것은 골드고, 체력과는 상관이 없다.

## 이 코드에 없는 것

기획서에서 다른 문서로 넘긴 부분이라 손대지 않았다.

- 문양, 코인, 유물의 실제 목록과 효과 (문양 · 코인 · 유물 기획서)
- 태그마다의 가치. 성소는 태그 가치의 합만 받아 가격을 계산한다 (문양 기획서)
- 아이템 상세 설명 팝업 (인게임 화면 기획서 14장). 커서가 올라갔다는 것만 이벤트로 알린다
- 상단 표시줄의 골드와 체력 표시 (상단 UI 바 기획서). 값이 바뀌었다는 것만 이벤트로 알린다
- 문양 변경에서 바꿀 문양을 고르는 화면. 목록을 이벤트로 넘기기만 하고 화면은 `CurrentBuild` 의 고르기 화면이 맡는다
- 성소와 행상의 배경, 모닥불과 상인 그림, 빨려들어가는 효과 (배경 요청 기획서)
- 방을 끝내고 맵으로 돌아가는 처리 (맵 코드)
