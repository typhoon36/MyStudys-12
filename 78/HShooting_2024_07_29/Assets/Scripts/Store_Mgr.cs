using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Store_Mgr : MonoBehaviour
{
    public Button BackBtn;
    public Text m_UserInfoText = null;

    public GameObject m_Item_ScContent; //ScrollView Content ���ϵ�� ������ Parent ��ü
    public GameObject m_SkProductNode;  //Node Prefab

    SkProductNode[] m_SkNodeList;       //��ũ�ѿ� �پ� �ִ� Item ��ϵ�...

    //--- ���� �� �����Ϸ��� �õ��� ����? ������ ���� ���� ����
    SkillType m_BuySkType;  //� ��ų �������� �����Ϸ��� �� ����?
    int m_SvMyGold;         //���� ���μ����� ���� �� ���� ����� : ������ �� ��尡 ������?
    int m_SvMyCount = 0;    //��ų ������ ���� ����� ����...
    //--- ���� �� �����Ϸ��� �õ��� ����? ������ ���� ���� ����

    // Start is called before the first frame update
    void Start()
    {
        GlobalValue.LoadGameData();

        if (BackBtn != null)
            BackBtn.onClick.AddListener(BackBtnClick);

        if (m_UserInfoText != null)
            m_UserInfoText.text = "����(" + GlobalValue.g_NickName + ") : �������(" +
                                    GlobalValue.g_UserGold + ")";

        //--- ������ ��� �߰�
        GameObject a_ItemObj = null;
        SkProductNode a_SkItemNode = null;
        for(int i = 0; i < GlobalValue.g_SkDataList.Count; i++)
        {
            a_ItemObj = Instantiate(m_SkProductNode);
            a_SkItemNode = a_ItemObj.GetComponent<SkProductNode>();
            a_SkItemNode.InitData(GlobalValue.g_SkDataList[i].m_SkType);
            a_ItemObj.transform.SetParent(m_Item_ScContent.transform, false);
        }
        //--- ������ ��� �߰�

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
    {  //����Ʈ �信 �ִ� ĳ���� ���� ��ư�� ���� ���� �õ��� �� ���

        string a_Mess = "";
        bool a_NeedDelegate = false;
        Skill_Info a_SkInfo = GlobalValue.g_SkDataList[(int)a_SkType];

        if(5 <= GlobalValue.g_CurSkillCount[(int)a_SkType])
        {
            a_Mess = "�ϳ��� �������� 5�������� ������ �� �ֽ��ϴ�.";
        }
        else if(GlobalValue.g_UserGold < a_SkInfo.m_Price)
        {
            a_Mess = "����(����) ��尡 �����մϴ�.";
        }
        else
        {
            a_Mess = "���� �����Ͻðڽ��ϱ�?";
            a_NeedDelegate = true;      //<-- �� ������ �� ����
        }

        m_BuySkType = a_SkType;
        m_SvMyGold = GlobalValue.g_UserGold;
        m_SvMyGold -= a_SkInfo.m_Price;
        m_SvMyCount = GlobalValue.g_CurSkillCount[(int)a_SkType];
        m_SvMyCount++;  //��ų ������ ���� ����� ����

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

    void TryBuySkItem()  //���� Ȯ�� �Լ�
    {
        if (m_BuySkType < SkillType.Skill_0 || SkillType.SkCount <= m_BuySkType)
            return;

        GlobalValue.g_UserGold = m_SvMyGold;    //��尪 ����
        GlobalValue.g_CurSkillCount[(int)m_BuySkType] = m_SvMyCount;    //��ų ������ ���� ����

        RefreshSkItemList();

        m_UserInfoText.text = "����(" + GlobalValue.g_NickName +
                                ") : �������(" + GlobalValue.g_UserGold + ")";

        //--- ���ÿ� ����
        PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
        PlayerPrefs.SetInt($"Skill_Item_{(int)m_BuySkType}", 
                                        GlobalValue.g_CurSkillCount[(int)m_BuySkType]);
        //--- ���ÿ� ����
    }
}
