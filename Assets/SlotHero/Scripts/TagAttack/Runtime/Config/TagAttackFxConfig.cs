using System.Collections.Generic;
using SlotHero.Combat;
using UnityEngine;

namespace SlotHero.TagAttack
{
    /// <summary>
    /// 태그별 공격 이펙트와 소리 설정. 원재가 확인용 씬에서 보고 고른 것이다(2026년 11월 1일).
    /// 태그마다 연상되는 것은 원재가 정했다. 수성 물, 금성 금속, 지구 생명, 화성 불, 목성 나무,
    /// 토성 모래먼지, 천왕성 바람, 해왕성 바다, 명왕성 검은색과 죽음.
    ///
    /// 프리팹, 소리, 그림은 `Slot Hero/태그 공격 이펙트 가져오기`(`ImportTagAttack`)가 물린다.
    /// 여기 기본값은 그 밖의 수치(그림 움직임, 자르는 시간)다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TagAttackFxConfig",
        menuName = "Slot Hero/태그 공격/태그 공격 이펙트 설정",
        order = 0)]
    public class TagAttackFxConfig : ScriptableObject
    {
        [Tooltip("태그마다 하나. 없는 태그는 레이저와 레이저 발사음만 난다.")]
        [SerializeField] private List<TagAttackFxEntry> _entries = DefaultEntries();
        public List<TagAttackFxEntry> Entries { get { return _entries; } set { _entries = value; } }

        [Tooltip("모든 태그 소리에 곱하는 크기. 설정의 효과음 크기도 함께 곱한다.")]
        [SerializeField, Range(0f, 1f)] private float _soundVolume = 1f;
        public float SoundVolume { get { return _soundVolume; } set { _soundVolume = value; } }

        [Tooltip("파티클을 캔버스보다 앞에 그리는 차례. 캔버스의 그리는 차례에 더한다.")]
        [SerializeField] private int _sortingOffset = 10;
        public int SortingOffset { get { return _sortingOffset; } set { _sortingOffset = value; } }

        /// <summary>그 태그의 설정. 없으면 null.</summary>
        public TagAttackFxEntry GetEntry(SymbolTagType tag)
        {
            if (_entries == null)
            {
                return null;
            }

            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i] != null && _entries[i].Tag == tag)
                {
                    return _entries[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 태그 아홉의 기본값. 프리팹과 소리는 가져오기가 물리므로 비워 둔다.
        /// 토성은 처음 1초 뒤의 연출을 자른다(2026년 11월 1일 원재).
        /// 목성은 Unity AI 로 만든 뿌리 그림이 발밑에서 솟아 적을 감고 조인다. 느린 안(2번)을 골랐다.
        /// </summary>
        public static List<TagAttackFxEntry> DefaultEntries()
        {
            List<TagAttackFxEntry> entries = new List<TagAttackFxEntry>();
            for (int i = 0; i <= (int)SymbolTagType.Pluto; i++)
            {
                entries.Add(new TagAttackFxEntry((SymbolTagType)i));
            }

            entries[(int)SymbolTagType.Saturn].EffectSeconds = 1f;

            TagAttackFxEntry jupiter = entries[(int)SymbolTagType.Jupiter];
            jupiter.SpriteSize = new Vector2(460f, 520f);
            jupiter.SpritePivot = new Vector2(0.5f, 0f);
            jupiter.SpriteOffset = new Vector2(0f, -240f);
            jupiter.SpriteFromScale = new Vector2(1f, 0f);
            jupiter.SpriteSqueeze = 0.2f;
            jupiter.SpriteInSeconds = 0.4f;
            jupiter.SpriteHoldSeconds = 0.8f;
            jupiter.SpriteOutSeconds = 0.4f;

            return entries;
        }
    }
}
