using System.Collections.Generic;
using UnityEngine;

// 드로우/핸드/버린 더미 관리 전용.
public class DeckSystem : MonoBehaviour
{
    public List<Card> drawPile = new List<Card>();
    public List<Card> discardPile = new List<Card>();
    public List<Card> hand = new List<Card>();
    
    public static DeckSystem Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    public int HandCount => hand.Count;

    public void AddToDraw(Card card) => drawPile.Add(card);

    
    public void BuildExampleDeck()
    {
        Card[] loadData= Resources.LoadAll<Card>("ScriptableObjects/Cards/CardData");

        AddToDraw(loadData[0]);
        AddToDraw(loadData[0]);
        AddToDraw(loadData[0]);
        AddToDraw(loadData[0]);
        AddToDraw(loadData[1]);
        AddToDraw(loadData[2]);
        AddToDraw(loadData[3]);
        AddToDraw(loadData[3]);
        AddToDraw(loadData[3]);
        AddToDraw(loadData[3]);
        Shuffle();
    }

    public void Shuffle()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            int rand = Random.Range(i, drawPile.Count);
            (drawPile[i], drawPile[rand]) = (drawPile[rand], drawPile[i]);
        }
    }

    // 한 장 뽑아 핸드에 추가. 양쪽 다 비었으면 null 반환.
    public Card Draw()
    {
        if (drawPile.Count == 0 && discardPile.Count > 0)
        {
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            Shuffle();
        }
        if (drawPile.Count == 0) return null;
        Card card = drawPile[0];
        HandManager.Instance.DrawCardObject(card);
        HandManager.Instance.UpdateHandLayout();
        drawPile.RemoveAt(0);
        hand.Add(card);
        BattleManager.Instance.UpdateUI();
        return card;
    }

    public void DiscardCard(int index)
    {
        Card card = hand[index];
        hand.RemoveAt(index);
        StartCoroutine(HandManager.Instance.DiscardCardObjectInHand(index));
        discardPile.Add(card);
    }

    public void DiscardEntireHand()
    {
        discardPile.AddRange(hand);
        StartCoroutine(HandManager.Instance.DiscardAllCardsInHand());
        BattleManager.Instance.UpdateUI();
        hand.Clear();
    }
}