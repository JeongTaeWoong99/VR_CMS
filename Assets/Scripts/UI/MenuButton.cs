using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviourPunCallbacks
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
            if (PhotonNetwork.CurrentRoom.Name == "Space")
            {
                Player[] inRoomPlayerList = PhotonNetwork.PlayerList;
                foreach (var inRoomPlayerLists in inRoomPlayerList)
                    FM_System.instance.DecoderDelete(inRoomPlayerLists);
            }
            // 화면 공유 화면(방 만들어져 있는데) -> 다른 화면
            // RPC를 통해, 접속한 교육생들이 나 방에서 나가게 함.
            else
            {
                FM_System.instance.photonView.RPC("OnReturnToMainMenu",RpcTarget.Others); // FM_System.instance에 있는 photonView컴포넌트를 상속하여 사용.
                VideoManager.instance.StopSetting();                                                // 스탑 세팅도 실행(동영상, 코루틴 등등 멈춰야 할 것)
            }
            PhotonNetwork.LeaveRoom();  // 방 떠나기
        }
        
        float timeout    = 10f;
        float timeWaited = 0f;

        while (!PhotonNetwork.InLobby && timeWaited < timeout)
        {
            timeWaited += Time.deltaTime;
            yield return null;
        }
        
        if (PhotonNetwork.InLobby)
        {
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
        
        yield return StartCoroutine(ResetRoomState(0));
        
        // 방 입장 중 아닐 때, 실행
        if (!PhotonNetwork.InRoom)
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers  = 20;
            PhotonNetwork.CreateRoom("Space", options, TypedLobby.Default);   // 방이 있으면, 내장함수를 통해 Join으로 들어감.
        }
    }

    public void OnVideoShare()
    {
        StartCoroutine(VideoShare());
    }

    private IEnumerator VideoShare()
    {
        PunSystem.instance.loadingScreen.SetActive(true);
        PunSystem.instance.feedbackText.gameObject.SetActive(true);
        PunSystem.instance.feedbackText.text = "정보 불러오는 중...";
    
        PunSystem.instance.CloseMenus();
        PunSystem.instance.videoShareScreen.SetActive(true);
        
        // ----------------------------
        // 다른 페이지에서 넘어올 때, 초기화
        VideoManager.instance.videoPlayer.targetTexture.Release();               // 재생 후 남아있는 윤곽 제거
        VideoManager.instance.shareSettingButtonList[0].interactable = true;     // 버튼 활성화
        VideoManager.instance.videoControllerScreen.gameObject.SetActive(false); // 동영상 제어 비활성화
        VideoManager.instance.VideoPlayButtonImage.sprite = VideoManager.instance.playSprite;
        VideoManager.instance.videoPlayer.url             = "";
        
        // 활성화 할 때, 접속 플레이어 텍스트 프리팹 다 비우기...
        if (PunSystem.instance)
        {
            var children = new List<GameObject>();
            if (PunSystem.instance.connectedPlayerGroup.transform.childCount != 0)
            {
                foreach (Transform child in PunSystem.instance.connectedPlayerGroup.transform)
                    children.Add(child.gameObject);
                foreach (GameObject child in children)
                    Destroy(child);
            }
        }
        // --------------------------
        
        yield return StartCoroutine(ResetRoomState(1));
    }

    public void OnAuthority()
    {
        StartCoroutine(Authority());
    }
    
    private IEnumerator Authority()
    {
        PunSystem.instance.loadingScreen.SetActive(true);
        
        PunSystem.instance.CloseMenus();
        PunSystem.instance.authorityScreen.SetActive(true);

        AccountManager.instance.RemoveAllUsersPrefabs();    // 기존 정보 모두 삭제하기
        AccountManager.instance.ReadAllUsersFromDatabase(); // 정보 뽑아오기
        
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
