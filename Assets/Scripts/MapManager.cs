using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager: MonoBehaviour
{
    private ScrollRect scrollRect;
    public LayeredGraphFactory layeredGraph;
    private Graph<VertexPosition> activeGraph; private Dictionary<VertexPosition, Image> vertexImages = new Dictionary<VertexPosition, Image>();
    private Dictionary<string, Image> edgeImages = new Dictionary<string, Image>();

    [Header("UI 설정")]
    public RectTransform mapGraphContent; // 💡 Scroll View의 'Content' 오브젝트를 연결하세요.
    public GameObject[] vertexObjs;     // 정점 프리랩
    public GameObject edgeObj;
    public int maxLayer = 8;
    public int maxVertPerLayer = 5;
    [SerializeField] private ScrollRect mapScrollRect;
    [SerializeField] private RectTransform viewportRect;



    [Header("오버스크롤 최대 제한 범위")]
    // 1.0이 정상 끝이며, 1.1은 10%만큼 더 밀려나갈 수 있다는 뜻입니다.
    public float maxOverScroll = 1.02f;
    public float minOverScroll = -0.02f;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        layeredGraph=new LayeredGraphFactory();
        activeGraph = layeredGraph.CreateLayeredGraph(maxLayer, maxVertPerLayer);
        DrawGraph();
    }
    void Start()
    {
        // 1. 세션 매니저를 확인하여 기존 데이터가 있다면 복원, 없다면 새로 생성
        if (MapSessionManager.Instance != null && MapSessionManager.Instance.CurrentGraph != null)
        {
            // 전투 씬 등에서 돌아온 경우: 기존 그래프 그대로 사용
            activeGraph = MapSessionManager.Instance.CurrentGraph;
        }
        else
        {
            // 게임을 처음 시작한 경우: 팩토리로 새로 생성 후 세션에 등록
            if (layeredGraph == null) layeredGraph = new LayeredGraphFactory();
            activeGraph = layeredGraph.CreateLayeredGraph(maxLayer, maxVertPerLayer);

            if (MapSessionManager.Instance != null)
            {
                MapSessionManager.Instance.InitializeNewSession(activeGraph);
            }
        }

        // 2. 맵 그리기 실행
        DrawGraph();

        // 3. 방문했던 경로 시각적 하이라이트 연출 적용
        HighlightVisitedPath();
    }
    private void HighlightVisitedPath()
    {
        if (MapSessionManager.Instance == null) return;

        List<string> visitedIds = MapSessionManager.Instance.VisitedNodeIds;

        // UI 렌더링이 완료된 후, 하이라키에 생성된 프리팹들을 순회하면서
        // visitedIds에 포함된 노드들의 색상을 변경하거나 선택 불가능하게 토글 처리하는 뼈대 구역
        // 예: 
        // string currentLoc = MapSessionManager.Instance.GetCurrentLocationId();
        // 현재 위치와 인접한(GetNeighbors) 노드들만 클릭 가능(Button.interactable = true)하도록 록킹
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

        // 가로 스크롤 범위 제한 (필요 시 활성화)
        /*
        if (scrollRect.horizontal)
        {
            float currentPos = scrollRect.horizontalNormalizedPosition;
            if (currentPos > maxOverScroll)
                scrollRect.horizontalNormalizedPosition = maxOverScroll;
            else if (currentPos < minOverScroll)
                scrollRect.horizontalNormalizedPosition = minOverScroll;
        }
        */
    }
    public void DrawGraph()
    {
        if (activeGraph == null || mapGraphContent == null) return;

        // 초기화
        vertexImages.Clear();
        edgeImages.Clear();
        foreach (Transform child in mapGraphContent) Destroy(child.gameObject);


        HashSet<string> drawnEdges = new HashSet<string>();

        // ==========================================
        // 1. 정점(Vertex) 생성 및 배치 구역 수정
        // ==========================================
        foreach (var vertex in activeGraph.GetAllVertices())
        {
            GameObject vGo = Instantiate(vertexObjs[(int)vertex.type], mapGraphContent);
            RectTransform vRect = vGo.GetComponent<RectTransform>();

            // 💡 [순서 중요] 위치를 대입하기 전에 앵커와 피벗을 왼쪽 아래(0,0) 절대 기준으로 고정합니다.
            vRect.anchorMin = Vector2.zero;
            vRect.anchorMax = Vector2.zero;
            vRect.pivot = new Vector2(0.5f, 0.5f); // 노드 자체는 자기 중심이 기준점이어야 좌표가 맞음

            // 앵커가 완전히 (0,0)으로 잠긴 후에 좌표를 대입해야 우측 쏠림이 해결됩니다.
            vRect.anchoredPosition = new Vector2(vertex.X, vertex.Y);

            MapNodeButton nodeBtnComponent = vGo.GetComponent<MapNodeButton>();
            if (nodeBtnComponent != null) nodeBtnComponent.Setup(vertex);

            Image vImg = vGo.GetComponent<Image>();
            if (vImg != null) vertexImages.Add(vertex, vImg);
        }

        // ==========================================
        // 2. 간선(Edge) 생성 및 배치 구역 수정
        // ==========================================
        foreach (var source in activeGraph.GetAllVertices())
        {
            foreach (var target in activeGraph.GetNeighbors(source))
            {
                // 언더바(_) 대신 하이픈(-)을 사용하여 ID 간 충돌 방지 록킹
                string edgeKey = source.Id.CompareTo(target.Id) < 0
                    ? $"{source.Id}-{target.Id}"
                    : $"{target.Id}-{source.Id}";

                if (drawnEdges.Contains(edgeKey)) continue;
                drawnEdges.Add(edgeKey);

                GameObject eGo = Instantiate(edgeObj, mapGraphContent);
                eGo.transform.SetAsFirstSibling();

                RectTransform eRect = eGo.GetComponent<RectTransform>();

                // 💡 간선 프리팹도 마찬가지로 위치 대입 전에 앵커를 좌하단(0,0)으로 강제 탈옥 방지 처리
                eRect.anchorMin = Vector2.zero;
                eRect.anchorMax = Vector2.zero;
                eRect.pivot = new Vector2(0.5f, 0f);

                // 부모 판때기가 (0,0) 기준이므로 출발점 좌표를 그대로 대입
                eRect.anchoredPosition = vertexImages[source].rectTransform.anchoredPosition;

                Vector2 direction = vertexImages[target].rectTransform.anchoredPosition - eRect.anchoredPosition;
                eRect.localScale = new Vector3(4f, direction.magnitude / 100f, 1f);
                eRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);

                Image eImg = eGo.GetComponent<Image>();
                if (eImg != null) edgeImages.Add(edgeKey, eImg);
            }
        }

        // 3. 💡 모든 드로우가 끝난 직후 하이라이트 함수 호출
        UpdateMapHighlight();
    }
    private void UpdateMapHighlight()
    {
        if (MapSessionManager.Instance == null || activeGraph == null) return;

        // 세션 매니저에서 데이터 수집
        List<string> visitedIds = MapSessionManager.Instance.VisitedNodeIds;
        string currentLocId = MapSessionManager.Instance.GetCurrentLocationId();

        // 현재 플레이어가 서 있는 정점 데이터 찾기
        VertexPosition currentVertex = null;
        foreach (var v in activeGraph.GetAllVertices())
        {
            if (v.Id == currentLocId)
            {
                currentVertex = v;
                break;
            }
        }

        // 현재 위치에서 갈 수 있는 다음 정점들의 ID 리스트 추출 (Possible Vert)
        HashSet<string> possibleVertIds = new HashSet<string>();
        if (currentVertex != null)
        {
            foreach (var neighbor in activeGraph.GetNeighbors(currentVertex))
            {
                // 계층형 그래프이므로 뒤로 돌아가는 역주행 방지 (현재 층보다 높은 층만 선택지로 인정)
                if (neighbor.Layer > currentVertex.Layer)
                {
                    possibleVertIds.Add(neighbor.Id);
                }
            }
        }
        // ==========================================
        // 1. 정점(Vertex) 하이라이트 갱신
        // ==========================================
        foreach (var pair in vertexImages)
        {
            VertexPosition vert = pair.Key;
            Image img = pair.Value;
            Button btn = img.GetComponent<Button>(); // 버튼 클릭 제어용

            if (visitedIds.Contains(vert.Id))
            {
                // 🟢 Visited Vert: 지나온 길 (녹색 연출 예시)
                img.color = new Color(1f, 1f, 1f, 1f);
                if (btn != null) btn.enabled = false; // 이미 방문한 곳은 클릭 불가
            }
            else if (possibleVertIds.Contains(vert.Id))
            {
                // 🟡 Possible Vert: 다음 갈 수 있는 선택지 (밝은 황색 연출 예시)
                img.color = new Color(1f, 1f, 1f, 1f);
                if (btn != null) btn.enabled = true;  // 이동 가능한 노드만 버튼 활성화
            }
            else
            {
                // ❌ 비활성화 노드: 아예 갈 수 없는 미래의 노드나 다른 갈래 길 (어둡고 투명하게)
                img.color = new Color(0.3f, 0.3f, 0.3f, 0.4f);
                if (btn != null) btn.enabled = false;
            }
        }

        // ==========================================
        // 2. 간선(Edge) 하이라이트 갱신
        // ==========================================

        foreach (var pair in edgeImages)
        {
            string edgeKey = pair.Key;
            Image img = pair.Value;

            // 💡 [핵심 수정] 하이픈(-)을 기준으로 분해하면 
            // split[0]에는 source ID가, split[1]에는 target ID가 온전히 들어옵니다.
            string[] split = edgeKey.Split('-');
            string idA = split[0]; // 예: "Node_0_0"
            string idB = split[1]; // 예: "Node_1_2"

            // 양쪽 노드가 모두 방문 리스트에 존재하고, 방문 순서상 연속적인지 판정
            int idxA = visitedIds.IndexOf(idA);
            int idxB = visitedIds.IndexOf(idB);
            bool isVisitedEdge = idxA != -1 && idxB != -1 && Mathf.Abs(idxA - idxB) == 1;

            // 현재 위치에서 다음 갈 수 있는 선택지 노드로 이어지는 선인지 판정
            bool isPossibleEdge = (idA == currentLocId && possibleVertIds.Contains(idB)) ||
                                 (idB == currentLocId && possibleVertIds.Contains(idA));

            if (isVisitedEdge)
            {
                // 🟢 Visited Edge
                img.color = new Color(1f, 1f, 1f, 1f);
            }
            else if (isPossibleEdge)
            {
                // 🟡 Possible Edge
                img.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                // ❌ 비활성화 간선
                img.color = new Color(0.2f, 0.2f, 0.2f, 0.15f);
            }
        }
        if (currentVertex != null)
        {
            FocusScrollToCurrentPosition(currentVertex.Y+100);
        }
    }
    private void FocusScrollToCurrentPosition(float currentVertexY)
    {
        if (mapScrollRect == null || mapGraphContent == null || viewportRect == null) return;

        // 1. 연산에 필요한 실제 높이 치수 확보
        float contentHeight = mapGraphContent.sizeDelta.y; // 맵 총 세로 길이 (예: 4000f)
        float viewportHeight = viewportRect.rect.height;   // 화면에 보이는 해상도 세로 길이

        // 스크롤이 불가능할 정도로 판때기가 작다면 연산 생략 (예외 방어)
        if (contentHeight <= viewportHeight) return;

        // 2. 💡 [정규화 수식 설계] 
        // 현재 노드가 화면 중앙에 올 때, 스크롤바의 가상 위치(0.0 ~ 1.0)를 역산합니다.
        // 꼭대기(contentHeight) 기준이 아닌 바닥(0)에서부터의 절대적 비율을 추출해야 합니다.
        float scrollableRange = contentHeight - viewportHeight;
        float desiredScrollY = currentVertexY - (viewportHeight * 0.5f);

        // 3. 화면 하단 및 상단 엣지 예외 방어 보정
        desiredScrollY = Mathf.Clamp(desiredScrollY, 0f, scrollableRange);

        // 4. 💡 [핵심 교체] 0.0(최하단 시작점) ~ 1.0(최상단 끝점) 사이의 비율값으로 치환
        float targetNormalizedRatio = desiredScrollY / scrollableRange;

        // 5. 엔진 내부 가상 스크롤바 제어 장치에 비율 대입 록킹
        // 이제 유니티가 스크롤바를 직접 마우스로 잡아 끈 것처럼 연산하므로 위아래 전 구역 스크롤이 풀립니다.
        mapScrollRect.verticalNormalizedPosition = targetNormalizedRatio;
    }
}