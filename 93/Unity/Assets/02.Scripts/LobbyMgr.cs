using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMgr : MonoBehaviour
{
    public Button m_Start_Btn;
    public Button m_Store_Btn;
    public Button m_Logout_Btn;
    public Button m_Clear_Save_Btn;

    public Text UserInfoText;

    [HideInInspector] public int m_MyRank = 0;

    public Button RestRk_Btn;

    public Text Rank_Txt;

    public Text Message_Txt;
    float ShowMsTime = 0.0f;

    [Header("ConfigBox")]
    public Button m_Cfg_Btn;
    public GameObject m_CfgBox;
    public GameObject m_Cfg_Canvas;

    //singleton pattern
    public static LobbyMgr Inst = null;

    void Awake()
    {
        Inst = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f; //�Ͻ������� ���� �ӵ���...
        GlobalValue.LoadGameData();

        if (m_Start_Btn != null)
            m_Start_Btn.onClick.AddListener(StartBtnClick);

        if (m_Store_Btn != null)
            m_Store_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("StoreScene");
            });

        if (m_Logout_Btn != null)
            m_Logout_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("TitleScene");
            });

        if (m_Clear_Save_Btn != null)
            m_Clear_Save_Btn.onClick.AddListener(Clear_Save_Click);

        RefreshUserInfo();


#if AutoRestore
        //�ڵ� ��ŷ ����
        if (RestRk_Btn != null)
            RestRk_Btn.gameObject.SetActive(false);
#else 
        if (RestRk_Btn != null)
            RestRk_Btn.onClick.AddListener(RestoreRank);
#endif

        if (m_Cfg_Btn != null)
        {
            m_Cfg_Btn.onClick.AddListener(() =>
            {
                if (m_CfgBox == null)
                    m_CfgBox = Resources.Load("Config_Box") as GameObject;


                GameObject a_CfgBox = Instantiate(m_CfgBox);
                a_CfgBox.transform.SetParent(m_Cfg_Canvas.transform, false);
                Time.timeScale = 0.0f;

            });
        }


    }







    // Update is called once per frame
    void Update()
    {
        if (0.0f < ShowMsTime)
        {
            ShowMsTime -= Time.deltaTime;
            if (ShowMsTime <= 0.0f)
            {
                MessageOn("", false);
            }
        }

    }

    void StartBtnClick()
    {
        if (100 <= GlobalValue.g_CurFloorNum)
        {
            //������ ���� ������ ���¿��� ������ ���� �ߴٸ�...
            //�ٷ� ���� ��(99��)���� �����ϰ� �ϱ�...
            GlobalValue.g_CurFloorNum = 99;
            //PlayerPrefs.SetInt("CurFloorNum", GlobalValue.g_CurFloorNum);
        }

        SceneManager.LoadScene("scLevel01");
        SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
    }

    void Clear_Save_Click()
    {
        PlayerPrefs.DeleteAll();
        GlobalValue.LoadGameData();
        RefreshUserInfo();
    }

    public void RefreshUserInfo()
    {
        UserInfoText.text = "������ : ����(" + GlobalValue.g_NickName +
                            ") : ����(" + m_MyRank + "��) : ����(" +
                            GlobalValue.g_BestScore.ToString("N0") + "��) : ���(" +
                            GlobalValue.g_UserGold.ToString("N0") + ")";
    }

    public void RefreshRankUI(RkRootInfo a_RkRootInfo)
    {
        Rank_Txt.text = "";

        for (int i = 0; i < a_RkRootInfo.RkList.Length; i++)
        {
            if (a_RkRootInfo.RkList[i].user_id == GlobalValue.g_Unique_ID)
                Rank_Txt.text += "<color=#00ff00>";

            Rank_Txt.text += (i + 1).ToString() + "�� : " +
                a_RkRootInfo.RkList[i].user_id + "( " + a_RkRootInfo.RkList[i].nick_name + " ) : " +
                a_RkRootInfo.RkList[i].best_score + "��" + "\n";

            if (a_RkRootInfo.RkList[i].user_id == GlobalValue.g_Unique_ID)
                Rank_Txt.text += "</color>";
        }


        m_MyRank = a_RkRootInfo.my_rank;
        RefreshUserInfo();
    }


    public void MessageOn(string Msg = "", bool IsMsg = true, float a_Time = 5.0f)
    {
        if (IsMsg)
        {
            Message_Txt.text = Msg;
            Message_Txt.gameObject.SetActive(true);
            ShowMsTime = a_Time;
        }

        else
        {
            Message_Txt.text = "";
            Message_Txt.gameObject.SetActive(false);
            ShowMsTime = 0.0f;
        }

    }


    void RestoreRank()
    {
        if (0.0f < LobbyNetwork_Mgr.Inst.RestoreTime)
        {
            MessageOn("�ּ� 7���ֱ�� ���ŵ˴ϴ�.");
            return;
        }

        LobbyNetwork_Mgr.Inst.GetRankList();
        LobbyNetwork_Mgr.Inst.RestoreTime = 7.0f;

    }

}
