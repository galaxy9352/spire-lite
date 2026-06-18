using TMPro;
using UnityEngine;
using static UnitEnemy;

public class UnitPlayer : Unit
{
    //[Header("Unit Status")]
    public int maxCost;
    public int curCost;


    public override void Initialize(int hp)
    {
        base.Initialize(hp);
        if (PlayerManger.Instance != null)
        {
            maxHP = PlayerManger.Instance.PlayerMaxHp;
            currentHP = PlayerManger.Instance.PlayerCurHp;
        }
        else
        {
            maxHP = hp;
            currentHP = hp;
        }

        block = strength = vulnerable = weak = poison = 0;
        strengthAddedLastTurn = strengthAddedThisTurn = 0;

        HideAllHeadText();
    }
    public void HideAllHeadText()
    {
        if (strengthText != null) strengthText.gameObject.SetActive(false);
        if (blockText != null) blockText.transform.parent.gameObject.SetActive(false);
    }

}
