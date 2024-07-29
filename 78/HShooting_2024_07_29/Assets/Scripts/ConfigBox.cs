using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfigBox : MonoBehaviour
{
    public delegate void CFG_Response();    //<--- 델리게이트 데이터(옵션)형 하나 선언
    public CFG_Response DltMethod = null;   //<--- 델리게이트 변수 선언(소켓 역할)

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
        //체크 상태가 변경되었을 때 호출되는 함수를 대기하는 코드

        if (m_Sound_Slider != null)
            m_Sound_Slider.onValueChanged.AddListener(SliderChanged);
        //슬라이드 상태가 변경 되었을 때 호출되는 함수 대기하는 코드

        m_RefHero = FindObjectOfType<HeroCtrl>();
        //Hierarchy쪽에서 HeroCtrl 컴포넌트가 붙어있는 게임오브젝트를 찾아서 객체를 찾아오는 방법

        //--- 체크상태, 슬라이드상태, 닉네임 로딩 후 UI 컨트롤에 적용
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
            //## 로컬
            // NickInputField.text = PlayerPrefs.GetString("NickName", "SBS영웅");

            //## 서버
            NickInputField.text = GlobalValue.g_NickName;

        }
        //--- 체크상태, 슬라이드상태, 닉네임 로딩 후 UI 컨트롤에 적용

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
        //--- 닉네임 주인공 머리위에 적용
        if (NickInputField != null)
        {
            string a_NickStr = NickInputField.text;
            a_NickStr = a_NickStr.Trim();   //앞뒤 공백을 제거해 주는 함수
            if (string.IsNullOrEmpty(a_NickStr) == true)
            {
                Message_Show("별명은 빈칸 없이 입력해 주세요.");
                return;
            }

            if ((3 <= a_NickStr.Length && a_NickStr.Length < 16) == false)
            {
                Message_Show("별명은 3글자 이상 15글자 이하로 작성해 주세요.");
                return;
            }

            //## 서버

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

              Time.timeScale = 1.0f;  //일시정지 풀어주기
              Destroy(gameObject);


          },
          (error) =>
          {
              //Debug.Log(error.GenerateErrorReport());
              //## 실패시 메시지 출력
              string a_StrErr = error.GenerateErrorReport();
              if (a_StrErr.Contains("Name not available") == true)
              {
                  Message_Show("이미 사용중인 별명입니다.");
              }
              else
              {
                  Message_Show(a_StrErr);
              }
          }
            );






        }//if(NickInputField != null)
         //--- 닉네임 주인공 머리위에 적용


    }

    private void CloseBtnClick()
    {
        Time.timeScale = 1.0f;  //일시정지 풀어주기
        Destroy(gameObject);
    }

    private void SoundOnOff(bool value) //체크 상태가 변경되었을 때 호출되는 함수
    {
        //--- 체크 상태 저장
        //if (value == true)
        //    PlayerPrefs.SetInt("SoundOnOff", 1);
        //else
        //    PlayerPrefs.SetInt("SoundOnOff", 0);

        int a_IntV = (value == true) ? 1 : 0;
        PlayerPrefs.SetInt("SoundOnOff", a_IntV);

        Sound_Mgr.Inst.SoundOnOff(value);    //사운드 켜 / 꺼
        //--- 체크 상태 저장
    }

    private void SliderChanged(float value)
    { //value 0.0f ~ 1.0f 슬라이드 상태가 변경 되었을 때 호출되는 함수
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
