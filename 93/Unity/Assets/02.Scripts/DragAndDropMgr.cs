using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DragAndDropMgr : MonoBehaviour
{
    public SlotScript[] m_ProductSlots;     // ��ǰ ����
    public SlotScript[] m_InvenSlots;       // TargetSlots
    public Image m_MsObj = null;            //���콺�� ���� �ٳ�� �ϴ� ������Ʈ
    int m_SaveIndex = -1;       //-1�� �ƴϸ� ������ ���� ���¿��� �巡�� ���̶�� ��

    public Text m_BagSizeText;
    public Text m_HelpText;
    float m_HelpDuring = 2.0f;
    float m_HelpAddTimer = 0.0f;
    float m_CacTimer = 0.0f;
    Color m_Color;

    Store_Mgr m_StMgr = null;

    //# ���� ��� 
    int m_SvMyGold = 0;
    int[] m_SvSkCnt = new int[3];
    string m_SvJson = ""; //�������� ���� Json ���� 

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
        {   //���� ���콺 ��ư Ŭ���ϴ� ����
            MouseBtnDown();
        }

        if (Input.GetMouseButton(0) == true)
        {   //���� ���콺 ��ư�� ������ �ִ� ����
            MousePress();
        }

        if (Input.GetMouseButtonUp(0) == true)
        {   //���� ���콺 ��ư�� �����ٰ� ���� ����
            MouseBtnUp();
        }

        //--- HelpText ������ ������� ó���ϴ� ����
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
        //--- HelpText ������ ������� ó���ϴ� ����

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
            //���� ���� �Ұ�
            m_SaveIndex = -1;
            m_MsObj.gameObject.SetActive(false);
            ShowMessage("���� ���Դϴ�. �ٽýõ����ּ���."); 
            return;

        }


        //�����ϱ� �ڵ�...
        int a_BuyIndex = -1;
        for (int i = 0; i < m_InvenSlots.Length; i++)
        {
            if (IsCollSlot(m_InvenSlots[i]) == true)
            {
                if (m_SaveIndex != i)  //�ٸ� ���Կ� �����Ϸ��� �õ��� ���
                {
                    //�޽��� ���
                    ShowMessage("�ش� ���Կ��� �������� ������ �� �����ϴ�.");
                    continue;
                }

                if (BuySkItem(m_SaveIndex) == true)
                { //���⼭ ��ǰ ���� �õ� �Լ� ȣ�� (�Լ� ȣ�� ��� ������ �϶��� �Ʒ� �ڵ� ���� �ǰ� ó��)
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
    {   //���콺�� UI ���� ���� �ִ���? �Ǵ��ϴ� �Լ�

        if (a_CkSlot == null)
            return false;

        Vector3[] v = new Vector3[4];
        a_CkSlot.GetComponent<RectTransform>().GetWorldCorners(v);
        //v[0] : �����ϴ�   v[1] : �������   v[2] : �������   v[3] : �����ϴ�
        //v������ ��ǥ�� : ȭ���� �����ϴ� 0, 0�̰� �������(�ְ��� �� 1280, 720)�� ��ǥ��
        //���콺 ��ǥ�� : ȭ���� �����ϴ��� 0, 0�̰� �������(�ְ��� �� 1280, 720)�� ��ǥ��
        //UI ��ǥ�� : ��Ŀ�� ������ �� �߾� 0, 0 �� ��ǥ��
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

    bool BuySkItem(int a_SkIdx)  //���� �õ� �Լ�
    {
        int a_Cost = 300;
        if (a_SkIdx == 1)
            a_Cost = 500;
        else if (a_SkIdx == 2)
            a_Cost = 1000;

        if (GlobalValue.g_UserGold < a_Cost)
        {
            ShowMessage("��尡 �����մϴ�.");
            return false;
        }

        int a_CurBagSize = 0;
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            a_CurBagSize += GlobalValue.g_SkillCount[i];

        if (10 <= a_CurBagSize)
        {
            ShowMessage("������ ���� á���ϴ�.");
            return false;
        }

        // ���� ���� ������ �巡�� �� ��� �� Ȯ�� ���̾˷α׸� ���� ������ ���� ��
        // �������� ���� Ȯ��, ���� �� Ŭ���̾�Ʈ�� ������ �ְ� UI�� ������ �ִ�
        // �������� �����ؾ� �Ѵ�.

        //## BackUp �ޱ�(���н� �����ϱ� ����)
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            m_SvSkCnt[i] = GlobalValue.g_SkillCount[i];

        m_SvMyGold = GlobalValue.g_UserGold;
        // ��� ��

        GlobalValue.g_SkillCount[a_SkIdx]++;
        GlobalValue.g_UserGold -= a_Cost;

        //--- ���� ���� ���ÿ� ����
        //string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        //PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);
        //PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
        //--- ���� ���� ���ÿ� ����

        RefreshUI();  //<-- UI ����

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
            m_StMgr.m_UserInfoText.text = "����(" + GlobalValue.g_NickName + ") : �������(" +
                                        GlobalValue.g_UserGold + ")";

        int a_CurBagSize = 0;
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            a_CurBagSize += GlobalValue.g_SkillCount[i];
        m_BagSizeText.text = "��������� : " + a_CurBagSize + " / 10";

    }//void RefreshUI()

    IEnumerator BuyReqCo()
    {
        //## Json ���� �����
        ItemList a_ItList = new ItemList();
        a_ItList.SkList = new int[3];

        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            a_ItList.SkList[i] = GlobalValue.g_SkillCount[i];

        //## Json ������ ���ڿ���  ��ȯ -> {"SkList":[1,2,3]}
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
        //## ������ ������ ������
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
            //### ����Ϸ�ɽ� ��ü ���� <-- ��ü�� �ް� ���� & m_SvMyPoint, m_BuySkType ������ �����ϴ� ��� ����

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
