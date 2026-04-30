
using System.Collections.Generic;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    public int maxHp = 0;
    public int curHp = 0;
    public int shield = 0;
    protected List<Fighter> enemies = new List<Fighter>();
    protected int power = 0;
    protected int dex = 0;

    protected Fighter()
    {

    }

    public void Attack(int enemyIndex, int amount)
    {
        Fighter target=enemies[enemyIndex];
        int damage = CalcDamage(amount);
        target.GetDamage(damage);
        Debug.Log(amount+", "+ target.curHp);
    }
    public void Guard(int amount)
    {
        shield += amount;
    }
    protected int CalcDamage(int amount)
    {
        return amount;
    }
    protected bool GetDamage(int damage)
    {
        if (shield >= damage)
        {
            shield-=damage;
            return false;
        }
        else
        {
            shield = 0;
            damage-=shield;
            curHp-=damage;
            return false;
        }
    }
    public bool IsGuard()
    {
        if (shield <= 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        curHp = maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
