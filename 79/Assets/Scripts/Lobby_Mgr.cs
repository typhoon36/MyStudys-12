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
    int m_MyRank = 0;
    public Button RstRk_Btn;
    float Restoretimer = 3.0f;
    //float DelayGetLb = 3.0f;
    //로비 진입후 3초 뒤 랭킹보드 재로드



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
                + "점수 :("+ GlobalValue.g_BestScore + ")";
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
        GetRankList();

        if (RstRk_Btn != null)
            RstRk_Btn.onClick.AddListener(RestoreRank);




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
                + "점수 :("+ GlobalValue.g_BestScore + ")";

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

    void CfgResponse() //환경설정 박스 Ok 후 호출되게 하기 위한 함수
    {
        if (m_UserInfoText != null)
            m_UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName + ") : 순위("+ m_MyRank+"등)"
                + "점수 :("+ GlobalValue.g_BestScore + ")";
    }

    //## 수동 리셋일때의 랭킹 리셋
    void RestoreRank()
    {
        if(0 < Restoretimer)
        {
            Debug.Log("7초 기다려주세요.7초 주기입니다.");
            return;
        }

        GetRankList();


        Restoretimer = 7.0f;
    }

    //# 랭킹 로드 함수
    void GetRankList()
    {
        //## 로그아웃상태
        if (GlobalValue.g_Unique_ID == "")
            return;
        //## 로그인인 상태
        var request = new GetLeaderboardRequest
        {
            //1등부터 시작
            StartPosition = 0,
            //playfab의 순위표 변수 기준("BestScore")
            StatisticName = "BestScore",

            //## 10등까지 가져오는것.ex)startPostion - 10 Maxresultscount 20으로 하면 10등~20등까지.
            MaxResultsCount = 10,

            //## 유저들 정보 가져오는 함수
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true,
                ShowAvatarUrl = true, //프로필 사진 주소를 요청(경험치 사용)

            }
        };

        PlayFabClientAPI.GetLeaderboard(
            request,
            (result) =>
            {
                //성공
                //Debug.Log("랭킹리더보드 가져오기 성공");
                if(Rank_Txt == null)
                    return;
         
                string a_StrBuff = "";

                for(int i = 0; i < result.Leaderboard.Count; i++)
                {
                    var curBoard = result.Leaderboard[i];
                    
                    //## 등수 안에 내가 존재시 색표시
                    if(curBoard.PlayFabId == GlobalValue.g_Unique_ID)                   
                        a_StrBuff += "<color=#00ff00>";

                    a_StrBuff += (i + 1).ToString() + "등 :" +
                    curBoard.DisplayName + " : " + curBoard.StatValue + "점 : " + "\n";

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


    //# 내 랭킹 가져오기
    void GetMyRank()
    {
        //GetLeaderboardAroundPlayer (특정 Id를 주변으로 랭킹을 가져옴)
        //즉,PlayFabId 주변으로 리스트를 불러오는 함수
        var request = new GetLeaderboardAroundPlayerRequest
        {
            //PlayFabId = GlobalValue.g_Unique_ID, --> 생략 가능(지정 안하면 로그인 된 ID 기준이 되어버림)

            StatisticName = "BestScore",

            //내 정보만 받아옴
            MaxResultsCount = 1

            

            //ProfileConstraints = new PlayerProfileViewConstraints()
            //{
            //    ShowDisplayName = true, //여기서는 필요없음(주인공은 알고있으니)
            //}




        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(
                       request,
                                  (result) =>
                                  {
                                      if(0 < result.Leaderboard.Count)
                                      {
                                          var CurBoard = result.Leaderboard[0];
                                          m_MyRank = CurBoard.Position + 1; //0부터 시작하니까 +1
                                          GlobalValue.g_BestScore = (int)CurBoard.StatValue;//최고점수 갱신
                                          CfgResponse();
                                      }
               
                                  },
                                  (error) =>
                                  {
                                      Debug.Log("내 랭킹 가져오기 실패");
                                  }
                                    );
    }

}
