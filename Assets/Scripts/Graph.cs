using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum vertType
{
    fight=0, rest=1, start=2, last=3 
} 
public class VertexPosition : IEquatable<VertexPosition>
{
    public string Id { get; }
    public int Layer { get; } // n번째 계층
    public int X { get; set; }
    public int Y { get; set; }

    public vertType type { get; set; }


    public VertexPosition(string id, int layer, int x, int y, int type)
    {
        Id = id;
        Layer = layer;
        X = x;
        Y = y;
        this.type = (vertType)type;
    }

    // Graph 내부에서 Contains나 Dictionary Key 비교를 위해 동등성 정의
    public bool Equals(VertexPosition other)
    {
        if (other == null) return false;
        return Id == other.Id;
    }

    public override bool Equals(object obj) => Equals(obj as VertexPosition);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"{Id}(X:{X}, Y:{Y})";
}
public class Graph<T>
{
    // 각 정점(Vertex)과 연결된 인접 정점들의 목록을 딕셔너리로 관리
    private Dictionary<T, List<T>> adjacencyList = new Dictionary<T, List<T>>();

    // 1. 정점 추가
    public void AddVertex(T vertex)
    {
        if (!adjacencyList.ContainsKey(vertex))
        {
            adjacencyList[vertex] = new List<T>();
        }
    }

    // 2. 간선 추가 (무방향이므로 양쪽에 서로 추가)
    public void AddEdge(T source, T target)
    {
        // 정점이 없다면 자동 생성
        AddVertex(source);
        AddVertex(target);

        // 중복 간선 방지 체크 후 연결
        if (!adjacencyList[source].Contains(target)) adjacencyList[source].Add(target);
        if (!adjacencyList[target].Contains(source)) adjacencyList[target].Add(source);
    }

    // 3. 그래프 구조 콘솔창에 텍스트로 보여주기
    public void PrintGraph()
    {
        Console.WriteLine("=== [Graph Adjacency List] ===");
        foreach (var vertex in adjacencyList)
        {
            Console.Write($"{vertex.Key} -> ");
            Console.WriteLine(string.Join(", ", vertex.Value));
        }
        Console.WriteLine("=============================\n");
    }
    public void PrintGraphUnity()
    {
        // 스트링 빌더를 사용해 가비지 컬렉터(GC) 과부하를 방지하며 문자열을 조립합니다.
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("<color=cyan><b>=== [Graph Adjacency List (Unity)] ===</b></color>");

        foreach (var vertex in adjacencyList)
        {
            sb.Append($"<b>{vertex.Key}</b> -> ");

            // 인접한 노드들의 이름을 쉼표(,)로 구분하여 한 줄로 조립
            sb.AppendLine(string.Join(", ", vertex.Value));
        }

        sb.AppendLine("<color=cyan><b>====================================</b></color>");

        // 조립이 끝난 전체 문자열을 유니티 콘솔창에 한 번에 띄웁니다.
        Debug.Log(sb.ToString());
    }
    /// <summary>
    /// 그래프에 존재하는 모든 정점(Key)들의 목록을 반환합니다.
    /// </summary>
    public IEnumerable<T> GetAllVertices()
    {
        // 딕셔너리의 Keys(정점 목록)를 안전하게 반환합니다.
        return adjacencyList.Keys;
    }

    /// <summary>
    /// 특정 정점과 연결된(인접한) 이웃 정점들의 리스트를 반환합니다.
    /// </summary>
    /// <param name="vertex">대상 정점</param>
    public List<T> GetNeighbors(T vertex)
    {
        // 딕셔너리에 해당 정점이 존재하는지 안전하게 검사 후 반환
        if (adjacencyList.ContainsKey(vertex))
        {
            return adjacencyList[vertex];
        }

        // 만약 예외적으로 없는 정점을 참조하려 했다면 빈 리스트를 반환하여 에러(Null) 방지
        return new List<T>();
    }
}
public class LayeredGraphFactory
{
    private System.Random random = new System.Random();

    // 화면 좌표계 한계 설정값
    private const int MinX = 0;
    private const int MaxX = 1920;
    private const int MinY = 300;
    private const int MaxY = 3700;

    /// <summary>
    /// n 계층을 가지고, 계층별 최대 m개의 정점을 가질 수 있는 계층 그래프를 생성합니다.
    /// </summary>
    public Graph<VertexPosition> CreateLayeredGraph(int totalLayers, int maxVertsPerLayer)
    {
        Graph<VertexPosition> graph = new Graph<VertexPosition>();

        // 1. 각 계층별 정점들을 담아둘 리스트 배열 (0층 ~ totalLayers-1층)
        List<VertexPosition>[] layers = new List<VertexPosition>[totalLayers];

        // Y좌표 간격 계산 (계층에 따라 Y좌표 제한)
        int yInterval = (MaxY - MinY) / (totalLayers - 1);

        // 2. 정점 생성 및 배치
        for (int layer = 0; layer < totalLayers; layer++)
        {
            layers[layer] = new List<VertexPosition>();
            int currentLayerY = MinY + (layer * yInterval); // 해당 계층의 고정 Y좌표

            // 시작점(0)과 끝점(최상위)은 무조건 정중앙에 1개만 배치
            if (layer == 0 )
            {
                var uniqueVert = new VertexPosition($"Node_{layer}_0", layer, (MinX + MaxX) / 2, currentLayerY, 2);
                layers[layer].Add(uniqueVert);
                graph.AddVertex(uniqueVert);
            }
            else if(layer == totalLayers - 1)
            {
                var uniqueVert = new VertexPosition($"Node_{layer}_0", layer, (MinX + MaxX) / 2, currentLayerY, 3);
                layers[layer].Add(uniqueVert);
                graph.AddVertex(uniqueVert);
            }
            else
            {
                int vertexCount = GetVertexCountForLayer(maxVertsPerLayer);
                int xInterval = (MaxX - MinX) / (vertexCount + 1);

                for (int i = 0; i < vertexCount; i++)
                {
                    // 1. 가로(X) 좌표 계산 및 약간의 랜덤 오프셋 (기존 유지)
                    int calculatedX = MinX + (xInterval * (i + 1)) + random.Next(-70, 70);
                    calculatedX = Math.Clamp(calculatedX, MinX + 100, MaxX - 100);

                    // 2. 💡 [핵심 수정] 세로(Y) 좌표에 무작위 위아래 오프셋 주입
                    // 해당 계층의 기본 Y값에서 위아래로 최대 150픽셀 정도 유연하게 흔들어줍니다.
                    // (주의: 계층간 간격인 yInterval의 절반을 넘지 않도록 세팅해야 층이 뒤섞이지 않습니다)
                    int calculatedY = currentLayerY + random.Next(-70, 70);
                    calculatedY = Math.Clamp(calculatedY, MinY + 50, MaxY - 50);

                    // 오프셋이 적용된 calculatedY를 대입하여 정점 생성
                    int type;
                    if ((random.NextDouble() > 0.3 || layer == 1) && layer != totalLayers - 2)
                    {
                        type = 0;
                    }
                    else
                    {
                        type = 1;
                    }
                    var vert = new VertexPosition($"Node_{layer}_{i}", layer, calculatedX, calculatedY, type);
                    layers[layer].Add(vert);
                    graph.AddVertex(vert);
                }

                // X 좌표 기준으로 정렬 (경로 교차 방지 뼈대 유지)
                layers[layer].Sort((a, b) => a.X.CompareTo(b.X));
            }
        }

        // 3. 계층 간 간선(Edge) 연결 (n층 -> n+1층)
        // 가로 축 순서대로 매칭하여 오버랩 및 선 뒤틀림을 원천 차단합니다.
        for (int layer = 0; layer < totalLayers - 1; layer++)
        {
            List<VertexPosition> currentLayer = layers[layer];
            List<VertexPosition> nextLayer = layers[layer + 1];

            int currCount = currentLayer.Count;
            int nextCount = nextLayer.Count;

            // 모든 현재 층 노드가 다음 층 노드로 최소 1개 이상 연결되도록 보장
            for (int i = 0; i < currCount; i++)
            {
                // 인덱스 비율 매핑을 통해 가로 위치가 가장 가까운 타겟 산출
                int targetIndex = (i * nextCount) / currCount;
                graph.AddEdge(currentLayer[i], nextLayer[targetIndex]);

                // 우측 혹은 좌측 인접 노드로의 확장 경로 보완 (선택적 멀티 경로 생성)
                if (targetIndex + 1 < nextCount && random.Next(0, 2) == 1)
                {
                    graph.AddEdge(currentLayer[i], nextLayer[targetIndex + 1]);
                }
            }

            // 다음 층의 노드가 낙동강 오리알이 되지 않도록 역방향 닫기 바인딩
            for (int j = 0; j < nextCount; j++)
            {
                int sourceIndex = (j * currCount) / nextCount;
                graph.AddEdge(currentLayer[sourceIndex], nextLayer[j]);
            }
        }

        return graph;
    }

    /// <summary>
    /// 추후 업데이트를 통해 고정 개수나 특정 규칙을 넣기 용이하도록 분리한 
    /// n번째 계층의 정점 개수 반환 함수
    /// </summary>
    private int GetVertexCountForLayer(int maxCount)
    {
        // 현재는 기본 요구사항인 [최소 1개 ~ 최대 m개] 사이의 갯수를 랜덤으로 반환
        return random.Next(2, maxCount + 1);
    }
}