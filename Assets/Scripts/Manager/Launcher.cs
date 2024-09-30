using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    public GameObject loadingScreen;
    public TMP_Text loadingText;

    public GameObject nameInputScreen;
    public TMP_InputField nameInput;
    public TMP_InputField passwordInput;
    public Toggle authorityToggle; // 권한 토글
    public static bool hasSetNick; // ☆ 정적 bool (게임을 끝내고 돌아와서도, true상태로 남아있음)

    public TextMeshProUGUI informationText;

    // public GameObject     createRoomScreen;
    // public TMP_InputField roomNameInput;

    // public GameObject     selectRoomScreen;
    // public TMP_Text       selectedRoomName;

    public GameObject roomScreen;
    public TMP_Text roomNameText, playerNameLabel;
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();

    public GameObject errorScreen;
    public TMP_Text errorText;

    public GameObject roomBrowserScreen;
    public RoomButton theRoomButton; // RoomButton 스크립트 타입의 변수
    private List<RoomButton> allRoomButtons = new List<RoomButton>();


    // public GameObject startButton;
    //
    // public GameObject roomTestButton;
    //
    // public string[] allMaps;
    // public bool     changeMapBetweenRounds = true;
    //
    // private List<RoomInfo> currentRoomListInfo = new List<RoomInfo>(); // 룸이 갱신될 때 마다, 정보가 계속 바뀜

    public GameObject mirroringScreen;

    private void Awake()
    {
        instance = this;

        Application.targetFrameRate = 30; // 게임 프레임 고정

        PhotonNetwork.SendRate = 20; // 초당 서버로 보내는 패킷 횟수 (기본값 20)
        PhotonNetwork.SerializationRate = 10; // 초당 동기화되는 데이터 횟수 (기본값 10)
    }

    void Start()
    {
        CloseMenus();

        loadingScreen.SetActive(true);
        loadingText.text = "Connecting To Network...";

        if (!PhotonNetwork.IsConnected) // 게임화면에서, 다시 메인메뉴로 돌아와서, 설정세팅을 하는 경우 방지
        {
            PhotonNetwork.ConnectUsingSettings(); // PhotonServerSettings 파일의 설정들로 네트워킹을 세팅한다.
        } // 네트워크가 정상적으로 접속되면, OnConnectedToMaster() 함수가 호출된다;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    // 서버접속 완료시 호출
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(); // 로비입장

        // 방을 처음 만든 사람이 마스터, 이후 마스터가 방을 나가면, 남아있는 렌덤한 사람에게 마스터 권한이 간다.
        // 마스터가 PhotonNetwork.LoadLevel()을 호출하면, 모든 플레이어가 동일한 레벨을 자동으로 로드(true면 로드 , false면 로드 x) -> StartGame버튼에서 로드레벨 사용
        PhotonNetwork.AutomaticallySyncScene = false;

        loadingText.text = "Joining Lobby...";
    }

    // 로비입장 완료시 호출
    public override void OnJoinedLobby()
    {
        CloseMenus();

        PhotonNetwork.NickName = "CMS MASTER"; // 닉네임 설정

        // 첫 게임접속 후 로비화면
        nameInputScreen.SetActive(true);

        // 게임 자체를 처음 접속
        if (!hasSetNick)
        {
            CloseMenus();
            nameInputScreen.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }

        Hashtable playerProperties = new Hashtable { { "CMS", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    void CloseMenus()
    {
        //createRoomScreen.SetActive(false);
        //selectRoomScreen.SetActive(false);
        loadingScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
        nameInputScreen.SetActive(false);
        mirroringScreen.SetActive(false);
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

    private void ListAllPlayers()
    {
        // 정보 비우기
        foreach (TMP_Text player in allPlayerNames)
        {
            Destroy(player.gameObject);
        }

        allPlayerNames.Clear();

        // 업데이트
        Player[] players = PhotonNetwork.PlayerList; // room안의 플레이어 정보를 받아온다.
        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);

            allPlayerNames.Add(newPlayerLabel);
        }
    }

    // 다른 플레이어가 방에서 입장시 호출(새로 들어온 플레이어만, 만들어 주면 됨)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);

        allPlayerNames.Add(newPlayerLabel);
    }

    // 다른 플레이어가 방에서 나갈시 호출(나간 플레이어를 지워주고, 새로 구성해야 함)
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    // // 방생성이 실패하면 호출(실패 코드와 메세지설명을 받을 수 있음)
    // public override void OnCreateRoomFailed(short returnCode, string message)
    // {
    //     errorText.text = "Failed To Create Room: " + message;
    //     CloseMenus();
    //     errorScreen.SetActive(true);
    // }

    // 버튼 함수
    
    public void CloseErrorScreen()
    {
        CloseMenus();
    }

    // 만든 방 삭제 및  삭제가 완료되면, Lobby로 다시 접속(Room -> Lobby)
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingText.text = "Leaving Room";
        loadingScreen.SetActive(true);
    }
    
    // 접속한 방을 떠나면, 호출됨.
    public override void OnLeftRoom()
    {
        CloseMenus();
    }

    // 버튼 함수
    public void OpenRoomBrowser()
    {
        CloseMenus();
        roomBrowserScreen.SetActive(true);
    }

    // 버튼 함수
    public void CloseRoomBrowser()
    {
        CloseMenus();
    }

    // 룸 리스트 초기화 - 현재 생성된 룸들의 정보가 담긴 리스트가 매개변수로 온다.
    // 로비 내에 룸이 생성되거나 사라질때 자동 호출되는 콜백
    public override void OnRoomListUpdate(List<RoomInfo> roomList) // 자동업데이트
    {
        foreach (RoomButton rb in allRoomButtons) // 기존 정보 모두 삭제
        {
            Destroy(rb.gameObject);
        }

        allRoomButtons.Clear();

        theRoomButton.gameObject.SetActive(false); // 예시 이미지 false

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList &&
                roomList[i].IsVisible)
            {
                RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);

                allRoomButtons.Add(newButton);
            }
        }
    }

    // 버튼 함수
    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);

        CloseMenus();
        loadingText.text = "Joining Room";
        loadingScreen.SetActive(true);
    }

    // 버튼 함수
    public void SetNickname()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            if (authorityToggle.isOn && passwordInput.text == "1234")
            {
                PhotonNetwork.NickName = nameInput.text;
                PlayerPrefs.SetString("playerName", nameInput.text);
                CloseMenus();

                informationText.text = "VR CMS | " + "관리자 | " + nameInput.text;

                hasSetNick = true;
            }
            else if (!authorityToggle.isOn)
            {
                PhotonNetwork.NickName = nameInput.text;
                PlayerPrefs.SetString("playerName", nameInput.text);
                CloseMenus();

                informationText.text = "VR CMS | " + "교육생 | " + nameInput.text;

                hasSetNick = true;
            }
        }
    }
    
    // 미러링 할 유저가 있으면(=방이 만들어져 있음.), 플레이어 리스트를 받아오고,
    // 디코더 등록
    public override void OnJoinedRoom()
    {
        Player[] inRoomPlayerList = PhotonNetwork.PlayerList;

        foreach (var inRoomPlayerLists in inRoomPlayerList)
        {
            // CMS 플레이어는 제외 하도록 한다.
            if(!(inRoomPlayerLists.CustomProperties.ContainsKey("CMS") && (bool)inRoomPlayerLists.CustomProperties["CMS"]))
                PunSystem.instance.DecoderRegistration(inRoomPlayerLists);
        }
    }
    
    // 미러링 할 유저가 없으면(=방이 말들어져 있지 않음.), 에러 메세지 출력 
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        CloseMenus();   
        errorScreen.SetActive(true);
        
        errorText.text = "Failed To Create Room Join : " + message;
    }

    // 로그인 권한 토글
    public void AuthorityToggle()
    {
        if (authorityToggle.isOn)
            passwordInput.gameObject.SetActive(true);
        else
            passwordInput.gameObject.SetActive(false);
    }

    // 룸의 마스터가 변경될 시 호출(새 마스터의 정보도 가져옴)
    // public override void OnMasterClientSwitched(Player newMasterClient)
    // {
    //     if (PhotonNetwork.IsMasterClient)
    //     {
    //         startButton.SetActive(true);
    //     }
    //     else
    //     {
    //         startButton.SetActive(false);
    //     }
    // }
    
    // 토글 함수

    public void OnMirroring()
    {
        CloseMenus();
        mirroringScreen.SetActive(true);
        
        PhotonNetwork.JoinRoom("VR Game"); // 방에 입장.
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
