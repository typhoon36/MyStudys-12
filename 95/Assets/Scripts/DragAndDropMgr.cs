using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragAndDropMgr : MonoBehaviour
{
    public SlotScript[] m_ProductSlots; // ProductSlots
    public SlotScript[] m_InvenSlots;   // TargetSlots
    public Image m_MsObj = null;        // 마우스를 따라 다녀야 하는 오브젝트
    int m_SaveIndex = -1;               // -1이 아니면 아이템을 잡은 상태에서 드래그 중이라는 뜻

    public Text m_BagSizeText;
    public Text m_HelpText;
    float m_HelpDuring = 1.5f;
    float m_HelpAddTimer = 0.0f;
    float m_CacTimer = 0.0f;
    Color m_Color;

    //--- 지금 뭘 구입하려고 시도한 건지? 저장해 놓기 위한 변수
    SkillType m_BuySkType;  //어떤 스킬 아이템을 구입하려고 한 건지?
    int m_SvMyGold;         //구입 프로세스에 진입 후 상태 저장용 : 차감된 내 골드가 얼마인지?
    int m_SvMyCount = 0;    //스킬 보유수 증가 백업해 놓기...
    //--- 지금 뭘 구입하려고 시도한 건지? 저장해 놓기 위한 변수

    Store_Mgr m_StMgr = null;

    // Start is called before the first frame update
    void Start()
    {
        m_StMgr = GameObject.FindObjectOfType<Store_Mgr>();

        RefreshUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) == true)
        {  //왼쪽 마우스 버튼 클릭하는 순간
            MouseBtnDown();
        }

        if(Input.GetMouseButton(0) == true)
        {  //왼쪽 마우스 버튼 누르고 있는 동안
            MousePress();
        }

        if(Input.GetMouseButtonUp(0) == true)
        {  //왼쪽 마우스 버튼을 누르다가 떼는 순간
            MouseBtnUp();
        }

        //--- HelpText 서서히 사라지게 처리하는 연출
        if(0.0f < m_HelpAddTimer)
        {
            m_HelpAddTimer -= Time.deltaTime;
            m_CacTimer = m_HelpAddTimer / (m_HelpDuring - 1.0f);
            if (1.0f < m_CacTimer)
                m_CacTimer = 1.0f;
            m_Color = m_HelpText.color;
            m_Color.a = m_CacTimer;
            m_HelpText.color = m_Color;

            if (m_HelpAddTimer <= 0.0f)
                m_HelpText.gameObject.SetActive(false);
        }
        //--- HelpText 서서히 사라지게 처리하는 연출

    }//void Update()

    void MouseBtnDown()
    {   //왼쪽 마우스 버튼 클릭하는 순간
        m_SaveIndex = -1;

        for(int i = 0; i < m_ProductSlots.Length; i++)
        {
            if (m_ProductSlots[i].ItemIcon.gameObject.activeSelf == true &&
                IsCollSlot(m_ProductSlots[i]) == true)
            {
                m_SaveIndex = i;
                Transform a_ChildImg = m_MsObj.transform.Find("MsIconImg");
                if (a_ChildImg != null)
                    a_ChildImg.GetComponent<Image>().sprite =
                                         m_ProductSlots[i].ItemIcon.sprite;
                //m_ProductSlots[i].ItemIcon.gameObject.SetActive(false);
                m_MsObj.gameObject.SetActive(true);
                break;
            }
        }//for(int i = 0; i < m_ProductSlots.Length; i++)
    }//void MouseBtnDown()

    void MousePress()
    {   //왼쪽 마우스 버튼 누르고 있는 동안
        if (0 <= m_SaveIndex)
            m_MsObj.transform.position = Input.mousePosition;
    }

    void MouseBtnUp()
    {   //왼쪽 마우스 버튼을 누르다가 떼는 순간
        if (m_SaveIndex < 0 || m_ProductSlots.Length <= m_SaveIndex)
            return;

        int a_BuyIndex = -1;
        for(int i = 0; i < m_InvenSlots.Length; i++)
        {
            if (IsCollSlot(m_InvenSlots[i]) == true)
            {
                if (m_SaveIndex == i)  //수평선상에 있는 슬롯 끼리만 장착 가능하도록...
                {
                    if (BuySkItem(m_SaveIndex) == true)  //상품 구매 시도 함수 호출
                    {
                        a_BuyIndex = i;
                        break;
                    }//if (BuySkItem(m_SaveIndex) == true)  //상품 구매 시도 함수 호출

                }//if (m_SaveIndex == i)  //수평선상에 있는 슬롯 끼리만 장착 가능하도록...
                else //다른 슬롯에 장착하려고 시도한 경우
                {
                    ShowMessage("해당 슬롯에는 아이템을 장착할 수 없습니다.");
                }

            }//if (IsCollSlot(m_InvenSlots[i]) == true)
        }//for(int i = 0; i < m_InvenSlots.Length; i++)

        if(0 <= a_BuyIndex)
        {
            Sprite a_MsIconImg = null;
            Transform a_ChildImg = m_MsObj.transform.Find("MsIconImg");
            if (a_ChildImg != null)
                a_MsIconImg = a_ChildImg.GetComponent<Image>().sprite;

            m_InvenSlots[a_BuyIndex].ItemIcon.sprite = a_MsIconImg;
            m_InvenSlots[a_BuyIndex].ItemIcon.gameObject.SetActive(true);
            m_InvenSlots[a_BuyIndex].m_CurItemIdx = m_SaveIndex;
        }//if(0 <= a_BuyIndex)
        //else
        //{
        //    m_ProductSlots[m_SaveIndex].ItemIcon.gameObject.SetActive(true);
        //}

        m_SaveIndex = -1;
        m_MsObj.gameObject.SetActive(false);

    }

    bool BuySkItem(int a_SkIdx)  //구매 시도 함수
    {
        int a_Cost = 300;
        if (a_SkIdx == 1)
            a_Cost = 500;
        else if (a_SkIdx == 2)
            a_Cost = 1000;

        if(GlobalValue.g_UserGold < a_Cost)
        {
            ShowMessage("골드가 부족합니다.");
            return false;
        }

        int a_CurBagSize = 0;
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            a_CurBagSize += GlobalValue.g_SkillCount[i];

        if(10 <= a_CurBagSize)
        {
            ShowMessage("가방이 가득 찼습니다.");
            return false;
        }

        // 정식 구매 과정은 드래그 앤 드롭시 확인 다이알로그 박스를 띄우고 유저의 동의 후
        // 서버에서 구매 확인, 승인 후 클라이언트에 응답을 주고 UI를 갱싱해 주는
        // 과정으로 진행해야 한다.

        //--- 지금 뭘 구입하려고 시도한 건지? 저장해 놓기 위한 변수
        m_BuySkType = (SkillType)a_SkIdx;  //어떤 스킬 아이템을 구입하려고 한 건지?
        m_SvMyGold  = GlobalValue.g_UserGold;  //구입 프로세스에 진입 후 상태 저장용 : 차감된 내 골드가 얼마인지?
        m_SvMyCount = GlobalValue.g_SkillCount[a_SkIdx];    //스킬 보유수 증가 백업해 놓기...
        //--- 지금 뭘 구입하려고 시도한 건지? 저장해 놓기 위한 변수

        GlobalValue.g_SkillCount[a_SkIdx]++;
        GlobalValue.g_UserGold -= a_Cost;

        ////--- 변동 사항 로컬에 저장
        //string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        //PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);
        //PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
        ////--- 변동 사항 로컬에 저장

        RefreshUI();  //<-- UI 갱신

        TryBuySkItem();

        return true;
    }

    void TryBuySkItem()  //구매 2단계 확정 함수 (서버에 데이터 값 전달하기...)
    {
        if (GlobalValue.g_Unique_ID == "")
            return;     //로그인 상태가 아니면 그냥 리턴

        Dictionary<string, string> a_ItemList = new Dictionary<string, string>();
        a_ItemList.Add("UserGold", GlobalValue.g_UserGold.ToString());
        a_ItemList.Add($"SkItem_{(int)m_BuySkType}", GlobalValue.g_SkillCount[(int)m_BuySkType].ToString());

        var request = new UpdateUserDataRequest()
        {
            Data = a_ItemList
        };

        PlayFabClientAPI.UpdateUserData(request,
                (result) =>
                {                   
                    //RefreshSkItemList();

                    //m_UserInfoText.text = "별명(" + GlobalValue.g_NickName +
                    //                        ") : 보유골드(" + GlobalValue.g_UserGold + ")";
                },
                (error) =>
                {
                    Debug.Log("데이터 저장 실패");

                    //업데이트 실패로 원래대로 돌려 놓기...
                    GlobalValue.g_UserGold = m_SvMyGold;    //골드값 조정
                    GlobalValue.g_SkillCount[(int)m_BuySkType] = m_SvMyCount; //스킬 보유수 증가 조정
                    RefreshUI();  //<-- UI 갱신
                }
        );
    }//void BuyRequestCo()  //구매 2단계 확정 함수 (서버에 데이터 값 전달하기...)

    void RefreshUI()
    {
        for(int i = 0; i< m_InvenSlots.Length; i++)
        {
            if(0 < GlobalValue.g_SkillCount[i])
            {
                m_InvenSlots[i].ItemCountText.text = GlobalValue.g_SkillCount[i].ToString();
                m_InvenSlots[i].ItemIcon.sprite = m_ProductSlots[i].ItemIcon.sprite;
                m_InvenSlots[i].ItemIcon.gameObject.SetActive(true);
                m_InvenSlots[i].m_CurItemIdx = i;
            }//if(0 < GlobalValue.g_SkillCount[i])
            else
            {
                m_InvenSlots[i].ItemCountText.text = "0";
                m_InvenSlots[i].ItemIcon.gameObject.SetActive(false);
            }
        }//for(int i = 0; i< m_InvenSlots.Length; i++)

        if (m_StMgr != null && m_StMgr.m_UserInfoText != null)
            m_StMgr.m_UserInfoText.text = "별명(" + GlobalValue.g_NickName + ") : 보유골드(" +
                                    GlobalValue.g_UserGold + ")";

        int a_CurBagSize = 0;
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            a_CurBagSize += GlobalValue.g_SkillCount[i];
        m_BagSizeText.text = "가방사이즈 : " + a_CurBagSize + " / 10";

    } //void RefreshUI()

    bool IsCollSlot(SlotScript a_CkSlot)
    {  //마우스가 UI 슬롯 위에 있는지? 판단하는 함수

        if(a_CkSlot == null)
            return false;

        Vector3[] v = new Vector3[4];
        a_CkSlot.GetComponent<RectTransform>().GetWorldCorners(v);
        //v[0] : 좌측하단  v[1] : 좌측상단  v[2] : 우측상단  v[3] : 우측하단
        //v[0] 좌표계 : 화면의 좌측하단이 0, 0 이고 우측상단(최고점 예 1280, 720)인 좌표계
        //마우스 좌표계 : 화면의 좌측하단이 0, 0 이고 우측상단(최고점 예  1280, 720)인 좌표계
        //UI 좌표계 : 앵커가 센터일 때 중앙이 0, 0 인 좌표계
        if (v[0].x <= Input.mousePosition.x && Input.mousePosition.x <= v[2].x && 
            v[0].y <= Input.mousePosition.y && Input.mousePosition.y <= v[2].y)
        {
            return true;
        }

        return false;
    }

    void ShowMessage(string a_Mess)
    {
        if (m_HelpText == null)
            return;

        m_HelpText.text = a_Mess;
        m_HelpText.gameObject.SetActive(true);
        m_HelpAddTimer = m_HelpDuring;
    }

}
