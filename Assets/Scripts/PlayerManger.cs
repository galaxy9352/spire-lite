using UnityEngine;

public class PlayerManger : MonoBehaviour
{
    public static PlayerManger Instance;
    public int PlayerMaxHp = 100;
    public int PlayerCurHp = 100;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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


}
