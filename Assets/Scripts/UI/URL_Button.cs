using UnityEngine;
using UnityEngine.UI;

public class URL_Button : MonoBehaviour
{
    public void OpenURL(string URL)
    {
        Application.OpenURL(URL);
    }
}