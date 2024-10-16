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
    private IEnumerator ResetRoomState(int notInteractableNum)
    {
        // 버튼 상태 제어(비활성화)
        foreach (var menuButtonLists in menuButtonList)
            menuButtonLists.interactable = false;
    
        // 방에 들어가 있으고
        if (PhotonNetwork.InRoom)
        {
            // 모니터링 버튼 화면 -> 다른 화면 누르면
            // 미러링 등록된, 플레이어 모두 제거
            if (PhotonNetwork.CurrentRoom.Name == "VR Game")
            {
                Player[] inRoomPlayerList = PhotonNetwork.PlayerList;
                foreach (var inRoomPlayerLists in inRoomPlayerList)
                    FM_System.instance.DecoderDelete(inRoomPlayerLists);
            }
            // 화면 공유 화면 -> 다른 화면
            else
            {
                    
            }
            PhotonNetwork.LeaveRoom();  // 방 떠나기
        }
        Debug.Log("ResetRoomState 실행 1");
        
        float timeout = 10f;
        float timeWaited = 0f;

        while (!PhotonNetwork.InLobby && timeWaited < timeout)
        {
            timeWaited += Time.deltaTime;
            yield return null;  // Wait for the next frame
        }
        
        if (PhotonNetwork.InLobby)
        {
            Debug.Log("ResetRoomState 실행 2");
            
            // 버튼 제어(미러링은 OnJoinRoom 오버라이드 함수에서 제어...)
            // 미러링은 바로, 룸으로 들어가기 때문에...
            if (notInteractableNum == 1 || notInteractableNum == 2)
            {
                foreach (var menuButtonLists in menuButtonList)
                    menuButtonLists.interactable = true;
                menuButtonList[notInteractableNum].interactable = false;
                
                PunSystem.instance.loadingScreen.SetActive(false);
            }

            // 화면 공유 전용 제어
            if (notInteractableNum == 1) 
            {
                VideoManager.instance.shareSetting.gameObject.SetActive(true);
                PunSystem.instance.feedbackText.gameObject.SetActive(true);
                PunSystem.instance.feedbackText.text = "공유할 동영상을 선택해 주세요.";
            }
        }
        else
        {
            Debug.LogError("Failed to return to the lobby after leaving the room.");
        }
    }
    
    public void OnMirroring()
    {
        Debug.Log("Mirroring");
        StartCoroutine(Mirroring());
    }
    
    private IEnumerator Mirroring()
    {
        PunSystem.instance.loadingScreen.SetActive(true);
        PunSystem.instance.feedbackText.gameObject.SetActive(true);
        PunSystem.instance.feedbackText.text = "정보 불러오는 중...";
    
        PunSystem.instance.CloseMenus();
        PunSystem.instance.mirroringScreen.SetActive(true);
        
        // 버튼 상태 제어(비활성화)
        foreach (var menuButtonLists in menuButtonList)
            menuButtonLists.interactable = false;
        VideoManager.instance.shareSetting.gameObject.SetActive(false);    // 쉐어세팅버튼 비활성화
        
        yield return StartCoroutine(ResetRoomState(0));
        
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
        Debug.Log("VideoShare");
        StartCoroutine(VideoShare());
    }

    private IEnumerator VideoShare()
    {
        PunSystem.instance.loadingScreen.SetActive(true);
        PunSystem.instance.feedbackText.gameObject.SetActive(true);
        PunSystem.instance.feedbackText.text = "정보 불러오는 중...";
    
        PunSystem.instance.CloseMenus();
        PunSystem.instance.videoShareScreen.SetActive(true);
        
        VideoManager.instance.shareSetting.gameObject.SetActive(false);    // 쉐어세팅버튼 비활성화
        
        yield return StartCoroutine(ResetRoomState(1));
    }

    public void OnAuthority()
    {
        Debug.Log("Authority");
        StartCoroutine(Authority());
    }
    
    private IEnumerator Authority()
    {
        PunSystem.instance.loadingScreen.SetActive(true);
        
        PunSystem.instance.CloseMenus();
        PunSystem.instance.authorityScreen.SetActive(true);
        
        VideoManager.instance.shareSetting.gameObject.SetActive(false);    // 쉐어세팅버튼 비활성화

        AccountManager.inctance.RemoveAllUsersPrefabs();    // 기존 정보 모두 삭제하기
        AccountManager.inctance.ReadAllUsersFromDatabase(); // 정보 뽑아오기
        
        yield return StartCoroutine(ResetRoomState(2));
    }

    
    // // 에러 스크린 버튼 닫기
    // public void CloseErrorScreen()
    // {
    //     PunSystem.instance.CloseMenus();
    // }

    public void QuitGame()
    {
        Debug.LogError("종료");
        Application.Quit();
    }
}
