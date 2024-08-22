using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PacketType
{
    BestScore,      //�ְ�����
    UserGold,       //�������
    UpdateItem,     //������ ������ ����
    NickUpdate,     //�г��Ӱ���
    UpdateExp,      //����ġ����
}

public class NetworkMgr : MonoBehaviour
{
    //--- ������ ������ ��Ŷ ó���� ť ���� ����
    List<PacketType> m_PacketBuff = new List<PacketType>();
    //���� ��Ŷ Ÿ�� ��� ����Ʈ (ť ����)

    float m_NetWaitTime = 0.0f;

    //�̱��� ������ ���� �ν��Ͻ� ���� ����
    public static NetworkMgr Inst = null;

    void Awake()
    {
        //NetworkMgr Ŭ������ �ν��Ͽ� ����
        Inst = this;
    }
    //�̱��� ������ ���� �ν��Ͻ� ���� ����

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

        if (m_NetWaitTime <= 0.0f) //���� ��Ŷ ó�� ���� ���°� �ƴϸ�...
        {
            if (0 < m_PacketBuff.Count) //��� ��Ŷ�� �����Ѵٸ�...
            {
                Req_NetWork();
            }
        }//if(m_NetWaitTime <= 0.0f) //���� ��Ŷ ó�� ���� ���°� �ƴϸ�...        
    }//void Update()

    void Req_NetWork()  //RequestNetWork
    {
        if (m_PacketBuff[0] == PacketType.BestScore)
            UpdateScoreCo();
        else if (m_PacketBuff[0] == PacketType.UserGold)
            UpdateGoldCo(); //Playfab ������ ��尻�� ��û �Լ�
        else if (m_PacketBuff[0] == PacketType.UpdateItem)
            UpdateItemCo(); //Playfab ������ ������ ������ ���� ��û �Լ�
        //else if (m_PacketBuff[0] == PacketType.UpdateExp)
        //    UpdateExpCo();

        m_PacketBuff.RemoveAt(0);
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

        m_NetWaitTime = 0.5f;

        PlayFabClientAPI.UpdatePlayerStatistics(
                        request,

                        (result) =>
                        { //������Ʈ ������ ���� �Լ�

                        },

                        (error) =>
                        { //������Ʈ ���н� ���� �Լ�

                        }
             );
    }

    void UpdateGoldCo() //Playfab ������ ��尻�� ��û �Լ�
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

        //var request = new UpdateUserDataRequest();
        //request.Permission = UserDataPermission.Private;
        //request.Data = new Dictionary<string, string>();
        //request.Data.Add("UserGold", GlobalValue.g_UserGold.ToString());
        //request.Data.Add("Level", GlobalValue.g_Level.ToString());
        //request.Data.Add("UserStar", GlobalValue.g_UserStar.ToString());

        // < �÷��̾� ������(Ÿ��Ʋ) > �� Ȱ�� �ڵ�
        var request = new UpdateUserDataRequest()
        {
            //Permission = UserDataPermission.Private, //����Ʈ��
            //Permission = UserDataPermission.Public,
            //Public �������� : �ٸ� �������� �� ���� �ְ� �ϴ� �ɼ�
            //Private ����� ����(�⺻������) : ���� �� �� �ִ� ���� �Ӽ��� ����

            Data = new Dictionary<string, string>()
            {
                { "UserGold", GlobalValue.g_UserGold.ToString() },
                //{ "Level", GlobalValue.g_Level.ToString() },
                //{ "UserStar", GlobalValue.g_UserStar.ToString() },
            }
        };

        m_NetWaitTime = 0.5f;

        PlayFabClientAPI.UpdateUserData(request,
            (result) =>
            {
                //Debug.Log("������ ���� ����");
            },
            (error) =>
            {
                //Debug.Log("������ ���� ���� " + error.GenerateErrorReport());
            });
    }

    void UpdateItemCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            return;     //���������� �α����� �Ǿ� �ִ� ������ ����...

        Dictionary<string, string> a_ItemList = new Dictionary<string, string>();
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            a_ItemList.Add($"SkItem_{i}", GlobalValue.g_SkillCount[i].ToString());
        }

        //< �÷��̾� ������(Ÿ��Ʋ) > �� Ȱ�� �ڵ�
        var request = new UpdateUserDataRequest()
        {
            Data = a_ItemList
        };

        m_NetWaitTime = 0.5f;

        PlayFabClientAPI.UpdateUserData(request,
                (result) =>
                {
                    //Debug.Log("������ ���� ����");
                },
                (error) =>
                {
                    //Debug.Log("������ ���� ����");
                }
            );
    }

    public void PushPacket(PacketType a_PType)
    {
        bool a_IsExist = false;
        for (int i = 0; i < m_PacketBuff.Count; i++)
        {
            if (m_PacketBuff[i] == a_PType)  //���� ó�� ���� ���� ��Ŷ�� �����ϸ�.
                a_IsExist = true;
            //�� �߰����� �ʰ� �⺻ ������ ��Ŷ���� ������Ʈ �Ѵ�.
        }

        if (a_IsExist == false)
            m_PacketBuff.Add(a_PType);
        //��� ���� �� Ÿ���� ��Ŷ�� ������ ���� �߰��Ѵ�.
    }

    public bool IsBackLobbyOk()
    {
        //���� ��Ŷ ó�� ���� ���°ų� ��� ��Ŷ�� �����Ѵٸ�...
        if (0.0f < m_NetWaitTime || 0 < m_PacketBuff.Count)
            return false;

        return true;
    }
}
