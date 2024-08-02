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
        GetRankingList,     //��ŷ ����Ʈ ��������
        GetMyRanking,       //�� ��� ��������
    }

    //--- ������ ������ ��Ŷ ó���� ť ���� ����
    bool isNetworkLock = false;
    List<PacketType> m_PacketBuff = new List<PacketType>();
    //�ܼ��� � ��Ŷ�� ���� �ʿ䰡 �ִ� ��� ���� PacketBuffer <ť>
    //--- ������ ������ ��Ŷ ó���� ť ���� ����

    //--- �̱��� ������ ���� �ν��Ͻ� ���� ����
    public static LobbyNetworkMgr Inst = null;

    void Awake()
    {
        //NetworkMgr Ŭ������ �ν��Ͻ��� ����
        Inst = this;
    }
    //--- �̱��� ������ ���� �ν��Ͻ� ���� ����

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isNetworkLock == false)  //���� ��Ŷ ó�� ���� ���°� �ƴϸ�...
        {
            if (0 < m_PacketBuff.Count) //��� ��Ŷ�� �����Ѵٸ�...
            {
                Req_Network();
            }
        }
    }//void Update()

    void Req_Network()   //RequestNetwork
    {
        if (m_PacketBuff[0] == PacketType.GetRankingList)
            GetRankingList();

        m_PacketBuff.RemoveAt(0);
    }

    void GetRankingList() //���� �ҷ�����...
    {
        if (GlobalValue.g_Unique_ID == "")  //�α��� ���¿�����...
            return;

        var request = new GetLeaderboardRequest
        {
            StartPosition = 0,              //0�� �ε��� �� 1�����
            StatisticName = "BestScore",
            //������ �������� ����ǥ ���� �� "BestScore" ����
            MaxResultsCount = 10,           //10�����
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true, //�г��ӵ� ��û
                ShowAvatarUrl = true  //���� ���� ����� �ּҵ� ��û(�̰� ����ġ�� ���)
            }
        };

        isNetworkLock = true;

        PlayFabClientAPI.GetLeaderboard(
            request,

            (result) =>
            { //��ŷ ����Ʈ �޾ƿ��� ����

                if (Lobby_Mgr.Inst == null)
                {
                    isNetworkLock = false;
                    return;
                }

                if (Lobby_Mgr.Inst.m_Ranking_Text == null)
                {
                    isNetworkLock = false;
                    return;
                }

                string a_strBuff = "";

                for (int i = 0; i < result.Leaderboard.Count; i++)
                {
                    var curBoard = result.Leaderboard[i];
                    int a_ULevel = LvMyJsonParser(curBoard.Profile.AvatarUrl);

                    //��� �ȿ� ���� �ִٸ� �� ǥ��
                    if (curBoard.PlayFabId == GlobalValue.g_Unique_ID)
                        a_strBuff += "<color=#008800>";

                    a_strBuff += (i + 1).ToString() + "�� : " +
                                    curBoard.DisplayName + " (Lv"+ (a_ULevel + 1) + ") : " +
                                    curBoard.StatValue + "��" + "\n";

                    //��� �ȿ� ���� �ִٸ� �� ǥ��
                    if (curBoard.PlayFabId == GlobalValue.g_Unique_ID)
                        a_strBuff += "</color>";

                }//for(int i = 0; i < result.Leaderboard.Count; i++)

                if (a_strBuff != "")
                    Lobby_Mgr.Inst.m_Ranking_Text.text = a_strBuff;

                //�������� ����� �ҷ��� �� �� �� ����� �ҷ� �´�.
                GetMyRanking();
            },

            (error) =>
            { //��ŷ ����Ʈ ����� ���� ���� �� 
                Debug.Log("�������� �ҷ����� ����");
                isNetworkLock = false;
            }
       );
    }//void GetRankingList() //���� �ҷ�����...

    void GetMyRanking()  //�� ��� �ҷ�����...
    {
        //GetLeaderboardAroundPlayer() : 
        //�� �Լ��� Ư�� PlayFabId(����ƮID) �ֺ����� ����Ʈ�� �ҷ����� �Լ��̴�.

        var request = new GetLeaderboardAroundPlayerRequest
        {
            //PlayFabId = GlobalValue.g_Unique_ID,
            //�������� ������ �� ����Ʈ ID(�α��ε� ID) ������ �ȴ�.
            StatisticName = "BestScore",
            MaxResultsCount = 1,    //�Ѹ� ������ �޾ƿ���� ��

            //ProfileConstraints = new PlayerProfileViewConstraints()
            //{
            //    ShowDisplayName = true  //�� �ɼ����� ���� ������ �޾ƿ� �� �ִµ�... ���ΰ��� �ƴϱ� ����
            //}
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(
                request,

                (result) =>
                {
                    if (Lobby_Mgr.Inst == null)
                    {
                        isNetworkLock = false;
                        return;
                    }

                    if (0 < result.Leaderboard.Count)
                    {
                        var curBoard = result.Leaderboard[0];
                        Lobby_Mgr.Inst.m_My_Rank = curBoard.Position + 1; //�� ��� ��������...
                        GlobalValue.g_BestScore = curBoard.StatValue; //�� �ְ� ���� ����

                        Lobby_Mgr.Inst.CfgResponse();  //<-- UI ����
                    }

                    isNetworkLock = false;
                },

                (error) =>
                {
                    Debug.Log("�� ��� �ҷ����� ����");
                    isNetworkLock = false;
                }
            );
    }//void GetMyRanking()

    public void PushPacket(PacketType a_PType)
    {
        bool a_isExist = false;
        for (int i = 0; i < m_PacketBuff.Count; i++)
        {
            //���� ó�� ���� ���� ��Ŷ�� �����ϸ�
            if (m_PacketBuff[i] == a_PType)
                a_isExist = true;
            //�� �߰����� �ʰ� ���� ������ ��Ŷ���� ������Ʈ �Ѵ�.
        }

        if (a_isExist == false)
            m_PacketBuff.Add(a_PType);
        //��� ���� �� Ÿ���� ��Ŷ�� ������ ���� �߰��Ѵ�.
    }

    int LvMyJsonParser(string AvatarUrl)
    {
        int a_Level = 0;

        //---- ���� ��������
        //--- JSON �Ľ�
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
        //--- JSON �Ľ�
        //---- ���� ��������

        return 0;
    }
}
