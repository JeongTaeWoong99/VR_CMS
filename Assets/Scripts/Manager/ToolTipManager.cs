using System;
using TMPro;
using UnityEngine;

public class ToolTipManager : MonoBehaviour
{
    public static ToolTipManager instance;

    public GameObject      tooltipObject;
    public TextMeshProUGUI nameText;

    private void Awake()
    {
        instance = this;
    }
    
    public void SetupToolTip(string name)
    {
        nameText.text = name;
    }
}
