using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum TurnState { PlayerTurn, EnemyTurn, Won, Lost }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Unit Status")]
    public TurnState state;
    public Unit playerUnit;
    public Unit enemy1Unit;
    public Unit enemy2Unit;

    [Header("UI Reference")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI deckText;
    public TextMeshProUGUI discardText;
    public GameObject ResultWindow;

    [Header("Card UI System")]
    public GameObject cardPrefab;
    public Transform handArea;

    [Header("System Variables")]
    public int currentTurn = 1;
    //public int currentCost = 3;
    //public int maxCost = 3;
    // int playerBaseStrength = 6;
    public int handDrawCount = 5;

    [HideInInspector] public Card selectedCard;
    [HideInInspector] public CardUI selectedCardUI;

    //private DeckSystem deck;
    private readonly List<CardUI> handUI = new List<CardUI>();

    void Awake() => Instance = this;

    void Start()
    {
        playerUnit.isPlayer = true;
        playerUnit.Initialize(100);
        //playerUnit.strength = playerBaseStrength;

        enemy1Unit.Initialize(50);
        enemy2Unit.Initialize(50);

        //BuildStarterDeck();
        DeckSystem.Instance.BuildExampleDeck();

        state = TurnState.PlayerTurn;
        PlayerTurnStart();
    }

    // ==================== 덱 ====================

    void BuildStarterDeck()
    {
        /*
        AddCards(12, "타격", 1, 0, Card.CardType.Strike);
        AddCards(6, "방어", 1, 5, Card.CardType.Defend);
        AddCards(3, "취약", 1, 2, Card.CardType.Vulnerable);
        AddCards(3, "약화", 2, 2, Card.CardType.Weak);
        AddCards(3, "중독", 1, 3, Card.CardType.Poison);
        AddCards(3, "힘", 1, 2, Card.CardType.Strength);
        deck.Shuffle();
        */
    }

    void AddCards(int count, Card card)
    {
        for (int i = 0; i < count; i++)
        {
            Card newCard = Instantiate(card);
            DeckSystem.Instance.AddToDraw(newCard);
        }
    }

    // ==================== 플레이어 턴 ====================

    void PlayerTurnStart()
    {
        if (state == TurnState.Won || state == TurnState.Lost) return;

        playerUnit.OnTurnStart();
        playerUnit.curCost = playerUnit.maxCost;
        
        // 적은 새 게임 턴이 시작될 때 block을 초기화하고 새 스탠스를 결정.
        // 방어 스탠스를 고르면 OnTurnStart 직후 block이 0이지만, DecideNextAction에서 즉시 5 부여됨.
        if (enemy1Unit.IsAlive)
        {
            enemy1Unit.OnTurnStart();
            enemy1Unit.DecideNextAction();
        }
        else enemy1Unit.HideAllHeadText();

        if (enemy2Unit.IsAlive)
        {
            enemy2Unit.OnTurnStart();
            enemy2Unit.DecideNextAction();
        }
        else enemy2Unit.HideAllHeadText();
        StartCoroutine(DrawCardsToHand(handDrawCount));
        RefreshAllHeadUI();
        UpdateUI();
    }

    IEnumerator DrawCardsToHand(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            yield return new WaitForSeconds(0.2f);
            DeckSystem.Instance.Draw();
            HandManager.Instance.UpdateHandLayout();
        }
        HandManager.Instance.UpdateHandLayout();
        /*
        for (int i = 0; i < amount; i++)
        {
            Card drawn = deck.Draw();
            if (drawn == null) break;

            GameObject obj = Instantiate(cardPrefab, handArea);
            CardUI ui = obj.GetComponent<CardUI>();
            ui.Setup(drawn);
            handUI.Add(ui);
        }
        */
    }

    // ==================== 카드 사용 ====================

    /*
    public void SelectCard(CardUI clickedCardUI)
    {
        if (state != TurnState.PlayerTurn) return;
        if (clickedCardUI == null || clickedCardUI.cardData == null) return;

        Card clickedCard = clickedCardUI.cardData;
        if (!CanAfford(clickedCard)) return;

        if (IsSelfTargetCard(clickedCard))
        {
            PayCost(clickedCard);
            ApplySelfCardEffect(clickedCard);
            DiscardAndHideCard(clickedCardUI);
            RefreshAllHeadUI();
            UpdateUI();
            return;
        }

        if (selectedCardUI != null && selectedCardUI != clickedCardUI)
            selectedCardUI.SetSelectedStatus(false);

        selectedCardUI = clickedCardUI;
        selectedCard = clickedCard;
        selectedCardUI.SetSelectedStatus(true);
    }

    public void OnEnemyClicked(Unit targetEnemy)
    {
        if (state != TurnState.PlayerTurn) return;
        if (selectedCard == null || selectedCardUI == null) return;
        if (targetEnemy == null || !targetEnemy.IsAlive) return;
        if (!CanAfford(selectedCard)) return;

        PayCost(selectedCard);
        ApplyEnemyCardEffect(selectedCard, targetEnemy);
        DiscardAndHideCard(selectedCardUI);

        RefreshAllHeadUI();
        UpdateUI();
        CheckWinLose();
    }
    bool CanAfford(Card card) => card != null && currentCost >= card.cost;

    bool IsSelfTargetCard(Card card)
        => card.type == Card.CardType.Defend || card.type == Card.CardType.Strength;
    void PayCost(Card card)
    {
        currentCost = Mathf.Max(0, currentCost - card.cost);
    }
    void ApplySelfCardEffect(Card card)
    {
        switch (card.type)
        {
            case Card.CardType.Defend: playerUnit.AddBlock(card.value); break;
            case Card.CardType.Strength: playerUnit.AddStrength(card.value); break;
        }
    }

    void ApplyEnemyCardEffect(Card card, Unit target)
    {
        switch (card.type)
        {
            case Card.CardType.Strike:
                int dmg = target.CalculateIncomingDamage(playerUnit.strength, playerUnit);
                target.TakeFinalDamage(dmg);
                break;
            case Card.CardType.Vulnerable: target.vulnerable += card.value; break;
            case Card.CardType.Weak: target.weak += card.value; break;
            case Card.CardType.Poison: target.poison += card.value; break;
        }
    }

    public void DiscardAndHideCard(CardUI targetCardUI)
    {
        if (targetCardUI == null) return;

        deck.DiscardCard(targetCardUI.cardData);

        CanvasGroup cg = targetCardUI.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        if (selectedCardUI == targetCardUI)
        {
            selectedCard = null;
            selectedCardUI = null;
        }
    }
    
    */
    // ==================== 머리 위 UI ====================
    // block > 0이면 파란 숫자, 공격 의도가 있으면 빨간 숫자. 정책상 동시에 켜지지 않음
    // (방어 스탠스를 고르면 nextAction=None이 되므로).

    public void RefreshAllHeadUI()
    {
        playerUnit.ShowStrengthText(playerUnit.strength);
        playerUnit.RefreshBlockDisplay();

        RefreshEnemyHead(enemy1Unit);
        RefreshEnemyHead(enemy2Unit);
    }

    void RefreshEnemyHead(Unit enemy)
    {
        if (!enemy.IsAlive) { enemy.HideAllHeadText(); return; }
        enemy.RefreshBlockDisplay();
        enemy.RefreshIntentDisplay(playerUnit);

    }

    // ==================== 적 턴 ====================

    public void OnEndTurnButton()
    {
        if (state != TurnState.PlayerTurn) return;

        DeckSystem.Instance.DiscardEntireHand();

        foreach (CardUI ui in handUI) { if (ui != null) Destroy(ui.gameObject); }
        handUI.Clear();
        selectedCard = null;
        selectedCardUI = null;

        playerUnit.OnTurnEnd();
        RefreshAllHeadUI();
        UpdateUI();

        state = TurnState.EnemyTurn;
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        yield return RunSingleEnemyTurn(enemy1Unit);
        if (state == TurnState.EnemyTurn) yield return RunSingleEnemyTurn(enemy2Unit);

        CheckWinLose();

        if (state == TurnState.EnemyTurn)
        {
            currentTurn++;
            state = TurnState.PlayerTurn;
            PlayerTurnStart();
        }
    }

    // 적 턴에는 OnTurnStart를 다시 부르지 않음 — 적의 턴 갱신은 다음 PlayerTurnStart에서 일괄 처리.
    // 방어 스탠스라면 nextAction이 None이라 ExecuteEnemyAction은 아무것도 안 함.
    IEnumerator RunSingleEnemyTurn(Unit enemy)
    {
        if (!enemy.IsAlive) yield break;

        ExecuteEnemyAction(enemy);

        RefreshEnemyHead(enemy);
        playerUnit.RefreshBlockDisplay();

        CheckWinLose();
        yield return new WaitForSeconds(0.8f);
    }

    void ExecuteEnemyAction(Unit enemy)
    {
        if (!enemy.IsAlive) return;

        if (enemy.nextAction == Unit.EnemyAction.Attack)
        {
            int dmg = enemy.GetAttackDamage(enemy.nextActionValue, playerUnit);
            playerUnit.TakeFinalDamage(dmg);
        }
        // Defend는 이미 DecideNextAction 시점에 block을 받아두었으므로 여기선 아무것도 하지 않음.

        enemy.actionExecuted = true;
    }

    // ==================== 승패 / UI ====================

    public void CheckWinLose()
    {
        if (state == TurnState.Won || state == TurnState.Lost) return;

        if (!playerUnit.IsAlive)
        {
            state = TurnState.Lost;
            Debug.Log("★ GAME OVER: 플레이어 사망 ★");
            return;
        }
        if (!enemy1Unit.IsAlive && !enemy2Unit.IsAlive)
        {
            state = TurnState.Won;
            ResultWindow.SetActive(true);
            Debug.Log("★ VICTORY: 모든 적 처치 ★");
        }
    }

    public void UpdateUI()
    {
        turnText.text = "턴: " + currentTurn;
        costText.text = "코스트: " + playerUnit.curCost + " / " + playerUnit.maxCost;
        costText.text = "코스트: " + playerUnit.curCost + " / " + playerUnit.maxCost;
        deckText.text = "남은 덱: " + DeckSystem.Instance.DrawPileCount;
        discardText.text = "버린 카드: " + DeckSystem.Instance.DiscardPileCount;
    }
}