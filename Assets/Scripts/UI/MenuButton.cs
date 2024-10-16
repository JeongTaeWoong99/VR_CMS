using System.Collections;
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

    // 미러링 초기화
    // 미러링 화면에서, 다른 화면으로 넘어가는 버튼을 눌렀을 때, 초기화 실시
    private IEnumerator ResetMirroring(int notInteractableNum)
    {
        // 버튼 상태 제어(비활성화)
        foreach (var menuButtonLists in menuButtonList)
            menuButtonLists.interactable = false;
    
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
        
        yield return new WaitUntil(() => PhotonNetwork.InLobby);    // 로비 입장까지 기다리기...
        
        // 버튼 상태 제어(활성화)
        foreach (var menuButtonLists in menuButtonList)
            menuButtonLists.interactable = true;
        menuButtonList[notInteractableNum].interactable = false;    // 누른 자신 제외
    }
    
    public void OnMirroring()
    {
        // 버튼 상태 제어(비활성화)
        foreach (var menuButtonLists in menuButtonList)
            menuButtonLists.interactable = false;
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
        PunSystem.instance.CloseMenus();
        PunSystem.instance.videoShareScreen.SetActive(true);
        StartCoroutine(ResetMirroring(1));
    }

    public void OnAuthority()
    {
        PunSystem.instance.CloseMenus();
        PunSystem.instance.authorityScreen.SetActive(true);
        StartCoroutine(ResetMirroring(2));

        AccountManager.inctance.RemoveAllUsersPrefabs();    // 기존 정보 모두 삭제하기
        AccountManager.inctance.ReadAllUsersFromDatabase(); // 정보 뽑아오기
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
