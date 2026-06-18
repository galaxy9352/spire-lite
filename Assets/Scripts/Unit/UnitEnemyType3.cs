using TMPro;
using UnityEngine;

public class UnitEnemyType3 : UnitEnemy
{
    public int nextActionHits; // 다단 히트(연타) 횟수
    private bool hasUsedUltimateDefense = false; // 99 방어도 패턴 사용 여부 추적

    public override void Initialize(int hp)
    {
        base.Initialize(150);
    }

    public override void DecideNextAction()
    {
        actionExecuted = false;

        // ==========================================
        // 1. 특수 패턴 (HP 20 이하 & 게임 중 단 1회)
        // ==========================================
        if (currentHP <= 20 && !hasUsedUltimateDefense)
        {
            // 즉시 99의 방어도를 얻고 턴을 넘김
            block += 99;
            hasUsedUltimateDefense = true; // 다시는 발동하지 않도록 록온

            nextAction = EnemyAction.None;
            nextActionValue = 0;
            nextActionHits = 0;
            return; // 일반 패턴으로 넘어가지 않도록 즉시 종료
        }

        // ==========================================
        // 2. 일반 패턴 (60% 공격, 40% 방어)
        // ==========================================
        int randomValue = Random.Range(0, 100);

        if (randomValue < 60)
        {
            // 60% 확률: 3의 공격력으로 3번 공격
            nextAction = EnemyAction.Attack;
            nextActionValue = 3;
            nextActionHits = 3;
        }
        else
        {
            // 40% 확률: 방어도 6 획득
            block += 6;
            nextAction = EnemyAction.None;
            nextActionValue = 0;
            nextActionHits = 0;
        }
    }

    // 의도(Intent) UI 표시 로직 (Type2의 연타 표시 로직 재사용)
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

    // ==========================================
    // 흡혈 전용 함수 (BattleManager에서 타격 직후 호출됨)
    // ==========================================
    public void ApplyLifesteal(int damageDealt)
    {
        if (damageDealt <= 0 || !IsAlive) return;

        // 데미지의 50% 계산 (소수점 버림)
        int healAmount = Mathf.FloorToInt(damageDealt * 0.5f);

        if (healAmount > 0)
        {
            // 최대 체력(150)을 넘지 않도록 안전하게 회복
            currentHP = Mathf.Min(maxHP, currentHP + healAmount);

            // 만약 체력바 UI가 있다면 여기서 갱신해줍니다. (hpBar가 연결된 경우)

            Debug.Log($"[흡혈 발동] {damageDealt} 피해를 입히고 {healAmount} 회복! 현재 HP: {currentHP}");
        }
    }
}