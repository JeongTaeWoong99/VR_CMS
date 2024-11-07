using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Storage;
using Photon.Pun;
using Photon.Realtime;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviourPunCallbacks
{
    public static VideoManager instance;
    
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    private FirebaseStorage  storage;
    private StorageReference stRef;

    public GameObject      videoControllerScreen;                       // 로우 이미지 및 슬라이더 및 버튼 전체 보이기 제어
    public GameObject      shareSetting;                                // 영상선택, 업로드 전체 보이기 제어
    public List<Button>    shareSettingButtonList = new List<Button>(); // 영상선택, 업로드 활성 및 비활성화 제어
    
    //public TextMeshProUGUI videoPlayButtonText;                         // ▶ ■ 버튼 텍스트
    public Image  VideoPlayButtonImage;
    public Sprite playSprite;
    public Sprite stopSprite;
    

    public Slider          playTimeSliderBar;                           // 플레이 타임 표시 슬라이더바
    
    public TextMeshProUGUI currentPlayTimeText;
    public TextMeshProUGUI maxPlayTimeText;

    [HideInInspector] 
    private IEnumerator playTimeUIRenewalCo;
    
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 기본 세팅
        videoControllerScreen.SetActive(false);
        playTimeUIRenewalCo = PlayTimeUIRenewal();  // 사용 코루틴(중복 실행을 방지하고자, 하나로 관리)
    
        // storage 세팅
        storage = FirebaseStorage.DefaultInstance;
        stRef   = storage.GetReferenceFromUrl("gs://cms-login-d93aa.appspot.com/");
        
        // FileBrowser 세팅
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "Images", ".jpg", ".png" ), new FileBrowser.Filter( "Text Files", ".txt", ".pdf" ),
                                                             new FileBrowser.Filter( "Video Files", ".mp4"));
        FileBrowser.SetDefaultFilter(".mp4");                                        // 기본 필터를 mp4로 설정
        FileBrowser.SetExcludedExtensions( ".lnk", ".tmp", ".zip", ".rar", ".exe" ); // 검색 제외
        FileBrowser.AddQuickLink( "Users", "C:\\Users", null);          // 기존 위치
    }
    
    // 영상선택 + 업로드 공통 사용
    private IEnumerator ShowLoadDialogCoroutine(int buttonNum)
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, null, null, "Select Files", "Load" );
        //Debug.Log(FileBrowser.Success);
    
        // 영상선택 버튼
        if (buttonNum == 0)
        {
            if (FileBrowser.Success)
                SelectVideo(FileBrowser.Result);
        }
        // 업로드 버튼
        else if (buttonNum == 1)
        {
            if (FileBrowser.Success)
                UpLoadVideo(FileBrowser.Result);
        }
    }
    
    // 영상선택 버튼
    public void OnSelectVideo()
    {
        StartCoroutine(ShowLoadDialogCoroutine(0));
    }

    private void SelectVideo(string[] filePaths)
    {
        PunSystem.instance.loadingScreen.SetActive(true);
        PunSystem.instance.feedbackText.gameObject.SetActive(true);
        PunSystem.instance.feedbackText.text = "방 생성 및 동영상 세팅 중...";

        if (filePaths.Length > 0 && !PhotonNetwork.InRoom)
        {
            string filePath = filePaths[0];  
            string fileName = FileBrowserHelpers.GetFilename(filePath); // 파일이름
            
            videoPlayer.url = filePath;
            Debug.Log($"Selected video file: {filePath}");
            
            videoPlayer.Prepare();                
            videoPlayer.Pause();                  // 첫 프레임 화면(동영상 윤곽이 보이도록)

            // 방 만들기
            string[] splitParts = AccountManager.instance.user.Email.Split('@');
            string frontPart    = splitParts[0];    // 만든사람 ID
            
            RoomOptions options = new RoomOptions();
            options.MaxPlayers  = 20;
            PhotonNetwork.CreateRoom(fileName + "$" + frontPart, options, TypedLobby.Default); // $를 통해서, 비디오 이름과 개설자를 구분...

            ResetPlayUI();                                      // UI 초기화
            videoControllerScreen.gameObject.SetActive(true);   // 비디오 컨트롤 보이기
        }
    }

    private void ResetPlayUI()
    {
        playTimeSliderBar.value  = 0;
        currentPlayTimeText.text = "00:00:00";
        maxPlayTimeText.text     = "00:00:00";
    }

    private IEnumerator PlayTimeUIRenewal()
    {
        int hour    = 0;
        int minutes = 0;
        int seconds = 0;

        while (true)
        {
            // 현재 재생시간 표시
            hour    = (int)videoPlayer.time / 3600;
            minutes = (int)(videoPlayer.time%3600) / 60;
            seconds = (int)(videoPlayer.time%3600) % 60;
            currentPlayTimeText.text = $"{hour:D2}:{minutes:D2}:{seconds:D2}";
            
            // 총 재생시간 표시
            hour    = (int)videoPlayer.length / 3600;
            minutes = (int)(videoPlayer.length%3600) / 60;
            seconds = (int)(videoPlayer.length%3600) % 60;
            maxPlayTimeText.text = $"{hour:D2}:{minutes:D2}:{seconds:D2}";
            
            // 슬라이더 재싱 시간 표시
            playTimeSliderBar.value = (float)(videoPlayer.time / videoPlayer.length);
            yield return new WaitForSeconds(1);
        }
    }
    
    // 동영상 제어 버튼
    public void OnPlayAndStopVideo()
    {
        // 재생 -> 멈춤
        if (videoPlayer.isPlaying)
            StopSetting();
        // 멈춤 -> 재생
        else
            PlaySetting();
    }

    private void PlaySetting()
    {
        VideoPlayButtonImage.sprite = stopSprite;
        videoPlayer.Play();  
        audioSource.Play();
        FM_System.instance.photonView.RPC("StartVideo", RpcTarget.Others); // 접속한 다른 교육생의 재생 제어(유일한 CMS / 나머지 교육생) // FM_System.instance에 있는 photonView컴포넌트를 상속하여 사용.
        StartCoroutine(playTimeUIRenewalCo);
    }

    public void StopSetting()
    {
        VideoPlayButtonImage.sprite = playSprite;
        videoPlayer.Pause(); // 영상은 Stop으로 멈추면, 처음으로 돌아가버림.
        audioSource.Stop();
        FM_System.instance.photonView.RPC("PauseVideo",RpcTarget.Others); // 접속한 다른 교육생의 재생 제어(유일한 CMS / 나머지 교육생) // FM_System.instance에 있는 photonView컴포넌트를 상속하여 사용.
        StopCoroutine(playTimeUIRenewalCo);
    }

    // 업로드 버튼
    public void OnUploadVideo()
    {
        StartCoroutine(ShowLoadDialogCoroutine(1));
    }
    
    private void UpLoadVideo(string[] filePaths)
    {
        PunSystem.instance.loadingScreen.SetActive(true);
        
        for (int i = 0; i < filePaths.Length; i++)
            Debug.Log(filePaths[i]);

        string filePath      = filePaths[0];                                   // 파일경로
        string fileName      = FileBrowserHelpers.GetFilename(filePath);       // 파일이름
        byte[] bytes         = FileBrowserHelpers.ReadBytesFromFile(filePath); // 파일정보
        string localFileHash = CalculateMD5Hash(bytes);                        // 파일의 MD5 해쉬 정보 계산(단방향 / 덜 안전 / 빠름 / 체크성)
        
        // string destinationPath = Path.Combine( Application.persistentDataPath, FileBrowserHelpers.GetFilename( filePath ));
        // FileBrowserHelpers.CopyFile(filePath, destinationPath);

        var newMetadata            = new MetadataChange();                                              // 저장소의 파일 타입 생성
        newMetadata.ContentType    = "video/mp4";                                                       // 타입 변경(저장소 직접 업로드 시, 동영상의 타입으로 맞춰줌)
        newMetadata.CustomMetadata = new Dictionary<string, string> { { "md5Hash", localFileHash } };   // 해쉬값
        
        StorageReference uploadRef = stRef.Child(fileName);
        uploadRef.GetMetadataAsync().ContinueWithOnMainThread((metadataTask) =>
        {
            // 저장소에 같은 파일 이름 존재 X -> 업로드 O
            if (metadataTask.IsFaulted || metadataTask.IsCanceled)
            {
                if (metadataTask.Exception != null)
                {
                    AccountManager.instance.result = "파일이 존재하지 않습니다. 파일을 업로드 합니다.";
                    UnityMainThreadDispatcher.instance.MethodEnqueue(AccountManager.instance.QueueFeedbackText);

                    uploadRef.PutBytesAsync(bytes, newMetadata).ContinueWithOnMainThread((task) =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                            Debug.Log(task.Exception.ToString());
                        else
                        {
                            AccountManager.instance.result = fileName + "업로드 성공.";
                            UnityMainThreadDispatcher.instance.MethodEnqueue(AccountManager.instance.QueueFeedbackText);
                            UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
                        }
                    });
                }
            }
            // 저장소에 같은 파일 이름 존재 O -> MD5 해쉬로 체크
            else
            {
                StorageMetadata metadata       = metadataTask.Result;                      // 파일의 메타정보 받아오기
                string          remoteFileHash = metadata.GetCustomMetadata("md5Hash"); // 파일의 사용자 정의 메타데이터에 액세스

                // 파일 이름 동일 + 해쉬 정보 동일  -> 업로드 X
                if (remoteFileHash == localFileHash)
                {
                    AccountManager.instance.result = fileName + "파일 이름이 동일하며, 해쉬 정보도 동일합니다.";
                    UnityMainThreadDispatcher.instance.MethodEnqueue(AccountManager.instance.QueueFeedbackText);
                    UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
                }
                // 파일 이름 동일 + 해쉬 정보 다름. -> 업로드 O
                else
                {
                    AccountManager.instance.result = fileName + " 파일이 존재하지만 정보가 다릅니다. 새 버전 업로드 중...";
                    uploadRef.PutBytesAsync(bytes, newMetadata).ContinueWithOnMainThread((task) =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                            Debug.Log(task.Exception.ToString());
                        else
                        {
                            AccountManager.instance.result = fileName + "업로드 성공.";
                            UnityMainThreadDispatcher.instance.MethodEnqueue(AccountManager.instance.QueueFeedbackText);
                            UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
                        }
                    });
                }
            }
        });
    }
    
    // MD5해쉬 계산
    private string CalculateMD5Hash(byte[] bytes)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        byte[] hashBytes = md5.ComputeHash(bytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

}