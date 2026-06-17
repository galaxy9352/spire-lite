using TMPro;
using UnityEngine;

// MonoBehaviour 대신 Unit을 상속받아 시스템이 Unit 타입으로 인식하게 합니다.
public class Unit02 : Unit
{
    public override void Initialize(int hp)
    {
        base.Initialize(40);
    }

    [Header("Unit02 전용 상태")]
    public int nextActionHits; // 다단 히트(연타) 횟수

    // 70% 확률로 4x2 공격, 30% 확률로 방어도 4를 얻도록 행동 결정
    public override void DecideNextAction()
    {
        actionExecuted = false;

        // 70% 확률로 공격 행동 선택
        if (Random.Range(0, 10) < 7)
        {
            nextAction = EnemyAction.Attack;
            nextActionValue = 4;
            nextActionHits = 2; // 2연타 세팅
        }
        else
        {
            // 방어 스탠스: 즉시 방어도 4 획득 후 대기
            block += 4;
            nextAction = EnemyAction.None;
            nextActionValue = 0;
            nextActionHits = 0;
        }
    }

    // 4x2 연타 데미지를 의도 UI에 올바르게 표시하기 위한 로직
    public override void RefreshIntentDisplay(Unit playerTarget)
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

        // 연타 횟수가 2 이상이면 "데미지x타수" (예: 4x2) 형태로 출력
        if (nextActionHits > 1)
        {
            damageText.text = $"{predicted}x{nextActionHits}";
        }
        else
        {
            damageText.text = predicted.ToString();
        }

        damageText.color = damageColor;
        damageText.transform.parent.gameObject.SetActive(true);
    }
}