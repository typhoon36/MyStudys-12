using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class Title_Mgr : MonoBehaviour
{
    public Button StartBtn;

    [Header("LoginPanel")]
    public GameObject m_LoginPanel;
    public InputField IdInputField;     //Email 로 받을 것임
    public InputField PassInputField;
    public Button m_LoginBtn;
    public Button m_CreateAccOpenBtn;

    [Header("CreateAccountPanel")]
    public GameObject m_CreateAccPanel;
    public InputField New_IdInputField;
    public InputField New_PassInputField;
    public InputField New_NickInputField;
    public Button m_CreateAccountBtn;
    public Button m_CancelBtn;

    [Header("Normal")]
    public Text MessageText;
    float ShowMsTimer = 0.0f;

    bool invalidEmailType = false;       // 이메일 포맷이 올바른지 체크
    bool isValidFormat = false;          // 올바른 형식인지 아닌지 체크

    // Start is called before the first frame update
    void Start()
    {
        StartBtn.onClick.AddListener(StartClick);

        //--- LoginPanel
        if (m_LoginBtn != null)
            m_LoginBtn.onClick.AddListener(LoginBtn);

        if (m_CreateAccOpenBtn != null)
            m_CreateAccOpenBtn.onClick.AddListener(OpenCreateAccBtn);

        //--- CreateAccountPanel
        if (m_CancelBtn != null)
            m_CancelBtn.onClick.AddListener(CreateCancelBtn);

        if (m_CreateAccountBtn != null)
            m_CreateAccountBtn.onClick.AddListener(CreateAccountBtn);

        Sound_Mgr.Inst.PlayBGM("sound_bgm_title_001", 1.0f);
        Sound_Mgr.Inst.m_AudioSrc.clip = null;  //배경음 플레이 끄기
    }

    //// Update is called once per frame
    void Update()
    {
        if(0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if(ShowMsTimer <= 0.0f)
            {
                Message("", false);
            }
        }
    }

    void StartClick()
    {
        bool IsFadeOk = false;
        if (Fade_Mgr.Inst != null)
            IsFadeOk = Fade_Mgr.Inst.SceneOutReserve("LobbyScene");
        if(IsFadeOk == false)
            SceneManager.LoadScene("LobbyScene");

        Sound_Mgr.Inst.PlayGUISound("Pop", 1.0f);
    }

    void LoginBtn()
    {
        string a_IdStr = IdInputField.text;
        string a_PwStr = PassInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();

        if(string.IsNullOrEmpty(a_IdStr) == true ||
           string.IsNullOrEmpty(a_PwStr) == true)
        {
            Message("Id, Pw 빈칸 없이 입력해 주세요.");
        }

        if (!(6 <= a_IdStr.Length && a_IdStr.Length <= 20))  // 6 ~ 20
        {
            Message("Id는 6글자부터 20글자까지 작성해 주세요.");
            return;
        }

        if (!(6 <= a_PwStr.Length && a_PwStr.Length <= 20))
        {
            Message("비밀번호는 6글자부터 20글자까지 작성해 주세요.");
            return;
        }

        if(!CheckEmailAddress(a_IdStr))
        {
            Message("Email 형식이 맞지 않습니다.");
            return;
        }

        //--- 로그인 성공시 어떤 유저 정보를 가져올지를 설정하는 옵션 객체 생성
        var option = new GetPlayerCombinedInfoRequestParams()
        {
            //--- DisplayName(닉네임)을 가져오기 위한 옵션
            GetPlayerProfile = true,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true,  //DisplayName(닉네임) 가져오기 위한 요청 옵션
            },

            //--- DisplayName(닉네임)을 가져오기 위한 옵션

            GetPlayerStatistics = true, //통계 데이터 가져오기



            //--- UserData(사용자 데이터)를 가져오기 위한 옵션
            GetUserData = true 
        };

        var request = new LoginWithEmailAddressRequest()
        {
            Email = a_IdStr,
            Password = a_PwStr,
            InfoRequestParameters = option
        };

        PlayFabClientAPI.LoginWithEmailAddress(request,
                                    OnLoginSuccess, OnLoginFailure);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Message("로그인 성공");

        GlobalValue.g_Unique_ID = result.PlayFabId;



        if (result.InfoResultPayload != null)
        {
            GlobalValue.g_NickName = result.InfoResultPayload.PlayerProfile.DisplayName;
            //닉네임 가져오기

            //## 플레이어 데이터 값 가져오기 ///보안성에서 좋지않은 방법이지만 일단은 이렇게 사용
            //--> 서버에 10골드를 넘겨주면 알아서 증가(100 -> 110으로 계산한다음 갱신인데
            //플레이팹은 서버에 전달할때는 갱신할때값을 전달이 아닌 증가되는 값을 전달.하지만 예외처리가 많아서 쉬운 방법으로 사용)
            
            //## 플레이어 통계 가져오기
            // 옵션 설정에 의해  LoginWithEmailAddress()만으로도  유저 통계값을 불러올수있음.
            foreach(var eachStat in result.InfoResultPayload.PlayerStatistics)
            {
                if(eachStat.StatisticName == "BestScore")
                {
                    GlobalValue.g_BestScore = eachStat.Value;
                }

                Debug.Log("통계 이름 : " + eachStat.StatisticName + " / 통계 값 : " + eachStat.Value);
                
            }


            int a_GetVal = 0;
            int Idx = 0;

            foreach(var eachData in result.InfoResultPayload.UserData)
            {
                if(eachData.Key == "UserGold")
                {
                    if(int.TryParse(eachData.Value.Value, out a_GetVal) == true)
                        GlobalValue.g_UserGold = a_GetVal;
                    
                }
            }
        }



        //Debug.Log("버튼을 클릭 했어요.");
        bool IsFadeOk = false;
        if (Fade_Mgr.Inst != null)
            IsFadeOk = Fade_Mgr.Inst.SceneOutReserve("LobbyScene");
        if (IsFadeOk == false)
            SceneManager.LoadScene("LobbyScene");
    }

    void OnLoginFailure(PlayFabError error)
    {
        if(error.GenerateErrorReport().Contains("User not found") == true)
        {
            Message("로그인 실패 : 해당 Id가 존재하지 않습니다.");
        }
        else if(error.GenerateErrorReport().Contains("Invalid email address or password") == true)
        {
            Message("로그인 실패 : 패스워드가 일치하지 않습니다.");
        }
        else
        {
            Message("로그인 실패 : " + error.GenerateErrorReport());
        }
    }

    void OpenCreateAccBtn()
    {
        if (m_LoginPanel != null)
            m_LoginPanel.SetActive(false);

        if(m_CreateAccPanel != null)
            m_CreateAccPanel.SetActive(true);   
    }

    void CreateCancelBtn()
    {
        if(m_LoginPanel != null)
            m_LoginPanel.SetActive(true);

        if (m_CreateAccPanel != null)
            m_CreateAccPanel.SetActive(false);
    }

    void CreateAccountBtn() //계정 생성 요청 함수
    {
        string a_IdStr = New_IdInputField.text;
        string a_PwStr = New_PassInputField.text;
        string a_NickStr = New_NickInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();
        a_NickStr = a_NickStr.Trim();

        if(string.IsNullOrEmpty(a_IdStr) == true ||
           string.IsNullOrEmpty(a_PwStr) == true ||
           string.IsNullOrEmpty(a_NickStr) == true)
        {
            Message("Id, Pw, 별명은 빈칸 없이 입력해 주세요.");
            return;
        }

        if( !(6 <= a_IdStr.Length && a_IdStr.Length <= 20) )  // 6 ~ 20
        {
            Message("Id는 6글자부터 20글자까지 작성해 주세요.");
            return;
        }

        if( !(6 <= a_PwStr.Length && a_PwStr.Length <= 20) )
        {
            Message("비밀번호는 6글자부터 20글자까지 작성해 주세요.");
            return;
        }

        if( !(3 <= a_NickStr.Length && a_NickStr.Length <= 20) )
        {
            Message("별명은 3글자부터 20글자까지 작성해 주세요.");
            return;
        }

        if(!CheckEmailAddress(a_IdStr))
        {
            Message("Email 형식이 맞지 않습니다.");
            return;
        }

        var request = new RegisterPlayFabUserRequest()
        {
            Email = a_IdStr,
            Password = a_PwStr,
            DisplayName = a_NickStr,

            RequireBothUsernameAndEmail = false //Email 을 기본 Id로 사용하겠다는 옵션
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, RegisterSuccess, RegisterFailure);

    }

    void RegisterSuccess(RegisterPlayFabUserResult result)
    {
        Message("가입 성공");
    }

    void RegisterFailure(PlayFabError error)
    {
        if (error.GenerateErrorReport().Contains("Email address already exists") == true)
        {
            Message("가입 실패 : " + "이미 존재하는 Id 입니다.");
        }
        else if (error.GenerateErrorReport().Contains("The display name entered is not available") == true)
        {
            Message("가입 실패 : " + "이미 존재하는 별명 입니다.");
        }
        else
        {
            Message("가입 실패 : " + error.GenerateErrorReport());
        }
    }

    //----------------- 이메일형식이 맞는지 확인하는 방법 스크립트
    //https://blog.naver.com/rlawndks4204/221591566567
    // <summary>
    /// 올바른 이메일인지 체크.
    /// </summary>
    private bool CheckEmailAddress(string EmailStr)
    {
        if (string.IsNullOrEmpty(EmailStr)) isValidFormat = false;

        EmailStr = Regex.Replace(EmailStr, @"(@)(.+)$", this.DomainMapper, RegexOptions.None);
        if (invalidEmailType) isValidFormat = false;

        // true 로 반환할 시, 올바른 이메일 포맷임.
        isValidFormat = Regex.IsMatch(EmailStr,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase);
        return isValidFormat;
    }

    /// <summary>
    /// 도메인으로 변경해줌.
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    private string DomainMapper(Match match)
    {
        // IdnMapping class with default property values.
        IdnMapping idn = new IdnMapping();

        string domainName = match.Groups[2].Value;
        try
        {
            domainName = idn.GetAscii(domainName);
        }
        catch (ArgumentException)
        {
            invalidEmailType = true;
        }
        return match.Groups[1].Value + domainName;
    }
    //----------------- 이메일형식이 맞는지 확인하는 방법 스크립트

    #region # 메시지 출력
    void Message(string msg = "" , bool IsShow = true)
    {
        if(IsShow == true)
        {
            MessageText.text = msg;
            ShowMsTimer = 7.0f;
            MessageText.gameObject.SetActive(true);
        }
        else
        {
            MessageText.text = "";
            MessageText.gameObject.SetActive(false);
        }
    }



    #endregion


}
