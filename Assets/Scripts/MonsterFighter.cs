using UnityEngine;

public class MonsterFighter : Fighter
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        GameObject[] playerObjects=GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObject in playerObjects)
        {
            enemies.Add(playerObject.GetComponent<Fighter>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
