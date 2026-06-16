using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Unit : MonoBehaviour, IPointerDownHandler
{
    [Header("Unit Status")]
    public string unitName;
    public bool isPlayer;
    public int maxHP;
    public int currentHP;
    public int maxCost;
    public int curCost;

    [Header("Status Effects")]
    public int block;
    public int strength;
    public int vulnerable;
    public int weak;
    public int poison;

    [HideInInspector] public int strengthAddedLastTurn;
    [HideInInspector] public int strengthAddedThisTurn;
    [HideInInspector] public bool actionExecuted;

    [Header("UI Reference")]
    //public Slider hpSlider;
    //public TextMeshProUGUI hpText;

    [Header("Head UI")]
    public TextMeshProUGUI strengthText; // 플레이어 전용
    public TextMeshProUGUI damageText;   // 적의 공격 예정 데미지 (빨강)
    public TextMeshProUGUI blockText;    // 실제 block 수치 (파랑)

    [Header("Colors")]
    public Color damageColor = new Color(1f, 0.3f, 0.3f);
    public Color blockColor = new Color(0.4f, 0.7f, 1f);

    [Header("Next Action (적 전용)")]
    public EnemyAction nextAction;
    public int nextActionValue;
    public enum EnemyAction { None, Attack, Defend }

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
    public void Initialize(int hp)
    {
        if (isPlayer && PlayerManger.Instance != null)
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
        actionExecuted = false;
        nextAction = EnemyAction.None;
        nextActionValue = 0;

        HideAllHeadText();
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
        strengthText.color = damageColor;
    }

    // block > 0이면 파란 숫자 표시, 0이면 자동 숨김.
    public void RefreshBlockDisplay()
    {
        if (blockText == null) return;
        if (!IsAlive || block <= 0)
        {
            blockText.transform.parent.gameObject.SetActive(false);
            return;
        }
        blockText.transform.parent.gameObject.SetActive(true);
        blockText.text = block.ToString();
        blockText.color = blockColor;
    }

    // 공격 의도가 있을 때만 빨간 예정 데미지 표시. 그 외에는 숨김.
    public void RefreshIntentDisplay(Unit playerTarget)
    {
        if (damageText == null) return;

        if (!IsAlive || actionExecuted || nextAction != EnemyAction.Attack)
        {
            damageText.transform.parent.gameObject.SetActive(false);
            return;
        }

        int predicted = (playerTarget != null)
        ? playerTarget.CalculateIncomingDamage(GetAttackDamage(nextActionValue, playerTarget)) 
        : GetAttackDamage(nextActionValue, null);

        damageText.text = predicted.ToString();
        damageText.color = damageColor;
        damageText.transform.parent.gameObject.SetActive(true);
    }

    public void HideAllHeadText()
    {
        if (strengthText != null) strengthText.gameObject.SetActive(false);
        if (damageText != null) damageText.transform.parent.gameObject.SetActive(false);
        if (blockText != null) blockText.transform.parent.gameObject.SetActive(false);
    }

    // 플레이어 턴 시작 시 호출. 방어를 고르면 즉시 block을 부여하고 의도는 None으로.
    // 공격을 고르면 nextAction=Attack으로 두고 적 턴에 실행.
    public void DecideNextAction()
    {
        actionExecuted = false;

        if (Random.Range(0, 10) < 8)
        {
            nextAction = EnemyAction.Attack;
            nextActionValue = 10;
        }
        else
        {
            // 방어 스탠스: 즉시 block 부여, 적 턴에는 아무 행동도 안 함.
            block += 5;
            nextAction = EnemyAction.None;
            nextActionValue = 0;
        }
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