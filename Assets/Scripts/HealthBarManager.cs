using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    Unit parent;
    RectTransform barVal;
    TextMeshProUGUI barTxt;
    Image barBg;
    GameObject guardIndicator;
    TextMeshProUGUI guaTxt;
    float hpbarWidth;
    string hpIndicator;
    string guardingColor = "#AAAAAA"; // 바꾸고 싶은 색상 코드
    string noGuardColor = "#000000"; // 바꾸고 싶은 색상 코드
    Color curColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parent = gameObject.GetComponentInParent<Unit>();
        barVal = transform.GetChild(1).GetComponent<RectTransform>();
        barTxt = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        barBg = transform.GetChild(0).GetComponent<Image>();
        guardIndicator = transform.GetChild(3).gameObject;
        guaTxt = guardIndicator.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
    }
    void LateUpdate()
    {
        // 실드 있는 경우/없는 경우 체력바 변경
        if (parent.block > 0)
        {
            ColorUtility.TryParseHtmlString(guardingColor, out curColor);
            barBg.color = curColor;
            guardIndicator.SetActive(true);
        }
        else
        {
            ColorUtility.TryParseHtmlString(noGuardColor, out curColor);
            barBg.color = curColor;
            guardIndicator.SetActive(false);
        }
        guaTxt.text = parent.block.ToString();

        float targetWidth = ((float)parent.currentHP / parent.maxHP) * 2f;
        float lerpSpeed = 4f;
        float currentWidth = Mathf.Lerp(barVal.sizeDelta.x, targetWidth, Time.deltaTime * lerpSpeed);
        barVal.sizeDelta = new Vector2(currentWidth, barVal.sizeDelta.y);

        //체력 표시(텍스트)
        hpIndicator = parent.currentHP.ToString() + "/" + parent.maxHP.ToString();
        barTxt.text = hpIndicator;
        // 부모의 스케일이 어떻게 변하든 UI는 항상 일정한 크기를 유지합니다.
        transform.localScale = new Vector3(1f / transform.parent.localScale.x,
                                           1f / transform.parent.localScale.y,
                                           1f);

        // 만약 단순히 뒤집히는 것만 막고 싶다면
        // transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), ...);
    }
}
