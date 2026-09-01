using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    /*
     * 노드 종류
     *
     * 0 = 일반 전투
     * 1 = 엘리트 전투
     * 2 = 상점
     * 3 = 회복
     * 4 = 이벤트
     * 5 = 보스 전투
     */

    [Header("노드 프리팹")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject destNodePrefab;
    [SerializeField] private GameObject linePrefab;

    [Header("맵 UI")]
    [SerializeField] private GameObject mapCanvas;
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private RectTransform nodeSet;
    [SerializeField] private RectTransform lineSet;

    [Header("시작 노드")]
    [SerializeField] private Node logicalStartNode;

    [Header("맵 크기")]
    [SerializeField, Min(1)] private int mapWidth = 7;
    [SerializeField, Min(1)] private int mapHeight = 15;
    [SerializeField, Min(1)] private int pathCount = 6;

    [Header("노드 종류 확률")]
    [SerializeField, Range(0, 100)] private int normalBattleChance = 80;

    [Header("노드 배치")]
    [SerializeField] private float horizontalSpacing = 100f;
    [SerializeField] private float verticalSpacing = 140f;

    [Header("라인")]
    [SerializeField] private float lineThickness = 2f;

    [Header("마우스 맵 이동")]
    [SerializeField] private RectTransform upperBound;
    [SerializeField] private RectTransform lowerBound;
    [SerializeField] private float mapScrollSpeed = 500f;

    [Header("현재 이동 가능 행 포커스")]
    [SerializeField] private RectTransform availableRowFocusTarget;
    [SerializeField, Min(0f)] private float focusMoveDuration = 0.35f;
    [SerializeField]
    private AnimationCurve focusMoveCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("전투 씬")]
    [SerializeField] private string normalBattleSceneName = "BattleScene";
    [SerializeField] private string bossBattleSceneName = "BossBattleScene";

    [Header("일반 전투 적 목록")]
    [SerializeField]
    private List<EnemyDefinitionData> normalEnemyDefinitions =
        new List<EnemyDefinitionData>();

    [Header("엘리트 전투 적 목록")]
    [SerializeField]
    private List<EnemyDefinitionData> eliteEnemyDefinitions =
        new List<EnemyDefinitionData>();

    [Header("보스 적")]
    [SerializeField] private EnemyDefinitionData bossEnemyDefinition;

    // 플레이어가 현재 위치한 노드
    public Node currentNode;

    // 일반 노드를 좌표별로 저장한다.
    private GameObject[,] nodes;

    // 맵 마지막의 보스 노드
    private Node destinationNode;

    // UI 좌표 변환에 사용하는 Canvas와 Camera
    private Canvas rootCanvas;
    private Camera uiCamera;

    private Coroutine focusCoroutine;

    // 전투 씬 중복 로드를 방지한다.
    private bool isBattleSceneLoading;

    private void Awake()
    {
        nodes = new GameObject[mapWidth, mapHeight];

        if (mapCanvas != null)
            rootCanvas = mapCanvas.GetComponentInParent<Canvas>();

        if (rootCanvas != null &&
            rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = rootCanvas.worldCamera;
        }
    }

    private void Start()
    {
        if (!ValidateReferences())
            return;

        logicalStartNode.mapManager = this;
        logicalStartNode.nextNodes.Clear();

        currentNode = logicalStartNode;

        CreateMap();

        // 시작 노드를 선택한 상태로 만들어 첫 행을 활성화한다.
        OnNodeSelected(logicalStartNode);

        // 첫 번째 선택 가능 행을 화면 기준점에 즉시 맞춘다.
        FocusAvailableRow(true);
    }

    private void Update()
    {
        UpdateMouseScrolling();
    }

    /// <summary>
    /// 맵 생성에 필요한 Inspector 참조를 확인한다.
    /// </summary>
    private bool ValidateReferences()
    {
        if (nodePrefab == null)
        {
            Debug.LogError("Node Prefab이 연결되지 않았습니다.", this);
            return false;
        }

        if (destNodePrefab == null)
        {
            Debug.LogError("Dest Node Prefab이 연결되지 않았습니다.", this);
            return false;
        }

        if (linePrefab == null)
        {
            Debug.LogError("Line Prefab이 연결되지 않았습니다.", this);
            return false;
        }

        if (mapContent == null)
        {
            Debug.LogError("Map Content가 연결되지 않았습니다.", this);
            return false;
        }

        if (nodeSet == null)
        {
            Debug.LogError("Node Set이 연결되지 않았습니다.", this);
            return false;
        }

        if (lineSet == null)
        {
            Debug.LogError("Line Set이 연결되지 않았습니다.", this);
            return false;
        }

        if (logicalStartNode == null)
        {
            Debug.LogError("Logical Start Node가 연결되지 않았습니다.", this);
            return false;
        }

        if (availableRowFocusTarget == null)
        {
            Debug.LogError("Available Row Focus Target이 연결되지 않았습니다.", this);
            return false;
        }

        if (nodeSet.parent != mapContent || lineSet.parent != mapContent)
        {
            Debug.LogError(
                "Node Set과 Line Set은 반드시 Map Content의 자식이어야 합니다.",
                this
            );

            return false;
        }

        return true;
    }

    /// <summary>
    /// 여러 개의 랜덤 경로와 마지막 보스 노드를 생성한다.
    /// </summary>
    private void CreateMap()
    {
        List<int> availableFirstColumns = new List<int>();

        for (int column = 0; column < mapWidth; column++)
            availableFirstColumns.Add(column);

        for (int pathIndex = 0; pathIndex < pathCount; pathIndex++)
        {
            int currentColumn;

            // 처음 두 경로는 첫 행에서 서로 다른 위치로 시작한다.
            if (pathIndex < 2 && availableFirstColumns.Count > 0)
            {
                int randomIndex =
                    Random.Range(0, availableFirstColumns.Count);

                currentColumn =
                    availableFirstColumns[randomIndex];

                availableFirstColumns.RemoveAt(randomIndex);
            }
            else
            {
                currentColumn = Random.Range(0, mapWidth);
            }

            Node firstNode = GetOrCreateNode(currentColumn, 0);

            if (firstNode == null)
                continue;

            AddNextNodeUnique(logicalStartNode, firstNode);

            // 첫 행부터 마지막 일반 행까지 경로를 생성한다.
            for (int row = 1; row < mapHeight; row++)
            {
                int previousColumn = currentColumn;

                currentColumn = GetNextColumn(currentColumn);

                Node previousNode =
                    GetNode(previousColumn, row - 1);

                Node nextNode =
                    GetOrCreateNode(currentColumn, row);

                CreateConnection(previousNode, nextNode);
            }
        }

        // 일반 경로 위에 보스 노드를 생성한다.
        destinationNode = CreateDestinationNode();

        if (destinationNode == null)
            return;

        int lastRow = mapHeight - 1;

        // 마지막 일반 행의 모든 노드를 보스 노드와 연결한다.
        for (int column = 0; column < mapWidth; column++)
        {
            Node finalRowNode = GetNode(column, lastRow);

            if (finalRowNode != null)
                CreateConnection(finalRowNode, destinationNode);
        }
    }

    /// <summary>
    /// 현재 열을 기준으로 다음 행의 열을 결정한다.
    /// </summary>
    private int GetNextColumn(int currentColumn)
    {
        if (mapWidth <= 1)
            return 0;

        // 가장 왼쪽에서는 현재 열 또는 오른쪽 열만 선택한다.
        if (currentColumn <= 0)
            return Random.Range(0, Mathf.Min(2, mapWidth));

        // 가장 오른쪽에서는 현재 열 또는 왼쪽 열만 선택한다.
        if (currentColumn >= mapWidth - 1)
        {
            return Random.Range(
                Mathf.Max(0, mapWidth - 2),
                mapWidth
            );
        }

        // 중앙에서는 왼쪽, 현재, 오른쪽 중 하나를 선택한다.
        return Random.Range(
            currentColumn - 1,
            currentColumn + 2
        );
    }

    /// <summary>
    /// 해당 좌표에 노드가 없으면 생성하고,
    /// 이미 있으면 기존 노드를 반환한다.
    /// </summary>
    private Node GetOrCreateNode(int column, int row)
    {
        if (!IsValidCoordinate(column, row))
        {
            Debug.LogError(
                $"잘못된 노드 좌표입니다. 열: {column}, 행: {row}",
                this
            );

            return null;
        }

        // 다른 경로가 같은 좌표를 이미 사용했다면 기존 노드를 사용한다.
        if (nodes[column, row] != null)
            return GetNodeComponent(nodes[column, row]);

        GameObject nodeObject =
            Instantiate(nodePrefab, nodeSet, false);

        Node node = GetNodeComponent(nodeObject);

        if (node == null)
        {
            Debug.LogError(
                $"{nodeObject.name}에 Node 컴포넌트가 없습니다.",
                nodeObject
            );

            Destroy(nodeObject);
            return null;
        }

        nodes[column, row] = nodeObject;

        node.mapManager = this;
        node.nextNodes.Clear();
        node.nodeCoordinate[0] = column;
        node.nodeCoordinate[1] = row;

        // 일반 전투와 엘리트 전투 중 하나를 선택한다.
        int selectedNodeType = GetRandomBattleNodeType();

        // 직접 nodeIndex에 대입하지 않고 SetNodeType을 호출해야
        // 노드 종류에 맞는 이미지도 함께 변경된다.
        node.SetNodeType(selectedNodeType);

        // 결정된 노드 종류에 맞는 적 데이터를 배정한다.
        AssignEnemyDefinition(node);

        float centerColumn = (mapWidth - 1) * 0.5f;
        float xPosition =
            (column - centerColumn) * horizontalSpacing;

        float yPosition = row * verticalSpacing;

        nodeObject.transform.localPosition =
            new Vector3(xPosition, yPosition, 0f);

        nodeObject.transform.localRotation =
            Quaternion.identity;

        return node;
    }

    /// <summary>
    /// 일반 전투와 엘리트 전투 중 하나를 확률에 따라 반환한다.
    /// </summary>
    private int GetRandomBattleNodeType()
    {
        int randomValue = Random.Range(0, 100);

        // normalBattleChance가 80이면
        // 일반 전투 80%, 엘리트 전투 20%가 된다.
        return randomValue < normalBattleChance ? 0 : 1;
    }

    /// <summary>
    /// 맵 최상단에 보스 노드를 생성한다.
    /// </summary>
    private Node CreateDestinationNode()
    {
        GameObject destinationObject =
            Instantiate(destNodePrefab, nodeSet, false);

        Node node = GetNodeComponent(destinationObject);

        if (node == null)
        {
            Debug.LogError(
                $"{destinationObject.name}에 Node 컴포넌트가 없습니다.",
                destinationObject
            );

            Destroy(destinationObject);
            return null;
        }

        node.mapManager = this;
        node.nextNodes.Clear();

        node.nodeCoordinate[0] = mapWidth / 2;
        node.nodeCoordinate[1] = mapHeight;

        // 5번을 보스 전용 노드 타입으로 사용한다.
        node.SetNodeType(5);
        node.SetEnemyDefinition(bossEnemyDefinition);

        destinationObject.transform.localPosition =
            new Vector3(
                0f,
                mapHeight * verticalSpacing,
                0f
            );

        destinationObject.transform.localRotation =
            Quaternion.identity;

        return node;
    }

    /// <summary>
    /// 지정된 좌표의 노드를 반환한다.
    /// </summary>
    private Node GetNode(int column, int row)
    {
        if (!IsValidCoordinate(column, row))
            return null;

        if (nodes[column, row] == null)
            return null;

        return GetNodeComponent(nodes[column, row]);
    }

    /// <summary>
    /// GameObject 자신 또는 자식에서 Node 컴포넌트를 찾는다.
    /// </summary>
    private Node GetNodeComponent(GameObject nodeObject)
    {
        if (nodeObject == null)
            return null;

        Node node = nodeObject.GetComponent<Node>();

        if (node == null)
            node = nodeObject.GetComponentInChildren<Node>();

        return node;
    }

    /// <summary>
    /// 일반 노드 배열의 유효한 좌표인지 확인한다.
    /// </summary>
    private bool IsValidCoordinate(int column, int row)
    {
        return column >= 0 &&
               column < mapWidth &&
               row >= 0 &&
               row < mapHeight;
    }

    /// <summary>
    /// 두 노드를 이동 관계로 연결하고 연결선을 생성한다.
    /// </summary>
    private void CreateConnection(
        Node startNode,
        Node endNode
    )
    {
        if (startNode == null || endNode == null)
            return;

        // 이미 연결되어 있다면 중복 연결선을 만들지 않는다.
        if (startNode.nextNodes.Contains(endNode))
            return;

        startNode.nextNodes.Add(endNode);

        GameObject lineObject =
            Instantiate(linePrefab, lineSet, false);

        RectTransform lineRect =
            lineObject.GetComponent<RectTransform>();

        if (lineRect == null)
        {
            Debug.LogError(
                $"{lineObject.name}에 RectTransform이 없습니다.",
                lineObject
            );

            Destroy(lineObject);
            return;
        }

        Vector3 startPosition =
            lineSet.InverseTransformPoint(
                startNode.transform.position
            );

        Vector3 endPosition =
            lineSet.InverseTransformPoint(
                endNode.transform.position
            );

        Vector3 direction =
            endPosition - startPosition;

        float distance = direction.magnitude;

        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg;

        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0f, 0.5f);

        lineRect.localPosition = startPosition;

        lineRect.localRotation =
            Quaternion.Euler(0f, 0f, angle);

        lineRect.localScale = Vector3.one;

        lineRect.sizeDelta =
            new Vector2(distance, lineThickness);
    }

    /// <summary>
    /// 시작 노드의 다음 노드 목록에 중복 없이 추가한다.
    /// </summary>
    private void AddNextNodeUnique(
        Node startNode,
        Node nextNode
    )
    {
        if (startNode == null || nextNode == null)
            return;

        if (!startNode.nextNodes.Contains(nextNode))
            startNode.nextNodes.Add(nextNode);
    }

    /// <summary>
    /// 노드가 클릭되었을 때 이동 가능 여부를 확인하고
    /// 노드 종류에 맞는 콘텐츠를 실행한다.
    /// </summary>
    public void OnNodeSelected(Node targetNode)
    {
        if (targetNode == null || isBattleSceneLoading)
            return;

        // 시작 노드가 아니라면 현재 위치에서 이동 가능한 노드인지 확인한다.
        if (targetNode != logicalStartNode)
        {
            if (currentNode == null ||
                !currentNode.nextNodes.Contains(targetNode))
            {
                Debug.LogWarning(
                    $"{targetNode.name}은 현재 위치에서 이동할 수 없는 노드입니다."
                );

                return;
            }
        }

        // 기존 이동 가능 노드들의 하이라이트를 끈다.
        DisableCurrentHighlights();

        // 선택한 노드를 현재 노드로 설정한다.
        currentNode = targetNode;

        // 새로운 다음 노드들의 하이라이트를 켠다.
        EnableNextHighlights();

        // 논리적 시작 노드는 실제 콘텐츠를 실행하지 않는다.
        if (targetNode == logicalStartNode)
            return;

        int selectedRow =
            targetNode.nodeCoordinate[1];

        // 이미 선택한 행의 다른 노드를 다시 누르지 못하게 한다.
        if (selectedRow >= 0 &&
            selectedRow < mapHeight)
        {
            DisableRowButtons(selectedRow);
        }

        switch (targetNode.nodeIndex)
        {
            // 일반 전투
            case 0:
                if (!PrepareEnemyBattle(targetNode))
                    return;

                OpenBattleScene(normalBattleSceneName);
                break;

            // 엘리트 전투
            // 일반 전투와 같은 씬을 사용하고 적 데이터만 다르게 전달한다.
            case 1:
                if (!PrepareEnemyBattle(targetNode))
                    return;

                OpenBattleScene(normalBattleSceneName);
                break;

            // 상점
            case 2:
                Debug.Log("상점 노드를 선택했습니다.");
                break;

            // 회복
            case 3:
                Debug.Log("체력 회복 노드를 선택했습니다.");
                break;

            // 이벤트
            case 4:
                Debug.Log("이벤트 노드를 선택했습니다.");
                break;

            // 보스 전투
            case 5:
                if (!PrepareEnemyBattle(targetNode))
                    return;

                OpenBattleScene(bossBattleSceneName);
                break;

            default:
                Debug.LogError(
                    $"지원하지 않는 노드 종류입니다. Index: {targetNode.nodeIndex}",
                    targetNode
                );
                break;
        }
    }

    /// <summary>
    /// 이전에 이동 가능했던 노드들의 하이라이트를 끈다.
    /// </summary>
    private void DisableCurrentHighlights()
    {
        if (currentNode == null)
            return;

        foreach (Node nextNode in currentNode.nextNodes)
        {
            if (nextNode != null)
                nextNode.SetHighlight(false);
        }
    }

    /// <summary>
    /// 현재 노드에서 이동 가능한 다음 노드들을 활성화한다.
    /// </summary>
    private void EnableNextHighlights()
    {
        if (currentNode == null)
            return;

        foreach (Node nextNode in currentNode.nextNodes)
        {
            if (nextNode != null)
                nextNode.SetHighlight(true);
        }
    }

    /// <summary>
    /// 선택한 행의 모든 일반 노드 버튼을 비활성화한다.
    /// </summary>
    private void DisableRowButtons(int row)
    {
        if (row < 0 || row >= mapHeight)
            return;

        for (int column = 0; column < mapWidth; column++)
        {
            GameObject nodeObject = nodes[column, row];

            if (nodeObject == null)
                continue;

            Node node = GetNodeComponent(nodeObject);

            if (node == null)
                continue;

            node.SetHighlight(false);
        }
    }


    /// <summary>
    /// 전투 씬을 맵 씬 위에 Additive 방식으로 불러온다.
    /// </summary>
    private void OpenBattleScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError(
                "전투 씬 이름이 비어 있습니다.",
                this
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"{sceneName} 씬이 Build Profiles에 등록되지 않았거나 이름이 잘못되었습니다.",
                this
            );

            return;
        }

        isBattleSceneLoading = true;

        // 맵 씬은 유지하고 맵 화면만 숨긴다.
        if (mapCanvas != null)
            mapCanvas.SetActive(false);

        SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Additive
        );
    }

    /// <summary>
    /// 전투 종료 후 맵 화면을 다시 표시한다.
    /// 전투 씬 언로드는 전투 종료 코드에서 따로 처리해야 한다.
    /// </summary>
    public void ReturnToMap()
    {
        isBattleSceneLoading = false;

        if (mapCanvas != null)
            mapCanvas.SetActive(true);

        FocusAvailableRow(false);
    }

    /// <summary>
    /// 현재 이동 가능한 행을 화면의 포커스 위치에 맞춘다.
    /// </summary>
    private void FocusAvailableRow(bool immediate)
    {
        if (!isActiveAndEnabled)
            return;

        if (focusCoroutine != null)
            StopCoroutine(focusCoroutine);

        focusCoroutine =
            StartCoroutine(
                CoFocusAvailableRow(immediate)
            );
    }

    /// <summary>
    /// 맵 화면을 현재 이동 가능 행까지 이동한다.
    /// </summary>
    private IEnumerator CoFocusAvailableRow(
        bool immediate
    )
    {
        // UI 레이아웃 계산이 끝나도록 한 프레임 기다린다.
        yield return null;

        Canvas.ForceUpdateCanvases();

        int availableRow = GetAvailableRow();

        if (availableRow < 0)
        {
            focusCoroutine = null;
            yield break;
        }

        Node sampleNode =
            FindNodeInRow(availableRow);

        if (sampleNode == null)
        {
            Debug.LogWarning(
                $"{availableRow}행에서 포커스 기준 노드를 찾을 수 없습니다.",
                this
            );

            focusCoroutine = null;
            yield break;
        }

        if (mapContent.parent == null)
        {
            focusCoroutine = null;
            yield break;
        }

        Vector3 focusTargetInParent =
            mapContent.parent.InverseTransformPoint(
                availableRowFocusTarget.position
            );

        Vector3 rowPositionInContent =
            mapContent.InverseTransformPoint(
                sampleNode.transform.position
            );

        Vector3 startPosition =
            mapContent.localPosition;

        Vector3 targetPosition =
            startPosition;

        targetPosition.y =
            focusTargetInParent.y -
            rowPositionInContent.y;

        targetPosition.y =
            ClampMapContentY(targetPosition.y);

        if (immediate || focusMoveDuration <= 0f)
        {
            mapContent.localPosition =
                targetPosition;

            focusCoroutine = null;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < focusMoveDuration)
        {
            elapsedTime +=
                Time.unscaledDeltaTime;

            float normalizedTime =
                Mathf.Clamp01(
                    elapsedTime /
                    focusMoveDuration
                );

            float evaluatedTime =
                focusMoveCurve.Evaluate(
                    normalizedTime
                );

            mapContent.localPosition =
                Vector3.LerpUnclamped(
                    startPosition,
                    targetPosition,
                    evaluatedTime
                );

            yield return null;
        }

        mapContent.localPosition =
            targetPosition;

        focusCoroutine = null;
    }

    /// <summary>
    /// 현재 위치에서 다음으로 이동 가능한 행 번호를 반환한다.
    /// </summary>
    private int GetAvailableRow()
    {
        if (currentNode == null)
            return -1;

        if (currentNode == logicalStartNode)
            return 0;

        foreach (Node nextNode in currentNode.nextNodes)
        {
            if (nextNode != null)
                return nextNode.nodeCoordinate[1];
        }

        return -1;
    }

    /// <summary>
    /// 지정한 행에서 위치 계산에 사용할 노드를 하나 반환한다.
    /// </summary>
    private Node FindNodeInRow(int row)
    {
        // mapHeight 행은 일반 배열 밖에 있는 보스 행이다.
        if (row == mapHeight)
            return destinationNode;

        if (row < 0 || row >= mapHeight)
            return null;

        for (int column = 0; column < mapWidth; column++)
        {
            Node node = GetNode(column, row);

            if (node != null)
                return node;
        }

        return null;
    }

    /// <summary>
    /// 마우스가 화면 위아래 경계를 넘으면 맵을 이동한다.
    /// </summary>
    private void UpdateMouseScrolling()
    {
        if (mapContent == null ||
            upperBound == null ||
            lowerBound == null)
        {
            return;
        }

        if (isBattleSceneLoading)
            return;

        Vector2 upperScreenPosition =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                upperBound.position
            );

        Vector2 lowerScreenPosition =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                lowerBound.position
            );

        float mouseY =
            Input.mousePosition.y;

        if (mouseY > upperScreenPosition.y)
        {
            MoveMapContent(
                -mapScrollSpeed *
                Time.unscaledDeltaTime
            );
        }
        else if (mouseY < lowerScreenPosition.y)
        {
            MoveMapContent(
                mapScrollSpeed *
                Time.unscaledDeltaTime
            );
        }
    }

    /// <summary>
    /// 맵 콘텐츠를 지정된 거리만큼 이동한다.
    /// </summary>
    private void MoveMapContent(float amount)
    {
        Vector3 position =
            mapContent.localPosition;

        position.y += amount;
        position.y =
            ClampMapContentY(position.y);

        mapContent.localPosition =
            position;
    }

    /// <summary>
    /// 맵이 허용된 스크롤 범위를 벗어나지 않도록 제한한다.
    /// </summary>
    private float ClampMapContentY(float targetY)
    {
        if (mapContent == null ||
            mapContent.parent == null ||
            availableRowFocusTarget == null)
        {
            return targetY;
        }

        float focusTargetY =
            mapContent.parent.InverseTransformPoint(
                availableRowFocusTarget.position
            ).y;

        float maximumY = focusTargetY;

        float minimumY =
            focusTargetY -
            mapHeight * verticalSpacing;

        return Mathf.Clamp(
            targetY,
            minimumY,
            maximumY
        );
    }

    /// <summary>
    /// 노드 종류에 맞는 적 데이터를 배정한다.
    /// </summary>
    private void AssignEnemyDefinition(Node node)
    {
        if (node == null)
            return;

        switch (node.nodeIndex)
        {
            // 일반 전투
            case 0:
                node.SetEnemyDefinition(
                    GetRandomNormalEnemyDefinition()
                );
                break;

            // 엘리트 전투
            case 1:
                node.SetEnemyDefinition(
                    GetRandomEliteEnemyDefinition()
                );
                break;

            // 보스 전투
            case 5:
                node.SetEnemyDefinition(
                    bossEnemyDefinition
                );
                break;

            // 상점, 회복, 이벤트에는 적이 없다.
            default:
                node.SetEnemyDefinition(null);
                break;
        }
    }

    /// <summary>
    /// 일반 적 목록에서 무작위 적을 반환한다.
    /// </summary>
    private EnemyDefinitionData GetRandomNormalEnemyDefinition()
    {
        List<EnemyDefinitionData> candidates =
            new List<EnemyDefinitionData>();

        foreach (
            EnemyDefinitionData definition
            in normalEnemyDefinitions
        )
        {
            if (definition != null)
                candidates.Add(definition);
        }

        if (candidates.Count == 0)
        {
            Debug.LogError(
                "MapManager에 사용할 수 있는 일반 적 데이터가 없습니다.",
                this
            );

            return null;
        }

        return candidates[
            Random.Range(0, candidates.Count)
        ];
    }

    /// <summary>
    /// 엘리트 적 목록에서 무작위 적을 반환한다.
    /// </summary>
    private EnemyDefinitionData GetRandomEliteEnemyDefinition()
    {
        List<EnemyDefinitionData> candidates =
            new List<EnemyDefinitionData>();

        foreach (
            EnemyDefinitionData definition
            in eliteEnemyDefinitions
        )
        {
            if (definition != null)
                candidates.Add(definition);
        }

        if (candidates.Count == 0)
        {
            Debug.LogError(
                "MapManager에 사용할 수 있는 엘리트 적 데이터가 없습니다.",
                this
            );

            return null;
        }

        return candidates[
            Random.Range(0, candidates.Count)
        ];
    }

    /// <summary>
    /// 선택한 노드의 적 데이터를 검증하고
    /// 전투 씬에서 사용할 수 있도록 BattleSelectionData에 저장한다.
    /// </summary>
    private bool PrepareEnemyBattle(Node targetNode)
    {
        if (targetNode == null)
            return false;

        EnemyDefinitionData definition =
            targetNode.EnemyDefinition;

        if (definition == null)
        {
            Debug.LogError(
                $"{targetNode.name}에 적 데이터가 배정되지 않았습니다.",
                targetNode
            );

            return false;
        }

        if (definition.UnitData == null)
        {
            Debug.LogError(
                $"{definition.name}에 Unit Data가 없습니다.",
                definition
            );

            return false;
        }

        if (definition.BrainData == null)
        {
            Debug.LogError(
                $"{definition.name}에 Brain Data가 없습니다.",
                definition
            );

            return false;
        }

        BattleSelectionData.SelectEnemy(definition);

        Debug.Log(
            $"전투 상대 선택: {definition.EnemyName}"
        );

        return true;
    }
}