using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


//[System.Serializable]
//public class UserInfo
//{
//    public string user_id;
//    public string nick_name;
//    public int best_score;
//}
//[System.Serializable]
//public class RkRootInfo
//{
//    public UserInfo[] Rklist;
//    public int my_rank;
//}
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
    public RkInfo[] RkList; // 필드 이름을 JSON 데이터와 일치하도록 수정
    public int my_rank;
}



public class LobbyNetwork_Mgr : MonoBehaviour
{
    bool IsNetworkLock = false;
    List<PacketType> m_PacketBuff = new List<PacketType>();

    string GetRankListUrl = "";
    RkRootInfo m_RkList = new RkRootInfo();

    [HideInInspector] public float RestoreTime = 0.0f;


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

        RestoreTime = 3.0f;

        GetRankList();

    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f < RestoreTime)
            RestoreTime -= Time.deltaTime;



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
                LobbyMgr.Inst.MessageOn("불러오는데 실패했습니다.");
            }
        }
        else
        {
            LobbyMgr.Inst.MessageOn("서버와 연결이 끊겼습니다.");
        }

        www.Dispose();
    }

    void RecMyRankList(string a_StrJon)
    {
        //## Json파일이 맞는지 체크
        if (a_StrJon.Contains("Get_Rank_List_Success~") == false) return;

        //## Json 파싱
        a_StrJon = a_StrJon.Replace("\nGet_Rank_List_Success~", "");
        m_RkList = JsonUtility.FromJson<RkRootInfo>(a_StrJon);

        //## Json 파싱이 제대로 되었는지 체크
        if (m_RkList == null)
        {
            Debug.LogError("JSON 파싱 실패");
            return;
        }

        LobbyMgr.Inst.RefreshRankUI(m_RkList);
    }

}
