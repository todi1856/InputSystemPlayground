using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UGUITests : MonoBehaviour
{
    public Button button;
    private int m_Clicks;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        m_Clicks++;
        button.GetComponentInChildren<Text>().text = m_Clicks.ToString();
    }
}
