using System;
using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Photon.Pun;
using TMPro;
using UnityEngine;

[Serializable]  // 직열화(Json형식)
public class DataToSave
{
    public string Email;
    public int    authority = 1;   // 기본 권한 = 1단계 (2단계 인증 회원 / 3단계 관리자)
    public string creationDate;    // 요청했을 때 날짜
}

public class AccountManager : MonoBehaviour
{
    public static AccountManager instance;
    
    [HideInInspector]
    public string result = "접속을 환영합니다.";
    
    [Header("인증")]
    private FirebaseAuth auth = null; // 로그인, 회원가입 등에 사용
    [HideInInspector]
    public FirebaseUser user = null; // 로그인 성공한 유저 정보

    [Header("데이터베이스")] 
    private DatabaseReference dbRef;              // 데이터베이스 참조
    public  GameObject        userRequestPrefabs;
    public  GameObject        requestGroup;       // 만들어진 유저리퀘스트 위치 그룹

    private void Awake()
    {
        instance = this;
    
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private void Start()
    {
#if UNITY_EDITOR
        PunSystem.instance.loginEmailInput.text    = "admin@123.com";
        PunSystem.instance.loginPasswordInput.text = "admin123";
#endif

        auth = FirebaseAuth.DefaultInstance;
        user = null;
    }

    private void CreateAccount()
    {
        PunSystem.instance.loadingScreen.SetActive(true);   // 비동기 전이라, 큐 사용 안해도 됨.
    
        auth.CreateUserWithEmailAndPasswordAsync(PunSystem.instance.accountEmailInput.text, PunSystem.instance.accountPasswordInput.text)
            .ContinueWith(task => 
            {   
                if (task.IsCanceled) 
                {
                    result = "회원가입이 취소되었습니다.";
                    UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
                    UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
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
                    UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
                    UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
                    return;
                }

                if (task.IsCompletedSuccessfully)
                {
                    // 인증 : 이메일 및 비밀번호 저장
                    result = "회원가입이 완료되었습니다!";
                    UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
                    
                    // 데이터베이스 : 필요 내용 저장(이메일 / 권한  등등등)
                    UnityMainThreadDispatcher.instance.MethodEnqueue(QueueDataBaseSave);

                    return;
                }
            });
    }
    
    // 일반 큐 메서드
    public void QueueFeedbackText() // 큐(메인 스레드)에서 작동.
    {
        PunSystem.instance.feedbackText.gameObject.SetActive(true);
        PunSystem.instance.feedbackText.text = result;
    }
    
    // 일반 큐 메서드 (데이터 베이스 저장)
    private void QueueDataBaseSave()
    {
        DataToSave newDTS  = new DataToSave();           // 객체 생성
        
        DateTime today = DateTime.Now;                   // 생성 날짜 저장
        newDTS.creationDate = today.ToString("yyyy-MM-dd");
        
        string[] ID_Split = PunSystem.instance.accountEmailInput.text.Split("@"); // @앞부분 아이디만 가져오기.
        string   userID   = ID_Split[0];                                                     // '@' 앞 부분을 가져옴
        newDTS.Email      = ID_Split[1];                                                     // '@' 뒷 부분을 가져옴(Email 데이터 저장)
        
        string json = JsonUtility.ToJson(newDTS); // 직열화
        dbRef.Child("Users").Child(userID).SetRawJsonValueAsync(json); // 데이터베이스의 users
                                                                       //               └── ID
                                                                       //                     └──  Json 형식 내용물
        PunSystem.instance.loadingScreen.SetActive(false);
        PunSystem.instance.accountScreen.SetActive(false);
        PunSystem.instance.accountEmailInput.text    = "";
        PunSystem.instance.accountPasswordInput.text = "";
    }

    public void Login()
    {
        PunSystem.instance.loadingScreen.SetActive(true);   // 비동기 전이라, 큐 사용 안해도 됨.
    
        if (!string.IsNullOrEmpty(PunSystem.instance.loginEmailInput.text))
        {
            auth.SignInWithEmailAndPasswordAsync(PunSystem.instance.loginEmailInput.text, PunSystem.instance.loginPasswordInput.text).ContinueWith(
                task =>
                {
                    if (task.IsCanceled) 
                    {
                        result = "로그인이 취소되었습니다.";
                        UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
                        UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
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
                        UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
                        UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
                        return;
                    }
                    
                    if (task.IsCompletedSuccessfully)
                    {
                        user = task.Result.User;    // 이메일 참조 등등에 사용
                        
                        UnityMainThreadDispatcher.instance.CoroutineEnqueue(QueueAuthorityCheck());
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
    
    // 코루틴 큐 메서드
    private IEnumerator QueueAuthorityCheck()
    {
        string[] ID_Split = PunSystem.instance.loginEmailInput.text.Split("@");  // @앞부분 아이디만 가져오기.
        string   userID   = ID_Split[0];                                                     // '@' 앞 부분을 가져옴

        var serverData = dbRef.Child("Users").Child(userID).GetValueAsync(); // 데이터 가져오기
        yield return new WaitUntil(predicate: () => serverData.IsCompleted);                 // GetValueAsync작업이 완료 될 때 까지 기다리기...
        
        DataSnapshot snapshot = serverData.Result;
        string       jsonData = snapshot.GetRawJsonValue();
        
        DataToSave bringDTS = new DataToSave();        // 객체 생성(서버에서 가져온 정보)
        
        if (jsonData != null)
        {
            bringDTS = JsonUtility.FromJson<DataToSave>(jsonData);
            switch (bringDTS.authority)
            {
                // 1단계 권한 : 접속 허용 X
                case 1:
                    PunSystem.instance.feedbackText.gameObject.SetActive(true);
                    PunSystem.instance.feedbackText.text = "관리자 승인이 필요합니다.";
                    break;
                // 2단계 권한 : 접속 허용 O
                case 2:
                    ScreenTransition(bringDTS.authority);
                    break;
                // 3단계 권한 : 접속 허용 O(관리자)
                case 3:
                    ScreenTransition(bringDTS.authority);
                    break;
            }
        }
        
        PunSystem.instance.loadingScreen.SetActive(false);
    }
    
    private void ScreenTransition(int buttonSee)
    {
        PunSystem.instance.CloseMenus();
        PunSystem.instance.feedbackText.gameObject.SetActive(false);
        PhotonNetwork.NickName = PunSystem.instance.loginEmailInput.text; // 닉네임 변경
        
        foreach (var menuButtonLists in MenuButton.instance.menuButtonList)                 // 버튼 모두 보이기
            menuButtonLists.gameObject.SetActive(true);
        switch (buttonSee)  // 권한에 따라, 권한승인 버튼 보이기 or 보이지 않기
        {
            case 2: // 2단계 권한 : 보이기 X
                MenuButton.instance.menuButtonList[2].gameObject.SetActive(false);
                BackGroundUI.instance.informationText.text = "VR CMS | " + "사용자 | " + user.Email;
                break;
            case 3: // 3단계 권한 : 보이기 O
                MenuButton.instance.menuButtonList[2].gameObject.SetActive(true);
                BackGroundUI.instance.informationText.text = "VR CMS | " + "관리자 | " + user.Email;
                break;
        }
        
        PunSystem.hasFistOnLobby = true;                                                    
    }

    // 권한 승인 : 데이터 가져오기 + 큐에 유저정보프리승인 프리팹 생성 작업 넣기
    public void ReadAllUsersFromDatabase()
    {
        PunSystem.instance.feedbackText.gameObject.SetActive(true);
        PunSystem.instance.feedbackText.text = "정보 불러오는 중...";
    
        dbRef.Child("Users").GetValueAsync().ContinueWith(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    bool isAuthorityExist = false; // 권한 승인 요청(권한 1)이 존재하는지 확인
                            
                    foreach (DataSnapshot userSnapshot in snapshot.Children)
                    {
                        string userID      = userSnapshot.Key;
                        
                        string jsonContent = userSnapshot.GetRawJsonValue();
                        DataToSave data    = JsonUtility.FromJson<DataToSave>(jsonContent);
                        
                        // 권한이 1인, 유저의 데이터들만 승인창에 뜨도록 하기.
                        if (data.authority == 1)
                        {
                            isAuthorityExist = true;
                            UnityMainThreadDispatcher.instance.MethodEnqueue(() => QueueRequestUserPanel(userID, jsonContent)); // 형식이 맞지 않는 문제로, 람다식으로 넣기.
                        }
                    }

                    if (isAuthorityExist)
                    {
                        result = "승인할 권한 요청을 선택하십시오.";
                        UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
                    }
                    else
                    {
                        result = "권한 승인 요청이 존재하지 않습니다.";
                        UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
                    }
                }
                else
                {
                    Debug.LogWarning("데이터베이스에 유저 정보가 없습니다..");
                }
            }
            else
                Debug.LogError("데이터베이스에서 데이터를 검색하지 못했습니다. : " + task.Exception);
        });
    }
    
    private void QueueRequestUserPanel(string userID, string jsonContent)
    {
        GameObject      clonePrefabs = Instantiate(userRequestPrefabs, requestGroup.transform, false); // 유저 요청 프리팹
        TextMeshProUGUI emailText    = clonePrefabs.transform.GetChild(0).GetComponent<TextMeshProUGUI>();          // 이메일 텍스트 
        TextMeshProUGUI creationDate = clonePrefabs.transform.GetChild(1).GetComponent<TextMeshProUGUI>();          // 생성날짜 텍스트
        
        DataToSave data   = JsonUtility.FromJson<DataToSave>(jsonContent);
        
        emailText.text = userID + "@" + data.Email; // 이메일 넣어주기
        
        creationDate.text = data.creationDate;      // 생성 날짜 넣어주기
    }

    public void RemoveAllUsersPrefabs() // 하위 오브젝트 모두 비우기
    {
        foreach (Transform child in requestGroup.transform)
            Destroy(child.gameObject);
    }
    
    public void RequestApproval(string email)   // '승인' 버튼 클릭 시, 확인해서, 승인.
    {
        string[] ID_Split  = email.Split("@"); // @앞부분 아이디만 가져오기.
        string   userID    = ID_Split[0];              // '@' 앞 부분을 가져옴(ID)
        string   userEmail = ID_Split[1];              // '@' 뒷 부분을 가져옴(Email)
        
        dbRef.Child("Users").GetValueAsync().ContinueWith(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    foreach (DataSnapshot userSnapshot in snapshot.Children)
                    {
                        string jsonContent = userSnapshot.GetRawJsonValue();
                        DataToSave data    = JsonUtility.FromJson<DataToSave>(jsonContent);
                        
                        if (userID == userSnapshot.Key && data.Email == userEmail)
                        {
                            Debug.Log("권한 2단계 승급");
                            data.authority = 2;
                            string updatedJsonContent = JsonUtility.ToJson(data);
                            
                            // 바뀐 정보 업데이트
                            dbRef.Child("Users").Child(userID).SetRawJsonValueAsync(updatedJsonContent)
                                .ContinueWith(saveTask => {
                                    if (saveTask.IsCompleted)
                                        Debug.Log("권한이 성공적으로 업데이트되었습니다.");
                                    else
                                        Debug.LogError("권한 업데이트 중 오류 발생: " + saveTask.Exception);
                                });
                        }
                    }
                }
                else
                    Debug.LogWarning("데이터베이스에 유저 정보가 없습니다..");
            }
            else
                Debug.LogError("데이터베이스에서 데이터를 검색하지 못했습니다. : " + task.Exception);
        });
    }

    public void LogOut()
    {
        Debug.LogError("로그아웃");
        auth.SignOut();
    }
}
