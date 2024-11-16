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
    public void RPC_SendMessage(byte[] _bytesData, string message)
    {
        if (message.Contains("VideoShare"))
        {
            foreach (var gameViewDecoderListS in gameViewDecoderList)
            {
                if(!gameViewDecoderListS.TestImg.IsActive())                 
                    gameViewDecoderListS.TestImg.gameObject.SetActive(true);
                gameViewDecoderListS.Action_ProcessImageData(_bytesData);
            }
        }
    }
    
    public void DecoderRegistration(Player player) // 디코더 등록(미러링 버튼 클릭 or 미러링 화면에서 플레이어 접속 시)
    {
        GameObject      clonePrefabs = Instantiate(decoderPrefabs, decoderGroup.transform, true); // 새 디코더 스크립트 생성
        GameViewDecoder cloneDecoder = clonePrefabs.GetComponentInChildren<GameViewDecoder>();
        TextMeshProUGUI cloneText    = clonePrefabs.GetComponentInChildren<TextMeshProUGUI>();
        
        fm_OnReceivedByteDataEvent.AddListener(cloneDecoder.Action_ProcessImageData); // 디코더를 이벤트 등록.
        cloneDecoder.label = player.ActorNumber;                                      // 디코더의 라벨 번호와 엑터 넘버를 일치.LogWarning("Invalid ActorNumber for player: " + player.ActorNumber);
        cloneText.GetComponent<TextMeshProUGUI>().text = player.NickName;             // 디코더 프리팹의 텍스트에 넥네임 표시.
        
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
}
