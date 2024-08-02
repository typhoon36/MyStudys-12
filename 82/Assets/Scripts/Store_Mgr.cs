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
        m_BuySkType = a_SkType;
        BuyBeforeJobCo();  
    }

    void BuyBeforeJobCo()  //���� 1�ܰ� �Լ�
    {  //�����κ��� ���, ������ ���� �޾ƿͼ� Ŭ���̾�Ʈ�� ����ȭ �����ֱ�...
        if (GlobalValue.g_Unique_ID == "")
            return;

        //< �÷��̾� ������(Ÿ��Ʋ) > �� Ȱ�� �ڵ�
        var request = new GetUserDataRequest()
        {
            PlayFabId = GlobalValue.g_Unique_ID
        };

        PlayFabClientAPI.GetUserData(request,
                (result) =>
                {
                    //���� ���� �޾ƿ��� ���� ���� ��
                    PlayerDataParse(result);
                },
                (error) =>
                {
                    //���� ���� �޾ƿ��� ���� ���� ��
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

                if ((int)m_BuySkType != Idx)    //���� ���� �Ϸ��� �ϴ� ��ǰ�� �ٸ��� Ȯ���Ѵ�.
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
            a_Mess = "������ ��尪�� ��ų ������ ������ ���������� �ʽ��ϴ�.\n����� ������ �ּ���.";
        }
        else if (5 <= GlobalValue.g_CurSkillCount[(int)m_BuySkType])
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

        if (a_IsParsefail == true)
        {
            a_Mess += "\n(������ ������ ������ �ֽ��ϴ�.\n���(aaa@aaa.aaa)�� ����ID�� �Բ� �˷� �ּ���.)";
        }

        //m_BuySkType = a_SkType;
        m_SvMyGold = GlobalValue.g_UserGold;
        m_SvMyGold -= a_SkInfo.m_Price;
        m_SvMyCount = GlobalValue.g_CurSkillCount[(int)m_BuySkType];
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

    void TryBuySkItem()  //���� 2�ܰ� Ȯ�� �Լ� (������ ������ �� �����ϱ�...)
    {
        if (GlobalValue.g_Unique_ID == "")
            return;     //�α��� ���°� �ƴϸ� �׳� ����

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
                    //�޴� ���¸� ������ ��� �Ѵ�.
                    GlobalValue.g_UserGold = m_SvMyGold;    //��尪 ����
                    GlobalValue.g_CurSkillCount[(int)m_BuySkType] = m_SvMyCount; //��ų ������ ���� ����

                    RefreshSkItemList();

                    m_UserInfoText.text = "����(" + GlobalValue.g_NickName +
                                            ") : �������(" + GlobalValue.g_UserGold + ")";
                },
                (error) =>
                {
                    Debug.Log("������ ���� ����");
                }
        );
    }//void BuyRequestCo()  //���� 2�ܰ� Ȯ�� �Լ� (������ ������ �� �����ϱ�...)
 
}
