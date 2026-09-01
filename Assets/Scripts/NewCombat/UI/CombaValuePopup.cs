using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class CombatValuePopup : MonoBehaviour
{
    [Header("팝업 텍스트")]
    [SerializeField] private TMP_Text valueText;

    [Header("표시 시간")]
    [SerializeField, Min(0.01f)]
    private float duration = 0.8f;

    [Header("이동")]
    [SerializeField] private float riseDistance = 100f;
    [SerializeField] private float horizontalRandomRange = 20f;

    [Header("크기")]
    [SerializeField] private float startScale = 0.8f;
    [SerializeField] private float maximumScale = 1.2f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        if (valueText == null)
        {
            valueText =
                GetComponentInChildren<TMP_Text>(true);
        }

        if (valueText != null)
            valueText.raycastTarget = false;
    }

    public void Show(
        string value,
        Color color
    )
    {
        if (valueText == null)
        {
            Debug.LogError(
                $"{name}에 TMP_Text가 연결되지 않았습니다.",
                this
            );

            Destroy(gameObject);
            return;
        }

        valueText.text = value;
        valueText.color = color;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine =
            StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        if (duration <= 0f)
        {
            Destroy(gameObject);
            yield break;
        }

        Vector2 startPosition =
            rectTransform.anchoredPosition;

        startPosition.x += Random.Range(
            -horizontalRandomRange,
            horizontalRandomRange
        );

        Vector2 targetPosition =
            startPosition +
            Vector2.up * riseDistance;

        rectTransform.anchoredPosition =
            startPosition;

        rectTransform.localScale =
            Vector3.one * startScale;

        canvasGroup.alpha = 1f;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime +=
                Time.unscaledDeltaTime;

            float normalizedTime =
                Mathf.Clamp01(
                    elapsedTime / duration
                );

            rectTransform.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    normalizedTime
                );

            float scaleProgress =
                Mathf.Sin(
                    normalizedTime * Mathf.PI
                );

            float currentScale =
                Mathf.Lerp(
                    startScale,
                    maximumScale,
                    scaleProgress
                );

            rectTransform.localScale =
                Vector3.one * currentScale;

            if (normalizedTime < 0.5f)
            {
                canvasGroup.alpha = 1f;
            }
            else
            {
                canvasGroup.alpha =
                    Mathf.Lerp(
                        1f,
                        0f,
                        (normalizedTime - 0.5f) * 2f
                    );
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}