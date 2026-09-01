using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    [Header("노드 정보")]
    public int nodeIndex;
    public int[] nodeCoordinate = new int[2];
    public MapManager mapManager;
    public bool isSelectable;
    public HashSet<Node> nextNodes = new HashSet<Node>();
    public Button nodeButton;

    [Header("노드 종류별 이미지")]
    [Tooltip("0: 일반 전투, 1: 엘리트 전투, 2: 상점, 3: 회복, 4: 이벤트, 5: 보스")]
    public Image[] nodeImage;

    [Header("전투 노드 적 설정")]
    [SerializeField] private EnemyDefinitionData enemyDefinition;

    [Header("하이라이트 크기 애니메이션")]
    [SerializeField] private float targetScaleMultiplier = 1.2f;
    [SerializeField] private float pulseSpeed = 5f;

    private Coroutine pulseCoroutine;
    private Vector3 initialScale;
    private bool isInitialized;

    public EnemyDefinitionData EnemyDefinition => enemyDefinition;

    private void Awake()
    {
        InitializeNode();
        ApplyNodeImage();
    }

    private void OnEnable()
    {
        InitializeNode();

        if (isSelectable)
            StartPulseAnimation();
    }

    private void InitializeNode()
    {
        if (isInitialized)
            return;

        if (nodeButton == null)
            nodeButton = GetComponent<Button>();

        if (nodeButton == null)
            nodeButton = GetComponentInChildren<Button>(true);

        initialScale = transform.localScale;

        if (Mathf.Approximately(initialScale.z, 0f))
            initialScale.z = 1f;

        isInitialized = true;
    }

    public void SetNodeType(int type)
    {
        nodeIndex = type;
        ApplyNodeImage();
    }

    private void ApplyNodeImage()
    {
        if (nodeImage == null || nodeImage.Length == 0)
            return;

        for (int i = 0; i < nodeImage.Length; i++)
        {
            if (nodeImage[i] != null)
                nodeImage[i].gameObject.SetActive(false);
        }

        if (nodeIndex < 0 || nodeIndex >= nodeImage.Length)
        {
            Debug.LogWarning(
                $"{name}의 nodeIndex가 이미지 배열 범위를 벗어났습니다. nodeIndex: {nodeIndex}, 이미지 개수: {nodeImage.Length}",
                this
            );

            return;
        }

        Image activeImage = nodeImage[nodeIndex];

        if (activeImage == null)
            return;

        activeImage.gameObject.SetActive(true);

        // 현재 활성화된 노드 이미지를 Button의 Target Graphic으로 설정한다.
        // 이 설정이 있어야 일반/엘리트 이미지 모두 비활성 색상이 정상 적용된다.
        if (nodeButton != null)
            nodeButton.targetGraphic = activeImage;
    }

    public void SetEnemyDefinition(EnemyDefinitionData definition)
    {
        enemyDefinition = definition;
    }

    public void SetHighlight(bool on)
    {
        isSelectable = on;

        // 노드 자신과 자식에 존재하는 모든 Button을 함께 변경한다.
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button != null)
                button.interactable = on;
        }

        if (nodeButton != null)
            nodeButton.interactable = on;

        if (on)
            StartPulseAnimation();
        else
            StopPulseAnimation();
    }

    private void StartPulseAnimation()
    {
        if (!isActiveAndEnabled)
            return;

        if (pulseCoroutine != null)
            return;

        pulseCoroutine = StartCoroutine(PulseAnimation());
    }

    private IEnumerator PulseAnimation()
    {
        float startTime = Time.unscaledTime;

        Vector3 targetScale = new Vector3(
            initialScale.x * targetScaleMultiplier,
            initialScale.y * targetScaleMultiplier,
            initialScale.z
        );

        while (isSelectable)
        {
            float phase = (Time.unscaledTime - startTime) * pulseSpeed;
            float lerpTime = (Mathf.Sin(phase) + 1f) * 0.5f;

            transform.localScale = new Vector3(
                Mathf.Lerp(initialScale.x, targetScale.x, lerpTime),
                Mathf.Lerp(initialScale.y, targetScale.y, lerpTime),
                initialScale.z
            );

            yield return null;
        }

        pulseCoroutine = null;
        transform.localScale = initialScale;
    }

    private void StopPulseAnimation()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        if (isInitialized)
            transform.localScale = initialScale;
    }

    public void OnNodeClicked()
    {
        if (!isSelectable)
            return;

        if (mapManager == null)
        {
            Debug.LogError(
                $"{name}에 MapManager가 연결되지 않았습니다.",
                this
            );

            return;
        }

        mapManager.OnNodeSelected(this);
    }

    private void OnDisable()
    {
        StopPulseAnimation();
    }
}