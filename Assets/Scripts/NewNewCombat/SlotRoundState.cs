public enum SlotRoundState
{
    // 첫 번째 베팅 가능
    WaitingFirstBet,

    // 첫 번째 슬롯 회전 중
    FirstSpin,

    // 첫 결과 확인 후
    // HOLD 선택 + 두 번째 베팅 가능
    SelectingHoldAndSecondBet,

    // 두 번째 슬롯 회전 중
    SecondSpin,

    // 플레이어 최종 계산 연출 중
    Resolving,

    // 플레이어 행동 종료
    // 적 행동이 끝나기를 기다리는 상태
    WaitingEnemyTurn
}