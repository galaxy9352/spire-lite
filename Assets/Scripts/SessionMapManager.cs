using System.Collections.Generic;
using UnityEngine;

public class MapSessionManager : MonoBehaviour
{
    // 싱글톤 인스턴스 (어디서나 접근 가능하도록)
    public static MapSessionManager Instance { get; private set; }

    [Header("현재 지도 세션 데이터")]
    // 1. 현재 씬에 가동 중인 전체 그래프 데이터 보관
    public Graph<VertexPosition> CurrentGraph { get; set; }

    // 2. 플레이어가 방문한 정점 ID들을 순서대로 기록하는 리스트
    public List<string> VisitedNodeIds { get; private set; } = new List<string>();

    void Awake()
    {
        // 씬이 바뀌어도 이 매니저 오브젝트가 파괴되지 않도록 록킹
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 데이터 제어 함수들 ---

    /// <summary>
    /// 새로운 지도를 시작할 때 세션을 초기화합니다.
    /// </summary>
    public void InitializeNewSession(Graph<VertexPosition> newGraph)
    {
        CurrentGraph = newGraph;
        VisitedNodeIds.Clear();

        // 시작점(0번째 계층의 유일한 노드)은 자동으로 방문 처리
        foreach (var v in newGraph.GetAllVertices())
        {
            if (v.Layer == 0)
            {
                VisitedNodeIds.Add(v.Id);
                break;
            }
        }
    }

    /// <summary>
    /// 플레이어가 새로운 노드를 선택해 이동했을 때 경로를 추가합니다.
    /// </summary>
    public void VisitNode(string nodeId)
    {
        if (!VisitedNodeIds.Contains(nodeId))
        {
            VisitedNodeIds.Add(nodeId);
            Debug.Log($"<color=green><b>[MapSession]</b> 노드 방문 기록 추가: {nodeId}</color>");
        }
    }

    /// <summary>
    /// 플레이어의 현재 위치(가장 마지막으로 방문한 노드 ID)를 반환합니다.
    /// </summary>
    public string GetCurrentLocationId()
    {
        if (VisitedNodeIds.Count > 0)
        {
            return VisitedNodeIds[VisitedNodeIds.Count - 1];
        }
        return null;
    }
}