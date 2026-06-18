using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

[System.Serializable]
public enum CardType
{
    Attack, Buff, Debuff
}
public enum EffectType
{
    Strike, Defend, Strength, Vulnerable, Weak, Poison, CostCard, DrawCard, Heal
}


[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/BasicCard")]
public class Card : ScriptableObject
{

    public string cardName;
    public int cost;
    public string description;
    public Sprite cardArt;
    public UnitPlayer playerUnit;
    public int effectCount;
    public List<EffectType> cardEffects = new List<EffectType>();
    public List<int> effectAmounts;
    public CardType cardType;
    public bool targetable= true;
    public IEnumerator UseCardRoutine(UnitPlayer user, Unit target)
    {
        for(int i = 0; i < effectCount; i++)
        {
            switch (cardEffects[i])
            {
                case EffectType.Defend:
                    user.block += effectAmounts[i];
                    break;
                case EffectType.Strength:
                    user.strength += effectAmounts[i];
                    break;
                case EffectType.Strike:
                    target.TakeFinalDamage(target.CalculateIncomingDamage(user.GetAttackDamage(effectAmounts[i], target)));
                    yield return new WaitForSeconds(0.2f);
                    break;
                case EffectType.Vulnerable:
                    target.vulnerable+= effectAmounts[i];
                    break;
                case EffectType.Weak:
                    target.weak += effectAmounts[i];
                    break;
                case EffectType.Poison:
                    target.poison += effectAmounts[i];
                    break;
                case EffectType.CostCard:
                   user.maxCost+= effectAmounts[i];
                    break;
                case EffectType.DrawCard:
                   
                    for (int j = 0; j < effectAmounts[i]; j++)
                    {
                        DeckSystem.Instance.Draw();
                        HandManager.Instance.UpdateHandLayout();
                    }
                    break;
                case EffectType.Heal:
                    if (user.currentHP + effectAmounts[i] >= user.maxHP)
                    {
                        user.currentHP = user.maxHP;
                    }
                    else { user.currentHP += effectAmounts[i]; }

                    break;

            }
        }
    }
    public bool isTargetable()
    {
        for(int i = 0;i < effectCount;i++)
        {
            if (cardEffects[i]!=EffectType.Defend && cardEffects[i] != EffectType.Strength&& cardEffects[i] != EffectType.CostCard&& cardEffects[i] != EffectType.DrawCard && cardEffects[i] != EffectType.Heal)
            {
                return true;
            }
        }
        return false;
    }
}