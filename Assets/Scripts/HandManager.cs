using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;
    public GameObject cardPrefab;
    public Transform handParent;  // ⚠️ 이제 이 부모 오브젝트의 Horizontal Layout Group은 꺼주세요!
    public List<GameObject> handCards=new List<GameObject>(10);
    //public DeckSystem deck;

    [Header("부채꼴 정렬 설정")]
    public float cardSpacing = 200f;   // 카드 간의 가로 간격
    public float arcHeight = 30f;      // 부채꼴로 휭 정도 (최대 높이)
    public float angleSpacing = 5f;    // 카드 한 장당 기울어질 각도
    void Awake()
	{
		if(Instance == null)
			Instance= this;
		else
		{
			Destroy(gameObject);
		}
	}
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DrawCard();
        }*/
    }

    //손패의 CardUI 오브젝트 생성, 파괴 담당
    public void DrawCardObject( Card drawn )
    {
        // UI 생성 (처음엔 부모 위치에 생성)
        GameObject newCard = Instantiate(cardPrefab,new Vector3(0,0,0), new Quaternion(), handParent);//
        newCard.GetComponent<CardUI>().Setup(drawn);
        handCards.Add(newCard);
    }
    public IEnumerator DiscardCardObjectInHand(int index)
    {
        UpdateOnDiscard(index);
        yield return new WaitForSeconds(0.2f);
        handCards[index].transform.SetParent(null);
        Destroy(handCards[index]);
        handCards.RemoveAt(index);
        UpdateHandLayout();
    }
    public IEnumerator DiscardAllCardsInHand()
    {
        while(handCards.Count > 0)
        {
            yield return StartCoroutine(DiscardCardObjectInHand(0));
        }
        handCards.Clear();
    }


    public void UpdateOnDiscard(int index)
    {
        CardUI[] cardsInHand = handParent.GetComponentsInChildren<CardUI>();
        int cardCount = DeckSystem.Instance.HandCount;

        Vector3 targetPos = new Vector3(1500, 20, 0);
        Quaternion targetRot = Quaternion.Euler(0, 0, -120);

        cardsInHand[index].SetTargetTransform(targetPos, targetRot);
    }

    // 카드를 썼을 때 파괴되는 타이밍 직후에도 정렬을 맞춰주기 위해 호출해야 합니다.
    public void UpdateHandLayout()
    {
        // handParent 자식으로 있는 모든 CardDisplay 컴포넌트를 가져옴
        CardUI[] cardsInHand = handParent.GetComponentsInChildren<CardUI>();
        int cardCount = DeckSystem.Instance.HandCount;

        if (cardCount == 0) return;

        // 중앙 인덱스 계산 (예: 5장이면 2번째 카드가 중심)
        float centerIndex = (cardCount - 1) / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            // 중앙으로부터의 상대적인 거리 (-2, -1, 0, 1, 2 형식)
            float offset = i - centerIndex;

            // 1. 가로 위치 (X) 계산
            float posX = offset * cardSpacing;

            // 2. 세로 위치 (Y) 계산 (이차함수 그래프 형태로 중심은 높고 양 끝은 낮게)
            // 중앙(offset=0)일 때 0이 되고 양끝으로 갈수록 -값이 되도록 연산 후 arcHeight를 곱함
            float posY = (1 - (offset * offset) / (centerIndex * centerIndex + 0.1f)) * arcHeight;
            // 만약 카드가 1장일 때 예외처리 (+0.1은 0으로 나누기 방지용)
            if (cardCount == 1) posY = 0;
            //카드가 다른 UI를 가리는 경우를 위한 위치 조정
            posY -= 30f;
            posX += 30f;

            // 3. 회전 각도 (Z) 계산 (왼쪽 카드는 양수 각도, 오른쪽 카드는 음수 각도)
            float angle = -offset * angleSpacing;

            // 최종 위치 및 회전 적용
            Vector3 targetPos = new Vector3(posX, posY, 0);
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);

            // CardDisplay에게 새로운 목적지 전달
            cardsInHand[i].SetTargetTransform(targetPos, targetRot);

            Debug.Log(i+", "+cardsInHand[i].finalTargetPos);
        }
       
       

    }
}