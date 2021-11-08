using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxController : MonoBehaviour
{
    Text text;
    public void ShowMessage(string message) 
    {
        gameObject.SetActive(true);
        if (text == null) 
            text = transform.Find("MessageText").GetComponent<Text>();
        text.text = message;
    }
    public void HideMessageBox() 
    {
        gameObject.SetActive(false);
    }
}
