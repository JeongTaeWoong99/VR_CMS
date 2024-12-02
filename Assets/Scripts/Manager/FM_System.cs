using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class FM_System : MonoBehaviourPunCallbacks
{
    public static FM_System instance;

    public List<GameViewDecoder> gameViewDecoderList = new List<GameViewDecoder>();
    
    public FMNetworkManager      fmNetworkManager;
    [HideInInspector]
    public UnityEventByteArray   fm_OnReceivedByteDataEvent;
    
    public GameObject            decoderPrefabs; // 미러링 스크린 + 디코더를 가지고 있는 프리팹
    public GameObject            decoderGroup;   // 만들어진 디코더 위치 그룹

    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        fm_OnReceivedByteDataEvent = fmNetworkManager.OnReceivedByteDataEvent;  // 위치 받아오기
    }
    
    // ★ Photon View가 들어가 있어야, RPC 메서드 받기 가능!! ★
    // ROOM에 입장해서 RPC 사용 가능!
    [PunRPC]
    public void RPC_SendMessage(byte[] _bytesData, string message, int battery)
    {
        if (message.Contains("VideoShare"))
        {
            foreach (var gameViewDecoderLists in gameViewDecoderList)
            {
                if(gameViewDecoderLists.TestImg.GetComponent<ImageVisibilityInScrollRect>().isVisibleNow)
                    gameViewDecoderLists.TestImg.color = new Color(1, 1, 1, 1); // 투명도 = 1 (보이게 하기)
                gameViewDecoderLists.Action_ProcessImageData(_bytesData);   // 바이트 이미지 변경 

                gameViewDecoderLists.batteryText.text = "남은 배터리 : " + battery; // 배터리 텍스트
            }
        }
    }
    
    public void DecoderRegistration(Player player) // 디코더 등록(미러링 버튼 클릭 or 미러링 화면에서 플레이어 접속 시)
    {
        GameObject      clonePrefabs = Instantiate(decoderPrefabs, decoderGroup.transform, true); // 새 디코더 스크립트 생성
        GameViewDecoder cloneDecoder = clonePrefabs.GetComponentInChildren<GameViewDecoder>();                  // 게임뷰 디코더 찾기
        // NickName Text 찾아서 넣어주기.
        Transform       nickNameTextTransform = clonePrefabs.transform.Find("NickName Text");
        TextMeshProUGUI cloneText             = nickNameTextTransform != null ? nickNameTextTransform.GetComponent<TextMeshProUGUI>() : null;
        if (cloneText == null)
        {
            Debug.Log("NickName Text를 찾지 못함...");
            return;
        }
        
        // 배터리 텍스트 찾아서 넣어주기.
        Transform       batteryTextTransform = clonePrefabs.transform.Find("Battery Text");
        TextMeshProUGUI cloneBatteryText     = batteryTextTransform != null ? batteryTextTransform.GetComponent<TextMeshProUGUI>() : null;
        if (cloneBatteryText == null)
        {
            Debug.Log("Battery Text를 찾지 못함.");
            return;
        }
        cloneDecoder.batteryText = cloneBatteryText; // GameViewDecoder에 배터리 텍스트 넣어주기.
        
        fm_OnReceivedByteDataEvent.AddListener(cloneDecoder.Action_ProcessImageData); // 디코더를 이벤트 등록.
        cloneDecoder.label = player.ActorNumber;                                      // 디코더의 라벨 번호와 엑터 넘버를 일치.LogWarning("Invalid ActorNumber for player: " + player.ActorNumber);
        
        string sceneName = player.CustomProperties.ContainsKey("SceneName") ? player.CustomProperties["SceneName"] as string : "알 수 없는 씬";  // 씬이름 찾기
        cloneText.text = $"{sceneName}/{player.NickName}";                                                                                     // 씬이름 + 닉네임

        gameViewDecoderList.Add(cloneDecoder);
    }
    
    public void DecoderDelete(Player player) // 디코더 삭제(다른 버튼 클릭 or 미러링 화면에서 플레이어가 방을 나갔을 시)
    {
        // decoderGroup아래 프리팹 확인하기
        foreach (Transform child in decoderGroup.transform)
        {
            GameViewDecoder decoder = child.GetComponentInChildren<GameViewDecoder>();

            // 나간 플레이어의 라벨 번호와 디코더의 라벨 번호가 같다면
            if (decoder != null && decoder.label == player.ActorNumber)
            {
                // 리스너 제거
                fm_OnReceivedByteDataEvent.RemoveListener(decoder.Action_ProcessImageData);

                // 리스트 제거
                gameViewDecoderList.Remove(decoder);
            
                // 프리팹 제거(=Mirroring Screen(+Decoder))
                // decoderGroup
                //      └── Mirroring Screen(+Decoder)
                //                  └── GameViewDecoder
                GameObject mirroringScreenObject = decoder.transform.parent.gameObject;
                Destroy(mirroringScreenObject);
                break;
            }
        }
    }
    
    [PunRPC]
    public void VideoSettingRequest(Player payer) // 클라의 세팅 리퀘스트를 받아서, 세팅 정보 보내주기 (CMS ->  클라)
    {
        // 비디어 동기화 공유(플레이 상태 / 시간 / 볼륨)
        double currentTime = VideoManager.instance.playTimeSliderBar.value * VideoManager.instance.videoPlayer.length;
        photonView.RPC("VideoSettingAccept", payer, VideoManager.instance.videoPlayer.isPlaying, currentTime, VideoManager.instance.audioSource.volume);
    }
    
    [PunRPC]
    public void PlayerSettingStateRenewal(string nickName,string settingState,int batteryState)
    {
        // 닉네임 체크하여, 리스트에서 플레이어의 상태를 갱신해 줌!
        foreach (Transform child in PunSystem.instance.connectedPlayerGroup.transform)
        {
            TextMeshProUGUI textMesh = child.GetComponent<TextMeshProUGUI>(); // 텍스트 접근
            
            // text의 순서 -> 닉네임/상태/배터리
            string[] splitText     = textMesh.text.Split('/');
            string   frontNickName = splitText[0];
                
            if (textMesh != null)
            {
                if (frontNickName == nickName)
                    textMesh.text = nickName + "/" + settingState + "/" + batteryState + "%";
            }
        }
    }
}
