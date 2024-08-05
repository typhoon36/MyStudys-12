using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    GameIng,        //게임 진행 중 상태
    GameEnd,        //게임 오버 상태
    GameExit,       //InGame을 나가려고 할 때의 상태
    GameReplay      //InGame에서 Replay 시도 상태
}

public class Game_Mgr : MonoBehaviour
{
    public GameState m_GameState = GameState.GameIng;

    public Text m_BestScoreText = null; //최대점수 표시 UI
    public Text m_CurScoreText = null;  //현재점수 표시 UI
    public Text m_GoldText = null;      //보유골드 표시 UI
    public Text m_UserInfoText = null;  //유저 정보 표시 UI
    public Button GoLobbyBtn = null;    //로비로 이동 버튼

    int m_CurScore = 0;     //이번 스테이지에서 얻은 게임 점수
    int m_CurGold = 0;      //이번 스테이지에서 얻은 골드값

    //--- 캐릭터 머리위에 데미지 띄우기용 변수 선언
    GameObject m_DmgClone;  //Damage Text 복사본을 받을 변수
    DmgTxt_Ctrl m_DmgTxt;   //Damage Text 복사본에 붙어 있는 DmgTxt_Ctrl 컴포넌트를 받을 변수
    Vector3 m_StCacPos;     //시작 위치를 계산해 주기 위한 변수
    [Header("--- Damage Text ---")]
    public Transform Damage_Canvas = null;
    public GameObject DmgTxtRoot = null;
    //--- 캐릭터 머리위에 데미지 띄우기용 변수 선언

    //--- 코인 아이템 관련 변수
    GameObject m_CoinItem = null;
    //--- 코인 아이템 관련 변수

    //--- 하트 아이템 관련 변수
    GameObject m_HeartItem = null;
    //--- 하트 아이템 관련 변수

    [Header("--- Skill Coll Timer ---")]
    public Transform m_SkillCoolRoot = null;
    public GameObject m_SkCollNode = null;

    HeroCtrl m_RefHero = null;

    [Header("--- Inventory Show OnOff ---")]
    public Button m_Inven_Btn = null;
    public Transform m_InventoryRoot = null;
    Transform m_ArrowIcon = null;
    bool m_Inven_ScOnOff = true;
    float m_ScSpeed = 1500.0f;
    Vector3 m_ScOnPos = new Vector3(-170.0f, 0.0f, 0.0f);
    Vector3 m_ScOffPos = new Vector3(-572.0f, 0.0f, 0.0f);

    //--- Config Box(환경설정) 관련 변수
    [Header("------- ConfigBox -------")]
    public Button m_CfgBtn = null;
    public GameObject Canvas_Dialog = null;
    GameObject m_ConfigBoxObj = null;
    //--- Config Box(환경설정) 관련 변수

    [Header("------- Game Over -------")]
    public GameObject GameOverPanel = null;
    public Text Result_Text = null;
    public Button Replay_Btn = null;
    public Button RstLobby_Btn = null;

    [Header("------- ExpLevel -------")]
    public Text ExpLevel_Text = null;
    int m_KillCount = 0;

    //--- 싱글톤 패턴
    public static Game_Mgr Inst = null;

    void Awake()
    {
        Inst = this;
    }
    //--- 싱글톤 패턴

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f; //원래 속도로...
        m_GameState = GameState.GameIng;
        GlobalValue.LoadGameData();
        InitRefreshUI();
        RefreshSkillList();
        RefreshExpLevel_UI();

        if (GoLobbyBtn != null)
            GoLobbyBtn.onClick.AddListener(() =>
            {
                //SceneManager.LoadScene("LobbyScene");
                Time.timeScale = 0.0f;  //일시정지
                m_GameState = GameState.GameExit;
            });

        m_CoinItem = Resources.Load("CoinPrefab") as GameObject;
        m_HeartItem = Resources.Load("HeartPrefab") as GameObject;

        m_RefHero = GameObject.FindObjectOfType<HeroCtrl>();

        if (m_Inven_Btn != null)
        {
            m_ArrowIcon = m_Inven_Btn.transform.Find("ArrowIcon");

            m_Inven_Btn.onClick.AddListener(() =>
            {
                m_Inven_ScOnOff = !m_Inven_ScOnOff;
            });
        }

        //--- 환경설정 Dlg 관련 구현 코드
        if (m_CfgBtn != null)
            m_CfgBtn.onClick.AddListener(() =>
            {
                if (m_ConfigBoxObj == null)
                    m_ConfigBoxObj = Resources.Load("ConfigBox") as GameObject;

                GameObject a_CfgBoxObj = Instantiate(m_ConfigBoxObj) as GameObject;
                a_CfgBoxObj.transform.SetParent(Canvas_Dialog.transform, false);
                a_CfgBoxObj.GetComponent<ConfigBox>().DltMethod = CfgResponse;

                Time.timeScale = 0.0f;  //일시정지
            });
        //--- 환경설정 Dlg 관련 구현 코드

        Sound_Mgr.Inst.PlayBGM("sound_bgm_island_001", 0.5f);

    }//void Start()

    // Update is called once per frame
    void Update()
    {
        //--- 단축키 이용으로 스킬 사용하기
        if(Input.GetKeyDown(KeyCode.Alpha1) ||
           Input.GetKeyDown(KeyCode.Keypad1))
        {
            UseSkill_Key(SkillType.Skill_0);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Keypad2))
        {
            UseSkill_Key(SkillType.Skill_1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) ||
            Input.GetKeyDown(KeyCode.Keypad3))
        {
            UseSkill_Key(SkillType.Skill_2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) ||
                Input.GetKeyDown(KeyCode.Keypad4))
        {
            UseSkill_Key(SkillType.Skill_3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) ||
                Input.GetKeyDown(KeyCode.Keypad5))
        {
            UseSkill_Key(SkillType.Skill_4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6) ||
                Input.GetKeyDown(KeyCode.Keypad6))
        {
            UseSkill_Key(SkillType.Skill_5);
        }

        ScrollOnOff_Update();

    }//void Update()

    public void DamageText(float a_Value, Vector3 a_Pos, Color a_Color)
    {
        if(Damage_Canvas == null || DmgTxtRoot == null) 
            return;

        m_DmgClone = Instantiate(DmgTxtRoot);
        m_DmgClone.transform.SetParent(Damage_Canvas);
        m_DmgTxt = m_DmgClone.GetComponent<DmgTxt_Ctrl>();
        if (m_DmgTxt != null)
            m_DmgTxt.InitDamage(a_Value, a_Color);
        m_StCacPos = new Vector3(a_Pos.x, a_Pos.y + 1.14f, 0.0f);
        m_DmgClone.transform.position = m_StCacPos;
    }

    public void SpawnCoin(Vector3 a_Pos, int a_Value = 10)
    {
        if(m_CoinItem == null)
            return;

        GameObject a_CoinObj = Instantiate(m_CoinItem);
        a_CoinObj.transform.position = a_Pos;
        Coin_Ctrl a_CoinCtrl = a_CoinObj.GetComponent<Coin_Ctrl>();
        if (a_CoinCtrl != null)
            a_CoinCtrl.m_RefHero = m_RefHero;
    }

    public void SpawnHeart(Vector3 a_Pos)
    {
        if (m_HeartItem == null)
            return;

        GameObject a_HeartObj = Instantiate(m_HeartItem);
        a_Pos.z = 0.0f;
        a_HeartObj.transform.position = a_Pos;
    }

    void UseSkill_Key(SkillType a_SkType)
    {
        if (GlobalValue.g_CurSkillCount[(int)a_SkType] <= 0)
            return;     //보유하고 있는 스킬 소진으로 사용할 수 없음

        if(m_RefHero == null)
            return;

        m_RefHero.UseSkill(a_SkType);

        //--- 스킬 아이템 사용시 UI 갱신 코드
        if (m_InventoryRoot == null)
            return;

        SkInvenNode[] a_SkIvnNodes = m_InventoryRoot.GetComponentsInChildren<SkInvenNode>();
        for (int i = 0; i < a_SkIvnNodes.Length; i++)
        {
            if (a_SkIvnNodes[i].m_SkType == a_SkType)
            {
                a_SkIvnNodes[i].Refresh_UI(a_SkType);
                break;
            }
        }
        //--- 스킬 아이템 사용시 UI 갱신 코드
    }

    public void SkillCoolMethod(SkillType a_SkType, float a_Time, float a_During)
    {
        GameObject a_Obj = Instantiate(m_SkCollNode);
        a_Obj.transform.SetParent(m_SkillCoolRoot, false);
        //두번째 매개변수 worldPositionStays (기본값은 true)

        SkillCool_Ctrl a_SCtrl = a_Obj.GetComponent<SkillCool_Ctrl>();  
        if(a_SCtrl != null)
            a_SCtrl.InitState(a_SkType, a_Time, a_During);
    }

    void ScrollOnOff_Update()
    {
        if(m_InventoryRoot == null) 
            return;

        if(Input.GetKeyDown(KeyCode.R) == true)
        {
            m_Inven_ScOnOff = !m_Inven_ScOnOff;
        }

        if(m_Inven_ScOnOff == false)
        {
            if(m_InventoryRoot.localPosition.x > m_ScOffPos.x)
            {
                m_InventoryRoot.localPosition =
                    Vector3.MoveTowards(m_InventoryRoot.localPosition, 
                                            m_ScOffPos, m_ScSpeed * Time.deltaTime);

                if(m_ScOffPos.x <= m_InventoryRoot.localPosition.x)
                {
                    m_ArrowIcon.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                }
            }//if(m_InventoryRoot.localPosition.x > m_ScOffPos.x)
        }// if(m_Inven_ScOnOff == false)
        else //if(m_Inven_ScOnOff == true)
        {
            if(m_ScOnPos.x > m_InventoryRoot.localPosition.x)
            {
                m_InventoryRoot.localPosition =
                    Vector3.MoveTowards(m_InventoryRoot.localPosition,
                                            m_ScOnPos, m_ScSpeed * Time.deltaTime);

                if(m_InventoryRoot.localPosition.x <= m_ScOnPos.x)
                {
                    m_ArrowIcon.transform.eulerAngles = new Vector3(0.0f, 0.0f, 180.0f);
                }
            }
        }//else //if(m_Inven_ScOnOff == true)

    }//void ScrollOnOff_Update()

    public void AddScore(int a_Value = 10)
    {
        if (m_CurScore <= int.MaxValue - a_Value)
            m_CurScore += a_Value;
        else
            m_CurScore = int.MaxValue;

        if (m_CurScore < 0)
            m_CurScore = 0;

        m_CurScoreText.text = "현재점수(" + m_CurScore + ")";
        if(GlobalValue.g_BestScore < m_CurScore)
        {
            GlobalValue.g_BestScore = m_CurScore;
            m_BestScoreText.text = "최고점수(" + GlobalValue.g_BestScore + ")";

            //PlayerPrefs.SetInt("BestScore", GlobalValue.g_BestScore);
            NetworkMgr.Inst.PushPacket(PacketType.BestScore);
        }
    }

    public void AddGold(int a_Value = 10)
    {
        //이번 스테이지에서 얻은 골드값
        if(m_CurGold <= int.MaxValue - a_Value)
            m_CurGold += a_Value;
        else
            m_CurGold = int.MaxValue;

        //로컬에 저장되어 있는 유저 보유 골드값
        if(GlobalValue.g_UserGold <= int.MaxValue - a_Value)
            GlobalValue.g_UserGold += a_Value;
        else
            GlobalValue.g_UserGold = int.MaxValue;

        m_GoldText.text = "보유골드(" + GlobalValue.g_UserGold + ")";
        //PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
        NetworkMgr.Inst.PushPacket(PacketType.UserGold);
    }

    public void AddExpLevel(int a_AddExp = 3)
    {
        if (GlobalValue.g_Level < GlobalValue.g_LvTable.Count)  //아직 최고 레벨에 도달하지 않았다면...
        {
            GlobalValue.g_Exp += a_AddExp;  //경험치 증가 계산

            //---- 레벨 증가 계산
            for(int i = 0; i < 100; i++)  //경험치가 갑자기 확 늘어날 수 있어서...
            {
                //위에서 경험치를 증가시킨 결과가 다음 레벨을 증가 시키기 위한 경험치에 아직 도달하지 않았다면...
                if (GlobalValue.g_Exp < GlobalValue.g_LvTable[GlobalValue.g_Level].m_DestExp)
                    break;  //레벨은 증가 시키지 않게 위해 아래 레벨 증가 코드 스킵

                GlobalValue.g_Exp -= GlobalValue.g_LvTable[GlobalValue.g_Level].m_DestExp;
                GlobalValue.g_Level++;

                if (GlobalValue.g_LvTable.Count <= GlobalValue.g_Level) //최고 레벨 도달
                    break;
            }
            //---- 레벨 증가 계산

        }//if (GlobalValue.g_Level < GlobalValue.g_LvTable.Count)  //아직 최고 레벨에 도달하지 않았다면...

        NetworkMgr.Inst.PushPacket(PacketType.UpdateExp);

        RefreshExpLevel_UI();
    }//public void AddExpLevel(int a_AddExp = 10)

    void RefreshExpLevel_UI()
    {
        if (ExpLevel_Text == null)
            return;

        string a_strExp = "<color=#ff0000>최고레벨도달</color>";
        if (GlobalValue.g_Level < GlobalValue.g_LvTable.Count)
        {
            a_strExp = GlobalValue.g_Exp + " / " + 
                       GlobalValue.g_LvTable[GlobalValue.g_Level].m_DestExp;
        }

        ExpLevel_Text.text = "경험치(" + a_strExp + ") " +
                             "레벨(" + (GlobalValue.g_Level + 1) + ")";
    }

    void InitRefreshUI()
    {
        if (m_BestScoreText != null)
            m_BestScoreText.text = "최고점수(" + GlobalValue.g_BestScore + ")";

        if (m_CurScoreText != null)
            m_CurScoreText.text = "현재점수(" + m_CurScore + ")";

        if (m_GoldText != null)
            m_GoldText.text = "보유골드(" + GlobalValue.g_UserGold + ")";

        if (m_UserInfoText != null)
            m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + ")";
    }

    void RefreshSkillList() //보유 Skill Item 목록을 UI에 복원하는 함수
    {
        SkInvenNode[] a_SkIvnNodes = m_InventoryRoot.GetComponentsInChildren<SkInvenNode>();
        for(int i = 0; i < a_SkIvnNodes.Length; i++)
        {
            if (GlobalValue.g_CurSkillCount.Count <= i)
                continue;

            a_SkIvnNodes[i].InitState((SkillType)i);

        }//for(int i = 0; i < a_SkIvnNodes.Length; i++)

    }//void RefreshSkillList()

    public void GameOverMethod()
    {
        if (GameOverPanel != null && GameOverPanel.activeSelf == false)
            GameOverPanel.SetActive(true);

        if (Result_Text != null)
            Result_Text.text = "NickName\n" + GlobalValue.g_NickName + "\n\n" +
                                "획득점수\n" + m_CurScore + "\n\n" +
                                "획득골드\n" + m_CurGold;

        if (Replay_Btn != null)
            Replay_Btn.onClick.AddListener(() =>
            {
                //SceneManager.LoadScene("GameScene");
                m_GameState = GameState.GameReplay;
            });

        if (RstLobby_Btn != null)
            RstLobby_Btn.onClick.AddListener(() =>
            {
                //SceneManager.LoadScene("LobbyScene");
                m_GameState = GameState.GameExit;
            });
    }

    void CfgResponse() //환경설정 박스 Ok 후 호출되게 하기 위한 함수
    {       
        if (m_UserInfoText != null)
            m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + ")";
    }

}//public class Game_Mgr : MonoBehaviour
