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

    float ClearLockTime = 0.0f; //연타방지용 타이머

    public Button Store_Btn;
    public Button MyRoom_Btn;
    public Button Exit_Btn;
    public Button GameStart_Btn;

    public Text m_GoldText;
    public Text m_UserInfoText;
    public Text m_Ranking_Text;

    //--- 환경설정 Dlg 관련 변수
    [Header("--- ConfigBox ---")]
    public Button m_CfgBtn = null;
    public GameObject Canvas_Dialog = null;
    GameObject m_ConfigBoxObj = null;
    //--- 환경설정 Dlg 관련 변수

    [HideInInspector] public int m_My_Rank = 0;  // 내 등수
    public Button RestRk_Btn;   //Restore Ranking Button
    float RestoreTimer = 3.0f;  //랭킹 갱신 타이머

    float ShowMsTimer = 0.0f;   //메시지를 몇 초동안 보이게 할건지에 대한 타이머
    public Text MessageText;    //메시지 내용을 표시할 UI

    //--- 싱글턴 패턴
    public static Lobby_Mgr Inst = null;

    void Awake()
    {
        Inst = this;
    }
    //--- 싱글턴 패턴

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
            //"N0" 엔 제로 <-- 소수점 밑으로는 제외시키고 천단위 마다 쉼표 붙여주기...
        }

        //if(m_UserInfoText != null)
        //{
        //    m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + ") : 순위(" + m_My_Rank + 
        //                          "등) : 점수(" + GlobalValue.g_BestScore + "점)";
        //}

        if (m_UserInfoText != null)
        {
            m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + " : Lv" +
                                  (GlobalValue.g_Level + 1) + ") : 순위(" + m_My_Rank +
                                  "등) : 점수(" + GlobalValue.g_BestScore + "점)";
        }

        //--- 환경설정 Dlg 관련 구현 부분
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
        //--- 환경설정 Dlg 관련 구현 부분

        Sound_Mgr.Inst.PlayBGM("sound_bgm_title_001", 0.5f);

        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.GetRankingList);

#if AutoRestore
        //--- 자동 리스토어인 경우
        if (RestRk_Btn != null)
            RestRk_Btn.gameObject.SetActive(false);
        //--- 자동 리스토어인 경우
#else
        //--- 수동 리스토어인 경우
        if (RestRk_Btn != null)
            RestRk_Btn.onClick.AddListener(RestoreRank);
        //--- 수동 리스토어인 경우
#endif

     


    }

    void ClearSvData()
    {
        if (0.0f < ClearLockTime)
        {
            MessageOnOff("저장정보초기화중입니다...");
            return;
        }

        //## 0으로 설정
        GlobalValue.g_BestScore = 0;
        GlobalValue.g_UserGold = 0;
        GlobalValue.g_Exp = 0;
        GlobalValue.g_Level = 0;

        for (int i = 0; i < GlobalValue.g_CurSkillCount.Count; i++)
        {
            GlobalValue.g_CurSkillCount[i] = 0;
        }

        m_My_Rank = 0;

        PlayerPrefs.DeleteAll();    //로컬에 저장되어 있었던 모든 정보를 지워준다.
        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.ClearSvData);
        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.ClearExpData);
        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.ClearScoreData);

        // UI 갱신
        if (m_GoldText != null)
            m_GoldText.text = GlobalValue.g_UserGold.ToString("N0");

        if (m_UserInfoText != null)
        {
            m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + " : Lv" +
                                  (GlobalValue.g_Level + 1) + ") : 순위(" + m_My_Rank +
                                  "등) : 점수(" + GlobalValue.g_BestScore + "점)";
        }

        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.GetRankingList);
        RestoreTimer = 7.0f;

        Sound_Mgr.Inst.PlayGUISound("Pop", 1.0f);

        ClearLockTime = 8.0f;
    }

    private void StoreBtnClick()
    {
        //Debug.Log("상점으로 가기 버튼 클릭");
        //SceneManager.LoadScene("StoreScene");
        MyLoadScene("StoreScene");
    }

    private void MyRoomBtnClick()
    {
        //Debug.Log("꾸미기 방 가기 버튼 클릭");
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
        //-- 자동 리스토어인 경우
        RestoreTimer -= Time.deltaTime;
        if(RestoreTimer <= 0.0f)
        {
            LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.GetRankingList);
            RestoreTimer = 7.0f;
        }
        //-- 자동 리스토어인 경우
#else
        //<-- 수동 리스토어인 경우
        if (0.0f < RestoreTimer)
            RestoreTimer -= Time.deltaTime;
        //<-- 수동 리스토어인 경우
#endif

        if(0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if(ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false); //메시지 끄기
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

    public void CfgResponse() //환경설정 박스 Ok 후 호출되게 하기 위한 함수
    {
        //if (m_UserInfoText != null)
        //    m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + ") : 순위(" + m_My_Rank +
        //                          "등) : 점수(" + GlobalValue.g_BestScore + "점)";

        if (m_UserInfoText != null)
        {
            m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + " : Lv" + 
                                  (GlobalValue.g_Level + 1) + ") : 순위(" + m_My_Rank +
                                  "등) : 점수(" + GlobalValue.g_BestScore + "점)";
        }
    }

    void RestoreRank() //수동 리셋인 경우
    {
        if(0.0f < RestoreTimer)
        {
            MessageOnOff("최소 7초 주기로만 갱신됩니다.");
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
