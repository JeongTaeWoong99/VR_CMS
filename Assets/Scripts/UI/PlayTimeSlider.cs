using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayTimeSlider : MonoBehaviour, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        // 바뀐 playTimeSliderBar.value를 통해, 바뀐 재생 시간을 만듬.
        double newTime = VideoManager.instance.playTimeSliderBar.value * VideoManager.instance.videoPlayer.length;
        
        // 시간 변경
        VideoManager.instance.videoPlayer.time = newTime;
        
        // ---------
        // 여기다가 RPC로 보내주면 되겠누 ㅋㅋ
    }
}