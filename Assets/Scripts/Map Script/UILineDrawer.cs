using UnityEngine;
using UnityEngine.UI;

public class UILineDrawer : MonoBehaviour
{
    public RectTransform startPoint; // 시작 노드
    public RectTransform endPoint;   // 끝 노드
    private RectTransform lineRect;
    private Image lineImage;

    void Awake()
    {
        lineRect = GetComponent<RectTransform>();
        lineImage = GetComponent<Image>();
    }
    private void Update()
    {
        UpdateLine();   
    }
    public void UpdateLine()
    {
        if (startPoint == null || endPoint == null) return;

        // 두 점의 중간 지점을 선의 중심으로 설정
        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;
        lineRect.position = (startPos + endPos) / 2;

        // 선의 길이 = 두 점 사이 거리
        float distance = Vector3.Distance(startPos, endPos);
        lineRect.sizeDelta = new Vector2(distance, 5f); // 선의 두께 조정 가능

        // 두 점을 잇는 방향으로 회전
        Vector3 direction = (endPos - startPos).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
    }
}
