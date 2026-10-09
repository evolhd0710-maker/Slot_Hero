using System;
using System.Collections.Generic;

namespace SlotHero.Save
{
    /// <summary>
    /// 진행 중인 런 하나를 담는다.
    /// 저장 시스템 기획서 v0.1 / 05 런 데이터 의 구성 개요와 상태·소유물 두 표를 옮긴 것이다.
    ///
    /// 프로필마다 하나만 둔다. 런이 끝나면 지운다.
    /// 맵 구조와 방 타입, 보상 후보 같은 것은 담지 않는다.
    /// 같은 시드에서 다시 만들어지기 때문이다. 10쪽의 "저장하지 않는 값"이 그렇게 정했다.
    /// </summary>
    [Serializable]
    public class RunSaveData
    {
        /// <summary>런을 시작할 때 매기는 고유 번호. 한 번 정하면 바뀌지 않는다.</summary>
        public int RunNumber;

        /// <summary>맵과 조우 몬스터와 보상과 행상 목록을 정하는 난수 값. 바뀌지 않는다.</summary>
        public int Seed;

        /// <summary>지금 진행 중인 스테이지. 1부터 센다.</summary>
        public int StageIndex = 1;

        /// <summary>
        /// 지금 있는 방.
        /// 기획서는 그리드 좌표라고 적었으나 맵 노드 번호를 대신 쓴다.
        /// 노드 번호가 시드에서 똑같이 다시 나오고 단계와 줄을 그대로 가리키기 때문이다.
        /// 아직 첫 방에 들어가지 않았으면 -1 이다.
        /// </summary>
        public int CurrentNodeId = -1;

        /// <summary>지금 있는 방을 깼는지.</summary>
        public bool CurrentRoomCleared;

        /// <summary>이번 스테이지에서 지금까지 들른 방 번호. 지도의 지나온 길을 되살리는 데 쓴다.</summary>
        public List<int> VisitedNodeIds = new List<int>();

        /// <summary>
        /// 이번 런에서 이미 만난 이벤트의 식별자. **같은 런에서는 같은 이벤트를 다시 보지 않는다.** 2026년 10월 9일 원재가 정했다.
        /// 이벤트 방에 들어가 이벤트가 정해지면 적고, 방 완료 저장에 함께 실린다.
        /// 방 안에서 나갔다 이어 하면 방에 들어갈 때의 목록으로 다시 뽑으므로 같은 이벤트가 그대로 나온다.
        /// 스테이지를 넘겨도 비우지 않는다. 이 칸이 없던 예전 저장은 빈 목록으로 읽힌다.
        /// </summary>
        public List<string> SeenEventIds = new List<string>();

        /// <summary>런 동안 변하는 캐릭터 수치와 재화.</summary>
        public RunStatusData Status = new RunStatusData();

        /// <summary>런에서 모은 문양과 유물과 코인.</summary>
        public RunOwnedData Owned = new RunOwnedData();

        /// <summary>이번 런에서 쌓인 통계.</summary>
        public RunStatisticsData Statistics = new RunStatisticsData();

        /// <summary>
        /// 유물이 나올 순서를 어디까지 꺼냈는지.
        ///
        /// **순서 자체는 시드에서 다시 만들 수 있지만 어디까지 꺼냈는지는 아니다.**
        /// 성소에 들어갈 때마다 셋, 새로고침할 때마다 셋을 꺼내는데 새로고침은 플레이어가 고른다.
        /// 이것을 저장하지 않으면 이어할 때 처음부터 다시 꺼내 이미 본 유물이 또 나온다.
        /// 2026년 10월 5일에 원재가 저장하기로 정했다.
        /// </summary>
        public RelicPoolSaveData RelicPool = new RelicPoolSaveData();

        /// <summary>
        /// 방을 깨고 아직 보상을 받지 않았는지.
        /// 켜져 있으면 이어할 때 그 방의 보상 화면에서 시작한다. 보상을 받으면 꺼진다.
        /// 모든 보상은 받을 때 한 번에 받으므로 반쯤 받은 상태는 없다(2026년 10월 6일 원재).
        /// 보상 내용은 저장하지 않는다. 시드와 방에서 다시 만들어진다. 오버킬만 아래에 따로 적는다.
        /// 2026년 10월 5일에 원재가 저장하기로 정했다.
        /// </summary>
        public bool RewardPending;

        /// <summary>
        /// 그 보상의 오버킬 골드. 전투에서 얼마나 넘치게 때렸는지에서 나와 시드로는 다시 만들 수 없다.
        /// 이어할 때 보상 화면을 다시 띄우면 이 값을 그대로 얹는다. 보상을 받으면 0 으로 돌아간다.
        /// 2026년 10월 6일 원재가 저장하기로 정했다.
        /// </summary>
        public int RewardOverkillGold;

        /// <summary>
        /// 그 보상에 합쳐지는 전투 골드. 전투가 정한 값이라 시드로는 다시 만들 수 없다.
        /// 전투를 이기는 순간 주지 않고 보상의 기본 골드에 더해 받을 때 한 번에 준다.
        /// 도전 이벤트가 연 전투면 두 배가 된 값이 들어 있다. 보상을 받으면 0 으로 돌아간다.
        /// 2026년 10월 6일 원재가 "전투 보상으로 받는 골드에 합친다" 고 정했다.
        /// </summary>
        public int RewardCombatGold;

        /// <summary>
        /// 이벤트가 연 전투에서 얻은 보상 고르기 횟수. 이벤트 방을 나갈 때 그만큼 카드를 고른다.
        /// 이긴 전투마다 1 이고 35 도전자 처럼 보상이 두 배면 2 다.
        /// 이벤트 기획서 3장 의 "전투 후 보상 선택(문양, 유물 등)을 2회 진행한다" 가 이것이다.
        /// 고르기 사이에 꺼져도 남은 횟수만큼 이어 고르게 저장한다. 방에 들어갈 때와 다 고른 뒤 0 으로 돌아간다.
        /// 2026년 10월 8일 외부 검토가 보상 두 배에 카드가 빠졌다고 짚어 더했다.
        /// </summary>
        public int RewardCardRounds;

        /// <summary>그 가운데 이미 고른 횟수. 고를 때마다 카드를 다르게 뽑는 데도 쓴다.</summary>
        public int RewardCardRoundsTaken;

        /// <summary>
        /// 저장 형식의 판 번호.
        /// 자료 구조가 바뀌면 이 값으로 예전 파일을 가려내 올린다.
        /// </summary>
        public int FormatVersion = SaveFormat.Current;

        /// <summary>올바르게 저장되었는지 검사하는 값. 저장할 때마다 다시 매긴다.</summary>
        public string Checksum = string.Empty;

        /// <summary>런을 새로 시작한다.</summary>
        public static RunSaveData StartNew(int runNumber, int seed, int maxHealth)
        {
            RunSaveData data = new RunSaveData();
            data.RunNumber = runNumber;
            data.Seed = seed;
            data.StageIndex = 1;
            data.CurrentNodeId = -1;
            data.CurrentRoomCleared = false;
            data.Status.MaxHealth = maxHealth;
            data.Status.Health = maxHealth;
            return data;
        }

        /// <summary>
        /// 값이 앞뒤가 맞는지 본다.
        /// 체크섬을 통과해도 안의 숫자가 말이 안 되면 읽지 않는다.
        /// </summary>
        public bool IsConsistent()
        {
            if (RunNumber <= 0 || StageIndex <= 0)
            {
                return false;
            }

            if (Status == null || Owned == null || Statistics == null)
            {
                return false;
            }

            if (!Status.IsConsistent())
            {
                return false;
            }

            // 아직 첫 방에 안 들어갔으면 들른 방도 없어야 하고 방을 깼을 리도 없다.
            if (CurrentNodeId < 0 && (CurrentRoomCleared || VisitedNodeIds.Count > 0))
            {
                return false;
            }

            return true;
        }
    }
}
