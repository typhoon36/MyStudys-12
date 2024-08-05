using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;
using SimpleJSON;

public class LobbyNetworkMgr : MonoBehaviour
{
    public enum PacketType
    {
        GetRankingList,     //랭킹 리스트 가져오기
        GetMyRanking,       //내 등수 가져오기

        ClearSvData,        //서버 데이터 초기화
        ClearScoreData,     //점수 데이터 초기화
        ClearExpData,       //경험치 데이터 초기화(레벨 포함)
    }

    //--- 서버에 전송할 패킷 처리용 큐 관련 변수
    List<PacketType> m_PacketBuff = new List<PacketType>();
    //단순히 어떤 패킷을 보낼 필요가 있다 라는 버퍼 PacketBuffer <큐>
    //--- 서버에 전송할 패킷 처리용 큐 관련 변수

    float m_NetWaitDelay = 0.0f;


    //--- 싱글턴 패턴을 위한 인스턴스 변수 선언
    public static LobbyNetworkMgr Inst = null;

    void Awake()
    {
        //NetworkMgr 클래스를 인스턴스에 대입
        Inst = this;
    }
    //--- 싱글턴 패턴을 위한 인스턴스 변수 선언

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (m_NetWaitDelay <= 0.0f)  //지금 패킷 처리 중인 상태가 아니면...
        {
            if (0 < m_PacketBuff.Count) //대기 패킷이 존재한다면...
            {
                Req_Network();
            }
        }
    }//void Update()

    void Req_Network()   //RequestNetwork
    {
        if (m_PacketBuff[0] == PacketType.GetRankingList)
            GetRankingList();
        else if (m_PacketBuff[0] == PacketType.ClearSvData)
            UpdateClearSvCo();
        else if (m_PacketBuff[0] == PacketType.ClearScoreData)
            UpdateClearScoreCo();
        else if (m_PacketBuff[0] == PacketType.ClearExpData)
            UpdateClearExpCo();

        m_PacketBuff.RemoveAt(0);
    }

    void GetRankingList() //순위 불러오기...
    {
        if (GlobalValue.g_Unique_ID == "")  //로그인 상태에서만...
            return;

        var request = new GetLeaderboardRequest
        {
            StartPosition = 0,              //0번 인덱스 즉 1등부터
            StatisticName = "BestScore",
            //관리자 페이지의 순위표 변수 중 "BestScore" 기준
            MaxResultsCount = 10,           //10명까지
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true, //닉네임도 요청
                ShowAvatarUrl = true  //유저 사진 썸네일 주소도 요청(이건 경험치로 사용)
            }
        };

        m_NetWaitDelay = 0.5f;

        PlayFabClientAPI.GetLeaderboard(
            request,

            (result) =>
            { //랭킹 리스트 받아오기 성공

                if (Lobby_Mgr.Inst == null)
                {

                    return;
                }

                if (Lobby_Mgr.Inst.m_Ranking_Text == null)
                {

                    return;
                }

                string a_strBuff = "";

                for (int i = 0; i < result.Leaderboard.Count; i++)
                {
                    var curBoard = result.Leaderboard[i];
                    int a_ULevel = LvMyJsonParser(curBoard.Profile.AvatarUrl);

                    //등수 안에 내가 있다면 색 표시
                    if (curBoard.PlayFabId == GlobalValue.g_Unique_ID)
                        a_strBuff += "<color=#008800>";

                    a_strBuff += (i + 1).ToString() + "등 : " +
                                    curBoard.DisplayName + " (Lv"+ (a_ULevel + 1) + ") : " +
                                    curBoard.StatValue + "점" + "\n";

                    //등수 안에 내가 있다면 색 표시
                    if (curBoard.PlayFabId == GlobalValue.g_Unique_ID)
                        a_strBuff += "</color>";

                }//for(int i = 0; i < result.Leaderboard.Count; i++)

                if (a_strBuff != "")
                    Lobby_Mgr.Inst.m_Ranking_Text.text = a_strBuff;

                //리더보드 등수를 불러온 직 후 내 등수를 불러 온다.
                GetMyRanking();
            },

            (error) =>
            { //랭킹 리스트 방오기 실패 했을 때 
                Debug.Log("리더보드 불러오기 실패");
                //isNetworkLock = false;
            }
       );
    }//void GetRankingList() //순위 불러오기...

    void GetMyRanking()  //내 등수 불러오기...
    {
        //GetLeaderboardAroundPlayer() : 
        //이 함수는 특정 PlayFabId(유니트ID) 주변으로 리스트를 불러오는 함수이다.

        var request = new GetLeaderboardAroundPlayerRequest
        {
            //PlayFabId = GlobalValue.g_Unique_ID,
            //지정하지 않으면 내 유니트 ID(로그인된 ID) 기준이 된다.
            StatisticName = "BestScore",
            MaxResultsCount = 1,    //한명에 정보만 받아오라는 뜻

            //ProfileConstraints = new PlayerProfileViewConstraints()
            //{
            //    ShowDisplayName = true  //이 옵션으로 유저 별명을 받아올 수 있는데... 주인공은 아니까 생략
            //}
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(
                request,

                (result) =>
                {
                    if (Lobby_Mgr.Inst == null)
                    {
                        //isNetworkLock = false;
                        return;
                    }

                    if (0 < result.Leaderboard.Count)
                    {
                        var curBoard = result.Leaderboard[0];
                        Lobby_Mgr.Inst.m_My_Rank = curBoard.Position + 1; //내 등수 가져오기...
                        GlobalValue.g_BestScore = curBoard.StatValue; //내 최고 점수 갱신

                        Lobby_Mgr.Inst.CfgResponse();  //<-- UI 갱신
                    }

                    //isNetworkLock = false;
                },

                (error) =>
                {
                    Debug.Log("내 등수 불러오기 실패");
                    //isNetworkLock = false;
                }
            );
    }//void GetMyRanking()


    //# 서버에서 현재 플레이어하고 있는 플레이어타이틀 초기화
    void UpdateClearSvCo()
    {
        if (GlobalValue.g_Unique_ID == "") return;

        var request = new UpdateUserDataRequest();

        request.KeysToRemove = new List<string>();

        //for (int i = 0; i < GlobalValue.g_CurSkillCount.Count; i++)
        //{
        //    request.KeysToRemove.Add($"Skill_Item_{i}");
        //}

        request.KeysToRemove.Add("UserGold");


        Dictionary<string, string> a_ItemList = new Dictionary<string, string>();
        for(int i = 0; i < GlobalValue.g_CurSkillCount.Count; i++)
        {
            a_ItemList.Add($"Skill_Item_{i}", (1).ToString());
        }

        request.Data = a_ItemList;


        m_NetWaitDelay = 0.5f;


        PlayFabClientAPI.UpdateUserData(request,
                       (result) =>
                       {
                          for(int i = 0; i < GlobalValue.g_CurSkillCount.Count; i++)
                           {
                               GlobalValue.g_CurSkillCount[i] = 1;
                          }
                       },
                       (error) =>
                       {
                           //Debug.Log("데이터 저장 실패 " + error.GenerateErrorReport());
                           //isNetworkLock = false;
                       }
                                                     );


    }

    void UpdateClearScoreCo()
    {
        if (GlobalValue.g_Unique_ID == "") return;

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate {StatisticName = "BestScore", Value = 0},
            }
        };

        m_NetWaitDelay = 0.5f;

        PlayFabClientAPI.UpdatePlayerStatistics(request,
                       (result) =>
                       {
                           //업데이트성공시 처리
                       },
                       (error) =>
                       {
                           //업데이트실패시 처리
                       }
                                                        );

    }
    void UpdateClearExpCo()
    {
        if (GlobalValue.g_Unique_ID == "") return;

        // AvatarUrl을 이용해서 유저의 레벨을 저장편법
        var request = new UpdateAvatarUrlRequest()
        {
            ImageUrl = "", // 초기화할 URL을 빈 JSON 객체로 설정
        };

        m_NetWaitDelay = 0.5f;

        PlayFabClientAPI.UpdateAvatarUrl(request,
            (result) =>
            {
                // 업데이트 성공 시 처리
                Debug.Log("Avatar URL 초기화 성공");
            },
            (error) =>
            {
                // 업데이트 실패 시 처리
                Debug.LogError("Avatar URL 초기화 실패: " + error.GenerateErrorReport());
            }
        );
    }






    public void PushPacket(PacketType a_PType)
    {
        bool a_isExist = false;
        for (int i = 0; i < m_PacketBuff.Count; i++)
        {
            //아직 처리 되지 않은 패킷이 존재하면
            if (m_PacketBuff[i] == a_PType)
                a_isExist = true;
            //또 추가하지 않고 기존 버퍼의 패킷으로 업데이트 한다.
        }

        if (a_isExist == false)
            m_PacketBuff.Add(a_PType);
        //대기 중인 이 타입의 패킷이 없으면 새로 추가한다.
    }

    int LvMyJsonParser(string AvatarUrl)
    {
        int a_Level = 0;

        //---- 레벨 가져오기
        //--- JSON 파싱
        if (string.IsNullOrEmpty(AvatarUrl) == true)
            return 0;

        if (AvatarUrl.Contains("{\"") == false)
            return 0;

        JSONNode a_ParseJs = JSON.Parse(AvatarUrl);
        if (a_ParseJs["UserLv"] != null)
        {
            a_Level = a_ParseJs["UserLv"].AsInt;
            return a_Level;
        }
        //--- JSON 파싱
        //---- 레벨 가져오기

        return 0;
    }
}
