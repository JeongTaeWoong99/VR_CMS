using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public GameObject      shareSetting;
    public List<Button>    shareSettingButtonList = new List<Button>();
    public TextMeshProUGUI videoPlayButtonText;

    public PhotonView photonView;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
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
        Debug.Log(FileBrowser.Success);
    
        // 영상선택 버튼
        if (buttonNum == 0)
        {
            if (FileBrowser.Success)
                VideoSetting(FileBrowser.Result);
        }
        // 업로드 버튼
        else if (buttonNum == 1)
        {
            if (FileBrowser.Success)
                OnFilesSelected(FileBrowser.Result);
        }
    }
    
    // 영상선택 버튼
    public void OnVideoSelect()
    {
        StartCoroutine(ShowLoadDialogCoroutine(0));
    }

    private void VideoSetting(string[] filePaths)
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
            string[] splitParts = AccountManager.inctance.user.Email.Split('@');
            string frontPart    = splitParts[0];    // 만든사람 ID
            
            RoomOptions options = new RoomOptions();
            options.MaxPlayers  = 20;
            PhotonNetwork.CreateRoom(fileName + "$" + frontPart, options, TypedLobby.Default); // $를 통해서, 비디오 이름과 개설자를 구분...
        }
    }
    
    // 업로드 버튼
    public void OnUploadVideo()
    {
        StartCoroutine(ShowLoadDialogCoroutine(1));
    }
    
    private void OnFilesSelected(string[] filePaths)
    {
        PunSystem.instance.loadingScreen.SetActive(true);
        
        for (int i = 0; i < filePaths.Length; i++)
            Debug.Log(filePaths[i]);

        string filePath = filePaths[0];                                   // 파일경로
        string fileName = FileBrowserHelpers.GetFilename(filePath);       // 파일이름
        byte[] bytes    = FileBrowserHelpers.ReadBytesFromFile(filePath); // 파일정보
        
        // string destinationPath = Path.Combine( Application.persistentDataPath, FileBrowserHelpers.GetFilename( filePath ));
        // FileBrowserHelpers.CopyFile(filePath, destinationPath);

        var newMetadata = new MetadataChange(); // 저장소의 파일 타입
        newMetadata.ContentType = "video/mp4";

        StorageReference uploadRef = stRef.Child(fileName);

        uploadRef.GetMetadataAsync().ContinueWithOnMainThread((metadataTask) =>
        {
            if (metadataTask.IsFaulted || metadataTask.IsCanceled)
            {
                if (metadataTask.Exception != null)
                {
                    AccountManager.inctance.result = "파일이 존재하지 않습니다. 파일을 업로드 합니다.";
                    UnityMainThreadDispatcher.instance.MethodEnqueue(AccountManager.inctance.QueueFeedbackText);

                    uploadRef.PutBytesAsync(bytes, newMetadata).ContinueWithOnMainThread((task) =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                            Debug.Log(task.Exception.ToString());
                        else
                        {
                            AccountManager.inctance.result = fileName + " 업로드 성공.";
                            UnityMainThreadDispatcher.instance.MethodEnqueue(AccountManager.inctance.QueueFeedbackText);
                            UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
                        }
                    });
                }
            }
            else
            {
                AccountManager.inctance.result = fileName + " 파일이 이미 존재합니다.";
                UnityMainThreadDispatcher.instance.MethodEnqueue(AccountManager.inctance.QueueFeedbackText);
                UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
            }
        });
    }
    
    // 동영상 제어 버튼
    public void OnPlayAndStopVideo()
    {
        // 재생 -> 멈춤
        if (videoPlayer.isPlaying)
        {
            videoPlayButtonText.text = "재생";
            videoPlayer.Pause(); // 영상은 Stop으로 멈추면, 처음으로 돌아가버림.
            audioSource.Stop();
            photonView.RPC("PauseVideo",RpcTarget.Others); // 접속한 다른 교육생의 재생 제어(유일한 CMS / 나머지 교육생)
        }
        // 멈춤 -> 재생
        else
        {
            videoPlayButtonText.text = "정지";
            videoPlayer.Play();  
            audioSource.Play();
            photonView.RPC("StartVideo", RpcTarget.Others); // 접속한 다른 교육생의 재생 제어(유일한 CMS / 나머지 교육생)
        }
    }
}