using System.Collections;
using Firebase.Extensions;
using Firebase.Storage;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    private FirebaseStorage  storage;
    private StorageReference stRef;

    public Button          videoPlayButton;
    public TextMeshProUGUI videoPlayButtonText;
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
        FileBrowser.AddQuickLink( "Users", "C:\\Users", null);         // 기존 위치
    }

    private void OnEnable()
    {
        // 다른 페이지에서 넘어올 때, 초기화
        videoPlayer.targetTexture.Release();    // 재생 후 남아있는 윤곽 제거
        videoPlayButton.interactable = false;
        videoPlayButtonText.text     = "재생";
        videoPlayer.url              = "";
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
            {
                VideoSetting(FileBrowser.Result);   
            }
        }
        // 업로드 버튼
        else if (buttonNum == 1)
        {
            if (FileBrowser.Success)
            {
                OnFilesSelected(FileBrowser.Result);
            }
        }
    }
    
    // 영상선택 버튼
    public void OnVideoSelect()
    {
        StartCoroutine(ShowLoadDialogCoroutine(0));
    }

    private void VideoSetting(string[] filePaths)
    {
        if (filePaths.Length > 0)
        {
            string filePath = filePaths[0];  // Get the first selected file path
            videoPlayer.url = filePath;
            Debug.Log($"Selected video file: {filePath}");
            
            videoPlayer.Prepare();                
            videoPlayer.Pause();                  // 첫 프레임 화면(동영상 윤곽이 보이도록)
            videoPlayButton.interactable = true;  // 제어 버튼 켜기
        }
    }
    
    // 업로드 버튼
    public void OnUploadVideo()
    {
        StartCoroutine(ShowLoadDialogCoroutine(1));
    }
    
    private void OnFilesSelected(string[] filePaths)
    {
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
                    Debug.Log("파일이 존재하지 않습니다. 파일을 업로드 합니다.");

                    uploadRef.PutBytesAsync(bytes, newMetadata).ContinueWithOnMainThread((task) =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                            Debug.Log(task.Exception.ToString());
                        else
                            Debug.Log("업로드 성공.");
                    });
                }
            }
            else
            {
                Debug.Log(fileName + " 파일이 이미 존재합니다.");
            }
        });
    }
    
    // 동영상 제어 버튼
    public void OnPlayAndStopVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayButtonText.text = "재생";
            videoPlayer.Pause();    // 영상은 Stop으로 멈추면, 처음으로 돌아가버림.
            audioSource.Stop();     
        }
        else
        {
            videoPlayButtonText.text = "정지";
            videoPlayer.Play();  
            audioSource.Play();
        }
    }
}