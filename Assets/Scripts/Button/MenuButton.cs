using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    public static MenuButton instance;
    
    public List<Button> menuButtonList = new List<Button>();

    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        // 버튼 초기화
        foreach (var menuButtonLists in menuButtonList) // 모든 버튼 끄기
            menuButtonLists.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        // 미러링 버튼 활성화(0번 버튼)
        // 비디오 버튼 비활성화 && 룸 X && 로비 O
        if (!menuButtonList[1].interactable && !PhotonNetwork.InRoom && PhotonNetwork.InLobby && !menuButtonList[0].interactable)
        {
            Debug.Log(menuButtonList[0].name + " 활성화 ");
            menuButtonList[0].interactable = true;
        }
        
        // 비디오 버튼 활성화(1번 버튼)
        // 미러링 버튼 비활성화 && 룸 O 
        if (!menuButtonList[0].interactable && PhotonNetwork.InRoom && !menuButtonList[1].interactable)
        {
            Debug.Log(menuButtonList[1].name + " 활성화 ");
            menuButtonList[1].interactable = true;
        }
    }

    // 미러링 초기화
    // 미러링 화면에서, 다른 화면으로 넘어가는 버튼을 눌렀을 때, 초기화 실시
    private void ResetMirroring()
    {
        // 방에 들어가 있으면 (= 모니터링 버튼 화면에서 비디오 쉐어 버튼 누르면)
        if (PhotonNetwork.InRoom)
        {
            // 미러링 등록된, 플레이어 모두 제거
            Player[] inRoomPlayerList = PhotonNetwork.PlayerList;
            foreach (var inRoomPlayerLists in inRoomPlayerList)      
            {
                FM_System.instance.DecoderDelete(inRoomPlayerLists);
            }
            PhotonNetwork.LeaveRoom();  // 방 떠나기
        }
    }
    
    public void OnMirroring()
    {
        menuButtonList[0].interactable = false;
        PunSystem.instance.CloseMenus();
        PunSystem.instance.mirroringScreen.SetActive(true);
        
        // 방 입장 중 아닐 때, 실행
        if (!PhotonNetwork.InRoom)
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers  = 20;
            PhotonNetwork.CreateRoom("VR Game", options, TypedLobby.Default);   // 방이 있으면, 내장함수를 통해 Join으로 들어감.
        }
    }
    
    public void OnVideoShare()
    {
        menuButtonList[1].interactable = false;
        ResetMirroring();
        PunSystem.instance.CloseMenus();
        PunSystem.instance.videoShareScreen.SetActive(true);
    }
    
    // 에러 스크린 버튼 닫기
    public void CloseErrorScreen()
    {
        PunSystem.instance.CloseMenus();
    }

    public void QuitGame()
    {
        Debug.LogError("종료");
        Application.Quit();
    }
}
