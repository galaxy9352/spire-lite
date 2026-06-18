using UnityEngine;
using System.Collections;
public class CardUseManager : MonoBehaviour
{
    private UnitPlayer player;
    private Unit target;

    public static CardUseManager Instance; 
    // [HideInInspector] // 필요시 주석 해제 (인스펙터에서 안보이게)
    public CardUI selectedCard = null; // 현재 위로 올라와 있는 '선택된 카드'
    public bool waitingTarget = false;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<UnitPlayer>();
        }
        else
        {
            Debug.LogError("🚨 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다!");
        }
    }

    // 💡 [새로 추가] 손패의 카드가 클릭되었을 때의 처리 (핵심 로직)
    public void OnCardClickedInHand(CardUI clickedCard)
    {
        // 1. 이미 다른 카드가 선택되어 있었다면, 그 카드를 다시 내립니다.
        if (selectedCard != null && selectedCard != clickedCard)
        {
            CancelSelection();
        }

        clickedCard.isSelected = true;
        selectedCard = clickedCard;


        if (selectedCard.cardData.isTargetable())
        {
            waitingTarget = true;
        }
        else
        {
            ProcessCardUsage(selectedCard);
        }
    }

    public void OnTargetSelected(Unit target)
    {
        if (selectedCard == null) return;
        this.target = target;
        waitingTarget = false;

        ProcessCardUsage (selectedCard);
    }

    private IEnumerator ProcessCardUsageRoutine(CardUI cardToUse, Unit targetUnit)
    {
        Card ctuData = Instantiate(cardToUse.cardData);

        // 코스트(에너지) 체크
        if (player == null || player.curCost < ctuData.cost)
        {
            Debug.LogWarning("❌ 에너지가 부족합니다!");
            cardToUse.isSelected = false;
            selectedCard = null;
            yield break; // 코루틴 즉시 종료
        }

        // 코스트 지불 및 UI 업데이트
        player.curCost -= ctuData.cost;
        BattleManager.Instance.UpdateUI();

        // 💡 변경된 코루틴 실행 및 대기 (다단히트 등 효과가 모두 끝날 때까지 대기)
        yield return StartCoroutine(ctuData.UseCardRoutine(player, targetUnit));

        Debug.Log($"🎯 {cardToUse.cardName} 카드가 성공적으로 사용되었습니다!");

        // 카드 사용이 모두 끝난 후 패에서 버림 처리
        selectedCard = null;
        DeckSystem.Instance.DiscardCard(cardToUse.transform.GetSiblingIndex());
        BattleManager.Instance.UpdateUI();
    }

    // 2️⃣ 기존 함수들이 있던 자리에 OnCardClickedInHand, OnTargetSelected에서 바로 부를 수 있도록 연결 함수를 작성해.
    private void ProcessCardUsage(CardUI cardToUse)
    {
        StartCoroutine(ProcessCardUsageRoutine(cardToUse, target));
    }

    public void CancelSelection()
    {
        selectedCard.isSelected = false;
        selectedCard=null;
    }
    public void DiscardAllCards()
    {

    }

    // (기존 '카드 내기' UI 버튼에 연결된 함수 - 필요시 유지)
    public void OnUseButtonClicked()
    {
        if (selectedCard != null)
        {
            ProcessCardUsage(selectedCard);
        }
        else
        {
            Debug.LogWarning("❌ 먼저 손패에서 카드를 선택해 주세요!");
        }
    }
}