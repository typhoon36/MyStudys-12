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
    public Image m_MsObj = null;        // ���콺�� ���� �ٳ�� �ϴ� ������Ʈ
    int m_SaveIndex = -1;               // -1�� �ƴϸ� �������� ���� ���¿��� �巡�� ���̶�� ��

    public Text m_BagSizeText;
    public Text m_HelpText;
    float m_HelpDuring = 1.5f;
    float m_HelpAddTimer = 0.0f;
    float m_CacTimer = 0.0f;
    Color m_Color;

    //--- ���� �� �����Ϸ��� �õ��� ����? ������ ���� ���� ����
    SkillType m_BuySkType;  //� ��ų �������� �����Ϸ��� �� ����?
    int m_SvMyGold;         //���� ���μ����� ���� �� ���� ����� : ������ �� ��尡 ������?
    int m_SvMyCount = 0;    //��ų ������ ���� ����� ����...
    //--- ���� �� �����Ϸ��� �õ��� ����? ������ ���� ���� ����

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
        {  //���� ���콺 ��ư Ŭ���ϴ� ����
            MouseBtnDown();
        }

        if(Input.GetMouseButton(0) == true)
        {  //���� ���콺 ��ư ������ �ִ� ����
            MousePress();
        }

        if(Input.GetMouseButtonUp(0) == true)
        {  //���� ���콺 ��ư�� �����ٰ� ���� ����
            MouseBtnUp();
        }

        //--- HelpText ������ ������� ó���ϴ� ����
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
        //--- HelpText ������ ������� ó���ϴ� ����

    }//void Update()

    void MouseBtnDown()
    {   //���� ���콺 ��ư Ŭ���ϴ� ����
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
    {   //���� ���콺 ��ư ������ �ִ� ����
        if (0 <= m_SaveIndex)
            m_MsObj.transform.position = Input.mousePosition;
    }

    void MouseBtnUp()
    {   //���� ���콺 ��ư�� �����ٰ� ���� ����
        if (m_SaveIndex < 0 || m_ProductSlots.Length <= m_SaveIndex)
            return;

        int a_BuyIndex = -1;
        for(int i = 0; i < m_InvenSlots.Length; i++)
        {
            if (IsCollSlot(m_InvenSlots[i]) == true)
            {
                if (m_SaveIndex == i)  //���򼱻� �ִ� ���� ������ ���� �����ϵ���...
                {
                    if (BuySkItem(m_SaveIndex) == true)  //��ǰ ���� �õ� �Լ� ȣ��
                    {
                        a_BuyIndex = i;
                        break;
                    }//if (BuySkItem(m_SaveIndex) == true)  //��ǰ ���� �õ� �Լ� ȣ��

                }//if (m_SaveIndex == i)  //���򼱻� �ִ� ���� ������ ���� �����ϵ���...
                else //�ٸ� ���Կ� �����Ϸ��� �õ��� ���
                {
                    ShowMessage("�ش� ���Կ��� �������� ������ �� �����ϴ�.");
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

    bool BuySkItem(int a_SkIdx)  //���� �õ� �Լ�
    {
        int a_Cost = 300;
        if (a_SkIdx == 1)
            a_Cost = 500;
        else if (a_SkIdx == 2)
            a_Cost = 1000;

        if(GlobalValue.g_UserGold < a_Cost)
        {
            ShowMessage("��尡 �����մϴ�.");
            return false;
        }

        int a_CurBagSize = 0;
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            a_CurBagSize += GlobalValue.g_SkillCount[i];

        if(10 <= a_CurBagSize)
        {
            ShowMessage("������ ���� á���ϴ�.");
            return false;
        }

        // ���� ���� ������ �巡�� �� ��ӽ� Ȯ�� ���̾˷α� �ڽ��� ���� ������ ���� ��
        // �������� ���� Ȯ��, ���� �� Ŭ���̾�Ʈ�� ������ �ְ� UI�� ������ �ִ�
        // �������� �����ؾ� �Ѵ�.

        //--- ���� �� �����Ϸ��� �õ��� ����? ������ ���� ���� ����
        m_BuySkType = (SkillType)a_SkIdx;  //� ��ų �������� �����Ϸ��� �� ����?
        m_SvMyGold  = GlobalValue.g_UserGold;  //���� ���μ����� ���� �� ���� ����� : ������ �� ��尡 ������?
        m_SvMyCount = GlobalValue.g_SkillCount[a_SkIdx];    //��ų ������ ���� ����� ����...
        //--- ���� �� �����Ϸ��� �õ��� ����? ������ ���� ���� ����

        GlobalValue.g_SkillCount[a_SkIdx]++;
        GlobalValue.g_UserGold -= a_Cost;

        ////--- ���� ���� ���ÿ� ����
        //string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        //PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);
        //PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
        ////--- ���� ���� ���ÿ� ����

        RefreshUI();  //<-- UI ����

        TryBuySkItem();

        return true;
    }

    void TryBuySkItem()  //���� 2�ܰ� Ȯ�� �Լ� (������ ������ �� �����ϱ�...)
    {
        if (GlobalValue.g_Unique_ID == "")
            return;     //�α��� ���°� �ƴϸ� �׳� ����

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

                    //m_UserInfoText.text = "����(" + GlobalValue.g_NickName +
                    //                        ") : �������(" + GlobalValue.g_UserGold + ")";
                },
                (error) =>
                {
                    Debug.Log("������ ���� ����");

                    //������Ʈ ���з� ������� ���� ����...
                    GlobalValue.g_UserGold = m_SvMyGold;    //��尪 ����
                    GlobalValue.g_SkillCount[(int)m_BuySkType] = m_SvMyCount; //��ų ������ ���� ����
                    RefreshUI();  //<-- UI ����
                }
        );
    }//void BuyRequestCo()  //���� 2�ܰ� Ȯ�� �Լ� (������ ������ �� �����ϱ�...)

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
            m_StMgr.m_UserInfoText.text = "����(" + GlobalValue.g_NickName + ") : �������(" +
                                    GlobalValue.g_UserGold + ")";

        int a_CurBagSize = 0;
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            a_CurBagSize += GlobalValue.g_SkillCount[i];
        m_BagSizeText.text = "��������� : " + a_CurBagSize + " / 10";

    } //void RefreshUI()

    bool IsCollSlot(SlotScript a_CkSlot)
    {  //���콺�� UI ���� ���� �ִ���? �Ǵ��ϴ� �Լ�

        if(a_CkSlot == null)
            return false;

        Vector3[] v = new Vector3[4];
        a_CkSlot.GetComponent<RectTransform>().GetWorldCorners(v);
        //v[0] : �����ϴ�  v[1] : �������  v[2] : �������  v[3] : �����ϴ�
        //v[0] ��ǥ�� : ȭ���� �����ϴ��� 0, 0 �̰� �������(�ְ��� �� 1280, 720)�� ��ǥ��
        //���콺 ��ǥ�� : ȭ���� �����ϴ��� 0, 0 �̰� �������(�ְ��� ��  1280, 720)�� ��ǥ��
        //UI ��ǥ�� : ��Ŀ�� ������ �� �߾��� 0, 0 �� ��ǥ��
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
