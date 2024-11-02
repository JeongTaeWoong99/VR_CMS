using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PunSystem : MonoBehaviourPunCallbacks
{
    public static PunSystem instance;
    
    [Header("로딩")]
    public GameObject loadingScreen;
    private Image     loadingBG;        // 백그라운드 이미지
    
    [Header("피드백")]
    public TextMeshProUGUI feedbackText;         // 버튼 클릭 시, 파이어베이스 피드백 텍스트
    
    [Header("로그인")]
    public GameObject     loginScreen;
    public TMP_InputField nicknameOrEmailInput;
    public TMP_InputField passwordInput;
    public static bool    hasFistOnLobby;       // ☆ 정적 bool (게임을 끝내고 돌아와서도, true상태로 남아있음) // 맨처음 로비에 입장인지 체크
    
    [Header("미러링")]
    public GameObject mirroringScreen;
    
    [Header("비디오공유")]
    public GameObject videoShareScreen;
    public GameObject connectedPlayerTextPrefabs;
    public GameObject connectedPlayerGroup;

    [Header("권한승인")]
    public GameObject authorityScreen;
    
    // public GameObject     createRoomScreen;
    // public TMP_InputField roomNameInput;

    // public GameObject     selectRoomScreen;
    // public TMP_Text       selectedRoomName;

    // public  GameObject     roomScreen;
    // public  TMP_Text       roomNameText, playerNameLabel;
    // private List<TMP_Text> allPlayerNames = new List<TMP_Text>();
    
    // public GameObject roomBrowserScreen;
    // public RoomButton theRoomButton; // RoomButton 스크립트 타입의 변수
    // private List<RoomButton> allRoomButtons = new List<RoomButton>();


    // public GameObject startButton;
    //
    // public GameObject roomTestButton;
    //
    // public string[] allMaps;
    // public bool     changeMapBetweenRounds = true;
    //
    // private List<RoomInfo> currentRoomListInfo = new List<RoomInfo>(); // 룸이 갱신될 때 마다, 정보가 계속 바뀜


    private void Awake()
    {
        instance = this;

        Application.targetFrameRate     = 60; // 게임 프레임 고정
        
        PhotonNetwork.SendRate          = 30; // 초당 서버로 보내는 패킷 횟수 (기본값 20)
        PhotonNetwork.SerializationRate = 20; // 초당 동기화되는 데이터 횟수 (기본값 10)
    }

    void Start()
    {
        CloseMenus();
        
        loadingScreen.SetActive(true);
        loadingBG = loadingScreen.GetComponent<Image>();

        if (!PhotonNetwork.IsConnected) // 게임화면에서, 다시 메인메뉴로 돌아와서, 설정세팅을 하는 경우 방지
        {
            PhotonNetwork.ConnectUsingSettings(); // PhotonServerSettings 파일의 설정들로 네트워킹을 세팅한다.
        }                                         // 네트워크가 정상적으로 접속되면, OnConnectedToMaster() 함수가 호출된다;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }


    // 서버접속 완료시 호출
    public override void OnConnectedToMaster()
    {
        loadingScreen.SetActive(false);
        loadingBG.color = new Color(0f, 0f, 0f, 0.1f); // 처음 이후는, 로딩 패널의 백그라운드를 연하게...
    
        PhotonNetwork.JoinLobby(); // 로비입장

        // 방을 처음 만든 사람이 마스터, 이후 마스터가 방을 나가면, 남아있는 렌덤한 사람에게 마스터 권한이 간다.
        // 마스터가 PhotonNetwork.LoadLevel()을 호출하면, 모든 플레이어가 동일한 레벨을 자동으로 로드(true면 로드 , false면 로드 x) -> StartGame버튼에서 로드레벨 사용
        PhotonNetwork.AutomaticallySyncScene = false;
    }

    // 로비입장 완료시 호출(서버에 접속 시 / 룸에서 복귀 시)
    public override void OnJoinedLobby()
    {
        // 게임 자체를 처음 접속
        if (!hasFistOnLobby)
        {
            CloseMenus();
            loginScreen.SetActive(true);
        }
        
        // CMS 접속 플레이어 구분 헤쉬테이블 추가
        Hashtable playerProperties = new Hashtable { { "CMS", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    public void CloseMenus()
    {
        //createRoomScreen.SetActive(false);
        //selectRoomScreen.SetActive(false);
        //roomScreen.SetActive(false);
        //roomBrowserScreen.SetActive(false);
        loginScreen.SetActive(false);
        mirroringScreen.SetActive(false);
        videoShareScreen.SetActive(false);
        authorityScreen.SetActive(false);
    }

    // 버튼 함수
    // public void OpenRoomCreate()
    // {
    //     CloseMenus();
    //     createRoomScreen.SetActive(true);
    // }

    // public void CreateRoom()
    // {
    //     // 방제가 비어있는지 확인
    //     if(!string.IsNullOrEmpty(roomNameInput.text))
    //     {
    //         RoomOptions options = new RoomOptions();
    //         options.MaxPlayers = 8;
    //
    //         PhotonNetwork.CreateRoom(roomNameInput.text, options); // 방생성 및 설정된 옵션 전달
    //
    //         CloseMenus();
    //         loadingText.text = "Creating Room...";
    //         loadingScreen.SetActive(true);
    //     }
    // }

    // 버튼 함수
    // public void OpenSelectRoom()
    // {
    //     CloseMenus();
    //     selectRoomScreen.SetActive(true);
    // }

    // public void SelectRoom()
    // {
    //     bool foundMatchingRoom = false; // 룸 존재 여부 초기화
    //
    //     // 룸이 존재하는지 체크
    //     foreach (RoomInfo room in currentRoomListInfo)
    //     {
    //         // 룸 존재 O -> 방에 들어가기
    //         if (room.CustomProperties.ContainsKey("ControlType") && (string)room.CustomProperties["ControlType"] == selectedRoomName.text)
    //         {
    //             foundMatchingRoom = true; 
    //             PhotonNetwork.JoinRoom(selectedRoomName.text); // 방 바로 입장
    //             break;
    //         }
    //     }
    //     
    //     // 룸 존재 X -> 방 직접 만들고 입장 후, 게임 시작
    //     if (!foundMatchingRoom)
    //     {
    //         RoomOptions options = new RoomOptions();
    //         options.MaxPlayers = 8;
    //         
    //         PhotonNetwork.CreateRoom(selectedRoomName.text, options); // 방생성 및 설정된 옵션 전달
    //     }
    //     
    //     // 로딩창
    //     CloseMenus();
    //     loadingText.text = "Creating Room...";
    //     loadingScreen.SetActive(true);
    // }

    // 방 입장 후 플레이어 리스트 출력 + 플레이어가 방에서 나갈시

    // private void ListAllPlayers()
    // {
    //     // 정보 비우기
    //     foreach (TMP_Text player in allPlayerNames)
    //     {
    //         Destroy(player.gameObject);
    //     }
    //
    //     allPlayerNames.Clear();
    //
    //     // 업데이트
    //     Player[] players = PhotonNetwork.PlayerList; // room안의 플레이어 정보를 받아온다.
    //     for (int i = 0; i < players.Length; i++)
    //     {
    //         TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
    //         newPlayerLabel.text = players[i].NickName;
    //         newPlayerLabel.gameObject.SetActive(true);
    //
    //         allPlayerNames.Add(newPlayerLabel);
    //     }
    // }

    // 방생성이 실패하면 호출(실패 코드와 메세지설명을 받을 수 있음)
    // public override void OnCreateRoomFailed(short returnCode, string message)
    // {
    //     errorText.text = "Failed To Create Room: " + message;
    //     CloseMenus();
    //     errorScreen.SetActive(true);
    // }
    
    
    
    // 만든 방 삭제 및 삭제가 완료되면, Lobby로 다시 접속(Room -> Lobby)
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
    }
    
    // 버튼 함수
    // public void OpenRoomBrowser()
    // {
    //     CloseMenus();
    //     roomBrowserScreen.SetActive(true);
    // }

    // 버튼 함수
    // public void CloseRoomBrowser()
    // {
    //     CloseMenus();
    // }

    // 룸 리스트 초기화 - 현재 생성된 룸들의 정보가 담긴 리스트가 매개변수로 온다.
    // 로비 내에 룸이 생성되거나 사라질때 자동 호출되는 콜백
    // public override void OnRoomListUpdate(List<RoomInfo> roomList) // 자동업데이트
    // {
    //     foreach (RoomButton rb in allRoomButtons) // 기존 정보 모두 삭제
    //     {
    //         Destroy(rb.gameObject);
    //     }
    //
    //     allRoomButtons.Clear();
    //
    //     theRoomButton.gameObject.SetActive(false); // 예시 이미지 false
    //
    //     for (int i = 0; i < roomList.Count; i++)
    //     {
    //         if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList &&
    //             roomList[i].IsVisible)
    //         {
    //             RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
    //             newButton.SetButtonDetails(roomList[i]);
    //             newButton.gameObject.SetActive(true);
    //
    //             allRoomButtons.Add(newButton);
    //         }
    //     }
    // }

    // 버튼 함수
    // public void JoinRoom(RoomInfo inputInfo)
    // {
    //     PhotonNetwork.JoinRoom(inputInfo.Name);
    //
    //     CloseMenus();
    //     loadingText.text = "Joining Room";
    //     loadingScreen.SetActive(true);
    // }
    
    
    // 방 입장 성공 시 호출
    // 미러링 할 유저가 있으면(=방이 만들어져 있음.), 플레이어 리스트를 받아오고,
    // 디코더 등록
    public override void OnJoinedRoom()
    {
        // 미러링 룸에 들어온 경우...
        if (PhotonNetwork.CurrentRoom.Name == "VR Game")
        {
            // 버튼 상태 제어
            foreach (var menuButtonLists in MenuButton.instance.menuButtonList)
                menuButtonLists.interactable = true;
            MenuButton.instance.menuButtonList[0].interactable = false;
                                        
            // 모니터링 할 플레이어 만들기
            Player[] inRoomPlayerList = PhotonNetwork.PlayerList;
            foreach (var inRoomPlayerLists in inRoomPlayerList)
            {
                // CMS 플레이어는 제외 하도록 한다.
                if(!(inRoomPlayerLists.CustomProperties.ContainsKey("CMS") && (bool)inRoomPlayerLists.CustomProperties["CMS"]))
                    FM_System.instance.DecoderRegistration(inRoomPlayerLists);
            }
            
            if (FM_System.instance.gameViewDecoderList.Count <= 0)
            {
                feedbackText.gameObject.SetActive(true);
                feedbackText.text = "접속 중인 교육생이 없습니다.";
            }
            else
                feedbackText.gameObject.SetActive(false);
        }
        // 화면공유 룸에 들어온 경우...
        else
        {
            VideoManager.instance.shareSettingButtonList[0].interactable = false; // 영상공유 버튼 끄기
            
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "VR 영상 공유방을 만들었습니다.";
        }
        
        loadingScreen.SetActive(false); // 모든게 끝나고, 로딩창 없애기....
    }
    
    // 접속한 방을 다른 클라이언트가 떠나면, 호출됨.
    // 모니터링 화면에서, 비디오 쉐어 버튼을 눌렀을 때,(비디오 쉐어 버튼만 꺼져있어야 함.)
    // public override void OnLeftRoom()
    // {
    //     // // 비디오 쉐어 버튼 빼고, 상호작용 켜기.
    //     // for (int i = 0; i < MenuButton.instance.menuButtonList.Count - 1 ; i++)
    //     //     MenuButton.instance.menuButtonList[i].GetComponent<Button>().interactable = true;
    // }
    
    
    // 방 입장 실패 시 호출
    // 미러링 할 유저가 없으면(=방이 말들어져 있지 않음), 방 만들어서 들어가기.
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // 이미 교육용 게임 방이 존재하는 경우(미러링)
        if (returnCode == 32766)
        {
            PhotonNetwork.JoinRoom("VR Game"); // 방에 입장.
        }
        // 그 외, 오류 표시
        else
        {
            Debug.Log(returnCode);
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "오류코드 : " + returnCode;
            CloseMenus();
        }
    }
    
    // 다른 플레이어가 방에서 입장시 호출
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // 미러링 룸에 들어온 경우...
        if (PhotonNetwork.CurrentRoom.Name == "VR Game")
        {
            feedbackText.gameObject.SetActive(false);
            FM_System.instance.DecoderRegistration(newPlayer);
        }
        else
        {
            // 공유방에 누가 들어왔는지 표시...
            GameObject connectedPlayerClone = Instantiate(connectedPlayerTextPrefabs, connectedPlayerGroup.transform, true);    
            connectedPlayerClone.GetComponent<TextMeshProUGUI>().text = newPlayer.NickName;
        }
    }
    
    // 다른 플레이어가 방에서 나갈시 호출
    public override void OnPlayerLeftRoom(Player leftPlayer)
    {
        // 미러링 룸에 나간 경우...
        if (PhotonNetwork.CurrentRoom.Name == "VR Game")
        {
            FM_System.instance.DecoderDelete(leftPlayer);
            if (FM_System.instance.gameViewDecoderList.Count <= 0)
            {
                feedbackText.gameObject.SetActive(true);
                feedbackText.text = "접속 중인 교육생이 없습니다.";
            }
        }
        // 공유방에서 나간 경우...
        else
        {
            // 닉네임 체크 및 텍스트 프리팹 삭제해주기
            foreach (Transform child in connectedPlayerGroup.transform)
            {
                TextMeshProUGUI textMesh   = child.GetComponent<TextMeshProUGUI>(); // 텍스트 접근
                GameObject childGameObject = child.gameObject;                      // 현재 오브젝트
                if (textMesh != null)
                {
                    if (textMesh.text == leftPlayer.NickName)
                        Destroy(childGameObject);
                }
            }
        }

    }
}
