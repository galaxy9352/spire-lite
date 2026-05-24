using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{

    [Header("데이터 및 UI")]
    public Card cardData;
    public Image cardImage;
    public TextMeshProUGUI cardType;
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI cardDesc;

    [Header("이동 및 시각 효과")]
    public float selectLiftAmount = 60f; // 위로 올라갈 높이
    public float moveSpeed = 12f;       // 이동 속도
    public bool isSelected = false;     // 현재 선택 여부
    public Image backgroundImage;

    private Vector3 baseTargetPosition;
    private Quaternion targetRotation;
    public Vector3 finalTargetPos;

    public void Setup(Card card)
    {
        cardData = card;
        cardImage = gameObject.transform.GetChild(0).GetComponent<Image>();
        cardType = gameObject.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        cardName = gameObject.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
        cardDesc = gameObject.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>();
        ApplyCardData();
        //backgroundImage.color = Color.white;
    }
    void Update()
    {
        finalTargetPos = baseTargetPosition;
        // 선택 상태에 따라 최종 목표 Y축 위치를 계산
        if (isSelected)
        {
            finalTargetPos += new Vector3(0, selectLiftAmount, 0);
        }

    }

    public void LateUpdate()
    {
        transform.localScale = new Vector3(2,2,2);
        // 부드럽게 목표 좌표로 이동
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalTargetPos, Time.deltaTime * moveSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * moveSpeed);
        //Debug.Log(cardName.text+":"+baseTargetPosition+","+transform.localPosition);
    }

    public void SetTargetTransform(Vector3 position, Quaternion rotation)
    {
        baseTargetPosition = position;
        targetRotation = rotation;
    }


    public void OnCardClicked()
    {
        Debug.Log(cardData.cardName);
        CardUseManager.Instance.OnCardClickedInHand( this );
    }

    public void SetSelectedStatus(bool isSelected)
    {
        //backgroundImage.color = isSelected ? Color.yellow : Color.white;
    }
    public void ApplyCardData()
    {
        cardImage.sprite = cardData.cardArt;
        cardName.text = cardData.cardName + "(" + cardData.cost + ")";
        cardDesc.text = cardData.description;
        cardType.text = cardData.cardType.ToString();

    }
    /*
    public void OnPointerClick(PointerEventData eventData)
    {
        // Implement card click behavior here
        Debug.Log("Card clicked: " + cardData.cardName);

        CardManager.Instance.OnCardSelected(cardData);
        //cardData.UseCard(GameObject.FindGameObjectWithTag("Player").GetComponent<Fighter>(), GameObject.FindGameObjectWithTag("Enemy").GetComponent<Fighter>());

    }*/
}