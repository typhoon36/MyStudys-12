using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConfigBox : MonoBehaviour
{
    [Header("UI")]
    public Button m_Ok_Btn;
    public Button m_Close_Btn;
    public InputField NickInputField;
    public Text m_Message;

    float ShowMsTime = 0;


    // Start is called before the first frame update
    void Start()
    {
        if (m_Ok_Btn != null)
            m_Ok_Btn.onClick.AddListener(OkClick);

        if (m_Close_Btn != null)
            m_Close_Btn.onClick.AddListener(CloseClick);

        if (NickInputField != null)
        {
            NickInputField.text = GlobalValue.g_NickName;
        }

        //## 별명 입력을 위한 클릭시 기존 작성되어있던 내용 삭제
        if (NickInputField != null)
        {
            EventTrigger trigger = NickInputField.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { NickInputField.text = ""; });
            trigger.triggers.Add(entry);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (ShowMsTime > 0)
        {
            ShowMsTime -= Time.deltaTime;
            if (ShowMsTime <= 0)
            {
                ShowMessage("", false);
            }
        }

    }

    void OkClick()
    {
        string a_NickStr = NickInputField.text.Trim();
        if (string.IsNullOrEmpty(a_NickStr) == true)
        {
            ShowMessage("별명을 입력해주세요.");
            return;
        }

        if (!(2 <= a_NickStr.Length && a_NickStr.Length <= 10))
        {
            ShowMessage("별명은 2~10자 사이로 입력해주세요.");
            return;
        }

        LobbyNetwork_Mgr.Inst.m_NickStrBuff  = a_NickStr;
        LobbyNetwork_Mgr.Inst.m_RefCfgBox = this;
        LobbyNetwork_Mgr.Inst.PushPacket(PacketType.NickUpdate);


    }

    void CloseClick()
    {
        Time.timeScale = 1.0f;
        Destroy(this.gameObject);
    }

    void ShowMessage(string msg = "", bool isOn = true)
    {
        if (isOn)
        {
            m_Message.text = msg;
            m_Message.gameObject.SetActive(true);
            ShowMsTime= 5.0f;
        }

        else
        {
            m_Message.text = "";
            m_Message.gameObject.SetActive(false);
        }
    }

    public void ResultOkBtn(bool a_Wait, string a_str)
    {
        if (a_Wait)
        {
            ShowMessage(a_str);
        }
        else
        {
            Time.timeScale = 1.0f;
            Destroy(this.gameObject);

        }
    }
}
