using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

public class ToolTipController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string iconName;

    // 마우스가 ToolTipController가 들어간 UI에 닿으면, 발동
    public void OnPointerEnter(PointerEventData eventData)
    {
        ToolTipManager.instance.SetupToolTip(iconName);                                // 툴팁 텍스트 설정 
        ToolTipManager.instance.tooltipObject.transform.position = transform.position; // 툴티 위치 변경
        ToolTipManager.instance.tooltipObject.SetActive(true);                         // 켜기
    }

    // 마우스가 ToolTipController가 들어간 UI에서 나가면, 발동
    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.instance.tooltipObject.SetActive(false);  // 끄기
    }
}