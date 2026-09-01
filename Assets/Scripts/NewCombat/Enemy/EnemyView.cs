using UnityEngine;
using UnityEngine.UI;

public class EnemyView : MonoBehaviour
{
    [Header("적 이미지")]
    [SerializeField] private Image enemyImage;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;

    private RectTransform viewRect;

    private void Awake()
    {
        viewRect = transform as RectTransform;
    }

    public void ApplyDefinition(EnemyDefinitionData definition)
    {
        if (definition == null)
        {
            Clear();
            Debug.LogError("EnemyView에 적용할 적 데이터가 없습니다.", this);
            return;
        }

        if (enemyImage == null)
        {
            Debug.LogError("EnemyView에 Enemy Image가 연결되지 않았습니다.", this);
            return;
        }

        ResetViewTransform();

        Sprite sprite = definition.EnemySprite;

        enemyImage.sprite = sprite;
        enemyImage.color = Color.white;
        enemyImage.preserveAspect = true;
        enemyImage.raycastTarget = false;
        enemyImage.enabled = sprite != null;
        enemyImage.gameObject.SetActive(true);

        RectTransform imageRect = enemyImage.rectTransform;
        imageRect.sizeDelta = definition.VisualSize;
        imageRect.anchoredPosition = definition.VisualOffset;
        imageRect.localScale = Vector3.one;
        imageRect.localRotation = Quaternion.identity;

        ApplyAnimatorController(definition.AnimatorController);

        if (sprite == null)
            Debug.LogError($"{definition.name}에 Enemy Sprite가 없습니다.", definition);
    }

    private void ApplyAnimatorController(RuntimeAnimatorController controller)
    {
        if (animator == null)
            return;

        animator.enabled = false;
        animator.runtimeAnimatorController = controller;

        if (controller == null)
            return;

        animator.enabled = true;
        animator.Rebind();
        animator.Update(0f);
    }

    public void PlayAnimation(string triggerName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        if (string.IsNullOrWhiteSpace(triggerName))
            return;

        animator.SetTrigger(triggerName);
    }

    public void PlayIdle()
    {
        PlayAnimation("Idle");
    }

    public void PlayAttack()
    {
        PlayAnimation("Attack");
    }

    public void PlayHit()
    {
        PlayAnimation("Hit");
    }

    public void PlayDeath()
    {
        PlayAnimation("Death");
    }

    public void PlayStun()
    {
        PlayAnimation("Stun");
    }

    public void ResetViewTransform()
    {
        if (viewRect == null)
            viewRect = transform as RectTransform;

        if (viewRect == null)
            return;

        viewRect.anchoredPosition = Vector2.zero;
        viewRect.localScale = Vector3.one;
        viewRect.localRotation = Quaternion.identity;
    }

    public void Clear()
    {
        if (enemyImage != null)
        {
            enemyImage.sprite = null;
            enemyImage.enabled = false;
        }

        if (animator != null)
        {
            animator.enabled = false;
            animator.runtimeAnimatorController = null;
        }

        ResetViewTransform();
    }
}