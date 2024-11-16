using UnityEngine;
using UnityEngine.Serialization;

public class MirroringPanelButton : MonoBehaviour
{
    public RectTransform mirroringContent;

    public void UpViewportY()
    {
        Vector2 anchoredPosition = mirroringContent.anchoredPosition;
        anchoredPosition.y -= 350;
        mirroringContent.anchoredPosition = anchoredPosition;
    }
    
    public void DownViewport()
    {
        Vector2 anchoredPosition = mirroringContent.anchoredPosition;
        anchoredPosition.y += 350;
        mirroringContent.anchoredPosition = anchoredPosition;
    }
}
