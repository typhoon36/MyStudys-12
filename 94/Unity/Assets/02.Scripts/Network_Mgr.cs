using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;


public enum PacketType { Bestscore, UserGold, NickUpdate, InfoUpdate, FloorUpdate, ClearSave }
// 각각 최고점수,유저골드, 닉네임, 각종 정보 갱신, 층수정보, 서버 저장정보 초기화를 말함.

public class Network_Mgr : MonoBehaviour
{
    //# 서버 전송할 패킷 처리 리스트(큐대체)
    bool isNetworkLock = false;
    float m_NetWaitTime = 0.0f;
    List<PacketType> m_packetBuff = new List<PacketType>();

    //# Url 변수들
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
        InfoUpdateUrl = "http://typhoon.dothome.co.kr/InfoUpdate.php";
        UpdateFloorUrl = "http://typhoon.dothome.co.kr/UpdateFloor.php";
    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f < m_NetWaitTime)
        {

            //## 네트워크 대기시간 처리
            m_NetWaitTime -= Time.unscaledDeltaTime;
            if (m_NetWaitTime <= 0.0f)
            {
                isNetworkLock = false;
                //Debug.Log("네트워크 대기시간 해제");
            }

        }
        //## 패킷 처리
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
        else if (m_packetBuff[0] == PacketType.InfoUpdate)
            StartCoroutine(UpdateInfoCo());
        else if (m_packetBuff[0] == PacketType.FloorUpdate)
            StartCoroutine(UpdateFloorCo());

            m_packetBuff.RemoveAt(0);
        
    }

    IEnumerator UpdateFloorCo() 
    { 
        if(GlobalValue.g_Unique_ID == "") yield break;
        
        //## Json 파일 생성
        Floor_Info a_Finfo = new Floor_Info();
        a_Finfo.CurFloor = GlobalValue.g_CurFloorNum;
        a_Finfo.BestFloor = GlobalValue.g_BestFloor;
        string a_StrJson = JsonUtility.ToJson(a_Finfo);

        Debug.Log(a_StrJson);

        //## Form 생성
        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Floor_Info", a_StrJson, System.Text.Encoding.UTF8);

        //## 서버에 접속
        isNetworkLock = true;
        m_NetWaitTime = 3.0f;

        UnityWebRequest a_Request = UnityWebRequest.Post(UpdateFloorUrl, form);
        yield return a_Request.SendWebRequest();

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

    IEnumerator UpdateBestScoreCo()
    {
        // 비정상 로그인 상태
        if (GlobalValue.g_Unique_ID == "") yield break;

        //## Form 생성
        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID);
        form.AddField("Input_score", GlobalValue.g_BestScore.ToString());

        //## 서버에 접속
        isNetworkLock = true;
        m_NetWaitTime = 3.0f;//3초 대기 후 응답 없을시 패킷 처리

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
        //## 네트워크 대기시간 해제
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
        yield return a_Request.SendWebRequest();//서버에 접속
        
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

    IEnumerator UpdateInfoCo()
    {
        if(GlobalValue.g_Unique_ID == "") yield break;

        //## JSon 파일 생성
        ItemList a_ItList = new ItemList();
        a_ItList.SkList = new int[GlobalValue.g_SkillCount.Length];
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            a_ItList.SkList[i] = GlobalValue.g_SkillCount[i];
        }

        //## JSon 파일로 변환
        string a_StrJson = JsonUtility.ToJson(a_ItList);

        //## Form 생성
        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID,System.Text.Encoding.UTF8);
        form.AddField("Item_List", a_StrJson,System.Text.Encoding.UTF8);

        //## 서버에 접속
        isNetworkLock = true;
        m_NetWaitTime = 3.0f;

        UnityWebRequest a_Request = UnityWebRequest.Post(InfoUpdateUrl, form);
        yield return a_Request.SendWebRequest();


        if(a_Request.error == null) //## 성공시
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
            //## 중복 패킷은 무시
            if (m_packetBuff[i] == a_Packet)
            {
                a_IsExist = true;
                break;
            }
        }

        //## 중복 패킷이 없다면 추가
        if(a_IsExist == false)
            m_packetBuff.Add(a_Packet);
        

    }

}


