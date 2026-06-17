using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Image))] // Image 컴포넌트가 누락되는 것을 방지
public class MapNodeButton : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    [SerializeField] private Button nodeButton;

    // 이 버튼이 물리적으로 담당하는 그래프 내의 논리 데이터
    public VertexPosition NodeData { get; private set; }

    private void Awake()
    {
        // 인스펙터에서 버튼을 깜빡하고 연결 안 했을 때를 대비한 자동 예외 방어
        if (nodeButton == null)
        {
            nodeButton = GetComponent<Button>();
        }

        // 버튼 클릭 이벤트에 함수 바인딩 록킹
        if (nodeButton != null)
        {
            nodeButton.onClick.AddListener(OnNodeClicked);
        }
    }

    /// <summary>
    /// GraphManager가 노드를 생성(Instantiate)한 직후, 이 함수를 호출해 데이터를 주입해야 합니다.
    /// </summary>
    public void Setup(VertexPosition data)
    {
        NodeData = data;

        // 필요한 경우 프리팹 하위의 텍스트 컴포넌트에 노드 이름을 띄우는 뼈대 배치 가능
        // GetComponentInChildren<TMPro.TMP_Text>()?.SetText(data.Id);
    }

    /// <summary>
    /// 플레이어가 이 노드 버튼을 실제로 마우스로 눌렀을 때 발동하는 함수
    /// </summary>
    private void OnNodeClicked()
    {
        if (NodeData == null) return;
        if (MapSessionManager.Instance == null) return;

        // 1. 전역 세션 매니저에 현재 밟은 노드 ID 저장 록킹
        MapSessionManager.Instance.VisitNode(NodeData.Id);

        // 2. 씬에 상주 중인 GraphManager를 찾아 화면 하이라이트 상태 실시간 갱신 명령
        // (FindObjectOfType은 비용이 드나, 노드 클릭 시점에 단 한 번만 수행되므로 안전함)
        MapManager manager = FindFirstObjectByType<MapManager>();
        if (manager != null)
        {
            // 아까 캐싱해 둔 딕셔너리를 기반으로 불빛(Highlight)만 칼같이 새로고침
            manager.DrawGraph();
        }

        switch (NodeData.type)
        {
            case vertType.fight:
                int randomSceneIndex = UnityEngine.Random.Range(0, 2);

                // 0이 나오면 BattleScene01, 1이 나오면 BattleScene02를 로드합니다.
                string targetScene = (randomSceneIndex == 0) ? "BattleScene01" : "BattleScene02";

                SceneManager.LoadScene(targetScene);
                break;

            case vertType.rest:
                //SceneManager.LoadScene("RestScene");
                break;
        }
    }
}