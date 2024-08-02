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
using SimpleJSON;

public class Title_Mgr : MonoBehaviour
{
    public Button StartBtn;

    [Header("LoginPanel")]
    public GameObject m_LoginPanel;
    public InputField IdInputField;     //Email 로 받을 것임
    public InputField PassInputField;
    public Button m_LoginBtn;
    public Button m_CreateAccOpenBtn;
    public Toggle m_SaveIdToggle;

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


    string m_SvIdStr = "";
    string m_SvNewIdStr = "";
    string m_SvNewPwStr = "";


    // Start is called before the first frame update
    void Start()
    {
        GlobalValue.LoadGameData();

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

        string a_StrId = PlayerPrefs.GetString("MySaveId", "");
        if (PlayerPrefs.HasKey("MySaveId") == false || a_StrId == "")
        {
            m_SaveIdToggle.isOn = false;
        }
        else
        {
            m_SaveIdToggle.isOn = true;
            IdInputField.text = a_StrId;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if (ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false);
            }
        }
    }//void Update()

    void StartClick()
    {
        //Debug.Log("버튼을 클릭 했어요.");
        bool IsFadeOk = false;
        if (Fade_Mgr.Inst != null)
            IsFadeOk = Fade_Mgr.Inst.SceneOutReserve("LobbyScene");
        if (IsFadeOk == false)
            SceneManager.LoadScene("LobbyScene");

        Sound_Mgr.Inst.PlayGUISound("Pop", 1.0f);
    }

    void LoginBtn()
    {
        string a_IdStr = IdInputField.text;
        string a_PwStr = PassInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();

        if (string.IsNullOrEmpty(a_IdStr) == true ||
           string.IsNullOrEmpty(a_PwStr) == true)
        {
            MessageOnOff("Id, Pw 빈칸 없이 입력해 주세요.");
        }

        if (!(6 <= a_IdStr.Length && a_IdStr.Length <= 20))  // 6 ~ 20
        {
            MessageOnOff("Id는 6글자부터 20글자까지 작성해 주세요.");
            return;
        }

        if (!(6 <= a_PwStr.Length && a_PwStr.Length <= 20))
        {
            MessageOnOff("비밀번호는 6글자부터 20글자까지 작성해 주세요.");
            return;
        }

        if (!CheckEmailAddress(a_IdStr))
        {
            MessageOnOff("Email 형식이 맞지 않습니다.");
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
                ShowAvatarUrl = true,   //AvatarUrl(아바타 이미지) 가져오기 위한 요청 옵션
            },
            //--- DisplayName(닉네임)을 가져오기 위한 옵션

            GetPlayerStatistics = true,
            //--- 이 옵션으로 통계값(순위표에 관여하는)을 불러올 수 있다.
            GetUserData = true
            //--- 이 옵션으로 < 플레이어 테이터(타이틀) > 값을 불러올 수 있다.
        };

        m_SvIdStr = a_IdStr;


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
        MessageOnOff("로그인 성공");

        GlobalValue.g_Unique_ID = result.PlayFabId;

        if (result.InfoResultPayload != null)
        {
            GlobalValue.g_NickName = result.InfoResultPayload.PlayerProfile.DisplayName;
            //닉네임 가져오기

            string a_AvatarUrl = result.InfoResultPayload.PlayerProfile.AvatarUrl;
            //아바타 이미지 가져오기

            //Json 파싱
            if (string.IsNullOrEmpty(a_AvatarUrl)== false && a_AvatarUrl.Contains("{\"") == true)
            {
                JSONNode a_ParseJs = JSON.Parse(a_AvatarUrl);

                if (a_ParseJs["UserExp"] != null)
                {
                    GlobalValue.g_Exp = a_ParseJs["UserExp"].AsInt;
                    Debug.Log("경험치 : " + GlobalValue.g_Exp);
                }
                if (a_ParseJs["UserLevel"] != null)
                {
                    GlobalValue.g_Level = a_ParseJs["UserLevel"].AsInt;
                    Debug.Log("레벨 : " + GlobalValue.g_Level);
                }

            }

            //--- 유저 통계값(순위표에 관여하는) 불러오기 :
            //옵션 설정에 의해 LoginWithEmailAdress()만으로도
            //유저 통계값(순위표에 관여하는)을 불러올 수 있다.
            foreach (var eachStat in result.InfoResultPayload.PlayerStatistics)
            {
                if (eachStat.StatisticName == "BestScore")
                {
                    GlobalValue.g_BestScore = eachStat.Value;
                }
            }
            //--- 유저 통계값(순위표에 관여하는) 불러오기 :

            //--- < 플레이어 데이터(타이틀) > 값 받아오기
            int a_GetValue = 0;
            int Idx = 0;
            foreach (var eachData in result.InfoResultPayload.UserData)
            {
                if (eachData.Key == "UserGold")
                {
                    if (int.TryParse(eachData.Value.Value, out a_GetValue) == true)
                        GlobalValue.g_UserGold = a_GetValue;
                }
                else if (eachData.Key.Contains("Skill_Item_") == true)
                {
                    bool a_IsDiff = false; //IsDifferent

                    //"Skill_Item_1"
                    //string[] strArr = { "Skill", "Item", "1" };

                    Idx = 0;
                    string[] strArr = eachData.Key.Split('_');
                    if (3 <= strArr.Length)
                    {
                        if (int.TryParse(strArr[2], out Idx) == false)
                            a_IsDiff = true;
                    }
                    else
                        a_IsDiff = true;

                    if (GlobalValue.g_CurSkillCount.Count <= Idx)
                        a_IsDiff = true;

                    if (int.TryParse(eachData.Value.Value, out a_GetValue) == false)
                        a_IsDiff = true;

                    if (a_IsDiff == true)
                    {
                        MessageOnOff("아이템 정보 파싱 실패");
                        continue;
                    }

                    GlobalValue.g_CurSkillCount[Idx] = a_GetValue;
                }
            }//foreach( var eachData in result.InfoResultPayload.UserData)
            //--- < 플레이어 데이터(타이틀) >값 받아오기

        }//if (result.InfoResultPayload != null)

        //## 로그인 성공 후 체크 버튼 상태에 따른 저장/제거
        if (m_SaveIdToggle.isOn == true)
        {
            PlayerPrefs.SetString("MySaveId", m_SvIdStr);
        }
        else
        {
            PlayerPrefs.DeleteKey("MySaveId");
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
        if (error.GenerateErrorReport().Contains("User not found") == true)
        {
            MessageOnOff("로그인 실패 : 해당 Id가 존재하지 않습니다.");
        }
        else if (error.GenerateErrorReport().Contains("Invalid email address or password") == true)
        {
            MessageOnOff("로그인 실패 : 패스워드가 일치하지 않습니다.");
        }
        else
        {
            MessageOnOff("로그인 실패 : " + error.GenerateErrorReport());
        }
    }

    void OpenCreateAccBtn()
    {
        if (m_LoginPanel != null)
            m_LoginPanel.SetActive(false);

        if (m_CreateAccPanel != null)
            m_CreateAccPanel.SetActive(true);
    }

    void CreateCancelBtn()
    {
        if (m_LoginPanel != null)
            m_LoginPanel.SetActive(true);

        if (m_CreateAccPanel != null)
            m_CreateAccPanel.SetActive(false);

        New_IdInputField.text = "";
        New_PassInputField.text = "";
        New_NickInputField.text = "";

    }

    void CreateAccountBtn() //계정 생성 요청 함수
    {
        string a_IdStr = New_IdInputField.text;
        string a_PwStr = New_PassInputField.text;
        string a_NickStr = New_NickInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();
        a_NickStr = a_NickStr.Trim();

        if (string.IsNullOrEmpty(a_IdStr) == true ||
           string.IsNullOrEmpty(a_PwStr) == true ||
           string.IsNullOrEmpty(a_NickStr) == true)
        {
            MessageOnOff("Id, Pw, 별명은 빈칸 없이 입력해 주세요.");
            return;
        }

        if (!(6 <= a_IdStr.Length && a_IdStr.Length <= 20))  // 6 ~ 20
        {
            MessageOnOff("Id는 6글자부터 20글자까지 작성해 주세요.");
            return;
        }

        if (!(6 <= a_PwStr.Length && a_PwStr.Length <= 20))
        {
            MessageOnOff("비밀번호는 6글자부터 20글자까지 작성해 주세요.");
            return;
        }

        if (!(3 <= a_NickStr.Length && a_NickStr.Length <= 20))
        {
            MessageOnOff("별명은 3글자부터 20글자까지 작성해 주세요.");
            return;
        }

        if (!CheckEmailAddress(a_IdStr))
        {
            MessageOnOff("Email 형식이 맞지 않습니다.");
            return;
        }

        m_SvNewIdStr = a_IdStr;
        m_SvNewPwStr = a_PwStr;


        var request = new RegisterPlayFabUserRequest()
        {
            Email = a_IdStr,
            Password = a_PwStr,
            DisplayName = a_NickStr,

            RequireBothUsernameAndEmail = false //Email 을 기본 Id로 사용하겠다는 옵션
        };

        MessageOnOff("가입중...");

        ShowMsTimer = 300.0f;

        PlayFabClientAPI.RegisterPlayFabUser(request, RegisterSuccess, RegisterFailure);

    }

    void RegisterSuccess(RegisterPlayFabUserResult result)
    {
        //MessageOnOff("가입 성공");

        Invoke("ExpSkil", 0.3f);
    }


    //# 체험스킬 주기
    void ExpSkil()
    {
        Dictionary<string, string> a_ItemList = new Dictionary<string, string>();
        for (int i = 0; i < GlobalValue.g_CurSkillCount.Count; i++)
        {
            a_ItemList.Add($"Skill_Item_{i}", (1).ToString());

        }

        var request = new UpdateUserDataRequest()
        {
            Data = a_ItemList
        };

        PlayFabClientAPI.UpdateUserData(request,
                           (result) =>
                           {
                               //Debug.Log("데이터 저장 성공");
                               MessageOnOff("가입성공");

                               IdInputField.text = m_SvNewIdStr;
                               PassInputField.text = m_SvNewPwStr;
                           },
                            (error) =>
                            {
                                //Debug.Log("데이터 저장 실패");
                                MessageOnOff("가입 실패 : 충전 실패 "+ error.GenerateErrorReport());
                            }
                         );
    }


    void RegisterFailure(PlayFabError error)
    {
        if (error.GenerateErrorReport().Contains("Email address already exists") == true)
        {
            MessageOnOff("가입 실패 : " + "이미 존재하는 Id 입니다.");
        }
        else if (error.GenerateErrorReport().Contains("The display name entered is not available") == true)
        {
            MessageOnOff("가입 실패 : " + "이미 존재하는 별명 입니다.");
        }
        else
        {
            MessageOnOff("가입 실패 : " + error.GenerateErrorReport());
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

    void MessageOnOff(string Mess = "", bool isOn = true)
    {
        if (isOn == true)
        {
            MessageText.text = Mess;
            MessageText.gameObject.SetActive(true);
            ShowMsTimer = 7.0f;
        }
        else
        {
            MessageText.text = "";
            MessageText.gameObject.SetActive(false);
        }
    }
}
