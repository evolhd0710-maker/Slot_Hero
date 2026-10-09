# Slot Hero 맵 생성 · 맵 화면 코드

맵 생성 규칙 기획서 v0.2와 인게임 화면 기획서 v0.2 / 09 맵 화면을 그대로 옮긴 코드다.
깃허브 저장소를 아직 못 받았으므로 기존 작업물과 겹치는 부분은 없다고 보고 새로 썼다.
저장소를 받으면 이 `Map` 폴더를 통째로 `Assets/Scripts/Map`으로 옮기면 된다.
`Editor` 폴더는 유니티가 알아서 에디터 전용으로 취급하므로 그대로 두면 된다.

## 파일 구성

```
Map/
  Runtime/
    Data/
      RoomType.cs            방 타입 열거형. 기획서 04 방 목록의 코드와 1:1
      MapNode.cs             방 하나. 단계, 줄, 방 타입, 지터, 앞뒤 간선
      MapEdge.cs             간선 하나
      StageMap.cs            스테이지 맵 전체. 저장 시스템에 그대로 실어 보낼 수 있다
      MapProgress.cs         진행 상태. 지나온 노드와 현재 위치, 선택 가능 판정
    Config/
      RoomTypeWeight.cs      방 타입 하나의 가중치
      MapGenerationConfig.cs 생성 수치 전부. ScriptableObject
      MapVisualConfig.cs     화면 크기와 색. ScriptableObject
    Generation/
      MapRandom.cs           시드 전용 난수. xorshift32
      MapGenerator.cs        03 맵 구조의 절차 ① ~ ⑧
      RoomTypeAssigner.cs    06 배치 규칙의 절차 ① ~ ⑫
      MapValidator.cs        보장 조건 검사
    UI/
      MapLayout.cs           격자 좌표를 픽셀 좌표로
      MapNodeView.cs         노드 하나의 표시
      MapEdgeView.cs         간선 하나의 표시
      MapScreenView.cs       맵 화면 표시 전체와 좌우 이동
      MapScreenController.cs 맵 생성, 노드 선택, 화면 여닫기
  Editor/
    MapGeneratorWindow.cs    에디터 미리보기와 일괄 검증 창
  Art/
    노드_일반.png            64 × 64, 알파
    노드_엘리트.png          64 × 64, 알파
    노드_성소.png            64 × 64, 알파
    노드_이벤트.png          64 × 64, 알파
    노드_보스.png            64 × 64, 알파
    노드_완료체크.png        32 × 32, 알파, 초록
```

## 아이콘

`Art` 폴더의 스프라이트를 `MapVisualConfig`의 방 타입 표시 목록에 넣고,
완료 체크는 노드 프리팹의 체크 `Image`에 넣는다.

유니티 임포트 설정은 다음과 같이 맞춘다.

- Texture Type: Sprite (2D and UI)
- Alpha Is Transparency 켜기
- Filter Mode: Bilinear, Compression: None 또는 High Quality

방 타입 구분은 아이콘 모양으로 하고 색은 진행 상태에만 쓴다.
그래서 `MapVisualConfig.UseRoomTypeColor`를 기본으로 꺼 뒀다.
맵 생성 규칙 기획서 04 방 목록의 노드 표시 색을 쓰려면 이 값을 켜면 된다.

아이콘은 원본이 43 ~ 55픽셀이라 70 × 70 노드에 넣으면 조금 늘어난다.
원본 벡터나 더 큰 PNG가 있으면 같은 이름으로 덮어쓰면 코드는 그대로 둬도 된다.

## 기획서 대응표

### 맵 생성 규칙 기획서 / 03 맵 구조 · 절차

| 기획서 | 코드 |
| --- | --- |
| ① 격자 준비 | `MapGenerator.GenerateOnce`의 `occupied` 배열 |
| ② 경로 그리기 1회차 | `MapGenerator.DrawPaths`, `run == 0` |
| ③ 경로 그리기 2회차 | `DrawPaths`의 `avoidFirstPath`. 1회차 노드를 피해 각 단계 최소 2개를 보장 |
| ④ 경로 그리기 3회차 | `PickStartRow`의 `needsFreshStart`. 시작 줄 3개를 보장 |
| ⑤ 4 ~ 6회차 | `DrawPaths`의 나머지 반복 |
| ⑥ 노드 확정 | `occupied`에서 노드를 만들고 `AddEdge`에서 중복 간선을 무시 |
| ⑦ 교차 정리 | `MapGenerator.RemoveCrossings` |
| ⑧ 방 노드 지터 | `MapGenerator.ApplyJitter` |

### 맵 생성 규칙 기획서 / 06 배치 규칙 · 절차

| 기획서 | 코드 |
| --- | --- |
| ① 고정 단계 배치 | `RoomTypeAssigner.AssignFixedStages` |
| ② ~ ⑥ 중간 성소 | `PlaceMidSanctums`, `SelectSanctumTarget`, `CollectNodesOnPathsThrough` |
| ⑦ 배분 대상 개수 | `CollectUnassigned` |
| ⑧ 토큰 수 계산 | `ChooseTokenCount` |
| ⑨ 타입별 토큰 분배 | `DistributeShares`, `BuildPool` |
| ⑩ 연속 불가 타입 우선 배치 | `PlaceNonConsecutiveFirst` |
| ⑪ 나머지 노드 배치 | `FillRemaining` |
| ⑫ 풀 초기화 | `Assign` 끝의 `pool.Clear()` |

### 인게임 화면 기획서 / 09 맵 화면

| 기획서 | 코드 |
| --- | --- |
| 지도 1600 × 800 | `MapVisualConfig.MapSize`. **와이어프레임대로 1300 × 500 이다.** 아래 "양피지" 를 본다 |
| 노드 70 × 70 | `MapVisualConfig.NodeSize`. 70 그대로다. 아이콘 모양은 칸보다 작다 |
| 간선은 노드에서 10픽셀 바깥부터 | `MapVisualConfig.EdgeGap`. **와이어프레임 실측대로 아이콘 모양에서 3 이다** |
| 완료한 방에 초록 체크 | `MapNodeDisplayState.Cleared` |
| 지나친 단계의 다른 방은 회색 | `MapNodeDisplayState.Skipped` |
| 이후 단계는 검은색 | `MapNodeDisplayState.Future`. 와이어프레임의 진한 갈색 #2B2016 |
| 지나온 간선은 진하게, 나머지는 흐리게 | `MapEdgeDisplayState`. 와이어프레임대로 지나친 방에 닿는 간선만 흐리다 |
| 좌우 이동만 지원, 세로 이동과 확대 축소 없음 | `MapScreenView.OnDrag`, `OnScroll` |
| 맵 화면이 열린 동안 지도 버튼 비활성 | 다음 방을 고르는 맵에서만 꺼진다. 아래 "지도 잠깐 보기" 를 본다 |

## 지도 잠깐 보기

2026년 10월 5일에 원재가 정했다. 방 진행 중에 상단 표시줄의 지도 버튼을 누르면
**방 화면을 닫지 않고** 그 위에 반투명 검은 막(`MapVisualConfig.PeekDimColor`)을 깔고 지도를 띄운다.
지도 버튼을 한 번 더 누르거나 ESC 를 누르면 걷힌다. 방은 보던 자리 그대로다.

| 지금 상황 | 지도 버튼 | 누르면 |
| --- | --- | --- |
| 방 진행 중 (성소, 이벤트, 전투, 보상) | 켜짐 | 지도를 잠깐 띄운다 |
| 지도를 잠깐 띄운 중 | 켜짐 | 방으로 돌아간다. ESC 도 같다 |
| 다음 방을 고르는 맵 | 꺼짐 | 없음 |
| 다음 방을 고르는 맵 + 현재 빌드 열림 | 켜짐 | 현재 빌드만 닫는다 |
| 방 진행 중 + 현재 빌드 열림 | 켜짐 | 현재 빌드를 닫고 지도를 잠깐 띄운다 |
| 설정이나 팝업이 떠 있음 | 꺼짐 | 없음 |

- 잠깐 띄운 지도에서는 노드를 고를 수 없다. 맵 진행 상태도 건드리지 않는다. `MapScreenController.OpenPeek`
- 다음 방을 고르는 맵은 `Open` 이고, 방을 끝냈을 때만 `ReturnToMap` 으로 지금 방을 깬 것으로 친다
- 게임 씬에서 맵은 방 화면과 보상 화면보다 위, 상단 표시줄과 현재 빌드와 설정과 팝업보다 아래에 놓인다
- 흐름 쪽 규칙은 `GameFlowController` 의 "지도 버튼" 묶음에 있다. 버튼 상태는 매 프레임 다시 본다

**예전에는 지도를 누르기만 해도 방이 끝났다.** 지도 버튼이 맵을 다시 여는 함수로 갔고,
그 함수가 지금 방을 깬 것으로 쳐서 보상도 없이 다음 방이 골라졌다. 돌아갈 길도 없었다.
지도 버튼을 끄는 함수도 아무 데서 부르지 않아 늘 켜져 있었다.

## 양피지

와이어프레임 `10 맵 화면` 의 `지도 배경 · 양피지_세로확장2.png` 를 노드 뒤에 깐다.
양 끝이 말린 두루마리다. 원본은 `Art/지도_양피지.png` 이고,
배경을 지울 때 남은 거의 투명한 붉은 찌꺼기만 털어 냈다.

**뷰포트 밖에 둔다.** 뷰포트는 노드가 지도 밖으로 안 나가게 잘라 내는데
양피지는 노드 영역보다 훨씬 커서 그 안에 두면 말린 끝이 잘린다.

### 지도 크기 1300 × 500

2026년 10월 3일에 원재가 "피그마에서 보이는 것과 거의 비슷하게" 로 정했다.
기획서의 1600 × 800 대신 와이어프레임 `10 맵 화면` 의 1300 × 500 을 쓴다.

양피지 종이 몸통이 화면 기준 가로 약 1270, 세로 약 510 이다.
1600 × 800 은 말린 끝과 찢긴 가장자리 밖으로 넘친다.
와이어프레임이 노드를 1300 × 500 에 놓은 까닭도 이것이다.

### 노드와 간선은 와이어프레임 실측에 맞췄다

피그마의 `맵 그래프` 와 `지나간 경로 그래프` 를 3배로 내보내 재었다.

| 항목 | 와이어프레임 | 지금 |
| --- | --- | --- |
| 일반 방 모양 | 57 × 48 | 55 × 51. 칸 70 에서 그림 64 가운데 50 × 47 |
| 노드와 간선 색 | #2B2016 | 같다. 간선은 불투명도 0.9 |
| 지나친 방 | #958E83 불투명 | 같다 |
| 지나친 방에 닿는 간선 | #968F84, 불투명도 0.78 | 같다 |
| 앞으로 갈 간선 | 지나온 간선과 같은 진한 색 | 같다 |
| 간선 두께 | 3 | 3 |
| 아이콘 모양과 간선 사이 | 3 | 3 |
| 완료 체크 | 약 31 × 25, 모양 오른쪽으로 6, 위로 4 튀어나옴 | 31.5 × 24.8, 6.5 와 3.1 |

**노드 아이콘을 흰 실루엣으로 바꿨다.** 예전 그림은 #242424 였다.
어두운 그림에 색을 곱하면 더 어두워지기만 해 갈색과 옅은 회색이 나올 수 없었다.
원본 어두운 그림은 `slot hero/맵 노드 아이콘` 에 그대로 있다.
상단 표시줄의 현재 방 아이콘도 같은 그림을 쓰므로 `TopBarVisualConfig.RoomIconColor` 로 예전 #242424 를 입힌다.

**10 단위 규칙에서 뺀 값이 둘 있다.** 간선 두께 3 과 완료 체크 36 이다.
10 단위로 맞추면 와이어프레임과 눈에 띄게 달라진다.

### 노드 칸끼리는 겹칠 수 있다

이웃 노드가 지터 끝까지 서로를 향해 쏠리면 가로로 중심 사이가 55.9 까지 좁아진다.
칸 70 끼리는 겹치지만 아이콘 모양은 대부분 닿지 않는다.
가장 넓은 이벤트 방 모양 둘이 같은 줄 이웃 단계에서 끝까지 쏠릴 때만 3.6 겹친다. 드물어서 받아들였다.

**누르는 영역은 아이콘 모양 크기로 줄였다.** 아이콘 Image 의 `raycastPadding` 을 칸 각 변에서 7.7 씩 준다.
누르는 영역 54.6 은 가장 가까운 중심 거리 55.9 보다 작아 엉뚱한 방이 눌리지 않는다.
`MapVisualConfig.ClickAreasNeverOverlap` 과 `NodesNeverOverlap` 을 검사가 본다.

## 쓰는 법

### 1. 에셋 만들기

프로젝트 창에서 우클릭 후 `Create > Slot Hero > 맵 > 맵 생성 설정`과 `맵 화면 표시 설정`을 하나씩 만든다.
기본값이 기획서 스테이지 1 기준이라 그대로 두면 된다.
이후 스테이지는 이 에셋을 복제해 단계 수, 가중치, 고정 배치 값만 바꾼다.

### 2. 씬 구성

```
MapScreen                 MapScreenController, 화면 루트
  Parchment               Image · 지도_양피지, 1800 × 1200, 가운데. 뷰포트 밖에 둔다
  Viewport                MapScreenView, Image(투명, Raycast Target 켬), Mask 또는 RectMask2D
    Content               크기 MapSize, 앵커와 피벗 가운데
      EdgeLayer           빈 RectTransform
      NodeLayer           빈 RectTransform
```

노드 프리팹은 루트에 `MapNodeView` + `Button` + `Image`, 자식에 아이콘 `Image`와 완료 체크 `Image`를 둔다.
간선 프리팹은 루트에 `MapEdgeView` + `Image` 하나만 두고 피벗을 (0, 0.5)로 맞춘다. 피벗은 코드에서도 다시 맞춘다.

### 3. 호출

```csharp
// 런 시작
mapScreen.StartNewStage(runSeed, stageIndex: 1);
mapScreen.Open();

// 노드를 골랐을 때
mapScreen.RoomEntered += node => { /* node.RoomType에 맞는 방으로 이동 */ };

// 방을 끝냈을 때
mapScreen.ClearCurrentRoom();
mapScreen.Open();

// 저장에서 불러올 때
mapScreen.Load(savedMap, savedProgress);
```

`StageMap`과 `MapProgress`는 둘 다 `[Serializable]`이라 `JsonUtility`로 그대로 직렬화된다.
불러온 뒤에는 `StageMap.RebuildIndex()`를 부르며, `MapScreenController.Load`가 알아서 부른다.

### 4. 에디터에서 확인

메뉴 `Slot Hero > 맵 생성 미리보기`를 열고 설정 에셋을 넣은 뒤 생성을 누르면
노드와 간선이 색으로 그려지고 검증 결과가 나온다.
아래의 일괄 검증은 시드를 바꿔 가며 한꺼번에 돌려 실패 사례와 방 타입 평균을 알려 준다.

## 검증 결과

모노 컴파일러로 순수 로직만 따로 빌드해 시드 3000개를 돌렸다.

- 3000개 전부 검증 통과
- 노드 수 31 ~ 47개, 평균 39.6개
- 시작 노드 3 ~ 5개
- 맵 하나당 평균 방 타입: 일반 21.2 · 이벤트 6.9 · 엘리트 5.6 · 성소 4.9 · 보스 1
- 5 ~ 8단계의 중간 성소는 2 ~ 6개, 3개가 가장 많음
- 같은 시드는 노드, 간선, 방 타입, 지터까지 완전히 같은 결과

검증 항목은 단계당 노드 수, 시작 노드 수, 고정 단계, 간선의 이동 폭,
모든 노드의 앞뒤 연결, 시작에서 보스까지의 도달, 성소와 엘리트의 연속 배치 금지다.

## 기획서에 없어 내가 정한 부분

확인이 필요하면 알려 주면 고친다.

1. **중간 성소 ③의 동점 처리** 기획서는 "적당한 노드 하나를 선택한다"이다.
   간선 수가 같은 노드 중 시드 난수로 하나를 골랐다.
2. **④ 후보 제거의 범위** "성소에 연결되는 경로에 있는 노드"를 그 성소의 앞뒤로 이어지는 모든 노드로 봤다.
   같은 단계의 다른 갈래는 후보로 남으므로 중간 성소가 보통 3개쯤 나온다.
3. **⑩에서 자리가 모자랄 때** 연속 배치 금지 때문에 놓을 자리가 없으면 남은 토큰은 버린다.
   ⑫에서 어차피 풀을 비우므로 같은 처리로 봤다.
4. **닿을 수 없는 방의 표시** 기획서는 "지나친 단계의 다른 방"만 회색으로 정했는데,
   맵 화면 이미지에서는 앞으로 갈 수 없는 방도 회색으로 보여 그렇게 맞췄다.
   `MapScreenView`의 `_dimUnreachable`을 끄면 기획서 문구 그대로 동작한다.
5. **고를 수 있는 방의 강조** 기획서 04 화면 공통 규칙의 카드 상태를 가져와 5퍼센트 확대로 했다.
6. **11, 12단계 노드의 지터** 단일 노드에도 지터를 넣었다. 빼려면 `ApplyJitter`에서 단계를 걸러 주면 된다.

## 이 코드에 없는 것

기획서에서 다른 문서로 넘긴 부분이라 손대지 않았다.

- 상단 표시줄 (상단 UI 바 기획서)
- 현재 빌드 버튼과 자동 저장 표시 (인게임 화면 기획서 04장, 13장)
- 각 방 안의 조우, 보상, 이벤트 내용
- 맵 배경 이미지와 두루마리 연출
- 스테이지 전환과 2스테이지 이후의 설정값
