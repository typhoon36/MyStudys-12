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
    //## ��Ŷ ó�� (���� ���� ����)
    bool isNetworkQueue = false;
    /// <summary> ������ ������ ��Ŷ ���� list </summary>
    List<PacketType> m_PacketBuff = new List<PacketType>();

    //## �̱���
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
            UpdateGoldCo(); //���� �ڷ�ƾ���� �ؾ��ϳ� �Ϲ��Լ��� ��ü(��� ��û �Լ�)

        m_PacketBuff.RemoveAt(0);
    }

    void UpdateScoreCo()
    {
        if (GlobalValue.g_Unique_ID == "")
        {
            Debug.Log("���� ���� ID�� �����ϴ�.");
            return;
        }

        var request = new UpdatePlayerStatisticsRequest
        {
            ///<summary>�÷��̾��� ��� �����͸� ������Ʈ</summary>
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
        //## ���� �α������� Ȯ��
        if (GlobalValue.g_Unique_ID == "")
        {
            Debug.Log("���� ���� ID�� �����ϴ�.");
            return;
        }

        /// <summary> �÷��̾� ������ �� Ȱ�� ���� </summary>
        var request = new UpdateUserDataRequest()
        {
            ///<summary"<���߿� �ٷ� �۹̼�>"</summary>
            //Permission = UserDataPermission.Private, //default is private(��尪�̶� �ٸ� �������� ������ �ʿ䰡 ����)
            //Permission = UserDataPermission.Public, (��� ���� �� ���� ����)

            //## ������Ʈ�� ������
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
            //### �̹� �����ϴ� ��Ŷ�̸� �ߺ����� ���� �ʴ´�.
            if (m_PacketBuff[i] == a_Type)
                isExist = true;


        }

        if (isExist == false)
            m_PacketBuff.Add(a_Type);

    }




}
