using Photon.Pun;
using UnityEngine.EventSystems;

public class VolumeSlider : MonoBehaviourPunCallbacks, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        // 슬라이더를 때는 순간, 슬라이더 벨류 값으로 오디오소스 볼륨값 변경 
        VideoManager.instance.audioSource.volume = VideoManager.instance.SoundSliderBar.value;
        
        // 교육생 RPC 볼륨값 변경
        FM_System.instance.photonView.RPC("VolumeChange", RpcTarget.Others, VideoManager.instance.audioSource.volume); // FM_System.instance에 있는 photonView컴포넌트를 상속하여 사용.
    }
}