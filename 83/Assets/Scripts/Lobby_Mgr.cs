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

    float ClearLockTime = 0.0f; //��Ÿ������ Ÿ�̸�

    public Button Store_Btn;
    public Button MyRoom_Btn;
    public Button Exit_Btn;
    public Button GameStart_Btn;

    public Text m_GoldText;
    public Text m_UserInfoText;
    public Text m_Ranking_Text;

    //--- ȯ�漳�� Dlg ���� ����
    [Header("--- ConfigBox ---")]
    public Button m_CfgBtn = null;
    public GameObject Canvas_Dialog = null;
    GameObject m_ConfigBoxObj = null;
    //--- ȯ�漳�� Dlg ���� ����

    [HideInInspector] public int m_My_Rank = 0;  // �� ���
    public Button RestRk_Btn;   //Restore Ranking Button
    float RestoreTimer = 3.0f;  //��ŷ ���� Ÿ�̸�

    float ShowMsTimer = 0.0f;   //�޽����� �� �ʵ��� ���̰� �Ұ����� ���� Ÿ�̸�
    public Text MessageText;    //�޽��� ������ ǥ���� UI

    //--- �̱��� ����
    public static Lobby_Mgr Inst = null;

    void Awake()
    {
        Inst = this;
    }
    //--- �̱��� ����

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

        if(m_GoldText != null)
        {
            m_GoldText.text = GlobalValue.g_UserGold.ToString("N0");
            //"N0" �� ���� <-- �Ҽ��� �����δ� ���ܽ�Ű�� õ���� ���� ��ǥ �ٿ��ֱ�...
        }

        //if(m_UserInfoText != null)
        //{
        //    m_UserInfoText.text = "������ : ����(" + GlobalValue.g_NickName + ") : ����(" + m_My_Rank + 
        //                          "��) : ����(" + GlobalValue.g_BestScore + "��)";
        //}

        if (m_UserInfoText != null)
        {
            m_UserInfoText.text = "������ : ����(" + GlobalValue.g_NickName + " : Lv" +
                                  (GlobalValue.g_Level + 1) + ") : ����(" + m_My_Rank +
                                  "��) : ����(" + GlobalValue.g_BestScore + "��)";
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

        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.GetRankingList);

#if AutoRestore
        //--- �ڵ� ��������� ���
        if (RestRk_Btn != null)
            RestRk_Btn.gameObject.SetActive(false);
        //--- �ڵ� ��������� ���
#else
        //--- ���� ��������� ���
        if (RestRk_Btn != null)
            RestRk_Btn.onClick.AddListener(RestoreRank);
        //--- ���� ��������� ���
#endif

     


    }

    void ClearSvData()
    {
        if (0.0f < ClearLockTime)
        {
            MessageOnOff("���������ʱ�ȭ���Դϴ�...");
            return;
        }

        //## 0���� ����
        GlobalValue.g_BestScore = 0;
        GlobalValue.g_UserGold = 0;
        GlobalValue.g_Exp = 0;
        GlobalValue.g_Level = 0;

        for (int i = 0; i < GlobalValue.g_CurSkillCount.Count; i++)
        {
            GlobalValue.g_CurSkillCount[i] = 0;
        }

        m_My_Rank = 0;

        PlayerPrefs.DeleteAll();    //���ÿ� ����Ǿ� �־��� ��� ������ �����ش�.
        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.ClearSvData);
        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.ClearExpData);
        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.ClearScoreData);

        // UI ����
        if (m_GoldText != null)
            m_GoldText.text = GlobalValue.g_UserGold.ToString("N0");

        if (m_UserInfoText != null)
        {
            m_UserInfoText.text = "������ : ����(" + GlobalValue.g_NickName + " : Lv" +
                                  (GlobalValue.g_Level + 1) + ") : ����(" + m_My_Rank +
                                  "��) : ����(" + GlobalValue.g_BestScore + "��)";
        }

        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.GetRankingList);
        RestoreTimer = 7.0f;

        Sound_Mgr.Inst.PlayGUISound("Pop", 1.0f);

        ClearLockTime = 8.0f;
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
       
        GlobalValue.g_Unique_ID = "";
        GlobalValue.g_NickName = "";
        GlobalValue.g_UserGold = 0;
        GlobalValue.g_BestScore = 0;
        GlobalValue.g_Exp = 0;
        GlobalValue.g_Level = 0;

        for (int i = 0; i < GlobalValue.g_CurSkillCount.Count; i++)
        {
            GlobalValue.g_CurSkillCount[i] = 0;
        }

        PlayFabClientAPI.ForgetAllCredentials();



        MyLoadScene("TitleScene");
    }

    // Update is called once per frame
    void Update()
    {

#if AutoRestore
        //-- �ڵ� ��������� ���
        RestoreTimer -= Time.deltaTime;
        if(RestoreTimer <= 0.0f)
        {
            LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.GetRankingList);
            RestoreTimer = 7.0f;
        }
        //-- �ڵ� ��������� ���
#else
        //<-- ���� ��������� ���
        if (0.0f < RestoreTimer)
            RestoreTimer -= Time.deltaTime;
        //<-- ���� ��������� ���
#endif

        if(0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if(ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false); //�޽��� ����
            }
        }//if(0.0f < ShowMsTimer)
        if (0 <= ClearLockTime)
            ClearLockTime -= Time.deltaTime;
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

    public void CfgResponse() //ȯ�漳�� �ڽ� Ok �� ȣ��ǰ� �ϱ� ���� �Լ�
    {
        //if (m_UserInfoText != null)
        //    m_UserInfoText.text = "������ : ����(" + GlobalValue.g_NickName + ") : ����(" + m_My_Rank +
        //                          "��) : ����(" + GlobalValue.g_BestScore + "��)";

        if (m_UserInfoText != null)
        {
            m_UserInfoText.text = "������ : ����(" + GlobalValue.g_NickName + " : Lv" + 
                                  (GlobalValue.g_Level + 1) + ") : ����(" + m_My_Rank +
                                  "��) : ����(" + GlobalValue.g_BestScore + "��)";
        }
    }

    void RestoreRank() //���� ������ ���
    {
        if(0.0f < RestoreTimer)
        {
            MessageOnOff("�ּ� 7�� �ֱ�θ� ���ŵ˴ϴ�.");
            return;
        }

        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.GetRankingList);

        RestoreTimer = 7.0f;
    }

    void MessageOnOff(string Mess = "", bool isOn = true)
    {
        if(isOn == true)
        {
            MessageText.text = Mess;
            MessageText.gameObject.SetActive(true);
            ShowMsTimer = 5.0f;
        }
        else
        {
            MessageText.text = "";
            MessageText.gameObject.SetActive(false);
        }
    }

}
