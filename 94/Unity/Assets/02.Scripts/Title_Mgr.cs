using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Title_Mgr : MonoBehaviour
{
    [Header("--- LoginPanel ---")]
    public GameObject m_LoginPanelObj;
    public Button m_LoginBtn = null;
    public Button m_CreateAccOpenBtn = null;
    public InputField IDInputField;
    public InputField PWInputField;

    [Header("--- CreateAccountPanel ---")]
    public GameObject m_CreateAccountPanelObj;
    public InputField New_IDInputField;
    public InputField New_PWInputField;
    public InputField New_NickInputField;
    public Button m_CreateAccBtn = null;
    public Button m_CancleBtn = null;

    [Header("Message")]
    public Text MessageTxt;
    float ShowTime = 0.0f;

    string LoginUrl = "";
    string CreateUrl = "";



    // Start is called before the first frame update
    void Start()
    {
        GlobalValue.LoadGameData();
        //--- LoginPanel
        if (m_LoginBtn != null)
            m_LoginBtn.onClick.AddListener(LoginBtnClick);

        if (m_CreateAccOpenBtn != null)
            m_CreateAccOpenBtn.onClick.AddListener(OpenCreateAccBtn);

        //## �������� �г�
        if (m_CancleBtn != null)
            m_CancleBtn.onClick.AddListener(CancleBtnClick);

        if (m_CreateAccBtn != null)
            m_CreateAccBtn.onClick.AddListener(CreateAccBtn);


        //## Url
        LoginUrl = "http://typhoon.dothome.co.kr/Login.php";

        CreateUrl = "http://typhoon.dothome.co.kr/CreateAccount.php";
    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f < ShowTime)
        {
            ShowTime -= Time.deltaTime;
            if (ShowTime <= 0.0f)
            {
                MessageOn("", false);
            }
        }

    }

    void LoginBtnClick()
    {
        // SceneManager.LoadScene("Lobby");

        string a_IdStr = IDInputField.text;
        string a_PwStr = PWInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();

        if (string.IsNullOrEmpty(a_IdStr) == true || string.IsNullOrEmpty(a_PwStr) == true)
        {
            MessageOn("Id,Pw�� ��Ȯ�� ä���ּ���.");
            return;
        }

        //## ����ó��
        //���̵�
        if (!(3 <= a_IdStr.Length && a_IdStr.Length <= 20))
        {
            MessageOn("Id�� 3~20�ڸ��� �Է����ּ���.");
            return;
        }

        //��й�ȣ
        if (!(4 <= a_PwStr.Length && a_PwStr.Length <= 20))
        {
            MessageOn("Pw�� 4~20�ڸ��� �Է����ּ���.");
            return;
        }

        //����������� �ڷ�ƾ �Լ� ȣ��
        StartCoroutine(LoginCo(a_IdStr, a_PwStr));

    }

    IEnumerator LoginCo(string a_IdStr, string a_PwStr)
    {
        //## �����
        WWWForm form = new WWWForm();

        form.AddField("Input_id", a_IdStr, System.Text.Encoding.UTF8);
        form.AddField("Input_pw", a_PwStr);

        UnityWebRequest a_Www = UnityWebRequest.Post(LoginUrl, form);

        //## ��û�� ������ ���
        yield return a_Www.SendWebRequest();

        //## ����
        if (a_Www.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_Www.downloadHandler.data);

            a_Www.Dispose();

            if (sz.Contains("Id does not exitst.")== true)
            {
                MessageOn("���̵� ���������ʽ��ϴ�.");
                yield break;
            }

            if (sz.Contains("Password does not Match.") == true)
            {
                MessageOn("��й�ȣ�� Ʋ�Ƚ��ϴ�.");
                yield break;
            }

            if (sz.Contains("Login_Success.") == false)
            {
                MessageOn("�α��� ����!�˼����� ������ �߻��߽��ϴ�.����� �ٽ� �õ����ּ���.");
                yield break;
            }

            if (sz.Contains("{\"") == false)
            {
                MessageOn("������ �������." + sz);
                yield break;
            }

            GlobalValue.g_Unique_ID = a_IdStr;


            string a_GetStr = sz.Substring(sz.IndexOf("{\""));
            a_GetStr = a_GetStr.Replace("\nLogin_Success.", "");

            svRespon respon = JsonUtility.FromJson<svRespon>(a_GetStr);

            GlobalValue.g_NickName = respon.nick_name;
            GlobalValue.g_BestScore = respon.best_score;
            GlobalValue.g_UserGold = respon.game_gold;

            //## ������ �ε�
            if (string.IsNullOrEmpty(respon.floor_info) == false)
            {
                Floor_Info a_FloorInfo = JsonUtility.FromJson<Floor_Info>(respon.floor_info);
                if (a_FloorInfo != null)
                {
                    GlobalValue.g_BestFloor = a_FloorInfo.BestFloor;
                    GlobalValue.g_CurFloorNum = a_FloorInfo.CurFloor;
                }
            }



            if (string.IsNullOrEmpty(respon.info)==false)
            {
                ItemList a_ItList = JsonUtility.FromJson<ItemList>(respon.info);

                //## �����۸���Ʈ �Ľ�
                if (a_ItList != null && a_ItList.SkList != null)
                {
                    for (int i = 0; i < a_ItList.SkList.Length; i++)
                    {
                        if (GlobalValue.g_SkillCount.Length <= i) continue;

                        GlobalValue.g_SkillCount[i] = a_ItList.SkList[i];
                    }
                }
            }
            


            //## �κ�� �̵�
            SceneManager.LoadScene("Lobby");


        }
        else
        {
            MessageOn(a_Www.error);


        }


    }






    void MessageOn(string a_Msg = "", bool isOn = true)
    {
        if (isOn == true)
        {
            MessageTxt.text = a_Msg;
            MessageTxt.gameObject.SetActive(true);
            ShowTime = 5.0f;
        }
        else
        {
            MessageTxt.text = "";
            MessageTxt.gameObject.SetActive(false);
        }
    }


    void OpenCreateAccBtn()
    {
        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(false);

        if (m_CreateAccountPanelObj != null)
            m_CreateAccountPanelObj.SetActive(true);
    }

    void CancleBtnClick()
    {
        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(true);

        if (m_CreateAccountPanelObj != null)
            m_CreateAccountPanelObj.SetActive(false);
    }

    void CreateAccBtn()
    {
        string a_IdStr = New_IDInputField.text;
        string a_PwStr = New_PWInputField.text;
        string a_NickStr = New_NickInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();
        a_NickStr = a_NickStr.Trim();

        //## ����ó��
        if (string.IsNullOrEmpty(a_IdStr) == true ||
            string.IsNullOrEmpty(a_PwStr) == true ||
            string.IsNullOrEmpty(a_NickStr) == true)
        {
            MessageOn("Id,Pw,������ ��Ȯ�� ä���ּ���.");
            return;
        }

        //���̵�
        if (!(3 <= a_IdStr.Length && a_IdStr.Length <= 20))
        {
            MessageOn("Id�� 3~20�ڸ��� �Է����ּ���.");
            return;
        }

        //��й�ȣ
        if (!(4 <= a_PwStr.Length && a_PwStr.Length <= 20))
        {
            MessageOn("Pw�� 4~20�ڸ��� �Է����ּ���.");
            return;
        }

        //�г���
        if (!(2 <= a_NickStr.Length && a_NickStr.Length <= 10))
        {
            MessageOn("NickName��  2~10�ڸ��� �Է����ּ���.");
            return;
        }


        //����������� �ڷ�ƾ �Լ� ȣ��
        StartCoroutine(CreateAccCo(a_IdStr, a_PwStr, a_NickStr));



    }

    IEnumerator CreateAccCo(string a_IdStr, string a_PwStr, string a_NickStr)
    {
        //## �����
        WWWForm form = new WWWForm();

        //## ������ ������ ������
        form.AddField("Input_id", a_IdStr, System.Text.Encoding.UTF8);
        form.AddField("Input_pw", a_PwStr);
        form.AddField("Input_nick", a_NickStr, System.Text.Encoding.UTF8);

        UnityWebRequest a_Www = UnityWebRequest.Post(CreateUrl, form);
        //## ��û�� ������ ���
        yield return a_Www.SendWebRequest();

        //## ����
        if (a_Www.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_Www.downloadHandler.data);
            a_Www.Dispose();

            if (sz.Contains("Create Success.")==true)
                MessageOn("���� ����!");
            else if(sz.Contains("Id is already exist.") == true)
                MessageOn("�̹� �����ϴ� ���̵��Դϴ�.");
            else if(sz.Contains("Nick is already exist.") == true)
                MessageOn("�̹� �����ϴ� �г����Դϴ�.");
            else
                MessageOn(sz);
        }
        else
        {
            MessageOn("���� ����!" + a_Www.error);
            a_Www.Dispose();
        }
    }


}
