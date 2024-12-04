using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PunSystem : MonoBehaviourPunCallbacks
{
    public static PunSystem instance;

    [Header("로딩")]
    public GameObject loadingScreen;
    private Image     loadingBG;                // 백그라운드 이미지
    
    [Header("피드백")]
    public TextMeshProUGUI feedbackText;        // 버튼 클릭 시, 파이어베이스 피드백 텍스트
    
    [Header("로그인")]
    public GameObject     loginScreen;
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public static bool    hasFistOnLobby;       // ☆ 정적 bool (게임을 끝내고 돌아와서도, true상태로 남아있음) // 맨처음 로비에 입장인지 체크
    
    [Header("계정생성")]
    public GameObject     accountScreen;
    public TMP_InputField accountEmailInput;
    public TMP_InputField accountPasswordInput;
    
    [Header("미러링")]
    public GameObject      mirroringScreen;
    public TextMeshProUGUI onLineText;      // 접속중인 교육생 수 체크

    [Header("비디오공유")]
    public GameObject videoShareScreen;
    public GameObject connectedPlayerTextPrefabs;
    public GameObject connectedPlayerGroup;

    [Header("권한승인")]
    public GameObject authorityScreen;

    private void Awake()
    {
        instance = this;
        
        Application.targetFrameRate = 60; // 게임 프레임 고정
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
        
        // CMS 접속 플레이어 구분 헤쉬테이블 추가(CMS 이름으로만 구분하면 되기 때문에, Lobby에 넣어도 됨. 클라는 안됨. 닉네임이 설정되고, 해쉬테이블을 넣어줘야 함.)
        Hashtable playerProperties = new Hashtable { { "CMS", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    public void CloseMenus()
    {
        loginScreen.SetActive(false);
        mirroringScreen.SetActive(false);
        videoShareScreen.SetActive(false);
        authorityScreen.SetActive(false);
    }
    
    
    // 만든 방 삭제 및 삭제가 완료되면, Lobby로 다시 접속(Room -> Lobby)
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
    }

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

            onLineText.text = OnLineCheck();
        }
        // 화면공유 룸에 들어온 경우...
        else
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "VR 영상 공유방을 만들었습니다.";
        }
        
        loadingScreen.SetActive(false); // 모든게 끝나고, 로딩창 없애기....
    }

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
            
            onLineText.text = OnLineCheck();
        }
        else
        {
            // 공유방에 누가 들어왔는지 표시...
            GameObject connectedPlayerClone = Instantiate(connectedPlayerTextPrefabs, connectedPlayerGroup.transform, true);    
            connectedPlayerClone.GetComponent<TextMeshProUGUI>().text = newPlayer.NickName + "/" + "X" + "/" + "0%";    // 닉네임 / 세팅 상태 / 배터리 상태
            
            // 비디오 존재 체크(입장한 친구만 체크)
            FM_System.instance.photonView.RPC("VideoExistCheck", newPlayer, VideoManager.instance.currentSettingVideoName);
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
            
            onLineText.text = OnLineCheck();
        }
        // 공유방에서 나간 경우...
        else
        {
            // 닉네임 체크 및 텍스트 프리팹 삭제해주기
            foreach (Transform child in connectedPlayerGroup.transform)
            {
                TextMeshProUGUI textMesh   = child.GetComponent<TextMeshProUGUI>(); // 텍스트 접근
                GameObject childGameObject = child.gameObject;                      // 현재 오브젝트
            
                // text의 순서 -> 닉네임/상태/배터리
                string[] splitText     = textMesh.text.Split('/');
                string   frontNickName = splitText[0];
                
                if (textMesh != null)
                {
                    if (frontNickName == leftPlayer.NickName)
                        Destroy(childGameObject);
                }
            }
        }
    }

    public void AccountPanelOnOff()
    {
        accountScreen.gameObject.SetActive(!accountScreen.gameObject.activeInHierarchy);
    }

    private string OnLineCheck()   // 미러링에서 교육중인 플레이어 체크
    {
        int count = PhotonNetwork.PlayerList.Length;    // 전체 플레이어 - cms 플레이어 제외
    
        // 교육중인 플레이어 체크에서, cms를 카운트하여, 빼주도록 한다.
        Player[] inRoomPlayerList = PhotonNetwork.PlayerList;
        foreach (var inRoomPlayerLists in inRoomPlayerList)
        {
            if (inRoomPlayerLists.CustomProperties.ContainsKey("CMS") && (bool)inRoomPlayerLists.CustomProperties["CMS"])
                count--;
        }

        string checkString = "접속한 교육생 수 : " + count;
        return checkString;
    }
}
