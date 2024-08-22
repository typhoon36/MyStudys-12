using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby_Mgr : MonoBehaviour
{
    public Button m_ClearSvDataBtn;
    float ClearLockTimer = 0.0f;        //데이터 초기화 대기 타이머

    public Button m_StoreBtn;
    public Button m_GameStartBtn;
    public Button m_LogoutBtn;

    public Text m_GoldText;
    public Text m_UserInfoText;
    public Text m_Ranking_Text;

    [HideInInspector] public int m_My_Rank = 0;  //내등수
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
        Time.timeScale = 1.0f;  //원래 속도로...

        GlobalValue.LoadGameData();

        if (m_ClearSvDataBtn != null)
            m_ClearSvDataBtn.onClick.AddListener(ClearSvData);

        if (m_StoreBtn != null)
            m_StoreBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("StoreScene");
            });

        if (m_GameStartBtn != null)
            m_GameStartBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("GameScene");
            });

        if (m_LogoutBtn != null)
            m_LogoutBtn.onClick.AddListener(() =>
            {
                GlobalValue.g_Unique_ID = "";   //유저의 고유번호
                GlobalValue.g_NickName  = "";   //유저의 별명
                GlobalValue.g_BestScore = 0;    //획득 최고점수
                GlobalValue.g_UserGold  = 0;    //유저의 보유골드
                GlobalValue.g_Exp   = 0;        //경험치 Experience
                GlobalValue.g_Level = 0;        //유저의 레벨
                for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
                {
                    GlobalValue.g_SkillCount[i] = 0; //아이템 보유수 초기화
                }
                    
                PlayFabClientAPI.ForgetAllCredentials(); // Playfab --> Logout

                SceneManager.LoadScene("TitleScene");
            });

        if(m_GoldText != null)
        {
            m_GoldText.text = GlobalValue.g_UserGold.ToString("N0");
            // "N0" <-- 소수점 밑으로는 제외시키고 천단위 마다 쉼표 붙여주기...
        }

        if(m_UserInfoText != null)
        {
            m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + ") : 순위(" + m_My_Rank +
                                   "등) : 점수(" + GlobalValue.g_BestScore + "점)";
        }

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

    }//void Start()

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

        if (0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if (ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false); //메시지 끄기
            }
        }//if(0.0f < ShowMsTimer)

        if (0.0f <= ClearLockTimer)
            ClearLockTimer -= Time.deltaTime;
    }

    void ClearSvData()
    {
        if(0.0f < ClearLockTimer)
        {
            Debug.Log("저장정보초기화 중입니다.");
            return;
        }

        PlayerPrefs.DeleteAll();    //로컬에 저장되어 있던 모든 정조를 지워준다.
        GlobalValue.LoadGameData();

        GlobalValue.g_BestScore = 0;    //획득 최고점수
        GlobalValue.g_UserGold = 0;     //유저의 보유골드
        GlobalValue.g_Exp   = 0;        //경험치 Experience
        GlobalValue.g_Level = 0;        //유저의 레벨
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            GlobalValue.g_SkillCount[i] = 0; //아이템 보유수 초기화
        }
        m_My_Rank = 0;

        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.ClearSave);
        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.ClearScore);
        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.ClearExp);

        if (m_GoldText != null)
            m_GoldText.text = GlobalValue.g_UserGold.ToString("N0");

        if (m_UserInfoText != null)
            m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + ") : 순위(" + m_My_Rank + 
                                   "등) : 점수(" + GlobalValue.g_BestScore + "점)";

        ClearLockTimer = 8.0f;
    }

    void MessageOnOff(string Mess = "", bool isOn = true)
    {
        if (isOn == true)
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

    public void CfgResponse() //환경설정 박스 Ok 후 호출되게 하기 위한 함수
    {
        if (m_UserInfoText != null)
        {
            m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + ") : 순위(" + m_My_Rank +
                                   "등) : 점수(" + GlobalValue.g_BestScore + "점)";
        }
    }

    void RestoreRank() //수동 리셋인 경우
    {
        if (0.0f < RestoreTimer)
        {
            MessageOnOff("최소 7초 주기로만 갱신됩니다.");
            return;
        }

        LobbyNetworkMgr.Inst.PushPacket(LobbyNetworkMgr.PacketType.GetRankingList);

        RestoreTimer = 7.0f;
    }
}
