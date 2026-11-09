using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Tools
{
    /// <summary>
    /// 태그 공격 이펙트 하나. 같은 프리팹을 여러 개 늘어놓거나 돌려 겹칠 수 있다.
    /// 자리와 간격은 화면 픽셀이다(1 단위 = 100 픽셀).
    /// </summary>
    [Serializable]
    public class TagFxLayer
    {
        [SerializeField] private GameObject _prefab;

        [Tooltip("파티클을 물들이는 색. 알파가 0 이면 원래 색 그대로 둔다.")]
        [SerializeField] private Color _tint = new Color(1f, 1f, 1f, 0f);

        [SerializeField] private float _scale = 1f;

        [Tooltip("좌우로 뒤집는다.")]
        [SerializeField] private bool _flipX;

        [Tooltip("위아래로 뒤집는다.")]
        [SerializeField] private bool _flipY;

        [Tooltip("재생 빠르기. 0.5 면 연출 시간이 두 배다.")]
        [SerializeField] private float _speed = 1f;

        [Tooltip("적 가운데에서 첫 이펙트까지(픽셀).")]
        [SerializeField] private Vector2 _offset;

        [Tooltip("이펙트를 돌리는 각도. 0 이면 위를 향한다. 90 이면 왼쪽을 향한다.")]
        [SerializeField] private float _angle;

        [SerializeField] private int _count = 1;

        [Tooltip("하나씩 늘어놓을 때 다음 것까지의 거리(픽셀).")]
        [SerializeField] private Vector2 _step;

        [Tooltip("하나씩 늘어놓을 때 다음 것을 더 돌리는 각도.")]
        [SerializeField] private float _stepAngle;

        [Tooltip("하나씩 늘어놓을 때 다음 것이 늦게 나오는 시간(초).")]
        [SerializeField] private float _stepDelay;

        public GameObject Prefab { get { return _prefab; } set { _prefab = value; } }
        public Color Tint { get { return _tint; } set { _tint = value; } }
        public float Scale { get { return _scale; } set { _scale = value; } }
        public bool FlipX { get { return _flipX; } set { _flipX = value; } }
        public bool FlipY { get { return _flipY; } set { _flipY = value; } }
        public float Speed { get { return _speed; } set { _speed = value; } }
        public Vector2 Offset { get { return _offset; } set { _offset = value; } }
        public float Angle { get { return _angle; } set { _angle = value; } }
        public int Count { get { return _count; } set { _count = value; } }
        public Vector2 Step { get { return _step; } set { _step = value; } }
        public float StepAngle { get { return _stepAngle; } set { _stepAngle = value; } }
        public float StepDelay { get { return _stepDelay; } set { _stepDelay = value; } }
    }

    /// <summary>태그 공격 소리 하나. 파일의 한 구간만 낼 수 있다.</summary>
    [Serializable]
    public class TagSoundLayer
    {
        [SerializeField] private AudioClip _clip;

        [Tooltip("파일의 이 지점(초)부터 낸다.")]
        [SerializeField] private float _start;

        [Tooltip("이만큼(초) 낸 뒤 끝을 줄인다. 0 이면 파일 끝까지 내되 1.5초가 넘으면 1.5초에서 줄인다.")]
        [SerializeField] private float _seconds;

        [SerializeField, Range(0f, 1f)] private float _volume = 1f;

        [Tooltip("공격이 닿은 뒤 이만큼(초) 늦게 낸다.")]
        [SerializeField] private float _delay;

        [Tooltip("끝을 줄이는 시간(초). 0 이면 0.3초다. 바로 뒤에 다른 소리가 붙은 파일은 짧게 줄여 그 소리가 새지 않게 한다.")]
        [SerializeField] private float _fadeSeconds;

        public AudioClip Clip { get { return _clip; } set { _clip = value; } }
        public float Start { get { return _start; } set { _start = value; } }
        public float Seconds { get { return _seconds; } set { _seconds = value; } }
        public float Volume { get { return _volume; } set { _volume = value; } }
        public float Delay { get { return _delay; } set { _delay = value; } }
        public float FadeSeconds { get { return _fadeSeconds; } set { _fadeSeconds = value; } }
    }

    /// <summary>
    /// 그림 한 장으로 도는 태그 공격 이펙트. 지금은 이것을 쓰는 안이 없다.
    /// 2026년 11월 9일까지 Unity AI 로 만든 목성 뿌리 그림에 썼고, 목성과 해왕성은 이제 파티클 프리팹을 쓴다.
    /// 나타나며 움직이고(들어오기), 잠시 머물며 조이고(머물기), 사라진다(나가기).
    /// 자리는 적 가운데 기준 픽셀이다.
    /// </summary>
    [Serializable]
    public class TagSpriteLayer
    {
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _color = Color.white;

        [Tooltip("그림 크기(픽셀).")]
        [SerializeField] private Vector2 _size = new Vector2(400f, 400f);

        [Tooltip("그림의 기준점. (0.5, 0) 이면 아래 가운데다. 커지기는 이 점을 중심으로 한다.")]
        [SerializeField] private Vector2 _pivot = new Vector2(0.5f, 0.5f);

        [SerializeField] private Vector2 _from;
        [SerializeField] private Vector2 _to;
        [SerializeField] private Vector2 _fromScale = Vector2.one;
        [SerializeField] private Vector2 _toScale = Vector2.one;

        [Tooltip("머무는 동안 가로로 조이는 정도. 0.15 면 가로가 85% 까지 좁아진다.")]
        [SerializeField] private float _squeeze;

        [SerializeField] private float _inSeconds = 0.25f;
        [SerializeField] private float _holdSeconds = 0.4f;
        [SerializeField] private float _outSeconds = 0.3f;
        [SerializeField] private float _delay;
        [SerializeField] private bool _flipX;

        public Sprite Sprite { get { return _sprite; } set { _sprite = value; } }
        public Color Color { get { return _color; } set { _color = value; } }
        public Vector2 Size { get { return _size; } set { _size = value; } }
        public Vector2 Pivot { get { return _pivot; } set { _pivot = value; } }
        public Vector2 From { get { return _from; } set { _from = value; } }
        public Vector2 To { get { return _to; } set { _to = value; } }
        public Vector2 FromScale { get { return _fromScale; } set { _fromScale = value; } }
        public Vector2 ToScale { get { return _toScale; } set { _toScale = value; } }
        public float Squeeze { get { return _squeeze; } set { _squeeze = value; } }
        public float InSeconds { get { return _inSeconds; } set { _inSeconds = value; } }
        public float HoldSeconds { get { return _holdSeconds; } set { _holdSeconds = value; } }
        public float OutSeconds { get { return _outSeconds; } set { _outSeconds = value; } }
        public float Delay { get { return _delay; } set { _delay = value; } }
        public bool FlipX { get { return _flipX; } set { _flipX = value; } }
    }

    /// <summary>공격 한 안. 이펙트와 소리를 함께 겹쳐 낸다.</summary>
    [Serializable]
    public class TagAttackPlan
    {
        [SerializeField] private string _name;
        [SerializeField] private TagFxLayer[] _effects = new TagFxLayer[0];
        [SerializeField] private TagSoundLayer[] _sounds = new TagSoundLayer[0];
        [SerializeField] private TagSpriteLayer[] _sprites = new TagSpriteLayer[0];

        public string Name { get { return _name; } set { _name = value; } }
        public TagSpriteLayer[] Sprites { get { return _sprites; } set { _sprites = value; } }
        public TagFxLayer[] Effects { get { return _effects; } set { _effects = value; } }
        public TagSoundLayer[] Sounds { get { return _sounds; } set { _sounds = value; } }
    }

    /// <summary>
    /// 태그별 공격 이펙트와 소리 후보를 눌러 보는 확인용 씬의 조종기.
    /// 원재가 2026년 11월 1일 태그마다 연상되는 이펙트와 소리를 바꾸자고 했고, 쓰기 전에 확인받기로 했다.
    ///
    /// 공격은 전투 화면(`CombatScreenView.FireLaser`)과 같은 차례로 흉내 낸다.
    /// 원판에서 태그 색 레이저가 뻗고, 적에게 닿는 순간 안의 이펙트가 터지고 소리가 난다.
    ///
    /// 플레이를 눌러야 움직인다. **게임 코드가 아니다.** 안을 고르면 전투 설정으로 옮긴다.
    /// </summary>
    public class TagAttackPreview : MonoBehaviour
    {
        /// 화면 1 단위가 몇 픽셀인지. 카메라 세로 크기 5.4 에 1080 픽셀이다.
        private const float PixelsPerUnit = 100f;

        [Header("태그")]
        [Tooltip("레이저를 물들이는 태그 색. 현재 빌드 설정의 태그 막대 색이다.")]
        [SerializeField] private Color _tagColor = Color.white;

        [Header("안")]
        [SerializeField] private TagAttackPlan[] _plans = new TagAttackPlan[0];

        [Tooltip("길이를 정하지 않은 소리가 이보다 길면 앞부분만 내고 끝을 줄인다(초).")]
        [SerializeField] private float _maxSoundSeconds = 1.5f;

        [Tooltip("소리의 끝을 줄이는 시간(초).")]
        [SerializeField] private float _soundFadeSeconds = 0.3f;

        [Header("자리")]
        [SerializeField] private Camera _camera;

        [Tooltip("레이저 그림. 원판 가운데에서 적 가운데까지 늘인다.")]
        [SerializeField] private Image _laser;

        [Tooltip("레이저가 출발하는 원판 자리.")]
        [SerializeField] private RectTransform _dial;

        [Tooltip("레이저가 닿고 이펙트가 터지는 적 그림.")]
        [SerializeField] private RectTransform _enemy;

        [Tooltip("레이저 굵기(픽셀). 전투 배치 설정의 LaserThickness 다.")]
        [SerializeField] private float _laserThickness = 90f;

        [Tooltip("파티클을 캔버스 위에 그리는 차례. 캔버스는 0 이다.")]
        [SerializeField] private int _sortingOrder = 100;

        [Header("레이저 시간")]
        [SerializeField] private float _laserGrowSeconds = 0.1f;
        [SerializeField] private float _laserHoldSeconds = 0.12f;
        [SerializeField] private float _laserFadeSeconds = 0.18f;
        [SerializeField] private float _laserWhiteMix = 0.3f;

        private readonly List<AudioSource> _voices = new List<AudioSource>();
        private readonly List<GameObject> _liveSprites = new List<GameObject>();

        /// <summary>안 수.</summary>
        public int PlanCount
        {
            get { return _plans != null ? _plans.Length : 0; }
        }

        /// <summary>안을 통째로 넣는다. 확인용 씬을 지을 때 쓴다.</summary>
        public void SetPlans(TagAttackPlan[] plans)
        {
            _plans = plans ?? new TagAttackPlan[0];
        }

        /// <summary>그 안의 이름. 버튼 글에 쓴다.</summary>
        public string GetPlanName(int plan)
        {
            return plan >= 0 && plan < PlanCount && _plans[plan] != null ? _plans[plan].Name : string.Empty;
        }

        /// <summary>그 안으로 공격한다. 레이저가 닿는 순간 이펙트와 소리가 난다.</summary>
        public void Attack(int plan)
        {
            if (!CanPlay(plan))
            {
                return;
            }

            StopAll();
            StartCoroutine(AttackRoutine(_plans[plan]));
        }

        /// <summary>그 안의 이펙트만 적 위에 터뜨린다.</summary>
        public void PlayEffects(int plan)
        {
            if (!CanPlay(plan))
            {
                return;
            }

            StartEffects(_plans[plan]);
        }

        /// <summary>그 안의 소리만 낸다.</summary>
        public void PlaySounds(int plan)
        {
            if (!CanPlay(plan))
            {
                return;
            }

            StopAll();
            StartSounds(_plans[plan]);
        }

        /// <summary>
        /// 그 안의 이펙트를 모두 놓고, 각각 몇 초 늦게 나오는지와 함께 돌려준다.
        /// 확인용 씬을 그림으로 뽑을 때 편집 모드에서 부른다.
        /// </summary>
        public List<KeyValuePair<GameObject, float>> SpawnPlanEffects(int plan)
        {
            List<KeyValuePair<GameObject, float>> spawned = new List<KeyValuePair<GameObject, float>>();
            if (plan < 0 || plan >= PlanCount || _plans[plan] == null)
            {
                return spawned;
            }

            TagFxLayer[] layers = _plans[plan].Effects;
            for (int l = 0; l < layers.Length; l++)
            {
                for (int i = 0; i < Mathf.Max(1, layers[l].Count); i++)
                {
                    GameObject fx = SpawnOne(layers[l], i, false);
                    if (fx != null)
                    {
                        spawned.Add(new KeyValuePair<GameObject, float>(fx, layers[l].StepDelay * i));
                    }
                }
            }

            return spawned;
        }

        private IEnumerator AttackRoutine(TagAttackPlan plan)
        {
            if (_laser == null || _dial == null || _enemy == null)
            {
                StartEffects(plan);
                StartSounds(plan);
                yield break;
            }

            RectTransform rect = _laser.rectTransform;
            Vector2 from = _dial.anchoredPosition;
            Vector2 to = _enemy.anchoredPosition;
            Vector2 delta = to - from;
            float length = delta.magnitude;

            rect.anchoredPosition = from;
            rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

            Color tint = Color.Lerp(_tagColor, Color.white, _laserWhiteMix);
            tint.a = 1f;
            _laser.color = tint;
            _laser.gameObject.SetActive(true);

            // 뻗는다. 빠르게 나가다 끝에서 느려진다.
            for (float t = 0f; t < _laserGrowSeconds; t += Time.deltaTime)
            {
                float k = t / _laserGrowSeconds;
                float eased = 1f - (1f - k) * (1f - k);
                rect.sizeDelta = new Vector2(length * eased, _laserThickness * Mathf.Lerp(0.5f, 1f, eased));
                yield return null;
            }

            rect.sizeDelta = new Vector2(length, _laserThickness);

            // 닿는 순간 이펙트와 소리.
            StartEffects(plan);
            StartSounds(plan);

            for (float t = 0f; t < _laserHoldSeconds; t += Time.deltaTime)
            {
                rect.sizeDelta = new Vector2(length, _laserThickness * (1f + 0.12f * Mathf.Sin(t * 60f)));
                yield return null;
            }

            for (float t = 0f; t < _laserFadeSeconds; t += Time.deltaTime)
            {
                float k = t / _laserFadeSeconds;
                rect.sizeDelta = new Vector2(length, _laserThickness * (1f - k));
                Color c = tint;
                c.a = 1f - k;
                _laser.color = c;
                yield return null;
            }

            _laser.gameObject.SetActive(false);
        }

        private void StartEffects(TagAttackPlan plan)
        {
            for (int l = 0; l < plan.Effects.Length; l++)
            {
                StartCoroutine(LayerRoutine(plan.Effects[l]));
            }

            TagSpriteLayer[] sprites = plan.Sprites ?? new TagSpriteLayer[0];
            for (int l = 0; l < sprites.Length; l++)
            {
                StartCoroutine(SpriteRoutine(sprites[l]));
            }
        }

        /// <summary>
        /// 그 안의 그림 이펙트를 머무는 순간의 모양으로 놓는다. 확인용 씬을 그림으로 뽑을 때 편집 모드에서 부른다.
        /// </summary>
        public List<GameObject> SpawnPlanSprites(int plan)
        {
            List<GameObject> spawned = new List<GameObject>();
            if (plan < 0 || plan >= PlanCount || _plans[plan] == null || _plans[plan].Sprites == null)
            {
                return spawned;
            }

            TagSpriteLayer[] sprites = _plans[plan].Sprites;
            for (int i = 0; i < sprites.Length; i++)
            {
                Image image = NewSpriteImage(sprites[i]);
                if (image != null)
                {
                    PoseSprite(image, sprites[i], sprites[i].To, sprites[i].ToScale, 1f);
                    spawned.Add(image.gameObject);
                }
            }

            return spawned;
        }

        private IEnumerator SpriteRoutine(TagSpriteLayer layer)
        {
            if (layer.Delay > 0f)
            {
                yield return new WaitForSeconds(layer.Delay);
            }

            Image image = NewSpriteImage(layer);
            if (image == null)
            {
                yield break;
            }

            _liveSprites.Add(image.gameObject);

            // 들어오기. 빠르게 나왔다가 끝에서 느려진다.
            for (float t = 0f; t < layer.InSeconds; t += Time.deltaTime)
            {
                float k = t / layer.InSeconds;
                float eased = 1f - (1f - k) * (1f - k);
                PoseSprite(image, layer,
                    Vector2.Lerp(layer.From, layer.To, eased),
                    Vector2.Lerp(layer.FromScale, layer.ToScale, eased),
                    Mathf.Clamp01(k * 2f));
                yield return null;
            }

            // 머물기. 가로로 조인다.
            for (float t = 0f; t < layer.HoldSeconds; t += Time.deltaTime)
            {
                float k = t / layer.HoldSeconds;
                Vector2 scale = layer.ToScale;
                scale.x *= 1f - layer.Squeeze * Mathf.Sin(k * Mathf.PI * 0.5f);
                PoseSprite(image, layer, layer.To, scale, 1f);
                yield return null;
            }

            // 나가기. 조인 채로 흐려진다.
            Vector2 last = layer.ToScale;
            last.x *= 1f - layer.Squeeze;
            for (float t = 0f; t < layer.OutSeconds; t += Time.deltaTime)
            {
                PoseSprite(image, layer, layer.To, last, 1f - t / layer.OutSeconds);
                yield return null;
            }

            _liveSprites.Remove(image.gameObject);
            Destroy(image.gameObject);
        }

        /// <summary>적 그림과 같은 캔버스에 그림 칸을 하나 만든다. 적보다 앞에 그린다.</summary>
        private Image NewSpriteImage(TagSpriteLayer layer)
        {
            if (layer == null || layer.Sprite == null || _enemy == null)
            {
                return null;
            }

            GameObject go = new GameObject("TagSprite", typeof(RectTransform), typeof(Image));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(_enemy.parent, false);
            rect.SetSiblingIndex(_enemy.GetSiblingIndex() + 1);
            rect.anchorMin = _enemy.anchorMin;
            rect.anchorMax = _enemy.anchorMax;
            rect.pivot = layer.Pivot;
            rect.sizeDelta = layer.Size;

            Image image = go.GetComponent<Image>();
            image.sprite = layer.Sprite;
            image.raycastTarget = false;
            image.preserveAspect = true;
            return image;
        }

        private void PoseSprite(Image image, TagSpriteLayer layer, Vector2 offset, Vector2 scale, float alpha)
        {
            RectTransform rect = image.rectTransform;
            rect.anchoredPosition = _enemy.anchoredPosition + offset;
            rect.localScale = new Vector3(layer.FlipX ? -scale.x : scale.x, scale.y, 1f);

            Color color = layer.Color;
            color.a *= alpha;
            image.color = color;
        }

        private IEnumerator LayerRoutine(TagFxLayer layer)
        {
            for (int i = 0; i < Mathf.Max(1, layer.Count); i++)
            {
                if (i > 0 && layer.StepDelay > 0f)
                {
                    yield return new WaitForSeconds(layer.StepDelay);
                }

                SpawnOne(layer, i, true);
            }
        }

        /// <summary>
        /// 이펙트 하나를 적 자리에 놓는다. 한 번만 돌고 사라지게 반복을 끈다.
        /// 플레이 중이면 바로 틀고 다 돌면 지운다.
        /// </summary>
        private GameObject SpawnOne(TagFxLayer layer, int index, bool play)
        {
            if (layer == null || layer.Prefab == null)
            {
                return null;
            }

            Vector3 at = _enemy != null ? _enemy.position : Vector3.zero;
            Vector2 shift = (layer.Offset + layer.Step * index) / PixelsPerUnit;
            at.x += shift.x;
            at.y += shift.y;

            // 캔버스는 카메라에서 100 떨어져 있다. 파티클은 그보다 앞에 놓고 그리는 차례도 높여 UI 위에 보이게 한다.
            if (_camera != null)
            {
                at.z = _camera.transform.position.z + 10f;
            }

            float angle = layer.Angle + layer.StepAngle * index;
            GameObject fx = Instantiate(layer.Prefab, at, Quaternion.Euler(0f, 0f, angle));

            // 좌우로 뒤집을 때는 축척을 음수로 둔다. 축척을 계층대로 따르게 해 두어 퍼지는 방향도 함께 뒤집힌다.
            // 그림 자체도 거울에 비추도록 파티클 뒤집기를 함께 켠다.
            float scale = layer.Scale;
            fx.transform.localScale = new Vector3(layer.FlipX ? -scale : scale, layer.FlipY ? -scale : scale, scale);

            // 묶음의 이펙트 프리팹 가운데 자기 소리를 내는 소리 칸이 든 것이 있다. 안의 소리와 섞이지 않게 뺀다.
            AudioSource[] packSounds = fx.GetComponentsInChildren<AudioSource>(true);
            for (int i = 0; i < packSounds.Length; i++)
            {
                packSounds[i].enabled = false;
            }

            float longest = 0f;
            ParticleSystem[] systems = fx.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem.MainModule main = systems[i].main;
                main.loop = false;
                main.prewarm = false;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                main.simulationSpeed = main.simulationSpeed * Mathf.Max(0.05f, layer.Speed);

                if (layer.Tint.a > 0f)
                {
                    main.startColor = Tint(main.startColor, layer.Tint);
                }

                float speed = Mathf.Max(0.05f, main.simulationSpeed);
                longest = Mathf.Max(longest, (main.startDelay.constantMax + main.duration + main.startLifetime.constantMax) / speed);

                ParticleSystemRenderer renderer = systems[i].GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = _sortingOrder + renderer.sortingOrder;

                    // 바닥에 눕혀 위에서 보게 만든 파티클은 정면 카메라에서 선처럼 납작해진다. 카메라를 보게 세운다.
                    if (renderer.renderMode == ParticleSystemRenderMode.HorizontalBillboard)
                    {
                        renderer.renderMode = ParticleSystemRenderMode.Billboard;
                    }

                    if (layer.FlipX || layer.FlipY)
                    {
                        renderer.flip = new Vector3(layer.FlipX ? 1f : renderer.flip.x, layer.FlipY ? 1f : renderer.flip.y, renderer.flip.z);
                    }

                    // 늘 카메라를 보는 파티클은 오브젝트를 돌려도 그림이 돌지 않는다. 돌릴 때는 오브젝트 방향을 따르게 한다.
                    if (!Mathf.Approximately(angle, 0f) && renderer.alignment == ParticleSystemRenderSpace.View)
                    {
                        renderer.alignment = ParticleSystemRenderSpace.Local;
                    }
                }
            }

            if (play && Application.isPlaying)
            {
                for (int i = 0; i < systems.Length; i++)
                {
                    systems[i].Play(false);
                }

                Destroy(fx, longest + 0.5f);
            }

            return fx;
        }

        private void StartSounds(TagAttackPlan plan)
        {
            for (int i = 0; i < plan.Sounds.Length; i++)
            {
                if (plan.Sounds[i] != null && plan.Sounds[i].Clip != null)
                {
                    StartCoroutine(SoundRoutine(plan.Sounds[i], GetVoice(i)));
                }
            }
        }

        /// <summary>소리 한 구간을 낸다. 길이를 정했거나 길면 끝을 줄인다.</summary>
        private IEnumerator SoundRoutine(TagSoundLayer layer, AudioSource voice)
        {
            if (layer.Delay > 0f)
            {
                yield return new WaitForSeconds(layer.Delay);
            }

            AudioClip clip = layer.Clip;
            float start = Mathf.Clamp(layer.Start, 0f, Mathf.Max(0f, clip.length - 0.01f));
            float remaining = clip.length - start;
            float seconds = layer.Seconds > 0f ? Mathf.Min(layer.Seconds, remaining) : Mathf.Min(remaining, _maxSoundSeconds);

            voice.Stop();
            voice.clip = clip;
            voice.volume = layer.Volume;
            voice.time = start;
            voice.Play();

            if (seconds >= remaining - 0.01f)
            {
                yield break;
            }

            float fade = layer.FadeSeconds > 0f ? layer.FadeSeconds : _soundFadeSeconds;
            yield return new WaitForSeconds(Mathf.Max(0f, seconds - fade));

            for (float t = 0f; t < fade; t += Time.deltaTime)
            {
                voice.volume = layer.Volume * (1f - t / fade);
                yield return null;
            }

            voice.Stop();
        }

        /// <summary>소리 겹마다 소리 자리를 하나씩 쓴다. 모자라면 만든다.</summary>
        private AudioSource GetVoice(int index)
        {
            while (_voices.Count <= index)
            {
                AudioSource voice = gameObject.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                _voices.Add(voice);
            }

            return _voices[index];
        }

        private void StopAll()
        {
            StopAllCoroutines();

            if (_laser != null)
            {
                _laser.gameObject.SetActive(false);
            }

            for (int i = 0; i < _voices.Count; i++)
            {
                _voices[i].Stop();
            }

            for (int i = 0; i < _liveSprites.Count; i++)
            {
                if (_liveSprites[i] != null)
                {
                    Destroy(_liveSprites[i]);
                }
            }

            _liveSprites.Clear();
        }

        /// <summary>파티클 시작 색에 태그 색을 곱한다. 흰 이펙트는 태그 색 그대로가 된다.</summary>
        private static ParticleSystem.MinMaxGradient Tint(ParticleSystem.MinMaxGradient source, Color tint)
        {
            switch (source.mode)
            {
                case ParticleSystemGradientMode.Color:
                    return new ParticleSystem.MinMaxGradient(source.color * tint);
                case ParticleSystemGradientMode.TwoColors:
                    return new ParticleSystem.MinMaxGradient(source.colorMin * tint, source.colorMax * tint);
                default:
                    return new ParticleSystem.MinMaxGradient(tint);
            }
        }

        private bool CanPlay(int plan)
        {
            if (plan < 0 || plan >= PlanCount || _plans[plan] == null)
            {
                return false;
            }

            if (Application.isPlaying)
            {
                return true;
            }

            Debug.Log("태그 공격 확인용 씬은 플레이를 눌러야 이펙트와 소리가 난다.");
            return false;
        }
    }
}
