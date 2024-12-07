using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayTimeSlider : MonoBehaviourPunCallbacks, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        if (VideoManager.instance.videoPlayer.length > 0)
        {
            VideoManager.instance.videoPlayer.time = Mathf.FloorToInt(VideoManager.instance.playTimeSliderBar.value);
            
            FM_System.instance.photonView.RPC("VideoTimeChange", RpcTarget.Others, Mathf.FloorToInt(VideoManager.instance.playTimeSliderBar.value));
        }
    }
}