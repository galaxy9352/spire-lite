using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{

    private ScrollRect scrollRect;
    public LayeredGraphFactory layeredGraph;
    private Graph<VertexPosition> activeGraph;

    private Dictionary<VertexPosition, Image> vertexImages = new Dictionary<VertexPosition, Image>();
    private Dictionary<string, Image> edgeImages = new Dictionary<string, Image>();

    [Header("UI Settings")]
    public RectTransform mapGraphContent;
    public GameObject[] vertexObjs;
    public GameObject edgeObj;
    public int maxLayer = 8;
    public int maxVertPerLayer = 5;

    public SetHP hpDisplay;

    [SerializeField] private ScrollRect mapScrollRect;
    [SerializeField] private RectTransform viewportRect;

    [Header("Scroll Constraints")]
    public float maxOverScroll = 1.02f;
    public float minOverScroll = -0.02f;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        layeredGraph = new LayeredGraphFactory();
        activeGraph = layeredGraph.CreateLayeredGraph(maxLayer, maxVertPerLayer);
        DrawGraph();
    }

    void Start()
    {
        // 세션 데이터 복원 또는 초기화
        if (MapSessionManager.Instance != null && MapSessionManager.Instance.CurrentGraph != null)
        {
            activeGraph = MapSessionManager.Instance.CurrentGraph;
        }
        else
        {
            if (layeredGraph == null) layeredGraph = new LayeredGraphFactory();
            activeGraph = layeredGraph.CreateLayeredGraph(maxLayer, maxVertPerLayer);

            if (MapSessionManager.Instance != null)
            {
                MapSessionManager.Instance.InitializeNewSession(activeGraph);
            }
        }

        DrawGraph();
        HighlightVisitedPath();
    }

    private void HighlightVisitedPath()
    {
        if (MapSessionManager.Instance == null) return;
        List<string> visitedIds = MapSessionManager.Instance.VisitedNodeIds;
        // 추가적인 경로 처리 로직이 필요하다면 이곳에 구현
    }

    void LateUpdate()
    {
        if (scrollRect == null) return;

        // 세로 스크롤 범위 제한
        if (scrollRect.vertical)
        {
            float currentPos = scrollRect.verticalNormalizedPosition;
            if (currentPos > maxOverScroll)
                scrollRect.verticalNormalizedPosition = maxOverScroll;
            else if (currentPos < minOverScroll)
                scrollRect.verticalNormalizedPosition = minOverScroll;
        }
    }

    public void DrawGraph()
    {
        if (activeGraph == null || mapGraphContent == null) return;

        vertexImages.Clear();
        edgeImages.Clear();

        // 기존 노드 초기화
        foreach (Transform child in mapGraphContent)
        {
            Destroy(child.gameObject);
        }

        HashSet<string> drawnEdges = new HashSet<string>();

        // 1. 노드(Vertex) 생성 및 배치
        foreach (var vertex in activeGraph.GetAllVertices())
        {
            GameObject vGo = Instantiate(vertexObjs[(int)vertex.type], mapGraphContent);
            RectTransform vRect = vGo.GetComponent<RectTransform>();

            // 앵커 및 피벗 초기화 후 위치 세팅
            vRect.anchorMin = Vector2.zero;
            vRect.anchorMax = Vector2.zero;
            vRect.pivot = new Vector2(0.5f, 0.5f);
            vRect.anchoredPosition = new Vector2(vertex.X, vertex.Y);

            MapNodeButton nodeBtnComponent = vGo.GetComponent<MapNodeButton>();
            if (nodeBtnComponent != null) nodeBtnComponent.Setup(vertex);

            Image vImg = vGo.GetComponent<Image>();
            if (vImg != null) vertexImages.Add(vertex, vImg);
        }

        // 2. 간선(Edge) 생성 및 배치
        foreach (var source in activeGraph.GetAllVertices())
        {
            foreach (var target in activeGraph.GetNeighbors(source))
            {
                // 중복 간선 방지를 위한 키 생성
                string edgeKey = source.Id.CompareTo(target.Id) < 0
                    ? $"{source.Id}-{target.Id}"
                    : $"{target.Id}-{source.Id}";

                if (drawnEdges.Contains(edgeKey)) continue;
                drawnEdges.Add(edgeKey);

                GameObject eGo = Instantiate(edgeObj, mapGraphContent);
                eGo.transform.SetAsFirstSibling();

                RectTransform eRect = eGo.GetComponent<RectTransform>();

                eRect.anchorMin = Vector2.zero;
                eRect.anchorMax = Vector2.zero;
                eRect.pivot = new Vector2(0.5f, 0f);
                eRect.anchoredPosition = vertexImages[source].rectTransform.anchoredPosition;

                // 방향 및 길이 계산
                Vector2 direction = vertexImages[target].rectTransform.anchoredPosition - eRect.anchoredPosition;
                eRect.localScale = new Vector3(4f, direction.magnitude / 100f, 1f);
                eRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);

                Image eImg = eGo.GetComponent<Image>();
                if (eImg != null) edgeImages.Add(edgeKey, eImg);
            }
        }

        UpdateMapHighlight();
    }

    private void UpdateMapHighlight()
    {
        if (MapSessionManager.Instance == null || activeGraph == null) return;

        List<string> visitedIds = MapSessionManager.Instance.VisitedNodeIds;
        string currentLocId = MapSessionManager.Instance.GetCurrentLocationId();

        VertexPosition currentVertex = null;
        foreach (var v in activeGraph.GetAllVertices())
        {
            if (v.Id == currentLocId)
            {
                currentVertex = v;
                break;
            }
        }

        // 이동 가능한 다음 노드 판별
        HashSet<string> possibleVertIds = new HashSet<string>();
        if (currentVertex != null)
        {
            foreach (var neighbor in activeGraph.GetNeighbors(currentVertex))
            {
                if (neighbor.Layer > currentVertex.Layer)
                {
                    possibleVertIds.Add(neighbor.Id);
                }
            }
        }

        // 노드 시각적 상태 갱신
        foreach (var pair in vertexImages)
        {
            VertexPosition vert = pair.Key;
            Image img = pair.Value;
            Button btn = img.GetComponent<Button>();

            if (visitedIds.Contains(vert.Id))
            {
                img.color = new Color(1f, 1f, 1f, 1f);
                if (btn != null) btn.enabled = false;
            }
            else if (possibleVertIds.Contains(vert.Id))
            {
                img.color = new Color(1f, 1f, 1f, 1f);
                if (btn != null) btn.enabled = true;
            }
            else
            {
                img.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                if (btn != null) btn.enabled = false;
            }
        }

        // 간선 시각적 상태 갱신
        foreach (var pair in edgeImages)
        {
            string edgeKey = pair.Key;
            Image img = pair.Value;

            string[] split = edgeKey.Split('-');
            string idA = split[0];
            string idB = split[1];

            int idxA = visitedIds.IndexOf(idA);
            int idxB = visitedIds.IndexOf(idB);
            bool isVisitedEdge = idxA != -1 && idxB != -1 && Mathf.Abs(idxA - idxB) == 1;

            bool isPossibleEdge = (idA == currentLocId && possibleVertIds.Contains(idB)) ||
                                  (idB == currentLocId && possibleVertIds.Contains(idA));

            if (isVisitedEdge)
            {
                img.color = new Color(1f, 1f, 1f, 1f);
            }
            else if (isPossibleEdge)
            {
                img.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                img.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }

        if (currentVertex != null)
        {
            FocusScrollToCurrentPosition(currentVertex.Y + 100);
        }
    }

    private void FocusScrollToCurrentPosition(float currentVertexY)
    {
        if (mapScrollRect == null || mapGraphContent == null || viewportRect == null) return;

        float contentHeight = mapGraphContent.sizeDelta.y;
        float viewportHeight = viewportRect.rect.height;

        if (contentHeight <= viewportHeight) return;

        float scrollableRange = contentHeight - viewportHeight;
        float desiredScrollY = currentVertexY - (viewportHeight * 0.5f);

        desiredScrollY = Mathf.Clamp(desiredScrollY, 0f, scrollableRange);

        float targetNormalizedRatio = desiredScrollY / scrollableRange;
        mapScrollRect.verticalNormalizedPosition = targetNormalizedRatio;
    }
}