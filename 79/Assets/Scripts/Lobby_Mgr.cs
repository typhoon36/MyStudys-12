using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby_Mgr : MonoBehaviour
{
    public Button m_ClearSvDataBtn;

    public Button Store_Btn;
    public Button MyRoom_Btn;
    public Button Exit_Btn;
    public Button GameStart_Btn;

    public Text m_GoldText;
    public Text m_UserInfoText;
    public Text Rank_Txt;

    //--- ȯ�漳�� Dlg ���� ����
    [Header("--- ConfigBox ---")]
    public Button m_CfgBtn = null;
    public GameObject Canvas_Dialog = null;
    GameObject m_ConfigBoxObj = null;
    //--- ȯ�漳�� Dlg ���� ����

    //## ��ŷ 
    int m_MyRank = 0;
    public Button RstRk_Btn;
    float Restoretimer = 3.0f;
    //float DelayGetLb = 3.0f;
    //�κ� ������ 3�� �� ��ŷ���� ��ε�



    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;
        GlobalValue.LoadGameData();

        if (m_ClearSvDataBtn != null)
            m_ClearSvDataBtn.onClick.AddListener(ClearSvData);

        if (Store_Btn != null)
            Store_Btn.onClick.AddListener(StoreBtnClick);

        if (MyRoom_Btn != null)
            MyRoom_Btn.onClick.AddListener(MyRoomBtnClick);

        if (Exit_Btn != null)
            Exit_Btn.onClick.AddListener(ExitBtnClick);

        if (GameStart_Btn != null)
            GameStart_Btn.onClick.AddListener(() =>
            {
                //SceneManager.LoadScene("GameScene");
                MyLoadScene("GameScene");
            });

        if (m_GoldText != null)
        {
            m_GoldText.text = GlobalValue.g_UserGold.ToString("N0");
            //"N0" �� ���� <-- �Ҽ��� �����δ� ���ܽ�Ű�� õ���� ���� ��ǥ �ٿ��ֱ�...
        }

        if (m_UserInfoText != null)
        {
            m_UserInfoText.text = "������ : ����(" + GlobalValue.g_NickName + ") : ����("+ m_MyRank+"��)"
                + "���� :("+ GlobalValue.g_BestScore + ")";
        }

        //--- ȯ�漳�� Dlg ���� ���� �κ�
        if (m_CfgBtn != null)
            m_CfgBtn.onClick.AddListener(() =>
            {
                if (m_ConfigBoxObj == null)
                    m_ConfigBoxObj = Resources.Load("ConfigBox") as GameObject;

                GameObject a_CfgBoxObj = Instantiate(m_ConfigBoxObj);
                a_CfgBoxObj.transform.SetParent(Canvas_Dialog.transform, false);
                a_CfgBoxObj.GetComponent<ConfigBox>().DltMethod = CfgResponse;

                Time.timeScale = 0.0f;
            });
        //--- ȯ�漳�� Dlg ���� ���� �κ�

        Sound_Mgr.Inst.PlayBGM("sound_bgm_title_001", 0.5f);

        //## ��ŷ
        GetRankList();

        if (RstRk_Btn != null)
            RstRk_Btn.onClick.AddListener(RestoreRank);




    }

    void ClearSvData()
    {
        PlayerPrefs.DeleteAll();    //���ÿ� ����Ǿ� �־��� ��� ������ �����ش�.

        GlobalValue.g_CurSkillCount.Clear();
        GlobalValue.LoadGameData();

        if (m_GoldText != null)
            m_GoldText.text = GlobalValue.g_UserGold.ToString("N0");

        if (m_UserInfoText != null)
            m_UserInfoText.text = "������ : ����(" + GlobalValue.g_NickName + ") : ����("+ m_MyRank+"��)"
                + "���� :("+ GlobalValue.g_BestScore + ")";

        Sound_Mgr.Inst.PlayGUISound("Pop", 1.0f);
    }

    private void StoreBtnClick()
    {
        //Debug.Log("�������� ���� ��ư Ŭ��");
        //SceneManager.LoadScene("StoreScene");
        MyLoadScene("StoreScene");
    }

    private void MyRoomBtnClick()
    {
        //Debug.Log("�ٹ̱� �� ���� ��ư Ŭ��");
        //SceneManager.LoadScene("MyRoomScene");
        MyLoadScene("MyRoomScene");
    }

    private void ExitBtnClick()
    {
        //Debug.Log("Ÿ��Ʋ ������ ������ ��ư Ŭ��");
        //SceneManager.LoadScene("TitleScene");
        MyLoadScene("TitleScene");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void MyLoadScene(string a_ScName)
    {
        bool IsFadeOk = false;
        if (Fade_Mgr.Inst != null)
            IsFadeOk = Fade_Mgr.Inst.SceneOutReserve(a_ScName);
        if (IsFadeOk == false)
            SceneManager.LoadScene(a_ScName);

        Sound_Mgr.Inst.PlayGUISound("Pop", 1.0f);
    }

    void CfgResponse() //ȯ�漳�� �ڽ� Ok �� ȣ��ǰ� �ϱ� ���� �Լ�
    {
        if (m_UserInfoText != null)
            m_UserInfoText.text = "������ : ����(" + GlobalValue.g_NickName + ") : ����("+ m_MyRank+"��)"
                + "���� :("+ GlobalValue.g_BestScore + ")";
    }

    //## ���� �����϶��� ��ŷ ����
    void RestoreRank()
    {
        if(0 < Restoretimer)
        {
            Debug.Log("7�� ��ٷ��ּ���.7�� �ֱ��Դϴ�.");
            return;
        }

        GetRankList();


        Restoretimer = 7.0f;
    }

    //# ��ŷ �ε� �Լ�
    void GetRankList()
    {
        //## �α׾ƿ�����
        if (GlobalValue.g_Unique_ID == "")
            return;
        //## �α����� ����
        var request = new GetLeaderboardRequest
        {
            //1����� ����
            StartPosition = 0,
            //playfab�� ����ǥ ���� ����("BestScore")
            StatisticName = "BestScore",

            //## 10����� �������°�.ex)startPostion - 10 Maxresultscount 20���� �ϸ� 10��~20�����.
            MaxResultsCount = 10,

            //## ������ ���� �������� �Լ�
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true,
                ShowAvatarUrl = true, //������ ���� �ּҸ� ��û(����ġ ���)

            }
        };

        PlayFabClientAPI.GetLeaderboard(
            request,
            (result) =>
            {
                //����
                //Debug.Log("��ŷ�������� �������� ����");
                if(Rank_Txt == null)
                    return;
         
                string a_StrBuff = "";

                for(int i = 0; i < result.Leaderboard.Count; i++)
                {
                    var curBoard = result.Leaderboard[i];
                    
                    //## ��� �ȿ� ���� ����� ��ǥ��
                    if(curBoard.PlayFabId == GlobalValue.g_Unique_ID)                   
                        a_StrBuff += "<color=#00ff00>";

                    a_StrBuff += (i + 1).ToString() + "�� :" +
                    curBoard.DisplayName + " : " + curBoard.StatValue + "�� : " + "\n";

                    if (curBoard.PlayFabId == GlobalValue.g_Unique_ID)
                        a_StrBuff += "</color>";

                }

                if(a_StrBuff != "")
                {
                    Rank_Txt.text = a_StrBuff;
                }



                GetMyRank();

            },
            (error) =>
            {
                
            }
                );

    }


    //# �� ��ŷ ��������
    void GetMyRank()
    {
        //GetLeaderboardAroundPlayer (Ư�� Id�� �ֺ����� ��ŷ�� ������)
        //��,PlayFabId �ֺ����� ����Ʈ�� �ҷ����� �Լ�
        var request = new GetLeaderboardAroundPlayerRequest
        {
            //PlayFabId = GlobalValue.g_Unique_ID, --> ���� ����(���� ���ϸ� �α��� �� ID ������ �Ǿ����)

            StatisticName = "BestScore",

            //�� ������ �޾ƿ�
            MaxResultsCount = 1

            

            //ProfileConstraints = new PlayerProfileViewConstraints()
            //{
            //    ShowDisplayName = true, //���⼭�� �ʿ����(���ΰ��� �˰�������)
            //}




        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(
                       request,
                                  (result) =>
                                  {
                                      if(0 < result.Leaderboard.Count)
                                      {
                                          var CurBoard = result.Leaderboard[0];
                                          m_MyRank = CurBoard.Position + 1; //0���� �����ϴϱ� +1
                                          GlobalValue.g_BestScore = (int)CurBoard.StatValue;//�ְ����� ����
                                          CfgResponse();
                                      }
               
                                  },
                                  (error) =>
                                  {
                                      Debug.Log("�� ��ŷ �������� ����");
                                  }
                                    );
    }

}
