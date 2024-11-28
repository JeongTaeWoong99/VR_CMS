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
    private bool          wasVisibleLastFrame = false;

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
        if (scrollRect != null && viewportTransform != null && PhotonNetwork.InRoom)
        {
            bool isVisibleNow = IsImageVisibleInViewport();
            
            Debug.Log(isVisibleNow + " && " + !wasVisibleLastFrame + " = 보임."  );
            Debug.Log(!isVisibleNow + " && " + wasVisibleLastFrame + " = 보이지 않음."  );
            if (isVisibleNow && !wasVisibleLastFrame)
            {
                Player targetTrainee = PhotonNetwork.PlayerListOthers.FirstOrDefault(p => p.CustomProperties.ContainsKey("Trainee") && (string)p.CustomProperties["Trainee"] == nickNameText.text);
                if (targetTrainee != null)
                {
                    wasVisibleLastFrame = isVisibleNow; // wasVisibleLastFrame 상태 변경...
                    FM_System.instance.photonView.RPC("Watching", targetTrainee, isVisibleNow); // FM_System.instance에 있는 photonView컴포넌트를 상속하여 사용.
                    Debug.Log(targetTrainee + " 상태 변경 -> true");
                }
                else
                {
                    foreach (var VARIABLE in PhotonNetwork.PlayerList)
                    {
                        Debug.Log(PhotonNetwork.CurrentRoom.Name + " | " + VARIABLE.NickName);
                    }
                    Debug.Log("targetTrainee 찾지 못함...!");
                }
            }
            else if (!isVisibleNow && wasVisibleLastFrame)
            {
                Player targetTrainee = PhotonNetwork.PlayerListOthers.FirstOrDefault(p => p.CustomProperties.ContainsKey("Trainee") && (string)p.CustomProperties["Trainee"] == nickNameText.text);
                if (targetTrainee != null)
                {
                    wasVisibleLastFrame = isVisibleNow; // wasVisibleLastFrame 상태 변경...
                    FM_System.instance.photonView.RPC("Watching", targetTrainee, isVisibleNow); // FM_System.instance에 있는 photonView컴포넌트를 상속하여 사용.
                    Debug.Log(targetTrainee + " 상태 변경 -> false");
                }
                else
                {
                    Debug.Log("targetTrainee 찾지 못함...!!");
                }
            }
        }
    }

    bool IsImageVisibleInViewport()
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