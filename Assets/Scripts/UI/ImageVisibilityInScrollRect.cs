using System;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageVisibilityInScrollRect : MonoBehaviour
{
    private RectTransform rectTransform;
    private ScrollRect    scrollRect;
    private RectTransform viewportTransform;

    [HideInInspector]
    public  bool isVisibleNow;                  // FM_System에서 isVisibleNow가 true인 경우, 투명도 1로 바꾸기 위함.
    private bool wasVisibleLastFrame = false;

    public TextMeshProUGUI nickNameText;

    void Start()
    {
        // 이 UI 요소의 RectTransform과 해당 요소가 있는 ScrollRect를 가져옵니다.
        rectTransform = GetComponent<RectTransform>();
        scrollRect    = GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
            viewportTransform = scrollRect.viewport;
        else
            Debug.Log("ScrollRect not found in parent hierarchy.");
    }

    void FixedUpdate()
    {
        ViewCheck();
    }

    private void ViewCheck()
    {
        // ※ BigMirroringCanvas.instance.backPanel이 active하지 않을 때, 스크롤 뷰 체크.
        // 6개의 화면에서, 내려가거나 올라가는 경우가 생길 때, 멈추지 않게 하기 위함.
        if (scrollRect != null && viewportTransform != null && PhotonNetwork.InRoom && !BigMirroingCanvas.instance.backPanel.activeInHierarchy)
        {
            isVisibleNow = IsImageVisibleInViewport();
            
            // Debug.Log( isVisibleNow + " && " + !wasVisibleLastFrame + " = 보임.");
            // Debug.Log(!isVisibleNow + " && " +  wasVisibleLastFrame + " = 보이지 않음.");
            
            // '/' 이전의 부분을 제거한 텍스트를 가져오기(씬 이름 부분 제거)
            string filteredNickName = nickNameText.text.Substring(nickNameText.text.IndexOf('/') + 1);
            
            if (isVisibleNow && !wasVisibleLastFrame)
            {
                // 타겟 플레이어 찾기
                Player targetTrainee = PhotonNetwork.PlayerListOthers.FirstOrDefault(p => p.CustomProperties.ContainsKey("Trainee") && (string)p.CustomProperties["Trainee"] == filteredNickName);
                if (targetTrainee != null)
                {
                    wasVisibleLastFrame = isVisibleNow; // wasVisibleLastFrame 상태 변경...
                    FM_System.instance.photonView.RPC("Watching", targetTrainee, isVisibleNow); // FM_System.instance에 있는 photonView컴포넌트를 상속하여 사용.
                    Debug.Log(targetTrainee + " 상태 변경 -> true");
                }
                else
                {
                    Debug.Log("targetTrainee 찾지 못함...!");
                }
            }
            else if (!isVisibleNow && wasVisibleLastFrame)
            {
                Player targetTrainee = PhotonNetwork.PlayerListOthers.FirstOrDefault(p => p.CustomProperties.ContainsKey("Trainee") && (string)p.CustomProperties["Trainee"] == filteredNickName);
                if (targetTrainee != null)
                {
                    wasVisibleLastFrame = isVisibleNow; // wasVisibleLastFrame 상태 변경...
                    FM_System.instance.photonView.RPC("Watching", targetTrainee, isVisibleNow); // FM_System.instance에 있는 photonView컴포넌트를 상속하여 사용.
                    Debug.Log(targetTrainee + " 상태 변경 -> false");
                    
                    gameObject.GetComponent<RawImage>().color = new Color(1, 1, 1, 0); // 투명도 = 0 (안 보이게)
                }
                else
                {
                    Debug.Log("targetTrainee 찾지 못함...!!");
                }
            }
        }
    }

    private bool IsImageVisibleInViewport()
    {
        // 눈에 보이는 상태가 변경된 경우에만 오류가 발생했습니다.
        Rect viewportRect             = viewportTransform.rect;
        Vector3 viewportWorldPosition = viewportTransform.position;
        viewportRect.position += new Vector2(viewportWorldPosition.x, viewportWorldPosition.y);

        // 월드 공간에서 이 이미지의 경계를 가져옵니다.
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // 이미지의 모서리가 뷰포트 내에 있는지 확인합니다.
        foreach (Vector3 corner in corners)
        {
            if (viewportRect.Contains(corner))
            {
                return true;
            }
        }
        
        return false;
    }
}