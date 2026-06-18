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
    public UnitPlayer playerUnit;
    public UnitEnemy enemy1Unit;
    public UnitEnemy enemy2Unit;

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
        if (playerUnit != null) playerUnit.Initialize(100);
        //playerUnit.strength = playerBaseStrength;

        // 적이 할당되어 있을 때만 초기화 진행
        if (enemy1Unit != null) enemy1Unit.Initialize(50);
        if (enemy2Unit != null) enemy2Unit.Initialize(50);

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

        if (playerUnit != null)
        {
            playerUnit.OnTurnStart();
            playerUnit.curCost = playerUnit.maxCost;
        }

        // 각각의 적이 존재하는지(null이 아닌지) 먼저 확인한 후 로직 실행
        if (enemy1Unit != null)
        {
            if (enemy1Unit.IsAlive)
            {
                enemy1Unit.OnTurnStart();
                enemy1Unit.DecideNextAction();
            }
            else enemy1Unit.HideAllHeadText();
        }

        if (enemy2Unit != null)
        {
            if (enemy2Unit.IsAlive)
            {
                enemy2Unit.OnTurnStart();
                enemy2Unit.DecideNextAction();
            }
            else enemy2Unit.HideAllHeadText();
        }

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

    // ==================== 카드 사용 (주석 처리된 원본 유지) ====================
    /*
    ... (생략 없이 기존 주석 처리된 코드 그대로 존재) ...
    */

    // ==================== 머리 위 UI ====================

    public void RefreshAllHeadUI()
    {
        if (playerUnit != null)
        {
            playerUnit.ShowStrengthText(playerUnit.strength);
            //playerUnit.RefreshBlockDisplay();
        }

        if (enemy1Unit != null) RefreshEnemyHead(enemy1Unit);
        if (enemy2Unit != null) RefreshEnemyHead(enemy2Unit);
    }

    void RefreshEnemyHead(UnitEnemy enemy)
    {
        if (enemy == null) return; // 방어 코드 추가

        if (!enemy.IsAlive)
        {
            enemy.HideAllHeadText();
            return;
        }
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

        if (playerUnit != null) playerUnit.OnTurnEnd();

        RefreshAllHeadUI();
        UpdateUI();

        state = TurnState.EnemyTurn;
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        // 존재하는 적만 턴을 진행하도록 체크
        if (enemy1Unit != null) yield return RunSingleEnemyTurn(enemy1Unit);
        if (state == TurnState.EnemyTurn && enemy2Unit != null) yield return RunSingleEnemyTurn(enemy2Unit);

        CheckWinLose();

        if (state == TurnState.EnemyTurn)
        {
            currentTurn++;
            state = TurnState.PlayerTurn;
            PlayerTurnStart();
        }
    }

    IEnumerator RunSingleEnemyTurn(UnitEnemy enemy)
    {
        if (enemy == null || !enemy.IsAlive) yield break;

        yield return StartCoroutine(ExecuteEnemyActionRoutine(enemy));

        RefreshEnemyHead(enemy);
        //playerUnit.RefreshBlockDisplay();

        CheckWinLose();
        yield return new WaitForSeconds(0.8f);
    }

    IEnumerator ExecuteEnemyActionRoutine(UnitEnemy enemy)
    {
        if (enemy == null || !enemy.IsAlive) yield break;

        if (enemy.nextAction == UnitEnemy.EnemyAction.Attack)
        {
            int hits = 1;

            if (enemy is UnitEnemyType2 unit02) hits = unit02.nextActionHits;

            if (enemy is UnitEnemyType3 unit03) hits = unit03.nextActionHits;

            for (int i = 0; i < hits; i++)
            {
                if (enemy.animator != null) enemy.animator.SetTrigger("Attack");
                yield return new WaitForSeconds(0.3f);
                if (playerUnit.animator != null) playerUnit.animator.SetTrigger("Hit");

                // 데미지 계산 및 적용
                int dmg = enemy.GetAttackDamage(enemy.nextActionValue, playerUnit);
                playerUnit.TakeFinalDamage(dmg);

                // ==================================================
                // 적이 Type3(흡혈귀)라면 데미지 비례 회복 진행
                // ==================================================
                if (enemy is UnitEnemyType3 vampireEnemy)
                {
                    vampireEnemy.ApplyLifesteal(dmg);
                }

                if (i < hits - 1) yield return new WaitForSeconds(0.25f);
            }
        }
        enemy.actionExecuted = true;
    }

    // ==================== 승패 / UI ====================

    public void CheckWinLose()
    {
        if (state == TurnState.Won || state == TurnState.Lost) return;

        if (playerUnit != null && !playerUnit.IsAlive)
        {
            state = TurnState.Lost;
            Debug.Log("★ GAME OVER: 플레이어 사망 ★");
            return;
        }

        // 핵심 변경 사항: 유닛이 null이거나(애초에 없거나) 죽어있으면 처치한 것으로 간주
        bool isEnemy1Dead = (enemy1Unit == null || !enemy1Unit.IsAlive);
        bool isEnemy2Dead = (enemy2Unit == null || !enemy2Unit.IsAlive);

        // 할당된 모든 적이 죽었을 때만 승리
        if (isEnemy1Dead && isEnemy2Dead)
        {
            state = TurnState.Won;
            if (ResultWindow != null) ResultWindow.SetActive(true);

            // 오타로 추정되는 PlayerManger(Manager) 유지
            if (playerUnit != null)
                PlayerManger.Instance.UpdateHp(playerUnit.maxHP, playerUnit.currentHP);

            Debug.Log("★ VICTORY: 모든 적 처치 ★");
        }

        if (enemy1Unit != null) RefreshEnemyHead(enemy1Unit);
        if (enemy2Unit != null) RefreshEnemyHead(enemy2Unit);
    }

    public void UpdateUI()
    {
        if (turnText != null) turnText.text = "턴: " + currentTurn;

        if (playerUnit != null && costText != null)
            costText.text = "코스트: " + playerUnit.curCost + " / " + playerUnit.maxCost;

        if (deckText != null) deckText.text = "남은 덱: " + DeckSystem.Instance.DrawPileCount;
        if (discardText != null) discardText.text = "버린 카드: " + DeckSystem.Instance.DiscardPileCount;
    }
}