using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MirroringScreenClick : MonoBehaviour, IPointerClickHandler
{
    public GameObject      bigRawImage;
    
    public TextMeshProUGUI nickNameText;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject createTargetGameObject = BigMirroingCanvas.instance.backPanel;
        
        // '/' 이전의 부분을 제거한 텍스트를 가져오기(씬 이름 부분 제거)
        // 타겟 플레이어 찾기
        string filteredNickName = nickNameText.text.Substring(nickNameText.text.IndexOf('/') + 1);
        Player targetTrainee = PhotonNetwork.PlayerListOthers.FirstOrDefault(p => p.CustomProperties.ContainsKey("Trainee") && (string)p.CustomProperties["Trainee"] == filteredNickName);

        if (createTargetGameObject != null)
        {
            if (bigRawImage != null)
            {
                createTargetGameObject.SetActive(true);
                
                GameObject clone = Instantiate(bigRawImage, createTargetGameObject.transform);
                clone.transform.localScale = new Vector3(18,10,1);
                
                BigMirroingCanvas.instance.currentWatchingNickName = filteredNickName;                       // 현재 보고있는 닉네임 변경.
                FM_System.instance.photonView.RPC("StreamChange", targetTrainee,5,80); // FPS 1 -> 10, Quality 30 -> 80 //FM_System.instance에 있는 photonView컴포넌트를 상속하여 사용.
            }
            else
            {
                Debug.Log("bigRawImage가 할당되지 않음.");
            }
        }
    }
}