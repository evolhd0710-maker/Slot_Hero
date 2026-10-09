using UnityEngine;

namespace SlotHero.Ui
{
    /// <summary>
    /// 칸 뒤에 깔린 그림자가 그 칸을 따라다니게 한다.
    ///
    /// 화면들은 자리를 `ApplyLayout` 에서 잡는다.
    /// 보상 박스처럼 카드 개수에 따라 크기가 바뀌는 칸도 있다.
    /// 그림자를 한 번 맞춰 두면 그때부터 어긋나므로 매 프레임 따라가게 한다.
    ///
    /// 그림자는 칸의 형제이고 바로 앞자리에 있다.
    /// 자식으로 두면 부모가 먼저 그려져 그림자가 칸 위에 올라온다.
    /// </summary>
    [ExecuteAlways]
    public class ShadowFollower : MonoBehaviour
    {
        [Header("따라갈 칸")]
        [SerializeField] private RectTransform _target;

        [Header("치수")]
        [Tooltip("칸보다 사방으로 이만큼 넓게 깐다.")]
        [SerializeField] private float _spread = UiScale.Px(10f);

        [Tooltip("칸보다 이만큼 아래로 내린다. 빛이 위에서 오므로 그림자는 아래에 생긴다.")]
        [SerializeField] private float _drop = UiScale.Px(6f);

        private RectTransform _rect;

        /// <summary>따라가는 칸.</summary>
        public RectTransform Target
        {
            get { return _target; }
        }

        private void LateUpdate()
        {
            Follow();
        }

        /// <summary>지금 칸 자리에 맞춘다.</summary>
        public void Follow()
        {
            if (_target == null)
            {
                return;
            }

            if (_rect == null)
            {
                _rect = (RectTransform)transform;
            }

            // 기준점과 피벗을 그대로 베껴야 칸이 어느 모서리를 기준으로 놓이든 따라간다.
            _rect.anchorMin = _target.anchorMin;
            _rect.anchorMax = _target.anchorMax;
            _rect.pivot = _target.pivot;

            _rect.sizeDelta = _target.sizeDelta + new Vector2(_spread * 2f, _spread * 2f);
            _rect.anchoredPosition = _target.anchoredPosition + new Vector2(0f, -_drop);

            // 칸이 꺼지면 그림자도 꺼진다.
            if (gameObject.activeSelf != _target.gameObject.activeSelf)
            {
                gameObject.SetActive(_target.gameObject.activeSelf);
            }
        }

        /// <summary>퍼짐과 내림을 정한다. 씬을 만드는 쪽이 쓴다.</summary>
        public void SetOffsets(float spread, float drop)
        {
            _spread = spread;
            _drop = drop;
        }

        /// <summary>그림자가 칸보다 뒤에 그려지는 자리에 있는지.</summary>
        public bool IsBehindTarget()
        {
            if (_target == null || transform.parent != _target.parent)
            {
                return false;
            }

            return transform.GetSiblingIndex() < _target.GetSiblingIndex();
        }
    }
}
