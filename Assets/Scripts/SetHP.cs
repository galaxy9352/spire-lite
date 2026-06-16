using TMPro;
using UnityEngine;

public class SetHP : MonoBehaviour
{
    public TextMeshProUGUI HpDisplay;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HpDisplay.text = "HP " + PlayerManger.Instance.PlayerCurHp + "/" + PlayerManger.Instance.PlayerMaxHp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
