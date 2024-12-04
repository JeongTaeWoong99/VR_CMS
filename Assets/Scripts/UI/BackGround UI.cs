using System;
using TMPro;
using UnityEngine;

public class BackGroundUI : MonoBehaviour
{
    public static BackGroundUI instance;

    public TextMeshProUGUI informationText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Screen.SetResolution(1920, 1080, false);
    }
}