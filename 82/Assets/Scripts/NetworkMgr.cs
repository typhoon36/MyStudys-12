using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.SceneManagement;
using PlayFab.Json;

public enum PacketType
{
    BestScore,      //최고점수
    UserGold,       //유저골드
    UpdateItem,     //아이템 보유수 갱신
    NickUpdate,     //닉네임갱신
    UpdateExp,      //경헌치갱신
}

public class NetworkMgr : MonoBehaviour
{
    //--- 서버에 전송할 패킷 처리용 큐 관련 변수
    bool isNetworkLock = false;     //Network 대기 상태 여부 변수
    List<PacketType> m_PacketBuff = new List<PacketType>();
    //보낼 패킷 타입 대기 리스트 (큐 역할)

    //싱글턴 패턴을 위한 인스턴스 변수 선언
    public static NetworkMgr Inst = null;

    void Awake()
    {
        //NetworkMgr 클래스를 인스턴에 대입
        Inst = this;
    }
    //싱글턴 패턴을 위한 인스턴스 변수 선언

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isNetworkLock == false) //지금 패킷 처리 중인 상태가 아니면...
        {
            if (0 < m_PacketBuff.Count) //대기 패킷이 존재한다면...
            {
                Req_NetWork();
            }
            else  //처리할 패킷이 하나도 없다면...
            {
                //매번 처리할 패킷이 하나도 없을 때만 종료처리 해야 할지 확인한다.
                Exe_GameEnd();
            }
        }//if(isNetworkLock == false) //지금 패킷 처리 중인 상태가 아니면...

    }//void Update()

    void Req_NetWork()  //RequestNetWork
    {
        if (m_PacketBuff[0] == PacketType.BestScore)
            UpdateScoreCo();
        else if (m_PacketBuff[0] == PacketType.UserGold)
            UpdateGoldCo(); //Playfab 서버에 골드갱신 요청 함수
        else if (m_PacketBuff[0] == PacketType.UpdateItem)
            UpdateItemCo(); //Playfab 서버에 아이템 보유수 갱신 요청 함수
        else if (m_PacketBuff[0] == PacketType.UpdateExp)
            UpdateExpCo(); //Playfab 서버에 경험치 갱신 요청 함수


        m_PacketBuff.RemoveAt(0);
    }

    float m_ExitTimer = 0.3f;
    void Exe_GameEnd()  //Execute //샐행하다.
    {  //매번 처리할 패킷이 하나도 없을 때만 종료처리 해야 할지 판단하는 함수
        if (isNetworkLock == true)
            return;

        if (Game_Mgr.Inst.m_GameState == GameState.GameExit ||
           Game_Mgr.Inst.m_GameState == GameState.GameReplay)
        {
            m_ExitTimer -= Time.unscaledDeltaTime;
            if (m_ExitTimer <= 0.0f)
                Exit_Game();

        }//if(Game_Mgr.Inst.m_GameState == GameState.GameExit ||
    }//void Exe_GameEnd()  //Execute //샐행하다.

    void Exit_Game()
    {
        if (Game_Mgr.Inst.m_GameState == GameState.GameExit)
        {   //"로비로 이동" 버튼이 눌려진 상태라면...
            SceneManager.LoadScene("LobbyScene");
        }
        else if (Game_Mgr.Inst.m_GameState == GameState.GameReplay)
        {   //"다시하기" 버튼이 눌려진 상태라면...
            SceneManager.LoadScene("GameScene");
        }
    }

    void UpdateScoreCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

        var request = new UpdatePlayerStatisticsRequest
        {
            //BestScore, BestLevel, ...
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "BestScore",
                                     Value = GlobalValue.g_BestScore },
                //new StatisticUpdate { StatisticName = "BestLevel", 
                //                     Value = GlobalValue.g_BestLevel }
            }
        };

        isNetworkLock = true;

        PlayFabClientAPI.UpdatePlayerStatistics(
                        request,

                        (result) =>
                        { //업데이트 성공시 응답 함수
                            isNetworkLock = false;
                        },

                        (error) =>
                        { //업데이트 실패시 응답 함수
                            isNetworkLock = false;
                        }
             );
    }

    void UpdateGoldCo() //Playfab 서버에 골드갱신 요청 함수
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

        //var request = new UpdateUserDataRequest();
        //request.Permission = UserDataPermission.Private;
        //request.Data = new Dictionary<string, string>();
        //request.Data.Add("UserGold", GlobalValue.g_UserGold.ToString());
        //request.Data.Add("Level", GlobalValue.g_Level.ToString());
        //request.Data.Add("UserStar", GlobalValue.g_UserStar.ToString());

        // < 플레이어 데이터(타이틀) > 값 활용 코드
        var request = new UpdateUserDataRequest()
        {
            //Permission = UserDataPermission.Private, //디폴트값
            //Permission = UserDataPermission.Public,
            //Public 공개설정 : 다른 유저들이 볼 수도 있게 하는 옵션
            //Private 비공개 설정(기본설정임) : 나만 볼 수 있는 값의 속성을 변경

            Data = new Dictionary<string, string>()
            {
                { "UserGold", GlobalValue.g_UserGold.ToString() },
                //{ "Level", GlobalValue.g_Level.ToString() },
                //{ "UserStar", GlobalValue.g_UserStar.ToString() },
            }
        };

        isNetworkLock = true;
        PlayFabClientAPI.UpdateUserData(request,
            (result) =>
            {
                isNetworkLock = false;
                //Debug.Log("데이터 저장 성공");
            },
            (error) =>
            {
                isNetworkLock = false;
                //Debug.Log("데이터 저장 실패 " + error.GenerateErrorReport());
            });
    }

    void UpdateItemCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            return;     //정상적으로 로그인이 되어 있는 상태일 때만...

        Dictionary<string, string> a_ItemList = new Dictionary<string, string>();
        for (int i = 0; i < GlobalValue.g_CurSkillCount.Count; i++)
        {
            a_ItemList.Add($"Skill_Item_{i}", GlobalValue.g_CurSkillCount[i].ToString());
        }

        //< 플레이어 데이터(타이틀) > 값 활용 코드
        var request = new UpdateUserDataRequest()
        {
            Data = a_ItemList
        };

        isNetworkLock = true;

        PlayFabClientAPI.UpdateUserData(request,
                (result) =>
                {
                    isNetworkLock = false;
                    //Debug.Log("데이터 저장 성공");
                },
                (error) =>
                {
                    isNetworkLock = false;
                    //Debug.Log("데이터 저장 실패");
                }
            );
    }

    public void UpdateExpCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

        //아바타 Url을 이용해서 저장(순위표 리스트 받을때 사용)

        /// 아바타Url 이용해서 Level 저장
        //## Json 형태로 저장

        JsonObject a_MkJson = new JsonObject();
        a_MkJson["UserExp"] = GlobalValue.g_Exp;
        a_MkJson["UserLevel"] = GlobalValue.g_Level;

        string a_StrJson = a_MkJson.ToString();


        var request = new UpdateAvatarUrlRequest()
        {
            ImageUrl = a_StrJson
        };

        isNetworkLock = true;

        PlayFabClientAPI.UpdateAvatarUrl(request,
                       (result) =>
           {
               isNetworkLock = false;
               Debug.Log("데이터 저장 성공");
           },
            (error) =>
            {
                isNetworkLock = false;
                Debug.Log("데이터 저장 실패");
            }
                                         );
    }




    public void PushPacket(PacketType a_PType)
    {
        bool a_IsExist = false;
        for (int i = 0; i < m_PacketBuff.Count; i++)
        {
            if (m_PacketBuff[i] == a_PType)  //아직 처리 되지 않은 패킷이 존재하면.
                a_IsExist = true;
            //또 추가하지 않고 기본 버퍼의 패킷으로 업데이트 한다.
        }

        if (a_IsExist == false)
            m_PacketBuff.Add(a_PType);
        //대기 중인 이 타입의 패킷이 없으면 새로 추가한다.
    }

}
