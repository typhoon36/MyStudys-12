using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;


public enum PacketType { Bestscore, UserGold, NickName, InfoUpdate, FloorUpdate, ClearSave }
// ���� �ְ�����,�������, �г���, ���� ���� ����, ��������, ���� �������� �ʱ�ȭ�� ����.

public class Network_Mgr : MonoBehaviour
{
    //# ���� ������ ��Ŷ ó�� ����Ʈ(ť��ü)
    bool isNetworkLock = false;
    float m_NetWaitTime = 0.0f;
    List<PacketType> m_packetBuff = new List<PacketType>();

    //# Url ������
    string BestScoreUrl = "";
    string MyGoldUrl = "";
    string InfoUpdateUrl = "";
    string UpdateFloorUrl = "";

    //# Singleton pattern
    public static Network_Mgr Inst = null;
    void Awake()
    {
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        BestScoreUrl = "http://typhoon.dothome.co.kr/UpdateBScore.php";
        MyGoldUrl = "http://typhoon.dothome.co.kr/UpdateMyGold.php";
    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f < m_NetWaitTime)
        {

            //## ��Ʈ��ũ ���ð� ó��
            m_NetWaitTime -= Time.unscaledDeltaTime;
            if (m_NetWaitTime <= 0.0f)
            {
                isNetworkLock = false;
                //Debug.Log("��Ʈ��ũ ���ð� ����");
            }

        }
        //## ��Ŷ ó��
        if (isNetworkLock == false)
        {
            if (0 < m_packetBuff.Count)
            {
                Req_Net();
            }

        }

    }


    void Req_Net()
    {
        if (m_packetBuff[0] == PacketType.Bestscore)
            StartCoroutine(UpdateBestScoreCo());
        else if (m_packetBuff[0] == PacketType.UserGold)
            StartCoroutine(UpdateGoldCo());


            m_packetBuff.RemoveAt(0);
        
    }

    IEnumerator UpdateBestScoreCo()
    {
        // ������ �α��� ����
        if (GlobalValue.g_Unique_ID == "") yield break;

        //## Form ����
        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID);
        form.AddField("Input_score", GlobalValue.g_BestScore.ToString());

        //## ������ ����
        isNetworkLock = true;
        m_NetWaitTime = 3.0f;//3�� ��� �� ���� ������ ��Ŷ ó��

        UnityWebRequest a_Request = UnityWebRequest.Post(BestScoreUrl, form);
        yield return a_Request.SendWebRequest();

        if (a_Request.error == null)
        {
            //Debug.Log("Update Success");
        }
        else
        {
            Debug.Log(a_Request.error);
        }

        a_Request.Dispose();
        //## ��Ʈ��ũ ���ð� ����
        isNetworkLock = false;
        m_NetWaitTime = 0.0f;
    }

    IEnumerator UpdateGoldCo()
    {
        if(GlobalValue.g_Unique_ID == "") yield break;  
        
        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID,
            System.Text.Encoding.UTF8);

        form.AddField("Input_gold", GlobalValue.g_UserGold);

        isNetworkLock = true;
        m_NetWaitTime = 3.0f;

        UnityWebRequest a_Request = UnityWebRequest.Post(MyGoldUrl, form);
        yield return a_Request.SendWebRequest();//������ ����
        
        if(a_Request.error == null)
        {
            //Debug.Log("Update Success");
        }
        else
        {
            Debug.Log(a_Request.error);
        }

        a_Request.Dispose();

        isNetworkLock = false;
        m_NetWaitTime = 0.0f;

    }


    public void PushPacket(PacketType a_Packet)
    {
        bool a_IsExist = false;
        for (int i = 0; i < m_packetBuff.Count; i++)
        {
            //## �ߺ� ��Ŷ�� ����
            if (m_packetBuff[i] == a_Packet)
            {
                a_IsExist = true;
                break;
            }
        }

        //## �ߺ� ��Ŷ�� ���ٸ� �߰�
        if(a_IsExist == false)
            m_packetBuff.Add(a_Packet);
        

    }

}


