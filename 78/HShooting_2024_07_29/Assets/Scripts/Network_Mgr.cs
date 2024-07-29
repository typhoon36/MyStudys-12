using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public enum PacketType
{
    BestScore,
    UserGold,
    NickUpdate,
    UpdateExp,
}

public class Network_Mgr : MonoBehaviour
{
    //## 패킷 처리 (서버 전송 목적)
    bool isNetworkQueue = false;
    /// <summary> 서버로 전송할 패킷 버퍼 list </summary>
    List<PacketType> m_PacketBuff = new List<PacketType>();

    //## 싱글톤
    public static Network_Mgr Inst;



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
        if (isNetworkQueue == false)
        {
            if (0 < m_PacketBuff.Count)
            {
                Req_Net();
            }
        }
    }

    void Req_Net()
    {
        if (m_PacketBuff[0] == PacketType.BestScore)
            UpdateScoreCo();

        else if (m_PacketBuff[0] == PacketType.UserGold)
            UpdateGoldCo(); //원래 코루틴으로 해야하나 일반함수로 대체(골드 요청 함수)

        m_PacketBuff.RemoveAt(0);
    }

    void UpdateScoreCo()
    {
        if (GlobalValue.g_Unique_ID == "")
        {
            Debug.Log("유저 고유 ID가 없습니다.");
            return;
        }

        var request = new UpdatePlayerStatisticsRequest
        {
            ///<summary>플레이어의 통계 데이터를 업데이트</summary>
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate{StatisticName = "BestScore",Value = GlobalValue.g_BestScore}  //,
                //new StatisticUpdate{ StatisticName = "BestLevel", Value = GlobalValue.g_BestLevel}
            }
        };
        isNetworkQueue = true;

        PlayFabClientAPI.UpdatePlayerStatistics(request,
        (result) =>
        {
            isNetworkQueue = false;

        },
        (error) =>
         {
            isNetworkQueue = false;

        });
    }

    void UpdateGoldCo()
    {
        //## 정상 로그인인지 확인
        if (GlobalValue.g_Unique_ID == "")
        {
            Debug.Log("유저 고유 ID가 없습니다.");
            return;
        }

        /// <summary> 플레이어 데이터 값 활용 로직 </summary>
        var request = new UpdateUserDataRequest()
        {
            ///<summary"<나중에 다룰 퍼미션>"</summary>
            //Permission = UserDataPermission.Private, //default is private(골드값이라 다른 유저에게 공개할 필요가 없다)
            //Permission = UserDataPermission.Public, (계급 레벨 등 공개 정보)

            //## 업데이트할 데이터
            Data = new Dictionary<string, string>
            {
                { "UserGold", GlobalValue.g_UserGold.ToString() }
            }



        };

        isNetworkQueue = true;
        PlayFabClientAPI.UpdateUserData(request, (result) =>
        {
            isNetworkQueue = false;

        },
        (error) =>
        {
            isNetworkQueue = false;


        });

    }





    public void PushPacket(PacketType a_Type)
    {
        bool isExist = false;
        for (int i = 0; i < m_PacketBuff.Count; i++)
        {
            //### 이미 존재하는 패킷이면 중복으로 넣지 않는다.
            if (m_PacketBuff[i] == a_Type)
                isExist = true;


        }

        if (isExist == false)
            m_PacketBuff.Add(a_Type);

    }




}
