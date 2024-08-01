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

    //--- 환경설정 Dlg 관련 변수
    [Header("--- ConfigBox ---")]
    public Button m_CfgBtn = null;
    public GameObject Canvas_Dialog = null;
    GameObject m_ConfigBoxObj = null;
    //--- 환경설정 Dlg 관련 변수

    //## 랭킹 
    [HideInInspector]public int m_MyRank = 0;
    public Button RstRk_Btn;
    float Restoretimer = 3.0f;
    //로비 진입후 3초 뒤 랭킹보드 재로드

    //## 메시지 
    float ShowMsgTimer = 0.0f;
    public Text Msg_Txt;




    //## 싱글턴
    public static Lobby_Mgr Inst = null;

    void Awake()
    {
        Inst = this;
    }




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
            //"N0" 엔 제로 <-- 소수점 밑으로는 제외시키고 천단위 마다 쉼표 붙여주기...
        }

        if (m_UserInfoText != null)
        {
            m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + ") : 순위("+ m_MyRank+"등)"
                + "점수 :("+ GlobalValue.g_BestScore + ")점";
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

        //## 랭킹
        //GetRankList();
        LobbyNet_Mgr.Inst.PushPacket(LobbyNet_Mgr.PacketType.GetRankList);


#if AutoRestore
        //# 자동 갱신
        if(RstRk_Btn != null)
            RstRk_Btn.gameObject.SetActive(false);

#else
        //# 수동 갱신
        if (RstRk_Btn != null)
            RstRk_Btn.onClick.AddListener(RestoreRank);
#endif
    }

    void ClearSvData()
    {
        PlayerPrefs.DeleteAll();    //로컬에 저장되어 있었던 모든 정보를 지워준다.

        GlobalValue.g_CurSkillCount.Clear();
        GlobalValue.LoadGameData();

        if (m_GoldText != null)
            m_GoldText.text = GlobalValue.g_UserGold.ToString("N0");

        if (m_UserInfoText != null)
            m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + ") : 순위("+ m_MyRank+"등)"
                + "점수 :("+ GlobalValue.g_BestScore + ")점";

        Sound_Mgr.Inst.PlayGUISound("Pop", 1.0f);
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
        //Debug.Log("타이틀 씬으로 나가기 버튼 클릭");
        //SceneManager.LoadScene("TitleScene");
        MyLoadScene("TitleScene");
    }

    // Update is called once per frame
    void Update()
    {
#if AutoRestore
        //## 자동 리셋
        Restoretimer -= Time.deltaTime;
        if (Restoretimer <= 0.0f)
        { 
            LobbyNet_Mgr.Inst.PushPacket(LobbyNet_Mgr.PacketType.GetRankList);
            Restoretimer = 7.0f;
        }

#else
        //## 수동 리셋
        if (0 < Restoretimer)
            Restoretimer -= Time.deltaTime;

#endif

        if(0 < ShowMsgTimer)
        {
            ShowMsgTimer -= Time.deltaTime;
            if(ShowMsgTimer <= 0.0f)
            {
                ShowMsg("", false);
            }
        }
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
        if (m_UserInfoText != null)
            m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + ") : 순위("+ m_MyRank+"등)"
                + "점수 :("+ GlobalValue.g_BestScore + ") 점";
    }

    //## 수동 리셋일때의 랭킹 리셋
    void RestoreRank()
    {
        if(0 < Restoretimer)
        {
            ShowMsg("Error)7초 기다려주세요.7초 주기입니다.");
            return;
        }

        //GetRankList();
        LobbyNet_Mgr.Inst.PushPacket(LobbyNet_Mgr.PacketType.GetRankList);


        Restoretimer = 7.0f;
    }

    //## 메시지 
    public void ShowMsg(string a_Msg = "", bool isTrigger = true)
    {
        if(isTrigger == true)
        {
            Msg_Txt.text = a_Msg;   
            Msg_Txt.gameObject.SetActive(true);
            ShowMsgTimer = 5.0f;
        }
        else
        {
            Msg_Txt.text = "";
            Msg_Txt.gameObject.SetActive(false);
        }
    }



  

}
