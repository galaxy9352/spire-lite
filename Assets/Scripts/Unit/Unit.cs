using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Unit : MonoBehaviour, IPointerDownHandler
{
    //[Header("Unit Status")]
    public string unitName;
    public int maxHP;
    public int currentHP;

    //[Header("Status Effects")]
    public int block;
    public int strength;
    public int vulnerable;
    public int weak;
    public int poison;

    [HideInInspector] public int strengthAddedLastTurn;
    [HideInInspector] public int strengthAddedThisTurn;

    //[Header("UI Reference")]
    //public Slider hpSlider;
    //public TextMeshProUGUI hpText;

    //[Header("Head UI")]
    public TextMeshProUGUI strengthText; // 플레이어 전용
    public TextMeshProUGUI blockText;    // 실제 block 수치 (파랑)


    public bool IsAlive => currentHP > 0;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (CardUseManager.Instance == null)
        {
            return;
        }
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            CardUseManager.Instance.OnTargetSelected(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            CardUseManager.Instance.CancelSelection();
        }
    }
    public virtual void Initialize(int hp)
    {

    }

    // 게임 턴 시작 시 호출 (플레이어/적 모두). block은 매 게임 턴 새로 결정됨.
    public void OnTurnStart()
    {
        block = 0;

        if (strengthAddedLastTurn > 0)
        {
            strength = Mathf.Max(0, strength - strengthAddedLastTurn);
            strengthAddedLastTurn = 0;
        }
        strengthAddedLastTurn = strengthAddedThisTurn;
        strengthAddedThisTurn = 0;

        if (poison > 0)
        {
            ApplyDirectDamage(poison);
            poison--;
        }

        if (vulnerable > 0) vulnerable--;
        if (weak > 0) weak--;
    }

    public void OnTurnEnd() { }

    public int CalculateIncomingDamage(int baseDamage)
    {
        float calc = baseDamage;
        if (vulnerable > 0) calc *= 1.5f;
        return Mathf.FloorToInt(calc);
    }
    public int GetAttackDamage(int baseDamage, Unit target) //공격자가 지닌 힘 등을 기반으로 공격력 책정(추후 카드의 공격력(논타겟)표기에도 사용 예정)
    {
        float calc = baseDamage += strength;
        if (weak > 0) calc *= 0.75f;
        return Mathf.FloorToInt(calc);
    }
    public void TakeFinalDamage(int finalDamage)
    {
        if (finalDamage <= 0) return;

        int afterBlock = Mathf.Max(0, finalDamage - block);
        block = Mathf.Max(0, block - finalDamage);

        ApplyDirectDamage(afterBlock);
        BattleManager.Instance.CheckWinLose();
    }

    // block을 우회하는 직접 피해 (중독 등). HP 0 미만 클램프 단일 경로.
    private void ApplyDirectDamage(int amount)
    {
        if (amount <= 0) return;
        currentHP = Mathf.Max(0, currentHP - amount);
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.CheckWinLose();
        }
    }
    public void ShowStrengthText(int amount)
    {
        if (strengthText == null) return;
        if (amount <= 0) { strengthText.gameObject.SetActive(false); return; }
        strengthText.gameObject.SetActive(true);
        strengthText.text = amount.ToString();
        //strengthText.color = damageColor;
    }


    public void AddStrength(int amount)
    {
        strength += amount;
        strengthAddedThisTurn += amount;
    }

    public void AddBlock(int amount)
    {
        if (amount <= 0) return;
        block += amount;
    }
}