using System;
using SlotHero.Combat;
using UnityEngine;

namespace SlotHero.TagAttack
{
    /// <summary>
    /// 태그 하나의 공격 이펙트와 소리.
    /// 레이저가 적에게 닿는 순간 적 자리에 이펙트를 터뜨리고 소리를 낸다(2026년 11월 1일 원재).
    ///
    /// 이펙트는 둘 중 하나 또는 둘 다다.
    /// 파티클 프리팹(`Effect`)은 적 자리에 놓고 다 돌면 지운다.
    /// 그림 한 장(`Sprite`)은 적과 같은 캔버스에 띄워 들어오기, 머물기(가로로 조이기), 나가기로 움직인다.
    /// 자리와 크기는 화면 픽셀이고 적 그림 가운데가 기준이다.
    /// </summary>
    [Serializable]
    public class TagAttackFxEntry
    {
        [SerializeField] private SymbolTagType _tag;

        [Header("파티클")]
        [Tooltip("적 자리에 놓는 파티클 프리팹. `Slot Hero/태그 공격 이펙트 가져오기` 가 만들어 물린다.")]
        [SerializeField] private GameObject _effect;

        [Tooltip("이펙트를 이 시간(초)에서 잘라 지운다. 0 이면 파티클이 다 돌 때까지 둔다.")]
        [SerializeField] private float _effectSeconds;

        [Header("소리")]
        [Tooltip("레이저 발사음 대신 내는 소리. 고른 구간만 잘라 구운 파일이다(`TagAttack/Art/Sound`).")]
        [SerializeField] private AudioClip _sound;

        [SerializeField, Range(0f, 1f)] private float _volume = 1f;

        [Header("그림 이펙트")]
        [SerializeField] private Sprite _sprite;

        [Tooltip("그림 크기(픽셀).")]
        [SerializeField] private Vector2 _spriteSize = new Vector2(400f, 400f);

        [Tooltip("그림의 기준점. (0.5, 0) 이면 아래 가운데라 발밑에서 솟는다.")]
        [SerializeField] private Vector2 _spritePivot = new Vector2(0.5f, 0.5f);

        [Tooltip("적 가운데에서 그림 기준점까지(픽셀).")]
        [SerializeField] private Vector2 _spriteOffset;

        [Tooltip("들어오기를 시작할 때의 배율. 가로 1, 세로 0 이면 바닥에서 솟아오른다.")]
        [SerializeField] private Vector2 _spriteFromScale = new Vector2(1f, 0f);

        [Tooltip("머무는 동안 가로로 조이는 정도. 0.2 면 가로가 80% 까지 좁아진다.")]
        [SerializeField] private float _spriteSqueeze;

        [SerializeField] private float _spriteInSeconds = 0.25f;
        [SerializeField] private float _spriteHoldSeconds = 0.5f;
        [SerializeField] private float _spriteOutSeconds = 0.3f;

        public SymbolTagType Tag { get { return _tag; } set { _tag = value; } }
        public GameObject Effect { get { return _effect; } set { _effect = value; } }
        public float EffectSeconds { get { return _effectSeconds; } set { _effectSeconds = value; } }
        public AudioClip Sound { get { return _sound; } set { _sound = value; } }
        public float Volume { get { return _volume; } set { _volume = value; } }
        public Sprite Sprite { get { return _sprite; } set { _sprite = value; } }
        public Vector2 SpriteSize { get { return _spriteSize; } set { _spriteSize = value; } }
        public Vector2 SpritePivot { get { return _spritePivot; } set { _spritePivot = value; } }
        public Vector2 SpriteOffset { get { return _spriteOffset; } set { _spriteOffset = value; } }
        public Vector2 SpriteFromScale { get { return _spriteFromScale; } set { _spriteFromScale = value; } }
        public float SpriteSqueeze { get { return _spriteSqueeze; } set { _spriteSqueeze = value; } }
        public float SpriteInSeconds { get { return _spriteInSeconds; } set { _spriteInSeconds = value; } }
        public float SpriteHoldSeconds { get { return _spriteHoldSeconds; } set { _spriteHoldSeconds = value; } }
        public float SpriteOutSeconds { get { return _spriteOutSeconds; } set { _spriteOutSeconds = value; } }

        public TagAttackFxEntry()
        {
        }

        public TagAttackFxEntry(SymbolTagType tag)
        {
            _tag = tag;
        }
    }
}
