using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfigBox : MonoBehaviour
{
    public delegate void CFG_Response();    //<--- ��������Ʈ ������(�ɼ�)�� �ϳ� ����
    public CFG_Response DltMethod = null;   //<--- ��������Ʈ ���� ����(���� ����)

    public Button m_Ok_Btn = null;
    public Button m_Close_Btn = null;

    public InputField NickInputField = null;

    public Toggle m_Sound_Toggle = null;
    public Slider m_Sound_Slider = null;

    float ShowTimer = 0.0f;
    public Text m_Message = null;


    HeroCtrl m_RefHero = null;

    // Start is called before the first frame update
    void Start()
    {
        if (m_Ok_Btn != null)
            m_Ok_Btn.onClick.AddListener(OkBtnClick);

        if (m_Close_Btn != null)
            m_Close_Btn.onClick.AddListener(CloseBtnClick);

        if (m_Sound_Toggle != null)
            m_Sound_Toggle.onValueChanged.AddListener(SoundOnOff);
        //üũ ���°� ����Ǿ��� �� ȣ��Ǵ� �Լ��� ����ϴ� �ڵ�

        if (m_Sound_Slider != null)
            m_Sound_Slider.onValueChanged.AddListener(SliderChanged);
        //�����̵� ���°� ���� �Ǿ��� �� ȣ��Ǵ� �Լ� ����ϴ� �ڵ�

        m_RefHero = FindObjectOfType<HeroCtrl>();
        //Hierarchy�ʿ��� HeroCtrl ������Ʈ�� �پ��ִ� ���ӿ�����Ʈ�� ã�Ƽ� ��ü�� ã�ƿ��� ���

        //--- üũ����, �����̵����, �г��� �ε� �� UI ��Ʈ�ѿ� ����
        int a_SoundOnOff = PlayerPrefs.GetInt("SoundOnOff", 1);
        if (m_Sound_Toggle != null)
        {
            //if (a_SoundOnOff == 1)
            //    m_Sound_Toggle.isOn = true;
            //else
            //    m_Sound_Toggle.isOn = false;

            m_Sound_Toggle.isOn = (a_SoundOnOff == 1) ? true : false;
        }

        if (m_Sound_Slider != null)
            m_Sound_Slider.value = PlayerPrefs.GetFloat("SoundVolume", 1.0f);

        //Text a_Placehoder = null;
        if (NickInputField != null)
        {
            //## ����
            // NickInputField.text = PlayerPrefs.GetString("NickName", "SBS����");

            //## ����
            NickInputField.text = GlobalValue.g_NickName;

        }
        //--- üũ����, �����̵����, �г��� �ε� �� UI ��Ʈ�ѿ� ����

    }

    // Update is called once per frame
    void Update()
    {

        if (0 < ShowTimer)
        {
            ShowTimer -= Time.unscaledDeltaTime;
            if (ShowTimer <= 0.0f)
            {
                Message_Show("", false);
            }
        }
    }

    private void OkBtnClick()
    {
        //--- �г��� ���ΰ� �Ӹ����� ����
        if (NickInputField != null)
        {
            string a_NickStr = NickInputField.text;
            a_NickStr = a_NickStr.Trim();   //�յ� ������ ������ �ִ� �Լ�
            if (string.IsNullOrEmpty(a_NickStr) == true)
            {
                Message_Show("������ ��ĭ ���� �Է��� �ּ���.");
                return;
            }

            if ((3 <= a_NickStr.Length && a_NickStr.Length < 16) == false)
            {
                Message_Show("������ 3���� �̻� 15���� ���Ϸ� �ۼ��� �ּ���.");
                return;
            }

            //## ����

            PlayFabClientAPI.UpdateUserTitleDisplayName(
          new UpdateUserTitleDisplayNameRequest
          {
              DisplayName = a_NickStr
          },
          (result) =>
          {

              GlobalValue.g_NickName = result.DisplayName;

              if (DltMethod != null)
                  DltMethod();

              Time.timeScale = 1.0f;  //�Ͻ����� Ǯ���ֱ�
              Destroy(gameObject);


          },
          (error) =>
          {
              //Debug.Log(error.GenerateErrorReport());
              //## ���н� �޽��� ���
              string a_StrErr = error.GenerateErrorReport();
              if (a_StrErr.Contains("Name not available") == true)
              {
                  Message_Show("�̹� ������� �����Դϴ�.");
              }
              else
              {
                  Message_Show(a_StrErr);
              }
          }
            );






        }//if(NickInputField != null)
         //--- �г��� ���ΰ� �Ӹ����� ����


    }

    private void CloseBtnClick()
    {
        Time.timeScale = 1.0f;  //�Ͻ����� Ǯ���ֱ�
        Destroy(gameObject);
    }

    private void SoundOnOff(bool value) //üũ ���°� ����Ǿ��� �� ȣ��Ǵ� �Լ�
    {
        //--- üũ ���� ����
        //if (value == true)
        //    PlayerPrefs.SetInt("SoundOnOff", 1);
        //else
        //    PlayerPrefs.SetInt("SoundOnOff", 0);

        int a_IntV = (value == true) ? 1 : 0;
        PlayerPrefs.SetInt("SoundOnOff", a_IntV);

        Sound_Mgr.Inst.SoundOnOff(value);    //���� �� / ��
        //--- üũ ���� ����
    }

    private void SliderChanged(float value)
    { //value 0.0f ~ 1.0f �����̵� ���°� ���� �Ǿ��� �� ȣ��Ǵ� �Լ�
        PlayerPrefs.SetFloat("SoundVolume", value);
        Sound_Mgr.Inst.SoundVolume(value);
    }

    void Message_Show(string a_Msg = "", bool IsShow = true)
    {
        if (IsShow == true)
        {
            m_Message.text = a_Msg;
            m_Message.gameObject.SetActive(true);
            ShowTimer = 5.0f;
        }
        else
        {
            m_Message.text = "";
            m_Message.gameObject.SetActive(false);
        }
    }

}
