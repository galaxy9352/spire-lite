using UnityEngine;
using UnityEngine.UI;

public class GuardExample : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Button thisButton;
    Fighter player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Fighter>();
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(ClickAttack);
    }
    void ClickAttack()
    {
        player.Guard(10);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
