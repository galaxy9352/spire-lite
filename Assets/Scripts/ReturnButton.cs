using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnButton : MonoBehaviour
{
    
    public void GotoMap()
    {
        SceneManager.LoadScene("MapScene");
    }
}
