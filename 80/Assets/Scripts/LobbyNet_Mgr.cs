using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;


//# �κ�+��Ʈ��ũ(�κ񿡼� ��Ʈ��ũ�ϵ���)   
public class LobbyNet_Mgr : MonoBehaviour
{
    public enum PacketType{GetRankList,GetMyRank,}

    //## ��Ŷ ó��
    bool IsNetWorking = false;
    List<PacketType> m_PacketBuff = new List<PacketType>();//<--�ܼ� ��Ŷ ���� �ʿ��ִٴ� ���� ť





    //## �̱���
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
        //## ��Ŷó��
        if(IsNetWorking == false)
        {
            if (0 < m_PacketBuff.Count)
            {
                Req_NetWork();
            }
        }

    }

    //## ��Ŷ ��û
    void Req_NetWork()
    {

        if (m_PacketBuff[0]== PacketType.GetRankList)
            GetRankList();

        m_PacketBuff.RemoveAt(0);

    }



    void GetRankList()
    {
        //## �α׾ƿ�����
        if (GlobalValue.g_Unique_ID == "")
            return;
        //## �α����� ����
        var request = new GetLeaderboardRequest
        {
            //1����� ����
            StartPosition = 0,
            //playfab�� ����ǥ ���� ����("BestScore")
            StatisticName = "BestScore",

            //## 10����� �������°�.ex)startPostion - 10 Maxresultscount 20���� �ϸ� 10��~20�����.
            MaxResultsCount = 10,

            //## ������ ���� �������� �Լ�
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true,
                ShowAvatarUrl = true, //������ ���� �ּҸ� ��û(����ġ ���)

            }
        };

        IsNetWorking = true;

        PlayFabClientAPI.GetLeaderboard(
            request,
            (result) =>
            {
                //����
                
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

                    //## ��� �ȿ� ���� ����� ��ǥ��
                    if (curBoard.PlayFabId == GlobalValue.g_Unique_ID)
                        a_StrBuff += "<color=#00ff00>";

                    a_StrBuff += (i + 1).ToString() + "�� :" +
                    curBoard.DisplayName + " : " + curBoard.StatValue + "�� : " + "\n";

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


    //# �� ��ŷ ��������
    void GetMyRank()
    {
        //GetLeaderboardAroundPlayer (Ư�� Id�� �ֺ����� ��ŷ�� ������)
        //��,PlayFabId �ֺ����� ����Ʈ�� �ҷ����� �Լ�
        var request = new GetLeaderboardAroundPlayerRequest
        {
            //PlayFabId = GlobalValue.g_Unique_ID, --> ���� ����(���� ���ϸ� �α��� �� ID ������ �Ǿ����)

            StatisticName = "BestScore",

            //�� ������ �޾ƿ�
            MaxResultsCount = 1



            //ProfileConstraints = new PlayerProfileViewConstraints()
            //{
            //    ShowDisplayName = true, //���⼭�� �ʿ����(���ΰ��� �˰�������)
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
                                          Lobby_Mgr.Inst.m_MyRank = CurBoard.Position + 1; //0���� �����ϴϱ� +1
                                          GlobalValue.g_BestScore = (int)CurBoard.StatValue;//�ְ����� ����
                                          Lobby_Mgr.Inst.CfgResponse();
                                      }

                                      IsNetWorking = false;

                                  },
                                  (error) =>
                                  {
                                      Debug.Log("�� ��ŷ �������� ����");
                                      IsNetWorking = false;
                                  }
                                    );
    }

    public void PushPacket(PacketType a_PType)
    {
        bool a_IsExist = false;

        for (int i = 0; i < m_PacketBuff.Count; i++)
        {
            //## ó���������� ��Ŷ �����
            if (m_PacketBuff[i] == a_PType)
            {
                a_IsExist = true;
            }
        }

        //## ó������ ���� ��Ŷ�� ���ٸ�
        if (a_IsExist == false)
        m_PacketBuff.Add(a_PType);//�߰�

        
    }

}
