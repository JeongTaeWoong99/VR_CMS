using System;
using Firebase.Auth;
using Photon.Pun;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    private FirebaseAuth auth; // 로그인, 회원가입 등에 사용
    private FirebaseUser user; // 로그인 성공한 유저 정보
    private bool         isScreenTransition = false;  // 로그인 성공하고, 1회 실행 함수

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = null;
        
        PunSystem.instance.nicknameOrEmailInput.text = "jto0402@naver.com";
        PunSystem.instance.passwordInput.text        = "tt0402!!";
    }

    private void FixedUpdate()
    {
        // 1회 실행
        // ☆ 파이어베이스 auth.SignInWithEmailAndPasswordAsync의 비동기 실행 때문에, 유니티의 작업을 여기에 넣으면, 실행되지 않는 문제 발생.
        // 그래서, FixedUpdate에서 실행하도록 한다.
        if (!isScreenTransition)
        {
            if(user == null)
                return;
            ScreenTransition();
        }
    }

    public void CreateAccount()    // 회원가입
    {
        auth.CreateUserWithEmailAndPasswordAsync(PunSystem.instance.nicknameOrEmailInput.text, PunSystem.instance.passwordInput.text).ContinueWith(
            task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("회원가입 취소");
                    return;
                }

                if (task.IsFaulted)
                {
                    // 실패 이유 -> 이메일이 비정상 / 비밀번호 너무 간단 / 이미 가입된 이메일 등등...
                    Debug.LogError("회원가입 실패");
                    return;
                }
                
                Debug.LogError("회원가입 완료");
                // AuthResult newUser = task.Result;
            });
    }
    
    public void Login()
    {
        if (!string.IsNullOrEmpty(PunSystem.instance.nicknameOrEmailInput.text))
        {
            // 관리자 로그인 체크
            if (PunSystem.instance.authorityToggle.isOn)
            {
                auth.SignInWithEmailAndPasswordAsync(PunSystem.instance.nicknameOrEmailInput.text, PunSystem.instance.passwordInput.text).ContinueWith(
                    task =>
                    {
                        if (task.IsCanceled)
                        {
                            Debug.LogError("로그인 취소");
                            return;
                        }

                        if (task.IsFaulted)
                        {
                            // 실패 이유 -> 이메일이 비정상 / 비밀번호 너무 간단 / 이미 가입된 이메일 등등...
                            Debug.LogError("로그인 실패");
                            return;
                        }
                        
                        if (task.IsCompletedSuccessfully)
                        {
                            // ☆ 파이어베이스 auth.SignInWithEmailAndPasswordAsync의 비동기 실행 때문에, 유니티의 작업을 여기에 넣으면, 실행되지 않는 문제 발생.
                            // 그래서, FixedUpdate에서 실행하도록 한다.
                            Debug.Log("관리자 로그인 완료");
                            user = task.Result.User;
                        }
                    });
            }
            // 교육생 로그인 체크
            else
            {
                Debug.Log("교육생 접속 완료");
                for (int i = 1; i < MenuButton.instance.menuButtonList.Count; i++) // 비디오 공유 버튼 보이기 제외
                    MenuButton.instance.menuButtonList[i].gameObject.SetActive(true);
                SettingLogin();
                BackGroundUI.instance.informationText.text = "VR CMS | " + "교육생 | " + PunSystem.instance.nicknameOrEmailInput.text;
            }
        }
    }

    private void ScreenTransition()
    {
        isScreenTransition = true;
        
        foreach (var menuButtonLists in MenuButton.instance.menuButtonList)
            menuButtonLists.gameObject.SetActive(true);
        SettingLogin();
        BackGroundUI.instance.informationText.text = "VR CMS | " + "관리자 | " + user.Email;
    }
    
    private void SettingLogin() // 관리자 + 교육생 공통 부분
    {
        PunSystem.instance.CloseMenus();
        PhotonNetwork.NickName   = PunSystem.instance.nicknameOrEmailInput.text;
        PunSystem.hasFistOnLobby = true;
    }
    
    // 로그인 권한 토글
    public void AuthorityToggle()
    {
        if (PunSystem.instance.authorityToggle.isOn)
        {
            PunSystem.instance.placeholderText.text = "이메일 입력...";
            PunSystem.instance.passwordInput.gameObject.SetActive(true);
            PunSystem.instance.createAccountButton.SetActive(true);
        }
        else
        {
            PunSystem.instance.placeholderText.text = "닉네임 입력...";
            PunSystem.instance.passwordInput.gameObject.SetActive(false);
            PunSystem.instance.createAccountButton.SetActive(false);
        }
        PunSystem.instance.nicknameOrEmailInput.text = "";  // 비우기
        PunSystem.instance.passwordInput.text        = "";  // 비우기
    }
    
    public void LogOut()
    {
        Debug.LogError("로그아웃");
        auth.SignOut(); 
    }
}
