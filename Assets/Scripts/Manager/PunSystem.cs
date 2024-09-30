using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PunSystem : MonoBehaviourPunCallbacks
{
    public static PunSystem instance;

    // public GameViewDecoder _gameViewDecoder_1;
    // public GameViewDecoder _gameViewDecoder_2;

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
        fm_OnReceivedByteDataEvent = fmNetworkManager.OnReceivedByteDataEvent;
    }

    public void DecoderRegistration(Player player)
    {
        GameObject      clonePrefabs = Instantiate(decoderPrefabs, decoderGroup.transform, true); // 새 디코더 스크립트 생성
        GameViewDecoder cloneDecoder = clonePrefabs.GetComponentInChildren<GameViewDecoder>();
        TextMeshProUGUI cloneText    = clonePrefabs.GetComponentInChildren<TextMeshProUGUI>();
        
        fm_OnReceivedByteDataEvent.AddListener(cloneDecoder.Action_ProcessImageData); // 디코더를 이벤트 등록.
        cloneDecoder.label = player.ActorNumber;                                      // 디코더의 라벨 번호와 엑터 넘버를 일치.LogWarning("Invalid ActorNumber for player: " + player.ActorNumber);
        cloneText.GetComponent<TextMeshProUGUI>().text = player.NickName;             // 디코더 프리팹의 텍스트에 넥네임 표시.
        
        gameViewDecoderList.Add(cloneDecoder);
    }
    
    [PunRPC] // ROOM에 입장해서 RPC 사용 가능!
    public void RPC_SendMessage(byte[] _bytesData, string message)
    {
        if (message.Contains("VideoShare"))
        {
            foreach (var gameViewDecoderListS in gameViewDecoderList)
            {
                gameViewDecoderListS.Action_ProcessImageData(_bytesData);
            }
            
            // Debug.Log("이미지 RPC 공유");
            // _gameViewDecoder_1.Action_ProcessImageData(_bytesData);
            // _gameViewDecoder_2.Action_ProcessImageData(_bytesData);
        }
    }
}
