using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkProductNode : MonoBehaviour
{
    [HideInInspector] public SkillType m_SkType = SkillType.SkCount;

    public Text  m_CountText;
    public Image m_SkIconImg;
    public Text  m_HelpText;
    public Text  m_BuyText;

    // Start is called before the first frame update
    void Start()
    {
        //리스트뷰에 있는 스킬 가격 버튼을 눌러 구입 시도한 경우
        Button a_BtnCom = this.GetComponentInChildren<Button>();
        if (a_BtnCom != null)
            a_BtnCom.onClick.AddListener(() =>
            {
                Store_Mgr a_StoreMgr = GameObject.FindObjectOfType<Store_Mgr>();
                if (a_StoreMgr != null)
                    a_StoreMgr.BuySkillItem(m_SkType);
            });
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public void InitData(SkillType a_SkType)
    {
        m_SkType = a_SkType;
        m_SkIconImg.sprite = GlobalValue.g_SkDataList[(int)a_SkType].m_IconImg;
        m_SkIconImg.GetComponent<RectTransform>().sizeDelta =
            new Vector2(GlobalValue.g_SkDataList[(int)a_SkType].m_IconSize.x * 135.0f,
                        135.0f);

        m_HelpText.text = GlobalValue.g_SkDataList[(int)a_SkType].m_SkillExp;
    }

    public void RefreshState()
    {
        if (m_SkType < SkillType.Skill_0 || SkillType.SkCount <= m_SkType)
            return;

        Skill_Info a_RefSkInfo = GlobalValue.g_SkDataList[(int)m_SkType];
        if (a_RefSkInfo == null)
            return;

        m_CountText.text = GlobalValue.g_CurSkillCount[(int)m_SkType] + "/5";

        m_BuyText.text = a_RefSkInfo.m_Price + " 골드";
    }
}
