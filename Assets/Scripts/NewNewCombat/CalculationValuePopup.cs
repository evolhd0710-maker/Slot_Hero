using System.Collections;
using TMPro;
using UnityEngine;

public class CalculationValuePopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text valueText;

    [SerializeField]
    private CanvasGroup canvasGroup;


    [Header("ø¨√‚")]
    [SerializeField, Min(0.01f)]
    private float duration = 0.7f;

    [SerializeField]
    private float moveDistance = 40f;


    private RectTransform rectTransform;

    private Vector2 startPosition;


    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();


        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponent<CanvasGroup>();
        }
    }


    public void Play(
        int value
    )
    {
        if (valueText != null)
        {
            valueText.text =
                $"+{value}";
        }


        if (rectTransform != null)
        {
            startPosition =
                rectTransform.anchoredPosition;
        }


        StartCoroutine(
            PlayRoutine()
        );
    }


    private IEnumerator PlayRoutine()
    {
        float elapsed =
            0f;


        if (canvasGroup != null)
        {
            canvasGroup.alpha =
                1f;
        }


        while (elapsed < duration)
        {
            elapsed +=
                Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );


            if (rectTransform != null)
            {
                rectTransform.anchoredPosition =
                    startPosition +
                    Vector2.up *
                    moveDistance *
                    t;
            }


            if (canvasGroup != null)
            {
                canvasGroup.alpha =
                    1f - t;
            }


            yield return null;
        }


        Destroy(
            gameObject
        );
    }
}