using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;



[System.Serializable]
public class RkInfo
{
    public string user_id;
    public string nick_name;
    public int best_score;
}

[System.Serializable]
public class RkRootInfo
{
    public RkInfo[] RkList; // �ʵ� �̸��� JSON �����Ϳ� ��ġ�ϵ��� ����
    public int my_rank;
}



public class LobbyNetwork_Mgr : MonoBehaviour
{
    bool IsNetworkLock = false;
    List<PacketType> m_PacketBuff = new List<PacketType>();

    string GetRankListUrl = "";
    RkRootInfo m_RkList = new RkRootInfo();

    [HideInInspector] public float RestoreTime = 0.0f;

    //## �г��� ����
    [HideInInspector] public string m_NickStrBuff = "";
    [HideInInspector] public ConfigBox m_RefCfgBox = null;
    string UpdateNickUrl = "";

    //# Singleton pattern
    public static LobbyNetwork_Mgr Inst = null;
    void Awake()
    {
        Inst = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        GetRankListUrl = "http://typhoon.dothome.co.kr/Get_ID_Rank.php";
        UpdateNickUrl = "http://typhoon.dothome.co.kr/UpdateNickname.php";

        RestoreTime = 3.0f;

        GetRankList();

    }

    // Update is called once per frame
    void Update()
    {
#if AtuoRestore
        RestoreTime -= Time.deltaTime;
        if (RestoreTime < 0.0f)
        {
            GetRankList();
            RestoreTime = 7.0f;
        }


#else
        //## ���� ��ŷ ����
        if (0.0f < RestoreTime)
            RestoreTime -= Time.deltaTime;

#endif

        if(IsNetworkLock == false)
        {
            if(0 < m_PacketBuff.Count)
            {
                Req_Net();
            }
        }

    }

    void Req_Net()
    {
        if (m_PacketBuff[0] == PacketType.NickUpdate)
            StartCoroutine(UpdateNickCo(m_NickStrBuff));
        
     

        m_PacketBuff.RemoveAt(0);

    }

    IEnumerator UpdateNickCo(string a_NickName)
    {
        if(GlobalValue.g_Unique_ID == "") yield break;

        if(a_NickName == "") yield break;

        IsNetworkLock = true;

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_nick", a_NickName, System.Text.Encoding.UTF8);

        UnityWebRequest www = UnityWebRequest.Post(UpdateNickUrl, form);

        //## Ÿ�Ӿƿ�
        float a_TimeOut = 3.0f;
        bool Is_TimeOut = false;
        float StartTime = Time.unscaledTime;

        //## ��û ������
        yield return www.SendWebRequest();

        //## Ÿ�Ӿƿ��� �߻��ϰų� ���� ������������ ���
        while (!www.isDone && !Is_TimeOut)
        {

            if (Time.unscaledTime - StartTime > a_TimeOut)
            {
                Is_TimeOut = true;
            }

            yield return null;
            
        }

        Debug.Log(Time.unscaledTime - StartTime);

        //## Ÿ�Ӿƿ�
        if (Is_TimeOut)
        {
            www.Abort();

            IsNetworkLock = false;

            if(m_RefCfgBox != null)
            {
                m_RefCfgBox.ResultOkBtn(true, "Request timed out");
            }

            yield break;
        }

        System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        string sz = enc.GetString(www.downloadHandler.data);
        bool IsWait = false;
        string a_MsgStr = "";

        if(www.error == null)
        {
            if(sz.Contains("Update Nick Success.") == true)
            {
                GlobalValue.g_NickName = a_NickName;
                LobbyMgr.Inst.RefreshUserInfo();

                //## ������ �ٲ������ ��ŷ �ޱ�
                GetRankList();
                RestoreTime = 7.0f;

            }
            else if(sz.Contains("Nickname does exist.") == true)
            {
                IsWait = true;
                a_MsgStr = "�ߺ��� �г����Դϴ�.";
            }
            

        }
        else
        {
            IsWait = true;
            a_MsgStr = sz + " : " + www.error;
        }

        www.Dispose();
        IsNetworkLock = false;

        if(m_RefCfgBox != null)
        {
            m_RefCfgBox.ResultOkBtn(IsWait, a_MsgStr);
        }
    }


    public void GetRankList()
    {
        StartCoroutine(GetRankListCo());
    }

    IEnumerator GetRankListCo()
    {
        if (GlobalValue.g_Unique_ID == "") yield break;

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);

        UnityWebRequest www = UnityWebRequest.Post(GetRankListUrl, form);

        yield return www.SendWebRequest();

        if (www.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;

            string a_Restr = enc.GetString(www.downloadHandler.data);


            if (a_Restr.Contains("Get_Rank_List_Success~") == true)
            {
                RecMyRankList(a_Restr);
            }
            else
            {
                LobbyMgr.Inst.MessageOn("�ҷ����µ� �����߽��ϴ�.");
            }
        }
        else
        {
            LobbyMgr.Inst.MessageOn("������ ������ ������ϴ�.");
        }

        www.Dispose();
    }

    void RecMyRankList(string a_StrJon)
    {
        //## Json������ �´��� üũ
        if (a_StrJon.Contains("Get_Rank_List_Success~") == false) return;

        //## Json �Ľ�
        a_StrJon = a_StrJon.Replace("\nGet_Rank_List_Success~", "");
        m_RkList = JsonUtility.FromJson<RkRootInfo>(a_StrJon);

        //## Json �Ľ��� ����� �Ǿ����� üũ
        if (m_RkList == null)
        {
            Debug.LogError("JSON �Ľ� ����");
            return;
        }

        LobbyMgr.Inst.RefreshRankUI(m_RkList);
    }

    public void PushPacket(PacketType a_Packet)
    {
        bool isExist = false;

        for (int i = 0; i < m_PacketBuff.Count; i++)
        {
            if (m_PacketBuff[i] == a_Packet)
            {
                isExist = true;
                break;
            }
        }


        if (isExist == false)
            m_PacketBuff.Add(a_Packet); 

    }

}
