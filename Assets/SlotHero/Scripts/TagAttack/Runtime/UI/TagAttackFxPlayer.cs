using System.Collections;
using SlotHero.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.TagAttack.UI
{
    /// <summary>
    /// 태그 공격 이펙트와 소리를 낸다. 전투 화면이 레이저가 적에게 닿는 순간 부른다(`CombatScreenView.FireLaser`).
    ///
    /// 파티클은 적 그림이 놓인 자리(캔버스 평면)보다 카메라 쪽에 놓고, 캔버스보다 높은 그리는 차례로 그린다.
    /// 그래서 캔버스의 다른 UI(상단 표시줄, 팝업)보다도 앞에 그려진다. 공격 이펙트는 적 주변에만 터지므로 겹치는 일은 드물다.
    /// 그림 이펙트는 적과 같은 캔버스에 그림 칸으로 띄워 적 바로 앞에 그린다.
    /// </summary>
    public class TagAttackFxPlayer : MonoBehaviour
    {
        /// 화면 1 단위가 몇 픽셀인지. 카메라 세로 크기 5.4 에 1080 픽셀이다.
        private const float PixelsPerUnit = 100f;

        [SerializeField] private TagAttackFxConfig _config;

        [Tooltip("태그 소리를 내는 소리 자리. 전투 화면의 효과음 자리를 함께 쓴다.")]
        [SerializeField] private AudioSource _audio;

        public TagAttackFxConfig Config { get { return _config; } }

        /// <summary>그 태그에 소리가 있는지. 있으면 전투 화면이 레이저 발사음을 내지 않는다.</summary>
        public bool HasSound(SymbolTagType tag)
        {
            TagAttackFxEntry entry = _config != null ? _config.GetEntry(tag) : null;
            return entry != null && entry.Sound != null;
        }

        /// <summary>그 태그에 이펙트(파티클이나 그림)가 있는지.</summary>
        public bool HasEffect(SymbolTagType tag)
        {
            TagAttackFxEntry entry = _config != null ? _config.GetEntry(tag) : null;
            return entry != null && (entry.Effect != null || entry.Sprite != null);
        }

        /// <summary>
        /// 그 태그의 이펙트를 적 자리에 터뜨리고 소리를 낸다.
        /// soundVolume 은 설정의 효과음 크기다(전체 × 효과음).
        /// </summary>
        public void Play(SymbolTagType tag, RectTransform target, float soundVolume)
        {
            TagAttackFxEntry entry = _config != null ? _config.GetEntry(tag) : null;
            if (entry == null || target == null)
            {
                return;
            }

            if (entry.Sound != null && _audio != null && soundVolume > 0f)
            {
                _audio.PlayOneShot(entry.Sound, entry.Volume * _config.SoundVolume * soundVolume);
            }

            if (entry.Effect != null)
            {
                SpawnEffect(entry, target);
            }

            if (entry.Sprite != null && isActiveAndEnabled)
            {
                StartCoroutine(SpriteRoutine(entry, target));
            }
        }

        private void SpawnEffect(TagAttackFxEntry entry, RectTransform target)
        {
            Canvas canvas = target.GetComponentInParent<Canvas>();
            Canvas root = canvas != null ? canvas.rootCanvas : null;
            Camera camera = root != null ? root.worldCamera : null;

            // 적 그림의 가운데. 전투 화면은 왼쪽 위를 기준점으로 두므로 position 이 아니라 사각형 가운데를 쓴다.
            // 캔버스는 카메라에서 떨어진 평면에 있다. 파티클은 그보다 카메라 쪽에 놓는다.
            Vector3 at = target.TransformPoint(target.rect.center);
            if (camera != null)
            {
                at.z = camera.transform.position.z + 10f;
            }

            GameObject fx = Instantiate(entry.Effect, at, entry.Effect.transform.rotation);
            int baseOrder = (root != null ? root.sortingOrder : 0) + _config.SortingOffset;

            float longest = 0f;
            ParticleSystem[] systems = fx.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem.MainModule main = systems[i].main;
                float speed = Mathf.Max(0.05f, main.simulationSpeed);
                longest = Mathf.Max(longest, (main.startDelay.constantMax + main.duration + main.startLifetime.constantMax) / speed);

                ParticleSystemRenderer renderer = systems[i].GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = baseOrder + renderer.sortingOrder;
                }

                systems[i].Play(false);
            }

            float life = entry.EffectSeconds > 0f ? entry.EffectSeconds : longest + 0.2f;
            Destroy(fx, life);
        }

        /// <summary>그림 이펙트. 들어오며 솟고, 머물며 가로로 조이고, 흐려지며 사라진다.</summary>
        private IEnumerator SpriteRoutine(TagAttackFxEntry entry, RectTransform target)
        {
            GameObject go = new GameObject("TagAttackSprite", typeof(RectTransform), typeof(Image));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(target.parent, false);
            rect.SetSiblingIndex(target.GetSiblingIndex() + 1);
            rect.anchorMin = target.anchorMin;
            rect.anchorMax = target.anchorMax;
            rect.pivot = entry.SpritePivot;
            rect.sizeDelta = entry.SpriteSize;

            Image image = go.GetComponent<Image>();
            image.sprite = entry.Sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            // 적 그림의 기준점이 가운데가 아닐 수 있어 가운데 자리를 따로 구한다.
            Vector2 center = target.anchoredPosition + Vector2.Scale(new Vector2(0.5f, 0.5f) - target.pivot, target.rect.size);
            rect.anchoredPosition = center + entry.SpriteOffset;

            for (float t = 0f; t < entry.SpriteInSeconds; t += Time.deltaTime)
            {
                float k = t / entry.SpriteInSeconds;
                float eased = 1f - (1f - k) * (1f - k);
                Pose(image, Vector2.Lerp(entry.SpriteFromScale, Vector2.one, eased), Mathf.Clamp01(k * 2f));
                yield return null;
            }

            for (float t = 0f; t < entry.SpriteHoldSeconds; t += Time.deltaTime)
            {
                float k = t / entry.SpriteHoldSeconds;
                Pose(image, new Vector2(1f - entry.SpriteSqueeze * Mathf.Sin(k * Mathf.PI * 0.5f), 1f), 1f);
                yield return null;
            }

            Vector2 squeezed = new Vector2(1f - entry.SpriteSqueeze, 1f);
            for (float t = 0f; t < entry.SpriteOutSeconds; t += Time.deltaTime)
            {
                Pose(image, squeezed, 1f - t / entry.SpriteOutSeconds);
                yield return null;
            }

            Destroy(go);
        }

        private static void Pose(Image image, Vector2 scale, float alpha)
        {
            image.rectTransform.localScale = new Vector3(scale.x, scale.y, 1f);
            Color color = Color.white;
            color.a = alpha;
            image.color = color;
        }
    }
}
