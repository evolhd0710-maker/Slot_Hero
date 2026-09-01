using System.Collections;
using UnityEngine;

public class SlotPanelSlider : MonoBehaviour
{
    [Header("이동시킬 슬롯 패널 (릴 전체를 감싸는 부모)")]
    public RectTransform slotPanel;

    [Header("왼쪽으로 밀리는 거리 (px)")]
    public float leftOffset = 300f;

    [Header("이동 시간")]
    public float slideDuration = 0.35f;

    private Vector2 centerPos;
    private Coroutine currentSlide;

    private void Awake()
    {
        centerPos = slotPanel.anchoredPosition;
    }

    public void SlideLeft()
    {
        Vector2 target = centerPos + new Vector2(-leftOffset, 0f);
        StartSlide(target);
    }

    public void SlideCenter()
    {
        StartSlide(centerPos);
    }

    public void ResetToCenterInstant()
    {
        if (currentSlide != null)
            StopCoroutine(currentSlide);
        slotPanel.anchoredPosition = centerPos;
    }

    private void StartSlide(Vector2 target)
    {
        if (currentSlide != null)
            StopCoroutine(currentSlide);

        currentSlide = StartCoroutine(Co_Slide(target));
    }

    private IEnumerator Co_Slide(Vector2 target)
    {
        Vector2 start = slotPanel.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // ease-in-out

            slotPanel.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        slotPanel.anchoredPosition = target;
    }
}