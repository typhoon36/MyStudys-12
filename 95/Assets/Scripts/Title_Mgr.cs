using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Globalization;
using System.Text.RegularExpressions;
using System;

[System.Serializable]
public class PlayerExpLv
{
    public int UserExp;
    public int UserLv;
}

public class Title_Mgr : MonoBehaviour
{
    public Button m_StartBtn;
    public Button m_ExitBtn;

    [Header("LoginPanel")]
    public GameObject m_LoginPanel;
    public InputField IdInputField;     //Email �� ���� ����
    public InputField PassInputField;
    public Button m_LoginBtn;
    public Button m_CreateAccOpenBtn;
    public Toggle SaveIdToggle;

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

    string m_SvIdStr = "";
    string m_SvNewIdStr = "";
    string m_SvNewPwStr = "";

    // Start is called before the first frame update
    void Start()
    {
        if (m_StartBtn != null)
            m_StartBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("LobbyScene");
            });

        if (m_ExitBtn != null)
            m_ExitBtn.onClick.AddListener(() => 
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

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

        string a_strId = PlayerPrefs.GetString("MySave_Id", "");
        if (PlayerPrefs.HasKey("MySave_Id") == false || a_strId == "")
        {
            SaveIdToggle.isOn = false;
        }
        else
        {
            SaveIdToggle.isOn = true;
            IdInputField.text = a_strId;
        }

    }//void Start()

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
                ShowDisplayName = true,  //DisplayName(�г��� �������� ���� ��û �ɼ�
                ShowAvatarUrl = true     //�ƹ� URL�� �������� �ɼ�
            },
            //--- DisplayName(�г���)�� �������� ���� �ɼ�

            GetPlayerStatistics = true,
            //--- �� �ɼ����� ��谪(����ǥ�� �����ϴ�)�� �ҷ��� �� �ִ�.
            GetUserData = true
            //--- �� �ɼ����� < �÷��̾� ������(Ÿ��Ʋ) > ���� �ҷ��� �� �ִ�.
        };

        m_SvIdStr = a_IdStr;    //�α��� ���̵� ����

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
        MessageOnOff("�α��� ����");

        GlobalValue.g_Unique_ID = result.PlayFabId;

        if (result.InfoResultPayload != null)
        {
            GlobalValue.g_NickName = result.InfoResultPayload.PlayerProfile.DisplayName;
            //�г��� ��������

            //--- ����ġ ��������
            string a_AvatarUrl = result.InfoResultPayload.PlayerProfile.AvatarUrl;
            //--- JSON�Ľ�
            if (string.IsNullOrEmpty(a_AvatarUrl) == false &&
                a_AvatarUrl.Contains("{\"") == true) //JSON �������� Ȯ���ϴ� �ڵ�
            {
                // JSON ���ڿ��� PlayerExpLv ��ü�� ��ȯ
                PlayerExpLv PExpLv = JsonUtility.FromJson<PlayerExpLv>(a_AvatarUrl);
                if(PExpLv != null)
                {
                    //����ġȭ ���� ����
                    GlobalValue.g_Exp   = PExpLv.UserExp;
                    GlobalValue.g_Level = PExpLv.UserLv;
                }
            }//if(string.IsNullOrEmpty(a_AvatarUrl) == false &&
            //--- ����ġ ��������

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
                else if (eachData.Key.Contains("SkItem_") == true)
                {
                    bool a_IsDiff = false; //IsDifferent

                    //"Skill_Item_1"
                    //string[] strArr = { "Skill", "Item", "1" };

                    Idx = 0;
                    string[] strArr = eachData.Key.Split('_');
                    if (2 <= strArr.Length)
                    {
                        if (int.TryParse(strArr[1], out Idx) == false)
                            a_IsDiff = true;
                    }
                    else
                        a_IsDiff = true;

                    if (GlobalValue.g_SkillCount.Length <= Idx)
                        a_IsDiff = true;

                    if (int.TryParse(eachData.Value.Value, out a_GetValue) == false)
                        a_IsDiff = true;

                    if (a_IsDiff == true)
                    {
                        MessageOnOff("������ ���� �Ľ� ����");
                        continue;
                    }

                    GlobalValue.g_SkillCount[Idx] = a_GetValue;
                }
            }//foreach( var eachData in result.InfoResultPayload.UserData)
            //--- < �÷��̾� ������(Ÿ��Ʋ) >�� �޾ƿ���

        }//if (result.InfoResultPayload != null)

        //�α��� �����ÿ� ...
        if (SaveIdToggle.isOn == true)  //üũ ��ư�� ���� ������
        {
            PlayerPrefs.SetString("MySave_Id", m_SvIdStr);
        }
        else  //üũ ��ư�� ���� ������
        {
            PlayerPrefs.DeleteKey("MySave_Id");
        }

        SceneManager.LoadScene("LobbyScene");

    }//void OnLoginSuccess(LoginResult result)

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

        New_IdInputField.text = "";
        New_PassInputField.text = "";
        New_NickInputField.text = "";
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

        m_SvNewIdStr = a_IdStr;
        m_SvNewPwStr = a_PwStr;

        var request = new RegisterPlayFabUserRequest()
        {
            Email = a_IdStr,
            Password = a_PwStr,
            DisplayName = a_NickStr,

            RequireBothUsernameAndEmail = false //Email �� �⺻ Id�� ����ϰڴٴ� �ɼ�
        };

        MessageOnOff("ȸ�� ���� ��... ��ø� ��ٷ� �ּ���.");
        ShowMsTimer = 300.0f;

        PlayFabClientAPI.RegisterPlayFabUser(request, RegisterSuccess, RegisterFailure);
    }

    void RegisterSuccess(RegisterPlayFabUserResult result)
    {
        //MessageOnOff("���� ����");
        Invoke("ExpSkillItem", 0.3f);
    }

    void ExpSkillItem()  //ü�� ��ų ������ ���� �ϱ� Experience
    {
        Dictionary<string, string> a_ItemList = new Dictionary<string, string>();        
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)   
        {
            a_ItemList.Add($"SkItem_{i}", (1).ToString());
        }

        var request = new UpdateUserDataRequest()
        {
            Data = a_ItemList
        };

        PlayFabClientAPI.UpdateUserData(request,
                (result) =>
                {
                    //Debug.Log("������ ���� ����");
                    MessageOnOff("���� ����");

                    IdInputField.text = m_SvNewIdStr;
                    PassInputField.text = m_SvNewPwStr;
                },
                (error) =>
                {
                    //Debug.Log("������ ���� ����");
                    MessageOnOff("���� ���� : ü�� ��ų ���� ���� : " + error.GenerateErrorReport());
                }
        );

    }//void ExpSkillItem()  //ü�� ��ų ������ ���� �ϱ� Experience

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
