using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DragAndDropMgr : MonoBehaviour
{
    public SlotScript[] m_ProductSlots;     // 상품 슬롯
    public SlotScript[] m_InvenSlots;       // TargetSlots
    public Image m_MsObj = null;            //마우스를 따라 다녀야 하는 오브젝트
    int m_SaveIndex = -1;       //-1이 아니면 아템을 잡은 상태에서 드래그 중이라는 뜻

    public Text m_BagSizeText;
    public Text m_HelpText;
    float m_HelpDuring = 2.0f;
    float m_HelpAddTimer = 0.0f;
    float m_CacTimer = 0.0f;
    Color m_Color;

    Store_Mgr m_StMgr = null;

    //# 서버 통신 
    int m_SvMyGold = 0;
    int[] m_SvSkCnt = new int[3];
    string m_SvJson = ""; //서버에서 받은 Json 형식 

    bool isNetworkLock = false;
    float m_BuyWaitTime = 0.0f;
    string BuyReqUrl = "";


    // Start is called before the first frame update
    void Start()
    {
        BuyReqUrl = "http://typhoon.dothome.co.kr/Buy_Request.php";

        m_StMgr = GameObject.FindObjectOfType<Store_Mgr>();

        RefreshUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(0.0f < m_BuyWaitTime)
            m_BuyWaitTime -= Time.deltaTime;
          
        


        if (Input.GetMouseButtonDown(0) == true)
        {   //왼쪽 마우스 버튼 클릭하는 순간
            MouseBtnDown();
        }

        if (Input.GetMouseButton(0) == true)
        {   //왼쪽 마우스 버튼을 누르고 있는 동안
            MousePress();
        }

        if (Input.GetMouseButtonUp(0) == true)
        {   //왼쪽 마우스 버튼을 누르다가 떼는 순간
            MouseBtnUp();
        }

        //--- HelpText 서서히 사라지게 처리하는 연출
        if (0.0f < m_HelpAddTimer)
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
    {
        m_SaveIndex = -1;

        for (int i = 0; i < m_ProductSlots.Length; i++)
        {
            if (m_ProductSlots[i].ItemImg.gameObject.activeSelf == true &&
                IsCollSlot(m_ProductSlots[i]) == true)
            {
                m_SaveIndex = i;
                Transform a_ChildImg = m_MsObj.transform.Find("MsIconImg");
                if (a_ChildImg != null)
                    a_ChildImg.GetComponent<Image>().sprite =
                                    m_ProductSlots[i].ItemImg.sprite;
                //m_ProductSlots[i].ItemImg.gameObject.SetActive(false);
                m_MsObj.gameObject.SetActive(true);
                break;
            }
        }//for(int i = 0; i < m_ProductSlots.Length; i++)

    }//void MouseBtnDown()

    void MousePress()
    {
        if (0 <= m_SaveIndex)
            m_MsObj.transform.position = Input.mousePosition;

    }//void MousePress()

    void MouseBtnUp()
    {
        if (m_SaveIndex < 0 || m_ProductSlots.Length <= m_SaveIndex)
            return;


        if(isNetworkLock == true || 0.0f< m_BuyWaitTime)
        {
            //아직 구매 불가
            m_SaveIndex = -1;
            m_MsObj.gameObject.SetActive(false);
            ShowMessage("구매 중입니다. 다시시도해주세요."); 
            return;

        }


        //장착하기 코드...
        int a_BuyIndex = -1;
        for (int i = 0; i < m_InvenSlots.Length; i++)
        {
            if (IsCollSlot(m_InvenSlots[i]) == true)
            {
                if (m_SaveIndex != i)  //다른 슬롯에 장착하려고 시도한 경우
                {
                    //메시지 출력
                    ShowMessage("해당 슬롯에는 아이템을 장착할 수 없습니다.");
                    continue;
                }

                if (BuySkItem(m_SaveIndex) == true)
                { //여기서 상품 구매 시도 함수 호출 (함수 호출 결과 성공이 일때만 아래 코드 실행 되게 처리)
                    a_BuyIndex = i;
                    break;
                }
            }//if (IsCollSlot(m_InvenSlots[i]) == true)
        }//for(int i = 0; i < m_InvenSlots.Length; i++)

        if (0 <= a_BuyIndex)
        {
            Sprite a_MsIconImg = null;
            Transform a_ChildImg = m_MsObj.transform.Find("MsIconImg");
            if (a_ChildImg != null)
                a_MsIconImg = a_ChildImg.GetComponent<Image>().sprite;

            m_InvenSlots[a_BuyIndex].ItemImg.sprite = a_MsIconImg;
            m_InvenSlots[a_BuyIndex].ItemImg.gameObject.SetActive(true);
            m_InvenSlots[a_BuyIndex].m_CurItemIdx = a_BuyIndex;

            StartCoroutine(BuyReqCo());

        }//if(0 <= a_BuyIndex)
        //else
        //{
        //    m_ProductSlots[m_SaveIndex].ItemImg.gameObject.SetActive(true);
        //}




        m_SaveIndex = -1;
        m_MsObj.gameObject.SetActive(false);

    }//void MouseBtnUp()

    bool IsCollSlot(SlotScript a_CkSlot)
    {   //마우스가 UI 슬롯 위헤 있는지? 판단하는 함수

        if (a_CkSlot == null)
            return false;

        Vector3[] v = new Vector3[4];
        a_CkSlot.GetComponent<RectTransform>().GetWorldCorners(v);
        //v[0] : 좌측하단   v[1] : 좌측상단   v[2] : 우측상단   v[3] : 우측하단
        //v변수의 좌표계 : 화면의 좌측하단 0, 0이고 우측상단(최고점 예 1280, 720)인 좌표계
        //마우스 좌표계 : 화면의 좌측하단이 0, 0이고 우측상단(최고점 예 1280, 720)인 좌표계
        //UI 좌표계 : 앵커가 센터일 때 중앙 0, 0 인 좌표계
        if (v[0].x <= Input.mousePosition.x && Input.mousePosition.x <= v[2].x &&
            v[0].y <= Input.mousePosition.y && Input.mousePosition.y <= v[2].y)
        {
            return true;
        }

        return false;

    }// bool IsCollSlot(SlotScript a_CkSlot)

    void ShowMessage(string a_Mess)
    {
        if (m_HelpText == null)
            return;

        m_HelpText.text = a_Mess;
        m_HelpText.gameObject.SetActive(true);
        m_HelpAddTimer = m_HelpDuring;
    }

    bool BuySkItem(int a_SkIdx)  //구매 시도 함수
    {
        int a_Cost = 300;
        if (a_SkIdx == 1)
            a_Cost = 500;
        else if (a_SkIdx == 2)
            a_Cost = 1000;

        if (GlobalValue.g_UserGold < a_Cost)
        {
            ShowMessage("골드가 부족합니다.");
            return false;
        }

        int a_CurBagSize = 0;
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            a_CurBagSize += GlobalValue.g_SkillCount[i];

        if (10 <= a_CurBagSize)
        {
            ShowMessage("가방이 가득 찼습니다.");
            return false;
        }

        // 정식 구매 과정은 드래그 앤 드롭 시 확인 다이알로그를 띄우고 유저의 동의 후
        // 서버에서 구매 확인, 승인 후 클라이언트에 응답을 주고 UI를 갱신해 주는
        // 과정으로 진행해야 한다.

        //## BackUp 받기(실패시 복구하기 위해)
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            m_SvSkCnt[i] = GlobalValue.g_SkillCount[i];

        m_SvMyGold = GlobalValue.g_UserGold;
        // 백업 끝

        GlobalValue.g_SkillCount[a_SkIdx]++;
        GlobalValue.g_UserGold -= a_Cost;

        //--- 변동 사항 로컬에 저장
        //string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        //PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);
        //PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
        //--- 변동 사항 로컬에 저장

        RefreshUI();  //<-- UI 갱신

        return true;
    }

    void RefreshUI()
    {
        for (int i = 0; i < m_InvenSlots.Length; i++)
        {
            if (0 < GlobalValue.g_SkillCount[i])
            {
                m_InvenSlots[i].ItemCountText.text = GlobalValue.g_SkillCount[i].ToString();
                m_InvenSlots[i].ItemImg.sprite = m_ProductSlots[i].ItemImg.sprite;
                m_InvenSlots[i].ItemImg.gameObject.SetActive(true);
                m_InvenSlots[i].m_CurItemIdx = i;
            }
            else
            {
                m_InvenSlots[i].ItemCountText.text = "0";
                m_InvenSlots[i].ItemImg.gameObject.SetActive(false);
            }
        }//for(int i = 0; i < m_InvenSlots.Length; i++)

        if (m_StMgr != null && m_StMgr.m_UserInfoText != null)
            m_StMgr.m_UserInfoText.text = "별명(" + GlobalValue.g_NickName + ") : 보유골드(" +
                                        GlobalValue.g_UserGold + ")";

        int a_CurBagSize = 0;
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            a_CurBagSize += GlobalValue.g_SkillCount[i];
        m_BagSizeText.text = "가방사이즈 : " + a_CurBagSize + " / 10";

    }//void RefreshUI()

    IEnumerator BuyReqCo()
    {
        //## Json 형식 만들기
        ItemList a_ItList = new ItemList();
        a_ItList.SkList = new int[3];

        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            a_ItList.SkList[i] = GlobalValue.g_SkillCount[i];

        //## Json 형식을 문자열로  변환 -> {"SkList":[1,2,3]}
        m_SvJson = JsonUtility.ToJson(a_ItList);
        Debug.Log(m_SvJson);

        if (string.IsNullOrEmpty(m_SvJson) == true)
        {
            RecoverItem();
            yield break;
        }

        if (string.IsNullOrEmpty(GlobalValue.g_Unique_ID) == true)
        {
            RecoverItem();
            yield break;
        }

        WWWForm form = new WWWForm();
        //## 서버에 전송할 데이터
        form.AddField("Input_user", GlobalValue.g_Unique_ID,
                       System.Text.Encoding.UTF8);

        form.AddField("Input_gold", GlobalValue.g_UserGold);

        form.AddField("Item_list", m_SvJson, System.Text.Encoding.UTF8);

        isNetworkLock = true;
        m_BuyWaitTime = 2.0f;

        UnityWebRequest request = UnityWebRequest.Post(BuyReqUrl, form);

        yield return request.SendWebRequest();

        if (request.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string a_ReStr = enc.GetString(request.downloadHandler.data);
            //### 응답완료될시 전체 갱신 <-- 전체값 받고 갱신 & m_SvMyPoint, m_BuySkType 가지고 갱신하는 방법 존재

            if (a_ReStr.Contains("Update Success~") == false)
            {
                RecoverItem();
                Debug.Log(a_ReStr);
            }
        }
        else
        {
            RecoverItem();
            Debug.Log(request.error);

        }

        request.Dispose();

        isNetworkLock = false;
    }

    void RecoverItem()
    {
        GlobalValue.g_UserGold = m_SvMyGold;
        for (int i = 0; i< m_InvenSlots.Length; i++)
        {
            GlobalValue.g_SkillCount[i] = m_SvSkCnt[i];
        }

        RefreshUI();
    }

}
