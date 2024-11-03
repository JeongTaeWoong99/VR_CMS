using Photon.Pun;
using UnityEngine.EventSystems;

public class PlayTimeSlider : MonoBehaviourPunCallbacks, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        // 바뀐 playTimeSliderBar.value를 통해, 바뀐 재생 시간을 만듬.
        double newTime = VideoManager.instance.playTimeSliderBar.value * VideoManager.instance.videoPlayer.length;
        
        // 시간 변경
        VideoManager.instance.videoPlayer.time = newTime;
        
        // 교육생 RPC 시간 변경
        FM_System.instance.photonView.RPC("VideoTimeChange", RpcTarget.Others, newTime); // FM_System.instance에 있는 photonView컴포넌트를 상속하여 사용.
    }
}