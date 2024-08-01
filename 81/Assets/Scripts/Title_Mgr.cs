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
    public InputField IdInputField;     //Email �� ���� ����
    public InputField PassInputField;
    public Button m_LoginBtn;
    public Button m_CreateAccOpenBtn;
    public Toggle m_IdRemember;

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

    bool invalidEmailType = false;       // �̸��� ������ �ùٸ��� üũ
    bool isValidFormat = false;          // �ùٸ� �������� �ƴ��� üũ

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

        // ��� ��ư�� ���¸� GlobalValue���� �ҷ���
        m_IdRemember.isOn = GlobalValue.g_IsIdRemember;

        // ��� ��ư�� ���� ������ ����� ���̵� �ҷ���
        if (m_IdRemember.isOn)
        {
            IdInputField.text = PlayerPrefs.GetString("SavedUserId", "");
        }
        else
        {
            IdInputField.text = "";
        }



        Sound_Mgr.Inst.PlayBGM("sound_bgm_title_001", 1.0f);
        Sound_Mgr.Inst.m_AudioSrc.clip = null;  //����� �÷��� ����
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
        //Debug.Log("��ư�� Ŭ�� �߾��.");
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
            MessageOnOff("Id, Pw ��ĭ ���� �Է��� �ּ���.");
        }

        if (!(6 <= a_IdStr.Length && a_IdStr.Length <= 20))  // 6 ~ 20
        {
            MessageOnOff("Id�� 6���ں��� 20���ڱ��� �ۼ��� �ּ���.");
            return;
        }

        if (!(6 <= a_PwStr.Length && a_PwStr.Length <= 20))
        {
            MessageOnOff("��й�ȣ�� 6���ں��� 20���ڱ��� �ۼ��� �ּ���.");
            return;
        }

        if (!CheckEmailAddress(a_IdStr))
        {
            MessageOnOff("Email ������ ���� �ʽ��ϴ�.");
            return;
        }

        //--- �α��� ������ � ���� ������ ���������� �����ϴ� �ɼ� ��ü ����
        var option = new GetPlayerCombinedInfoRequestParams()
        {
            //--- DisplayName(�г���)�� �������� ���� �ɼ�
            GetPlayerProfile = true,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true,  //DisplayName(�г���) �������� ���� ��û �ɼ�
            },
            //--- DisplayName(�г���)�� �������� ���� �ɼ�

            GetPlayerStatistics = true,
            //--- �� �ɼ����� ��谪(����ǥ�� �����ϴ�)�� �ҷ��� �� �ִ�.
            GetUserData = true
            //--- �� �ɼ����� < �÷��̾� ������(Ÿ��Ʋ) > ���� �ҷ��� �� �ִ�.
        };

        var request = new LoginWithEmailAddressRequest()
        {
            Email = a_IdStr,
            Password = a_PwStr,
            InfoRequestParameters = option
        };

        PlayFabClientAPI.LoginWithEmailAddress(request,
                                    OnLoginSuccess, OnLoginFailure);


        // ��� ��ư�� ���¿� ���� ���̵� �����ϰų� ����
        if (m_IdRemember.isOn)
        {
            PlayerPrefs.SetString("SavedUserId", IdInputField.text);
        }
        else
        {
            PlayerPrefs.DeleteKey("SavedUserId");
        }

        // ��� ��ư�� ���¸� GlobalValue�� ����
        GlobalValue.g_IsIdRemember = m_IdRemember.isOn;

    }

    void OnLoginSuccess(LoginResult result)
    {
        MessageOnOff("�α��� ����");

        GlobalValue.g_Unique_ID = result.PlayFabId;

        if (result.InfoResultPayload != null)
        {
            GlobalValue.g_NickName = result.InfoResultPayload.PlayerProfile.DisplayName;
            //�г��� ��������

            //--- ���� ��谪(����ǥ�� �����ϴ�) �ҷ����� :
            //�ɼ� ������ ���� LoginWithEmailAdress()�����ε�
            //���� ��谪(����ǥ�� �����ϴ�)�� �ҷ��� �� �ִ�.
            foreach (var eachStat in result.InfoResultPayload.PlayerStatistics)
            {
                if (eachStat.StatisticName == "BestScore")
                {
                    GlobalValue.g_BestScore = eachStat.Value;
                }
            }
            //--- ���� ��谪(����ǥ�� �����ϴ�) �ҷ����� :

            //--- < �÷��̾� ������(Ÿ��Ʋ) > �� �޾ƿ���


            int a_GetValue = 0;
            int Idx = 0;
            foreach (var eachData in result.InfoResultPayload.UserData)
            {
                if (eachData.Key == "UserGold")
                {
                    if (int.TryParse(eachData.Value.Value, out a_GetValue) == true)
                        GlobalValue.g_UserGold = a_GetValue;
                }
                else if (eachData.Key.Contains("Skill_Item") == true)
                {
                    bool a_IsDiff = false;
                    Idx = 0;
                    string[] StrArr = eachData.Key.Split('_');
                    if (StrArr.Length >= 3)
                    {
                        if (int.TryParse(StrArr[2], out Idx) == false)
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
                        MessageOnOff("��ų ������ ������ �ҷ����� ����");
                        continue;
                    }

                    GlobalValue.g_CurSkillCount[Idx] = a_GetValue;
                }

            }//foreach( var eachData in result.InfoResultPayload.UserData)
            //--- < �÷��̾� ������(Ÿ��Ʋ) >�� �޾ƿ���

        }//if (result.InfoResultPayload != null)

        //Debug.Log("��ư�� Ŭ�� �߾��.");
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
            MessageOnOff("�α��� ���� : �ش� Id�� �������� �ʽ��ϴ�.");
        }
        else if (error.GenerateErrorReport().Contains("Invalid email address or password") == true)
        {
            MessageOnOff("�α��� ���� : �н����尡 ��ġ���� �ʽ��ϴ�.");
        }
        else
        {
            MessageOnOff("�α��� ���� : " + error.GenerateErrorReport());
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
    }

    void CreateAccountBtn() //���� ���� ��û �Լ�
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
            MessageOnOff("Id, Pw, ������ ��ĭ ���� �Է��� �ּ���.");
            return;
        }

        if (!(6 <= a_IdStr.Length && a_IdStr.Length <= 20))  // 6 ~ 20
        {
            MessageOnOff("Id�� 6���ں��� 20���ڱ��� �ۼ��� �ּ���.");
            return;
        }

        if (!(6 <= a_PwStr.Length && a_PwStr.Length <= 20))
        {
            MessageOnOff("��й�ȣ�� 6���ں��� 20���ڱ��� �ۼ��� �ּ���.");
            return;
        }

        if (!(3 <= a_NickStr.Length && a_NickStr.Length <= 20))
        {
            MessageOnOff("������ 3���ں��� 20���ڱ��� �ۼ��� �ּ���.");
            return;
        }

        if (!CheckEmailAddress(a_IdStr))
        {
            MessageOnOff("Email ������ ���� �ʽ��ϴ�.");
            return;
        }

        MessageOnOff("ȸ������ ��..."); // ȸ������ �� �޽��� ���

        var request = new RegisterPlayFabUserRequest()
        {
            Email = a_IdStr,
            Password = a_PwStr,
            DisplayName = a_NickStr,

            RequireBothUsernameAndEmail = false //Email �� �⺻ Id�� ����ϰڴٴ� �ɼ�
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, RegisterSuccess, RegisterFailure);

    }

    void RegisterSuccess(RegisterPlayFabUserResult result)
    {
        MessageOnOff("ȸ������ ����!"); // ȸ������ ���� �޽��� ���
        IdInputField.text = New_IdInputField.text; // ID ����
        //PassInputField.text = New_PassInputField.text; // ��й�ȣ ����

        // ��� ��ų �������� 1���� ����
        Dictionary<string, string> skillItems = new Dictionary<string, string>();
        for (int i = 0; i < GlobalValue.g_CurSkillCount.Count; i++)
        {
            skillItems.Add($"Skill_Item_{i}", "1");
        }

        var request = new UpdateUserDataRequest()
        {
            Data = skillItems
        };

        PlayFabClientAPI.UpdateUserData(request,
            (result) =>
            {
                // �α��� �гη� ��ȯ
                m_CreateAccPanel.SetActive(false);
                m_LoginPanel.SetActive(true);

                // �Է� �ʵ� ���� ó��
                New_IdInputField.text = "";
                New_PassInputField.text = "";
                New_NickInputField.text = "";
            },
            (error) =>
            {
                MessageOnOff("��ų ������ ���� ����: " + error.GenerateErrorReport());
            });
    }
    void RegisterFailure(PlayFabError error)
    {
        if (error.GenerateErrorReport().Contains("Email address already exists") == true)
        {
            MessageOnOff("���� ���� : " + "�̹� �����ϴ� Id �Դϴ�.");
        }
        else if (error.GenerateErrorReport().Contains("The display name entered is not available") == true)
        {
            MessageOnOff("���� ���� : " + "�̹� �����ϴ� ���� �Դϴ�.");
        }
        else
        {
            MessageOnOff("���� ���� : " + error.GenerateErrorReport());
        }
    }

    //----------------- �̸��������� �´��� Ȯ���ϴ� ��� ��ũ��Ʈ
    //https://blog.naver.com/rlawndks4204/221591566567
    // <summary>
    /// �ùٸ� �̸������� üũ.
    /// </summary>
    private bool CheckEmailAddress(string EmailStr)
    {
        if (string.IsNullOrEmpty(EmailStr)) isValidFormat = false;

        EmailStr = Regex.Replace(EmailStr, @"(@)(.+)$", this.DomainMapper, RegexOptions.None);
        if (invalidEmailType) isValidFormat = false;

        // true �� ��ȯ�� ��, �ùٸ� �̸��� ������.
        isValidFormat = Regex.IsMatch(EmailStr,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase);
        return isValidFormat;
    }

    /// <summary>
    /// ���������� ��������.
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
    //----------------- �̸��������� �´��� Ȯ���ϴ� ��� ��ũ��Ʈ

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
