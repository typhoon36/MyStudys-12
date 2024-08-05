using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using SimpleJSON;

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

    float m_NetWaitDelay = 0.0f;

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

    //float m_NetDelay = 0.0f;

    // Update is called once per frame
    void Update()
    {
        m_NetWaitDelay -= Time.unscaledDeltaTime;
        if (m_NetWaitDelay < 0)
            m_NetWaitDelay = 0.0f;


        if (m_NetWaitDelay <= 0.0f) //���� ��Ŷ ó�� ���� ���°� �ƴϸ�...
        {
            if (0 < m_PacketBuff.Count) //��� ��Ŷ�� �����Ѵٸ�...
            {
                Req_NetWork();
            }
            else  //ó���� ��Ŷ�� �ϳ��� ���ٸ�...
            {
                //�Ź� ó���� ��Ŷ�� �ϳ��� ���� ���� ����ó�� �ؾ� ���� Ȯ���Ѵ�.
                Exe_GameEnd();
            }
        }//if(isNetworkLock == false) //���� ��Ŷ ó�� ���� ���°� �ƴϸ�...

    }//void Update()

    void Req_NetWork()  //RequestNetWork
    {
        if (m_PacketBuff[0] == PacketType.BestScore)
            UpdateScoreCo();
        else if (m_PacketBuff[0] == PacketType.UserGold)
            UpdateGoldCo(); //Playfab ������ ��尻�� ��û �Լ�
        else if (m_PacketBuff[0] == PacketType.UpdateItem)
            UpdateItemCo(); //Playfab ������ ������ ������ ���� ��û �Լ�
        else if (m_PacketBuff[0] == PacketType.UpdateExp)
            UpdateExpCo();

        m_PacketBuff.RemoveAt(0);
    }


    void Exe_GameEnd()  //Execute //�����ϴ�.
    {  //�Ź� ó���� ��Ŷ�� �ϳ��� ���� ���� ����ó�� �ؾ� ���� �Ǵ��ϴ� �Լ�


        //Debug.Log("Exit_Game");
        if (Game_Mgr.Inst.m_GameState == GameState.GameExit)
        {   //"�κ�� �̵�" ��ư�� ������ ���¶��...
            SceneManager.LoadScene("LobbyScene");
        }
        else if (Game_Mgr.Inst.m_GameState == GameState.GameReplay)
        {   //"�ٽ��ϱ�" ��ư�� ������ ���¶��...
            SceneManager.LoadScene("GameScene");
        }

    }//void Exe_GameEnd()  //Execute //�����ϴ�.


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
            }
        };

        m_NetWaitDelay = 0.5f;

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

        m_NetWaitDelay = 0.5f;

        PlayFabClientAPI.UpdateUserData(request,
            (result) =>
            {
                //isNetworkLock = false;
                //Debug.Log("������ ���� ����");
            },
            (error) =>
            {
                //isNetworkLock = false;
                //Debug.Log("������ ���� ���� " + error.GenerateErrorReport());
            });
    }

    void UpdateItemCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            return;     //���������� �α����� �Ǿ� �ִ� ������ ����...

        Dictionary<string, string> a_ItemList = new Dictionary<string, string>();
        for (int i = 0; i < GlobalValue.g_CurSkillCount.Count; i++)
        {
            a_ItemList.Add($"Skill_Item_{i}", GlobalValue.g_CurSkillCount[i].ToString());
        }

        //< �÷��̾� ������(Ÿ��Ʋ) > �� Ȱ�� �ڵ�
        var request = new UpdateUserDataRequest()
        {
            Data = a_ItemList
        };

        m_NetWaitDelay = 0.5f;

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

    public void UpdateExpCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

        //--- AvatarUrl �� �̿��ؼ� �����ϴ� ������ 
        //����ǥ ����Ʈ ���� �� AvatarUrl�� ���� �޾� �� �� �ִ�.

        //--- AvatarUrl(�����󱼻���)�� �̿��ؼ� ������ Leval�� �����ϵ��� Ȱ��
        //---JSON ����
        JSONObject a_MkJSON = new JSONObject();
        a_MkJSON["UserExp"] = GlobalValue.g_Exp;
        a_MkJSON["UserLv"]  = GlobalValue.g_Level;
        string a_strJson = a_MkJSON.ToString();
        //---JSON ����

        var request = new UpdateAvatarUrlRequest()
        {
            ImageUrl = a_strJson
        };

        m_NetWaitDelay = 0.5f;

        PlayFabClientAPI.UpdateAvatarUrl(request,
                (result) =>
                {
                    //Debug.Log("������ ���� ����");

                },
                (error) =>
                {
                    //Debug.LogError(error.GenerateErrorReport());

                }
        );
        //--- AvatarUrl(�����󱼻���)�� �̿��ؼ� ������ Leval�� �����ϵ��� Ȱ��
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
