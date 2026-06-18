using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MapNodeButton : MonoBehaviour
{
    [SerializeField] private Button nodeButton;

    public VertexPosition NodeData { get; private set; }

    private void Awake()
    {
        if (nodeButton == null)
        {
            nodeButton = GetComponent<Button>();
        }

        if (nodeButton != null)
        {
            nodeButton.onClick.AddListener(OnNodeClicked);
        }
    }

    public void Setup(VertexPosition data)
    {
        NodeData = data;
    }

    private void OnNodeClicked()
    {
        if (NodeData == null || MapSessionManager.Instance == null) return;

        // 세션에 방문 기록 저장
        MapSessionManager.Instance.VisitNode(NodeData.Id);

        // 맵 UI 하이라이트 즉시 갱신
        MapManager manager = FindFirstObjectByType<MapManager>();
        if (manager != null)
        {
            manager.DrawGraph();
        }

        switch (NodeData.type)
        {
            case vertType.last:
                SceneManager.LoadScene("BossScene01");
                break;
            case vertType.fight:
                ProcessFightNode();
                break;

            case vertType.rest:// 잃은 체력의 50%를 회복
                PlayerManger.Instance.PlayerCurHp += (int)math.ceil((PlayerManger.Instance.PlayerMaxHp - PlayerManger.Instance.PlayerCurHp) * 0.5f);
                // SceneManager.LoadScene("RestScene");
                break;
        }
    }

    /// <summary>
    /// 전투 노드 선택 시 씬 로드 흐름을 제어합니다.
    /// </summary>
    private void ProcessFightNode()
    {
        string targetScene = "";
        var session = MapSessionManager.Instance;

        if (session.BattleStep == 0)
        {
            // 1번째 일반 전투 (50% 확률 랜덤)
            int randomIdx = UnityEngine.Random.Range(0, 2);
            session.FirstBattleSceneIndex = randomIdx;

            targetScene = (randomIdx == 0) ? "BattleScene01" : "BattleScene02";
            session.BattleStep = 1;
        }
        else if (session.BattleStep == 1)
        {
            // 2번째 일반 전투 (1번째와 반대되는 씬 고정)
            targetScene = (session.FirstBattleSceneIndex == 0) ? "BattleScene02" : "BattleScene01";
            session.BattleStep = 2;
        }
        else
        {
            // 3번째 보스 전투 (확정 후 사이클 리셋)
            targetScene = "BossScene01";

            session.BattleStep = 0;
            session.FirstBattleSceneIndex = -1;
        }

        SceneManager.LoadScene(targetScene);
    }
}