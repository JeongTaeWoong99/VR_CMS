using Photon.Pun;
using UnityEngine;

public class LoginButton : MonoBehaviour
{
    // 로그인 권한 토글
    public void AuthorityToggle()
    {
        if (PunSystem.instance.authorityToggle.isOn)
            PunSystem.instance.passwordInput.gameObject.SetActive(true);
        else
            PunSystem.instance.passwordInput.gameObject.SetActive(false);
    }
    
    // 버튼 함수
    public void OnLogin()
    {
        if (!string.IsNullOrEmpty(PunSystem.instance.nicknameInput.text))
        {
            // 관리자 로그인 
            if (PunSystem.instance.authorityToggle.isOn && PunSystem.instance.passwordInput.text == "1234")
            {
                foreach (var menuButtonLists in MenuButton.instance.menuButtonList)  // 모든 버튼 보이기
                    menuButtonLists.gameObject.SetActive(true);
                SettingLogin();
                BackGroundUI.instance.informationText.text = "VR CMS | " + "관리자 | " + PunSystem.instance.nicknameInput.text;
            }
            // 교육생 로그인
            else if (!PunSystem.instance.authorityToggle.isOn)
            {
                for (int i = 0; i < MenuButton.instance.menuButtonList.Count - 1; i++) // 비디오 공유 버튼 보이기 제외
                    MenuButton.instance.menuButtonList[i].gameObject.SetActive(true);
                SettingLogin();
                BackGroundUI.instance.informationText.text = "VR CMS | " + "교육생 | " + PunSystem.instance.nicknameInput.text;
            }
        }
    }

    private void SettingLogin()
    {
        PhotonNetwork.NickName = PunSystem.instance.nicknameInput.text;
        PlayerPrefs.SetString("playerName", PunSystem.instance.nicknameInput.text);
        PunSystem.instance.CloseMenus();
        PunSystem.hasSetNick = true;
    }
}
