using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyNetworkMgr : MonoBehaviour
{
    public enum PacketType
    {
        GetRankingList,     //랭킹 리스트 가져오기
        GetMyRanking,       //내 등수 가져오기

        ClearSave,          //서버에 저장된 내용 초기화 하기 < 플레이어 데이터(타이틀) > 값
        ClearScore,         //서버에 저장된 Score(랭킹)값 초기화 하기
        ClearExp            //서버에 저장된 경험치, 레벨 초기화 하기
    }

    //--- 서버에 전송할 패킷 처리용 큐 관련 변수
    //bool isNetworkLock = false;
    float m_NetWaitTime = 0.0f;
    List<PacketType> m_PacketBuff = new List<PacketType>();
    //단순히 어떤 패킷을 보낼 필요가 있다 라는 버퍼 PacketBuffer <큐>
    //--- 서버에 전송할 패킷 처리용 큐 관련 변수

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
        m_NetWaitTime -= Time.unscaledDeltaTime;
        if (m_NetWaitTime < 0.0f)
            m_NetWaitTime = 0.0f;

        if (m_NetWaitTime <= 0.0f) //지금 패킷 처리 중인 상태가 아니면...
        {
            if (0 < m_PacketBuff.Count) //대기 패킷이 존재한다면...
            {
                Req_Network();
            }
        }
    }

    void Req_Network()   //RequestNetwork
    {
        if (m_PacketBuff[0] == PacketType.GetRankingList)
            GetRankingList();
        else if (m_PacketBuff[0] == PacketType.ClearSave)
            UpdateClearSaveCo();
        else if (m_PacketBuff[0] == PacketType.ClearScore)
            UpdateClearScoreCo();
        else if (m_PacketBuff[0] == PacketType.ClearExp)
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

        m_NetWaitTime = 0.5f;

        PlayFabClientAPI.GetLeaderboard(
            request,

            (result) =>
            { //랭킹 리스트 받아오기 성공

                if (Lobby_Mgr.Inst == null)
                {
                    //isNetworkLock = false;
                    return;
                }

                if (Lobby_Mgr.Inst.m_Ranking_Text == null)
                {
                    //isNetworkLock = false;
                    return;
                }

                string a_strBuff = "";

                for (int i = 0; i < result.Leaderboard.Count; i++)
                {
                    var curBoard = result.Leaderboard[i];
                    //int a_ULevel = LvMyJsonParser(curBoard.Profile.AvatarUrl);

                    //등수 안에 내가 있다면 색 표시
                    if (curBoard.PlayFabId == GlobalValue.g_Unique_ID)
                        a_strBuff += "<color=#008800>";

                    a_strBuff += (i + 1).ToString() + "등 : " +
                                    curBoard.DisplayName + " : " +
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

        //m_NetWaitTime = 0.5f;

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

    void UpdateClearSaveCo() //Playfab 서버에 현재 플레이하고 잇는 플레이어 데이터(타이틀) 값 초기화 함수
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

        // < 플레이어 데이터(타이틀) > 값 활용 코드
        var request = new UpdateUserDataRequest();
        //맴버변수 KeysToRemove : 특정키 값을 삭제 까지는 할 수 있다.
        request.KeysToRemove = new List<string>();
        //for (int i = 0; i < GlobalValue.g_CurSkillCount.Count; i++)
        //{
        //    request.KeysToRemove.Add($"Skill_Item_{i}");
        //}
        request.KeysToRemove.Add("UserGold");

        Dictionary<string, string> a_ItemList = new Dictionary<string, string>();
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            a_ItemList.Add($"SkItem_{i}", (1).ToString());
        }
        request.Data = a_ItemList;

        m_NetWaitTime = 0.5f;

        PlayFabClientAPI.UpdateUserData(request,
                        (result) =>
                        {
                            for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
                            {
                                GlobalValue.g_SkillCount[i] = 1; //아이템 보유수 초기화
                            }
                        },
                        (error) =>
                        {

                        }
                 );
    }//void UpdateClearSaveCo()

    void UpdateClearScoreCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "BestScore", Value = 0},
            }
        };

        m_NetWaitTime = 0.5f;

        PlayFabClientAPI.UpdatePlayerStatistics(
                request,

                (result) =>
                {   //업데이트 성공시 응답 함수

                },

                (error) =>
                {   //업데이트 실패시 응답 함수

                }
           );
    }//void UpdateClearScoreCo()

    void UpdateClearExpCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

        //--- AvatarUrl 를 이용해서 저장하는 장점은
        //순위표 리스트 받을 때 AvatarUrl을 같이 받아 올 수 있다.

        //--- AvatarUrl(유저얼굴사진)을 이용해서 유저의 Level을 저장하는 편법
        var request = new UpdateAvatarUrlRequest()
        {
            ImageUrl = "",
        };

        m_NetWaitTime = 0.5f;
        PlayFabClientAPI.UpdateAvatarUrl(request,
                (result) =>
                {

                },
                (error) =>
                {

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

}
