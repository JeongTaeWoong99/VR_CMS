using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    public  TMP_Text buttonText; // 룸 인포를 통해, 설정된 방이름을 버튼의 텍스트의 넣어 줌.
    private RoomInfo info;       // Photon.Realtime의 내장타입(방 정모들을 담을 수 있음)

    // OnRoomListUpdate에서 방 정보가 업데이트 되고, 텍스트 변경 때 호출
    public void SetButtonDetails(RoomInfo inputInfo)
    {
        info = inputInfo;            // 정보 저장
        buttonText.text = info.Name; // 정보에서 방 이름을 가져와, 버튼텍스트 변경
    }
   
    // 룸 버튼이 눌리는 순간, JoinRoom()함수에, info를 넣어서 실행
    public void OpenRoom()
    {
        Launcher.instance.JoinRoom(info);
    }
}
