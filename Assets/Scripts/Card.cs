using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum CardType
{
    Attack, Skill, Power
}
public enum EffectType
{
    Strike, Defend, Strength, Vulnerable, Weak, Poison
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/BasicCard")]
public class Card : ScriptableObject
{
    public string cardName;
    public int cost;
    public string description;
    public Sprite cardArt;
    public int effectCount;
    public List<EffectType> cardEffects = new List<EffectType>();
    public List<int> effectAmounts;
    public CardType cardType;
    public bool targetable= true;
    public void UseCard(Unit user, Unit target)
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
            }
        }
    }
    public bool isTargetable()
    {
        for(int i = 0;i < effectCount;i++)
        {
            if (cardEffects[i]!=EffectType.Defend && cardEffects[i] != EffectType.Strength)
            {
                return true;
            }
        }
        return false;
    }
}