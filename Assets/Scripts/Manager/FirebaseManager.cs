using Firebase;
using Firebase.Auth;
using Photon.Pun;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    private FirebaseAuth auth; // 로그인, 회원가입 등에 사용
    private FirebaseUser user; // 로그인 성공한 유저 정보

    private string result = "접속을 환영합니다.";

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = null;
        
        PunSystem.instance.nicknameOrEmailInput.text = "admin@123.com";
        PunSystem.instance.passwordInput.text        = "admin123";
    }

    private void CreateAccount()
    {
        auth.CreateUserWithEmailAndPasswordAsync(PunSystem.instance.nicknameOrEmailInput.text, PunSystem.instance.passwordInput.text)
            .ContinueWith(task => 
            {   
                if (task.IsCanceled) 
                {
                    result = "회원가입이 취소되었습니다.";
                    UnityMainThreadDispatcher.instance.MethodEnqueue(UpdateFeedbackText);
                    return;
                }
                
                if (task.IsFaulted) 
                {
                    string errorMessage = "회원가입 실패: ";
                    foreach (var exception in task.Exception.Flatten().InnerExceptions) 
                    {
                        if (exception is FirebaseException authEx) 
                        {
                            int errorCode = authEx.ErrorCode;
                            switch (errorCode) 
                            {
                                case 38: // ERROR_INVALID_EMAIL
                                    errorMessage += "유효하지 않은 이메일입니다.";
                                    break;
                                case 28: // ERROR_WEAK_PASSWORD
                                    errorMessage += "비밀번호가 너무 간단합니다.";
                                    break;
                                case 8: // ERROR_EMAIL_ALREADY_IN_USE
                                    errorMessage += "이미 사용 중인 이메일입니다.";
                                    break;
                                default:
                                    errorMessage += "알 수 없는 오류가 발생했습니다.";
                                    break;
                            }
                        } 
                        else 
                        {
                            errorMessage += "알 수 없는 오류가 발생했습니다.";
                        }
                    }
                    result = errorMessage;
                    UnityMainThreadDispatcher.instance.MethodEnqueue(UpdateFeedbackText);
                    return;
                }

                if (task.IsCompletedSuccessfully)
                {
                    result = "회원가입이 완료되었습니다!";
                    UnityMainThreadDispatcher.instance.MethodEnqueue(UpdateFeedbackText);
                    return;
                }
            });
    }
    
    // 일반 큐 메서드
    private void UpdateFeedbackText() // 큐(메인 스레드)에서 작동.
    {
        PunSystem.instance.feedbackText.text = result;
    }

    // 코루틴 큐 메서드
    // private IEnumerator UpdateFeedbackText()    // 큐(메인 스레드)에서 작동.
    // {
    //     PunSystem.instance.feedbackText.text = result;
    //     yield return null;
    // }
    
    public void Login()
    {
        if (!string.IsNullOrEmpty(PunSystem.instance.nicknameOrEmailInput.text))
        {
            auth.SignInWithEmailAndPasswordAsync(PunSystem.instance.nicknameOrEmailInput.text, PunSystem.instance.passwordInput.text).ContinueWith(
                task =>
                {
                    if (task.IsCanceled) 
                    {
                        result = "로그인이 취소되었습니다.";
                        UnityMainThreadDispatcher.instance.MethodEnqueue(UpdateFeedbackText);
                        return;
                    }
            
                    if (task.IsFaulted) 
                    {
                        string errorMessage = "로그인 실패: ";
                        foreach (var exception in task.Exception.Flatten().InnerExceptions) 
                        {
                            if (exception is FirebaseException authEx) 
                            {
                                int errorCode = authEx.ErrorCode;
                                Debug.Log(errorCode);
                                switch (errorCode) 
                                {
                                    case 1: // ERROR_INVALID_EMAIL
                                        errorMessage += "비밀번호가 일치하지 않습니다.";
                                        break;
                                    case 38: // ERROR_EMAIL_ALREADY_IN_USE
                                        errorMessage += "이메일 주소가 틀립니다.";
                                        break;
                                    default:
                                        errorMessage += "알 수 없는 오류가 발생했습니다.";
                                        break;
                                }
                            } 
                            else 
                            {
                                errorMessage += "알 수 없는 오류가 발생했습니다.";
                            }
                        }
                        result = errorMessage;
                        UnityMainThreadDispatcher.instance.MethodEnqueue(UpdateFeedbackText);
                        return;
                    }
                    
                    if (task.IsCompletedSuccessfully)
                    {
                        user = task.Result.User;
                        UnityMainThreadDispatcher.instance.MethodEnqueue(ScreenTransition); // 화면 전환
                    }
                });
        
            // // 관리자 로그인 체크
            // if (PunSystem.instance.authorityToggle.isOn)
            // {
            //     auth.SignInWithEmailAndPasswordAsync(PunSystem.instance.nicknameOrEmailInput.text, PunSystem.instance.passwordInput.text).ContinueWith(
            //         task =>
            //         {
            //             if (task.IsCanceled)
            //             {
            //                 Debug.LogError("로그인 취소");
            //                 return;
            //             }
            //
            //             if (task.IsFaulted)
            //             {
            //                 // 실패 이유 -> 이메일이 비정상 / 비밀번호 너무 간단 / 이미 가입된 이메일 등등...
            //                 Debug.LogError("로그인 실패");
            //                 return;
            //             }
            //             
            //             if (task.IsCompletedSuccessfully)
            //             {
            //                 // ☆ 파이어베이스 auth.SignInWithEmailAndPasswordAsync의 비동기 실행 때문에, 유니티의 작업을 여기에 넣으면, 실행되지 않는 문제 발생.
            //                 // 그래서, FixedUpdate에서 실행하도록 한다.
            //                 Debug.Log("관리자 로그인 완료");
            //                 user = task.Result.User;
            //             }
            //         });
            // }
            // // 교육생 로그인 체크
            // else
            // {
            //     Debug.Log("교육생 접속 완료");
            //     for (int i = 1; i < MenuButton.instance.menuButtonList.Count; i++) // 비디오 공유 버튼 보이기 제외
            //         MenuButton.instance.menuButtonList[i].gameObject.SetActive(true);
            //     SettingLogin();
            //     BackGroundUI.instance.informationText.text = "VR CMS | " + "교육생 | " + PunSystem.instance.nicknameOrEmailInput.text;
            // }
        }
    }

    // 일반 큐 메서드
    private void ScreenTransition()
    {
        PunSystem.instance.CloseMenus();                                            
        
        foreach (var menuButtonLists in MenuButton.instance.menuButtonList)                 // 버튼 모두 보이기
            menuButtonLists.gameObject.SetActive(true);
        
        PhotonNetwork.NickName   = PunSystem.instance.nicknameOrEmailInput.text;            // 텍스트 변경
        BackGroundUI.instance.informationText.text = "VR CMS | " + "관리자 | " + user.Email;
        PunSystem.hasFistOnLobby = true;                                                    
    }
    
    // private void SettingLogin() // 관리자 + 교육생 공통 부분
    // {
    //     PunSystem.instance.CloseMenus();
    //     PhotonNetwork.NickName   = PunSystem.instance.nicknameOrEmailInput.text;
    //     PunSystem.hasFistOnLobby = true;
    // }
    
    // 로그인 권한 토글
    // public void AuthorityToggle()
    // {
    //     if (PunSystem.instance.authorityToggle.isOn)
    //     {
    //         PunSystem.instance.placeholderText.text = "이메일 입력...";
    //         PunSystem.instance.passwordInput.gameObject.SetActive(true);
    //         PunSystem.instance.createAccountButton.SetActive(true);
    //     }
    //     else
    //     {
    //         PunSystem.instance.placeholderText.text = "닉네임 입력...";
    //         PunSystem.instance.passwordInput.gameObject.SetActive(false);
    //         PunSystem.instance.createAccountButton.SetActive(false);
    //     }
    //     PunSystem.instance.nicknameOrEmailInput.text = "";  // 비우기
    //     PunSystem.instance.passwordInput.text        = "";  // 비우기
    // }
    
    public void LogOut()
    {
        Debug.LogError("로그아웃");
        auth.SignOut(); 
    }
}
