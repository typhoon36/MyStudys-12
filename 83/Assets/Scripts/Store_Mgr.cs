using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class Store_Mgr : MonoBehaviour
{
    public Button BackBtn;
    public Text m_UserInfoText = null;

    public GameObject m_Item_ScContent; //ScrollView Content 차일드로 생성될 Parent 객체
    public GameObject m_SkProductNode;  //Node Prefab

    SkProductNode[] m_SkNodeList;       //스크롤에 붙어 있는 Item 목록들...

    //--- 지금 뭘 구입하려고 시도한 건지? 저장해 놓기 위한 변수
    SkillType m_BuySkType;  //어떤 스킬 아이템을 구입하려고 한 건지?
    int m_SvMyGold;         //구입 프로세스에 진입 후 상태 저장용 : 차감된 내 골드가 얼마인지?
    int m_SvMyCount = 0;    //스킬 보유수 증가 백업해 놓기...
    //--- 지금 뭘 구입하려고 시도한 건지? 저장해 놓기 위한 변수

    // Start is called before the first frame update
    void Start()
    {
        GlobalValue.LoadGameData();

        if (BackBtn != null)
            BackBtn.onClick.AddListener(BackBtnClick);

        if (m_UserInfoText != null)
            m_UserInfoText.text = "별명(" + GlobalValue.g_NickName + ") : 보유골드(" +
                                    GlobalValue.g_UserGold + ")";

        //--- 아이템 목록 추가
        GameObject a_ItemObj = null;
        SkProductNode a_SkItemNode = null;
        for(int i = 0; i < GlobalValue.g_SkDataList.Count; i++)
        {
            a_ItemObj = Instantiate(m_SkProductNode);
            a_SkItemNode = a_ItemObj.GetComponent<SkProductNode>();
            a_SkItemNode.InitData(GlobalValue.g_SkDataList[i].m_SkType);
            a_ItemObj.transform.SetParent(m_Item_ScContent.transform, false);
        }
        //--- 아이템 목록 추가

        RefreshSkItemList();

    }//void Start()

    // Update is called once per frame
    void Update()
    {
        
    }

    void BackBtnClick()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    void RefreshSkItemList()
    {
        if(m_Item_ScContent != null)
        {
            if (m_SkNodeList == null || m_SkNodeList.Length <= 0)
                m_SkNodeList = m_Item_ScContent.GetComponentsInChildren<SkProductNode>();
        }

        for(int i = 0; i < m_SkNodeList.Length; i++)
        {
            m_SkNodeList[i].RefreshState();
        }
    }//void RefreshSkItemList()

    public void BuySkillItem(SkillType a_SkType)
    {  //리스트 뷰에 있는 캐릭터 가격 버튼을 눌러 구입 시도를 한 경우
        m_BuySkType = a_SkType;
        BuyBeforeJobCo();  
    }

    void BuyBeforeJobCo()  //구매 1단계 함수
    {  //서버로부터 골드, 아이템 상태 받아와서 클라이언트와 동기화 시켜주기...
        if (GlobalValue.g_Unique_ID == "")
            return;

        //< 플레이어 데이터(타이틀) > 값 활용 코드
        var request = new GetUserDataRequest()
        {
            PlayFabId = GlobalValue.g_Unique_ID
        };

        PlayFabClientAPI.GetUserData(request,
                (result) =>
                {
                    //유저 정보 받아오기 성공 했을 때
                    PlayerDataParse(result);
                },
                (error) =>
                {
                    //유저 정보 받아오기 실패 했을 때
                }
            );
    }

    void PlayerDataParse(GetUserDataResult result)
    {
        bool a_IsParsefail = false; //Parse failed
        bool a_IsDiff = false; //IsDifferent

        int a_GetValue = 0;
        int Idx = 0;
        foreach (var eachData in result.Data)
        {
            if (eachData.Key == "UserGold")
            {
                if (int.TryParse(eachData.Value.Value, out a_GetValue) == false)
                {
                    a_IsParsefail = true;
                    continue;
                }

                if (a_GetValue != GlobalValue.g_UserGold)
                {
                    a_IsDiff = true;
                    break;
                }
            }
            else if (eachData.Key.Contains("Skill_Item_") == true)
            {
                //"Skill_Item_1"
                //string[] strArr = { "Skill", "Item", "1" };

                Idx = 0;
                string[] strArr = eachData.Key.Split('_');
                if (3 <= strArr.Length)
                {
                    if (int.TryParse(strArr[2], out Idx) == false)
                    {
                        a_IsParsefail = true;
                        continue;
                    }
                }
                else
                {
                    a_IsParsefail = true;
                    continue;
                }

                if (GlobalValue.g_CurSkillCount.Count <= Idx)
                {
                    a_IsParsefail = true;
                    continue;
                }

                if (int.TryParse(eachData.Value.Value, out a_GetValue) == false)
                {
                    a_IsParsefail = true;
                    continue;
                }

                if ((int)m_BuySkType != Idx)    //지금 구매 하려고 하는 상품만 다른지 확인한다.
                    continue;

                if (a_GetValue != GlobalValue.g_CurSkillCount[Idx])
                {
                    a_IsDiff = true;
                    break;
                }
            }
        }//foreach(var eachData in result.Data)

        string a_Mess = "";
        bool a_NeedDelegate = false;
        Skill_Info a_SkInfo = GlobalValue.g_SkDataList[(int)m_BuySkType];

        if (a_IsDiff == true)
        { 
            a_Mess = "서버의 골드값과 스킬 아이템 정보가 정상적이지 않습니다.\n운영진에 문의해 주세요.";
        }
        else if (5 <= GlobalValue.g_CurSkillCount[(int)m_BuySkType])
        {
            a_Mess = "하나의 아이템은 5개까지만 구매할 수 있습니다.";
        }
        else if(GlobalValue.g_UserGold < a_SkInfo.m_Price)
        {
            a_Mess = "보유(누적) 골드가 부족합니다.";
        }
        else
        {
            a_Mess = "정말 구입하시겠습니까?";
            a_NeedDelegate = true;      //<-- 이 조건일 때 구매
        }

        if (a_IsParsefail == true)
        {
            a_Mess += "\n(서버에 비정상 정보가 있습니다.\n운영진(aaa@aaa.aaa)에 유저ID와 함께 알려 주세요.)";
        }

        //m_BuySkType = a_SkType;
        m_SvMyGold = GlobalValue.g_UserGold;
        m_SvMyGold -= a_SkInfo.m_Price;
        m_SvMyCount = GlobalValue.g_CurSkillCount[(int)m_BuySkType];
        m_SvMyCount++;  //스킬 보유수 증가 백업해 놓기

        GameObject a_DlgRsc = Resources.Load("DialogBox") as GameObject;
        GameObject a_DlgBoxObj = Instantiate(a_DlgRsc);
        GameObject a_Canvas = GameObject.Find("Canvas");
        a_DlgBoxObj.transform.SetParent(a_Canvas.transform, false);
        DialogBox_Ctrl a_DlgBox = a_DlgBoxObj.GetComponent<DialogBox_Ctrl>();
        if(a_DlgBox != null)
        {
            if (a_NeedDelegate == true)
                a_DlgBox.InitMessage(a_Mess, TryBuySkItem);
            else
                a_DlgBox.InitMessage(a_Mess);
        }

    }//public void BuySkillItem(SkillType a_SkType)

    void TryBuySkItem()  //구매 2단계 확정 함수 (서버에 데이터 값 전달하기...)
    {
        if (GlobalValue.g_Unique_ID == "")
            return;     //로그인 상태가 아니면 그냥 리턴

        Dictionary<string, string> a_ItemList = new Dictionary<string, string>();
        a_ItemList.Add("UserGold", m_SvMyGold.ToString());
        a_ItemList.Add($"Skill_Item_{(int)m_BuySkType}", m_SvMyCount.ToString());

        var request = new UpdateUserDataRequest()
        {
            Data = a_ItemList
        };

        PlayFabClientAPI.UpdateUserData(request,
                (result) =>
                {
                    //메뉴 상태를 갱신해 줘야 한다.
                    GlobalValue.g_UserGold = m_SvMyGold;    //골드값 조정
                    GlobalValue.g_CurSkillCount[(int)m_BuySkType] = m_SvMyCount; //스킬 보유수 증가 조정

                    RefreshSkItemList();

                    m_UserInfoText.text = "별명(" + GlobalValue.g_NickName +
                                            ") : 보유골드(" + GlobalValue.g_UserGold + ")";
                },
                (error) =>
                {
                    Debug.Log("데이터 저장 실패");
                }
        );
    }//void BuyRequestCo()  //구매 2단계 확정 함수 (서버에 데이터 값 전달하기...)
 
}
