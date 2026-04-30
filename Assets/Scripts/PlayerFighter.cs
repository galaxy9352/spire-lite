using System;
using UnityEngine;

public class PlayerFighter : Fighter
{
    public int maxEnergy=3;
    public int curEnergy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log(enemyObjects.Length);
        foreach (var en in enemyObjects)
        {
            enemies.Add(en.GetComponent<Fighter>());
        }
        Debug.Log(enemies.Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
