using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCool_Ctrl : MonoBehaviour
{
    [HideInInspector] public SkillType m_SkType;
    public Sprite[] m_IconImg = null;
    float Skill_Time = 0.0f;
    float Skill_Duration = 0.0f;
    public Image Time_Img = null;
    public Image Icon_Img = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Skill_Time -= Time.deltaTime;
        Time_Img.fillAmount = Skill_Time / Skill_Duration;

        if (Skill_Time <= 0.0f)
            Destroy(gameObject);
    }

    public void InitState(SkillType a_SkType, float a_Time, float a_During)
    {
        m_SkType = a_SkType;
        Icon_Img.sprite = m_IconImg[(int)m_SkType];

        Skill_Time = a_Time;
        Skill_Duration = a_During;
    }
}
