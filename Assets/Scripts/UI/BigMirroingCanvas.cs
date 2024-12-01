using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;

public class BigMirroingCanvas : MonoBehaviour, IPointerClickHandler
{
    public static BigMirroingCanvas instance;

    public GameObject backPanel;
    [HideInInspector] 
    public string currentWatchingNickName;  // 현재 보고있는 닉네임
    
    private void Awake()
    {
        instance = this;
    }
    
    // 빅패널을 클릭해서 닫기
    public void OnPointerClick(PointerEventData eventData)
    {
        if (backPanel != null)
        {                                                                                               
            Player targetTrainee = PhotonNetwork.PlayerListOthers.FirstOrDefault(p => p.CustomProperties.ContainsKey("Trainee") && (string)p.CustomProperties["Trainee"] == currentWatchingNickName);
            FM_System.instance.photonView.RPC("StreamChange", targetTrainee,1,30); // 다시 원래대로 변경... // FM_System.instance에 있는 photonView컴포넌트를 상속하여 사용.
            
            foreach (Transform child in backPanel.transform)
            {
                Destroy(child.gameObject);
            }
            backPanel.gameObject.SetActive(false);
            
            Debug.Log("All child objects of 'Back Panel' have been deleted.");
        }
        else
        {
            Debug.LogWarning("'Back Panel' object not found.");
        }
    }
}