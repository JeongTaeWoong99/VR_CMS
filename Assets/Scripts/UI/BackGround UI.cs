using System;
using TMPro;
using UnityEngine;

public class BackGroundUI : MonoBehaviour
{
    public static BackGroundUI instance;

    public TextMeshProUGUI informationText;

    private void Start()
    {
        instance = this;
    }
}