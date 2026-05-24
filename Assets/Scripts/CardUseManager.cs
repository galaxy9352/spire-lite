using UnityEngine;

public class CardUseManager : MonoBehaviour
{
    private Unit player;
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
            player = playerObj.GetComponent<Unit>();
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

    // 💡 [기능 분리] 실제 카드 사용 및 오브젝트 파괴 로직
    private void ProcessCardUsage(CardUI cardToUse)
    {
        // 실제 효과 적용 시도
        if (UseCard(cardToUse, target))
        {
            selectedCard = null; // 선택 참조 초기화
            // 사용 성공 시
            DeckSystem.Instance.DiscardCard( cardToUse.transform.GetSiblingIndex());
            // 패 재정렬 호출
            if (HandManager.Instance != null)
            {
                //HandManager.Instance.UpdateHandLayout();
            }
        }
        else
        {
            // 💡 에너지 부족 등으로 사용 실패 시 -> 다시 아래로 내립니다.
            cardToUse.isSelected = false;
            selectedCard = null;
            
        }
        BattleManager.Instance.UpdateUI();
    }

    // [기존 UseCard 함수 내용 유지 - 리턴타입 bool인 버전]
    public bool UseCard(CardUI cardToUse, Unit target)
    {
        Card ctuData = Instantiate(cardToUse.cardData);
        if (player == null) return false;
        if (player.curCost < ctuData.cost)
        {
            Debug.LogWarning("❌ 에너지가 부족합니다!");
            return false;
        }
        ctuData.UseCard(player, target);
        player.curCost -= ctuData.cost;

        Debug.Log($"🎯 {cardToUse.cardName} 카드가 성공적으로 사용되었습니다!");
        return true;
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