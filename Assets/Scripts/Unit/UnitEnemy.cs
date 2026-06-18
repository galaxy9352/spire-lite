using TMPro;
using UnityEngine;

public class UnitEnemy : Unit
{

    [HideInInspector] public bool actionExecuted;

    //[Header("Head UI")]
    public TextMeshProUGUI damageText;   // 적의 공격 예정 데미지 (빨강)

    //[Header("Next Action (적 전용)")]
    public EnemyAction nextAction;
    public int nextActionValue;

    //[Header("Colors")]
    public Color damageColor = new Color(1f, 0.3f, 0.3f);
    public Color blockColor = new Color(0.4f, 0.7f, 1f);

    public enum EnemyAction { None, Attack, Defend }

    // 공격 의도가 있을 때만 빨간 예정 데미지 표시. 그 외에는 숨김.
    public virtual void RefreshIntentDisplay(Unit playerTarget)
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
     //block > 0이면 파란 숫자 표시, 0이면 자동 숨김.
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
    public override void Initialize(int hp)
    {
        base.Initialize(hp);
        maxHP = hp;
        currentHP = hp;

        block = strength = vulnerable = weak = poison = 0;
        strengthAddedLastTurn = strengthAddedThisTurn = 0;

        actionExecuted = false;
        nextAction = EnemyAction.None;
        nextActionValue = 0;

        HideAllHeadText();
    }

    // 플레이어 턴 시작 시 호출. 방어를 고르면 즉시 block을 부여하고 의도는 None으로.
    // 공격을 고르면 nextAction=Attack으로 두고 적 턴에 실행.
    public virtual void DecideNextAction()
    {
        actionExecuted = false;

        if (Random.Range(0, 10) < 8)
        {
            nextAction = EnemyAction.Attack;
            nextActionValue = 7;
        }
        else
        {
            // 방어 스탠스: 즉시 block 부여, 적 턴에는 아무 행동도 안 함.
            block += 5;
            nextAction = EnemyAction.None;
            nextActionValue = 0;
        }
    }
    public void HideAllHeadText()
    {
        if (damageText != null) damageText.transform.parent.gameObject.SetActive(false);
        if (blockText != null) blockText.transform.parent.gameObject.SetActive(false);
    }

    protected override void Die()
    {
        base.Die();

    }


}
