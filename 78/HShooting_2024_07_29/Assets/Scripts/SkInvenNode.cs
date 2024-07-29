using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkInvenNode : MonoBehaviour
{
    [HideInInspector] public SkillType m_SkType;
    [HideInInspector] public int m_CurSkCount = 0;

    public Text m_SkCountText;  //��ų ī��Ʈ �ؽ�Ʈ
    public Image m_SkIconImg;   //ĳ���� ������ �̹���

    // Start is called before the first frame update
    void Start()
    {
        Button a_BtnCom = this.GetComponent<Button>();
        if (a_BtnCom != null)
            a_BtnCom.onClick.AddListener(() =>
            {
                //�� ��ư�� ������ ��
                if (GlobalValue.g_CurSkillCount[(int)m_SkType] <= 0)
                    return; //��ų �������� ����� �� ����

                HeroCtrl a_Hero = GameObject.FindObjectOfType<HeroCtrl>();  
                if(a_Hero != null)
                    a_Hero.UseSkill(m_SkType);
                Refresh_UI(m_SkType);
            });

    }

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public void InitState(SkillType a_SkType)
    {
        m_SkType = a_SkType;
        m_CurSkCount = GlobalValue.g_CurSkillCount[(int)m_SkType];
        if (m_SkCountText != null)
            m_SkCountText.text = m_CurSkCount.ToString();
        if (m_SkIconImg != null)
        {
            if (m_CurSkCount <= 0 )
                m_SkIconImg.color = new Color32(255, 255, 255, 80);
            else
                m_SkIconImg.color = new Color32(255, 255, 255, 220);
        }//if (m_SkIconImg != null)
    }

    public void Refresh_UI(SkillType a_SkType)
    {
        if (m_SkType != a_SkType)
            return;

        m_CurSkCount = GlobalValue.g_CurSkillCount[(int)m_SkType];
        if (m_SkCountText != null)
            m_SkCountText.text = m_CurSkCount.ToString();
        if (m_SkIconImg != null)
        {
            if (m_CurSkCount <= 0)
                m_SkIconImg.color = new Color32(255, 255, 255, 80);
            else
                m_SkIconImg.color = new Color32(255, 255, 255, 220);
        }//if (m_SkIconImg != null)
    }
}
