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
        for (int i = 0; i < GlobalValue.g_SkDataList.Count; i++)
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
        if (m_Item_ScContent != null)
        {
            if (m_SkNodeList == null || m_SkNodeList.Length <= 0)
                m_SkNodeList = m_Item_ScContent.GetComponentsInChildren<SkProductNode>();
        }

        for (int i = 0; i < m_SkNodeList.Length; i++)
        {
            m_SkNodeList[i].RefreshState();
        }
    }//void RefreshSkItemList()

    public void BuySkillItem(SkillType a_SkType)
    {  //리스트 뷰에 있는 캐릭터 가격 버튼을 눌러 구입 시도를 한 경우
        m_BuySkType = a_SkType;
        BuyBeforeJobCo();
    }

    //# 구매 1단계 : 구매 전 확인
    void BuyBeforeJobCo()
    {
        //## 서버에서 상태 받아오고 클라이언트와 동기화 
        if (GlobalValue.g_Unique_ID == "")
            return;


        /// 플레이어 데이터 값 받아오기
        var request = new GetUserDataRequest
        {
            PlayFabId = GlobalValue.g_Unique_ID
        };

        PlayFabClientAPI.GetUserData(request,
        (result) =>
        {
            //### 성공시 
            PlayerDataParse(result);
        },
        (error) =>
        {
            //### 유저 정보 받아오기 실패
            Debug.Log("데이터 받아오기 실패");
        });
    }


    void PlayerDataParse(GetUserDataResult result)
    {
        bool a_IsDiff = false;
        int a_GetVal = 0;
        int Idx = 0;


        foreach (var eachData in result.Data)
        {
            if (eachData.Key == "UserGold")
            {
                if (int.TryParse(eachData.Value.Value, out a_GetVal)== false)
                {
                    a_IsDiff = true;
                    break;
                }
                if (a_GetVal != GlobalValue.g_UserGold)
                {
                    a_IsDiff = true;
                    break;
                }
                //GlobalValue.g_UserGold = a_GetVal;
            }
            else if (eachData.Key.Contains("Skill_Item") == true)
            {
                Idx = 0;
                string[] StrArr = eachData.Key.Split('_');
                if (StrArr.Length >= 3)
                {
                    if (int.TryParse(StrArr[2], out Idx) == false)
                    {
                        a_IsDiff = true;
                        break;
                    }

                }

                if (GlobalValue.g_CurSkillCount.Count <= Idx)
                {
                    a_IsDiff = true;
                    break;
                }

                if (int.TryParse(eachData.Value.Value, out a_GetVal) == false)
                {
                    a_IsDiff = true;
                    break;
                }
                if (a_GetVal != GlobalValue.g_CurSkillCount[Idx])
                {
                    a_IsDiff = true;
                    break;
                }

                //GlobalValue.g_CurSkillCount[Idx] = a_GetVal;

            }

        }


        string a_Mess = "";
        bool a_NeedDelegate = false;
        Skill_Info a_SkInfo = GlobalValue.g_SkDataList[(int)m_BuySkType];


        if (a_IsDiff == true)
        {
            a_Mess += "서버의 골드와 스킬 정보가 정상적이지 않습니다.\n문의해주세요.";
        }


        if (5 <= GlobalValue.g_CurSkillCount[(int)m_BuySkType])
        {
            a_Mess = "하나의 아이템은 5개까지만 구매할 수 있습니다.";
        }
        else if (GlobalValue.g_UserGold < a_SkInfo.m_Price)
        {
            a_Mess = "보유(누적) 골드가 부족합니다.";
        }
        else
        {
            a_Mess = "정말 구입하시겠습니까?";
            a_NeedDelegate = true;      //<-- 이 조건일 때 구매
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
        if (a_DlgBox != null)
        {
            if (a_NeedDelegate == true)
                a_DlgBox.InitMessage(a_Mess, TryBuySkItem);
            else
                a_DlgBox.InitMessage(a_Mess);
        }
    }

    //# 구매 2단계 
    void TryBuySkItem()
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

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
                           //## 성공시 메뉴상태 갱신
                           GlobalValue.g_UserGold = m_SvMyGold;    //골드값 조정
                           GlobalValue.g_CurSkillCount[(int)m_BuySkType] = m_SvMyCount;    //스킬 보유수 증가 조정

                           RefreshSkItemList();

                           m_UserInfoText.text = "별명(" + GlobalValue.g_NickName +
                                                   ") : 보유골드(" + GlobalValue.g_UserGold + ")";
                       },
                       (error) =>
                       {
                           //## 실패시
                           Debug.Log("실패!");
                       });


    }



    //void TryBuySkItem()  //구매 확정 함수(최종)
    //{
    //    if (m_BuySkType < SkillType.Skill_0 || SkillType.SkCount <= m_BuySkType)
    //        return;

    //    GlobalValue.g_UserGold = m_SvMyGold;    //골드값 조정
    //    GlobalValue.g_CurSkillCount[(int)m_BuySkType] = m_SvMyCount;    //스킬 보유수 증가 조정

    //    RefreshSkItemList();

    //    m_UserInfoText.text = "별명(" + GlobalValue.g_NickName +
    //                            ") : 보유골드(" + GlobalValue.g_UserGold + ")";

    //    ////--- 로컬에 저장
    //    //PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
    //    //PlayerPrefs.SetInt($"Skill_Item_{(int)m_BuySkType}",
    //    //                                GlobalValue.g_CurSkillCount[(int)m_BuySkType]);
    //    ////--- 로컬에 저장
    //}
}
