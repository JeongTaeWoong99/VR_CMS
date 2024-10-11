using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserRequestButton : MonoBehaviour
{
    public TextMeshProUGUI emailText;
    public Button          approveButton;
    
    public void RequestButtonClick()
    {
        approveButton.interactable = false;
        FirebaseManager.inctance.RequestApproval(emailText.text);
    }
}
