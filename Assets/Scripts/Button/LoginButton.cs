using Photon.Pun;
using UnityEngine;

public class LoginButton : MonoBehaviour
{
    
    

    // // 로그인 권한 토글
    // public void AuthorityToggle()
    // {
    //     if (PunSystem.instance.authorityToggle.isOn)
    //     {
    //         PunSystem.instance.placeholderText.text = "이메일 입력...";
    //         PunSystem.instance.passwordInput.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         PunSystem.instance.placeholderText.text = "닉네임 입력...";
    //         PunSystem.instance.passwordInput.gameObject.SetActive(false);
    //     }
    //     PunSystem.instance.nicknameOrEmailInput.text = "";  // 비우기
    //     PunSystem.instance.passwordInput.text        = "";  // 비우기
    // }
    
    // 버튼 함수
    
    // public void OnLogin()
    // {
    //     if (!string.IsNullOrEmpty(PunSystem.instance.nicknameOrEmailInput.text))
    //     {
    //         // 관리자 로그인 
    //         if (PunSystem.instance.authorityToggle.isOn && PunSystem.instance.passwordInput.text == "1234")
    //         {
    //             foreach (var menuButtonLists in MenuButton.instance.menuButtonList)  // 모든 버튼 보이기
    //                 menuButtonLists.gameObject.SetActive(true);
    //             SettingLogin();
    //             BackGroundUI.instance.informationText.text = "VR CMS | " + "관리자 | " + PunSystem.instance.nicknameOrEmailInput.text;
    //         }
    //         // 교육생 로그인
    //         else if (!PunSystem.instance.authorityToggle.isOn)
    //         {
    //             for (int i = 0; i < MenuButton.instance.menuButtonList.Count - 1; i++) // 비디오 공유 버튼 보이기 제외
    //                 MenuButton.instance.menuButtonList[i].gameObject.SetActive(true);
    //             SettingLogin();
    //             BackGroundUI.instance.informationText.text = "VR CMS | " + "교육생 | " + PunSystem.instance.nicknameOrEmailInput.text;
    //         }
    //     }
    // }

    private void SettingLogin()
    {
        PunSystem.instance.CloseMenus();
        PhotonNetwork.NickName = PunSystem.instance.nicknameOrEmailInput.text;
        PunSystem.hasFistOnLobby = true;
    }
}
