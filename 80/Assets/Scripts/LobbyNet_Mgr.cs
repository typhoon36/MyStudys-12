using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;


//# 로비+네트워크(로비에서 네트워크하도록)   
public class LobbyNet_Mgr : MonoBehaviour
{
    public enum PacketType{GetRankList,GetMyRank,}

    //## 패킷 처리
    bool IsNetWorking = false;
    List<PacketType> m_PacketBuff = new List<PacketType>();//<--단순 패킷 보낼 필요있다는 버퍼 큐





    //## 싱글톤
    public static LobbyNet_Mgr Inst = null;

    void Awake()
    {

        Inst = this;
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //## 패킷처리
        if(IsNetWorking == false)
        {
            if (0 < m_PacketBuff.Count)
            {
                Req_NetWork();
            }
        }

    }

    //## 패킷 요청
    void Req_NetWork()
    {

        if (m_PacketBuff[0]== PacketType.GetRankList)
            GetRankList();

        m_PacketBuff.RemoveAt(0);

    }



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

        IsNetWorking = true;

        PlayFabClientAPI.GetLeaderboard(
            request,
            (result) =>
            {
                //성공
                
                if(Lobby_Mgr.Inst == null)
                {
                    IsNetWorking = false;
                    return;
                }

                if (Lobby_Mgr.Inst.Rank_Txt == null)
                {
                    IsNetWorking = false;
                    return;
                }

                string a_StrBuff = "";

                for (int i = 0; i < result.Leaderboard.Count; i++)
                {
                    var curBoard = result.Leaderboard[i];

                    //## 등수 안에 내가 존재시 색표시
                    if (curBoard.PlayFabId == GlobalValue.g_Unique_ID)
                        a_StrBuff += "<color=#00ff00>";

                    a_StrBuff += (i + 1).ToString() + "등 :" +
                    curBoard.DisplayName + " : " + curBoard.StatValue + "점 : " + "\n";

                    if (curBoard.PlayFabId == GlobalValue.g_Unique_ID)
                        a_StrBuff += "</color>";

                }

                if (a_StrBuff != "")
                {
                    Lobby_Mgr.Inst.Rank_Txt.text = a_StrBuff;
                }



                GetMyRank();

            },
            (error) =>
            {
                IsNetWorking = false;
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
                                      if(Lobby_Mgr.Inst == null)
                                      {
                                          IsNetWorking = false;
                                          return;
                                      }



                                      if (0 < result.Leaderboard.Count)
                                      {
                                          var CurBoard = result.Leaderboard[0];
                                          Lobby_Mgr.Inst.m_MyRank = CurBoard.Position + 1; //0부터 시작하니까 +1
                                          GlobalValue.g_BestScore = (int)CurBoard.StatValue;//최고점수 갱신
                                          Lobby_Mgr.Inst.CfgResponse();
                                      }

                                      IsNetWorking = false;

                                  },
                                  (error) =>
                                  {
                                      Debug.Log("내 랭킹 가져오기 실패");
                                      IsNetWorking = false;
                                  }
                                    );
    }

    public void PushPacket(PacketType a_PType)
    {
        bool a_IsExist = false;

        for (int i = 0; i < m_PacketBuff.Count; i++)
        {
            //## 처리되지않은 패킷 존재시
            if (m_PacketBuff[i] == a_PType)
            {
                a_IsExist = true;
            }
        }

        //## 처리되지 않은 패킷이 없다면
        if (a_IsExist == false)
        m_PacketBuff.Add(a_PType);//추가

        
    }

}
