using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnButton : MonoBehaviour
{
    
    public void GotoMap()
    {
        if (MapSessionManager.Instance.VisitedNodeIds.Count == 8 || BattleManager.Instance.state == TurnState.Lost)
        {
            SceneManager.LoadScene("StartMenu");
            Destroy(MapSessionManager.Instance.gameObject);
            Destroy(PlayerManger.Instance.gameObject);
        }
        else
        { 
            SceneManager.LoadScene("MapScene");
        }

    }
}
