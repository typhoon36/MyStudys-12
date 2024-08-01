using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.SceneManagement;

public enum PacketType
{
    BestScore,      //�ְ�����
    UserGold,       //�������
    UpdateItme,     //�����۰���
    NickUpdate,     //�г��Ӱ���
    UpdateExp,      //����ġ����
}

public class NetworkMgr : MonoBehaviour
{
    //--- ������ ������ ��Ŷ ó���� ť ���� ����
    bool isNetworkLock = false;     //Network ��� ���� ���� ����
    List<PacketType> m_PacketBuff = new List<PacketType>();
    //���� ��Ŷ Ÿ�� ��� ����Ʈ (ť ����)

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
        if (isNetworkLock == false) //���� ��Ŷ ó�� ���� ���°� �ƴϸ�...
        {
            if (0 < m_PacketBuff.Count) //��� ��Ŷ�� �����Ѵٸ�...
            {
                Req_NetWork();
            }

            //### ó�� ��Ŷ ���� x
            else
            {
                Exec_GameEnd();
            }

        }//if(isNetworkLock == false) //���� ��Ŷ ó�� ���� ���°� �ƴϸ�...




    }//void Update()

    //# Execute GameEnd
    float m_ExitTime = 0.3f;

    void Exec_GameEnd()
    {

        if (isNetworkLock == true) //��Ʈ��ũ �۾� ���̸� ����
            return;


        if (Game_Mgr.Inst.m_GameState == GameState.GameExit ||
            Game_Mgr.Inst.m_GameState == GameState.GameReplay)
        {

            m_ExitTime -= Time.unscaledDeltaTime;

            if (m_ExitTime <= 0.0f)
                Exit_Game();





        }

    }



    void Exit_Game()
    {
        //## ó�� ��Ŷ 0�϶��� ����ó�� 

        if (Game_Mgr.Inst.m_GameState == GameState.GameExit)
        {
            //### �κ���̵�
            SceneManager.LoadScene("LobbyScene");
        }
        else if (Game_Mgr.Inst.m_GameState == GameState.GameReplay)
        {
            //### ���� �����
            SceneManager.LoadScene("GameScene");
        }


    }



    void Req_NetWork()  //RequestNetWork
    {
        if (m_PacketBuff[0] == PacketType.BestScore)
            UpdateScoreCo();
        else if (m_PacketBuff[0] == PacketType.UserGold)
            UpdateGoldCo(); //Playfab ������ ��尻�� ��û �Լ�

        else if (m_PacketBuff[0] == PacketType.UpdateItme)
        {
            UpdateItemCo();
        }


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

        isNetworkLock = true;

        PlayFabClientAPI.UpdatePlayerStatistics(
                        request,

                        (result) =>
                        { //������Ʈ ������ ���� �Լ�
                            isNetworkLock = false;
                        },

                        (error) =>
                        { //������Ʈ ���н� ���� �Լ�
                            isNetworkLock = false;
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

        isNetworkLock = true;
        PlayFabClientAPI.UpdateUserData(request,
            (result) =>
            {
                isNetworkLock = false;
                //Debug.Log("������ ���� ����");
            },
            (error) =>
            {
                isNetworkLock = false;
                //Debug.Log("������ ���� ���� " + error.GenerateErrorReport());
            });
    }


    //# ������ ����
    void UpdateItemCo()
    {

        if (GlobalValue.g_Unique_ID == "")
            return;

        //������ ����
        Dictionary<string, string> a_ItemList = new Dictionary<string, string>();

        for (int i = 0; i < GlobalValue.g_CurSkillCount.Count; i++)
        {
            a_ItemList.Add($"Skill_Item_{i}", GlobalValue.g_CurSkillCount[i].ToString());
        }

        /// �÷��̾� ������ �� Ȱ��
        var request = new UpdateUserDataRequest()
        {
            Data = a_ItemList
        };


        isNetworkLock = true;

        PlayFabClientAPI.UpdateUserData(request,
                                  (result) =>
                                  {
                                      isNetworkLock = false;
                                      //Debug.Log("������ ���� ����");
                                  },
                                  (error) =>
                                  {
                                      isNetworkLock = false;
                                      //Debug.Log("������ ���� ����");
                                  });
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

   


}
