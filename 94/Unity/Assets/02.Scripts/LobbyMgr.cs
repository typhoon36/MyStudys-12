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

    float ClearLockTime = 0.0f;

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
        Time.timeScale = 1.0f; //일시정지를 원래 속도로...
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
                GlobalValue.ClearGameData();
                SceneManager.LoadScene("TitleScene");
            });

        if (m_Clear_Save_Btn != null)
            m_Clear_Save_Btn.onClick.AddListener(Clear_Save_Click);

        RefreshUserInfo();


#if AutoRestore
        //자동 랭킹 갱신
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

        if (0.0f < ClearLockTime)
        {
            ClearLockTime -= Time.deltaTime;
            if (ClearLockTime <= 0.0f)
            {
                ClearLockTime = 0.0f;
                MessageOn("",false);
            }
            

        }
    }

    void StartBtnClick()
    {
        if (100 <= GlobalValue.g_CurFloorNum)
        {
            //마지막 층에 도달한 상태에서 게임을 시작 했다면...
            //바로 직전 층(99층)에서 시작하게 하기...
            GlobalValue.g_CurFloorNum = 99;
            //PlayerPrefs.SetInt("CurFloorNum", GlobalValue.g_CurFloorNum);
        }

        SceneManager.LoadScene("scLevel01");
        SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
    }

    void Clear_Save_Click()
    {
        if (0.0f < ClearLockTime)
        {
            MessageOn("저장정보초기화...");
            return;
        }
        PlayerPrefs.DeleteAll();
        GlobalValue.LoadGameData();
        //RefreshUserInfo();

        LobbyNetwork_Mgr.Inst.DltMethod = Result_Clear_Save;
        LobbyNetwork_Mgr.Inst.PushPacket(PacketType.ClearSave);

        ClearLockTime = 5.0f;
    }

    void Result_Clear_Save(bool IsSuccess)
    {
        //서버 초기화 후 모든 변수 초기화
        if(IsSuccess)
        {
            GlobalValue.g_BestScore = 0;
            GlobalValue.g_UserGold = 0;
            GlobalValue.g_Exp = 0;
            GlobalValue.g_Level = 0;

            GlobalValue.g_BestFloor = 1;
            GlobalValue.g_CurFloorNum = 1;

            for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            {
                GlobalValue.g_SkillCount[i] = 1;
            }

            RefreshUserInfo();
        }
    }

    public void RefreshUserInfo()
    {
        UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName +
                            ") : 순위(" + m_MyRank + "등) : 점수(" +
                            GlobalValue.g_BestScore.ToString("N0") + "점) : 골드(" +
                            GlobalValue.g_UserGold.ToString("N0") + ")";
    }

    public void RefreshRankUI(RkRootInfo a_RkRootInfo)
    {
        Rank_Txt.text = "";

        for (int i = 0; i < a_RkRootInfo.RkList.Length; i++)
        {
            if (a_RkRootInfo.RkList[i].user_id == GlobalValue.g_Unique_ID)
                Rank_Txt.text += "<color=#00ff00>";

            Rank_Txt.text += (i + 1).ToString() + "등 : " +
                a_RkRootInfo.RkList[i].user_id + "( " + a_RkRootInfo.RkList[i].nick_name + " ) : " +
                a_RkRootInfo.RkList[i].best_score + "점" + "\n";

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
            MessageOn("최소 7초주기로 갱신됩니다.");
            return;
        }

        LobbyNetwork_Mgr.Inst.GetRankList();
        LobbyNetwork_Mgr.Inst.RestoreTime = 7.0f;

    }

}
