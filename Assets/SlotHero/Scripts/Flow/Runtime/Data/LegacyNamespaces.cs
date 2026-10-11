// 저장소 코드와 함께 컴파일되게 하는 빈 네임스페이스 선언이다.
//
// 내 코드는 저장소의 태그 열거형 `SymbolTagType` 을 `using SlotHero.Combat;` 으로 쓴다.
// 내 컴퓨터의 사본(`코드/기존`)은 저장소 코드에 `SlotHero.Combat` 을 붙여 두었지만,
// 깃허브 저장소의 원본은 네임스페이스 없이 전역에 둔다.
// 원본에서는 `SlotHero.Combat` 이 없어 using 이 깨지므로 여기서 비어 있는 선언을 둔다.
//
// 이렇게 두면 양쪽에서 다 컴파일된다.
//   - 저장소 원본: using 은 빈 네임스페이스를 가리키고, `SymbolTagType` 은 전역에서 찾는다
//   - 내 사본: using 이 `SlotHero.Combat.SymbolTagType` 을 찾는다
// 2026년 10월 11일 다른 컴퓨터에서 받았을 때 "namespace Combat 이 없다" 는 오류가 나서 두었다.
// 저장소 코드에도 네임스페이스가 붙으면 이 파일은 지운다.

namespace SlotHero.Combat
{
}
