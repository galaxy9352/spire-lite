using UnityEngine;
using System.Collections;
using UnityEditor.Build;

public class CardManager: MonoBehaviour
{
	public static CardManager Instance;

	public Card selectedData;
    public UnitPlayer user;
    public Unit target;
	bool isTargeting;

    // Use this for initialization
    void Awake()
	{
		if(Instance == null)
			Instance= this;
		else
		{
			Destroy(gameObject);
		}
	}
	public void OnCardSelected(Card data)
	{
		selectedData = data;
		isTargeting = true;
    }
	public void OnTargetSelected(Unit target)
	{
		if (!isTargeting || selectedData == null) return;
		this.target = target;
		user= getCardUser();

        StartCoroutine(selectedData.UseCardRoutine(user, target));
        isTargeting =false;
		selectedData = null;
    }
	UnitPlayer getCardUser()
	{
		return GameObject.FindGameObjectWithTag("Player").GetComponent<UnitPlayer>();
    }

    // Update is called once per frame
    void Update()
	{

	}
}