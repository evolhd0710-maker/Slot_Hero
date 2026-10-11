// 에디터 어셈블리용 빈 네임스페이스 선언이다. `Flow/Runtime/Data/LegacyNamespaces.cs` 참고.
//
// 비어 있는 네임스페이스는 어셈블리 밖으로 보이지 않는다.
// 런타임 쪽에 둔 선언만으로는 에디터 어셈블리(Editor 폴더들)의 `using SlotHero.Combat;` 이 깨져
// 여기에도 같은 선언을 둔다. 저장소 코드에 네임스페이스가 붙으면 함께 지운다.

namespace SlotHero.Combat
{
}
