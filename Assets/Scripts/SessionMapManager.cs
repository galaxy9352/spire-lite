using System.Collections.Generic;
using UnityEngine;

public class MapSessionManager : MonoBehaviour
{
    public static MapSessionManager Instance { get; private set; }

    [Header("Session Data")]
    public Graph<VertexPosition> CurrentGraph { get; set; }
    public List<string> VisitedNodeIds { get; private set; } = new List<string>();

    // 전투 사이클 상태 (0: 첫 번째 전투, 1: 두 번째 전투, 2: 보스전)
    public int BattleStep { get; set; } = 0;

    // 첫 전투에서 배정된 씬 인덱스 (0: BattleScene01, 1: BattleScene02)
    public int FirstBattleSceneIndex { get; set; } = -1;

    void Awake()
    {
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

    /// <summary>
    /// 새로운 맵 생성 시 세션 데이터를 초기화합니다.
    /// </summary>
    public void InitializeNewSession(Graph<VertexPosition> newGraph)
    {
        CurrentGraph = newGraph;
        VisitedNodeIds.Clear();

        // 전투 진행도 초기화
        BattleStep = 0;
        FirstBattleSceneIndex = -1;

        // 시작 계층(0)의 노드는 기본적으로 방문 처리
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
    /// 노드 이동 시 방문 기록을 갱신합니다.
    /// </summary>
    public void VisitNode(string nodeId)
    {
        if (!VisitedNodeIds.Contains(nodeId))
        {
            VisitedNodeIds.Add(nodeId);
        }
    }

    /// <summary>
    /// 플레이어의 현재 위치(가장 최근에 방문한 노드)를 반환합니다.
    /// </summary>
    public string GetCurrentLocationId()
    {
        if (VisitedNodeIds.Count > 0)
        {
            return VisitedNodeIds[VisitedNodeIds.Count - 1];
        }
        return null;
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}