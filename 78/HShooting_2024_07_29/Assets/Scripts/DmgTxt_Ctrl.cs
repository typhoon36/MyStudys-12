using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DmgTxt_Ctrl : MonoBehaviour
{
    float m_EffTime = 0.0f;     //연출 시간 계산용 변수
    public Text DamageText = null;  //Text UI 접근용 변수

    //속도 = 거리 / 시간
    float MvVelocity = 1.1f / 1.05f;        //1.05초 동안에 1.1m 간다는 ... 속도
    float ApVelocity = 1.0f / (1.0f - 0.4f);
    //alpha 0.4초부터 1.0초까지 (0.6초 동안) : 0.0 -> 1.0 변화하는 속도

    Vector3 m_CurPos;       //위치 계산용 변수
    Color m_Color;          //색깔 계산용 변수

    // Start is called before the first frame update
    //void Start()
    //{
        
    //}

    // Update is called once per frame
    void Update()
    {
        m_EffTime += Time.deltaTime;

        if(m_EffTime < 1.05f)
        {
            m_CurPos = DamageText.transform.position;
            m_CurPos.y += Time.deltaTime * MvVelocity;
            DamageText.transform.position = m_CurPos;
        }

        if(0.4f < m_EffTime)
        {
            m_Color = DamageText.color;
            m_Color.a -= (Time.deltaTime * ApVelocity);
            if(m_Color.a < 0.0f)
                m_Color.a = 0.0f;
            DamageText.color = m_Color;
        }

        if(1.05f < m_EffTime)
        {
            Destroy(gameObject);    
        }

    }//void Update()

    public void InitDamage(float a_Damage, Color a_Color)
    {
        if (DamageText == null)
            DamageText = this.GetComponentInChildren<Text>();

        if(a_Damage <= 0.0f)
        {
            int a_Dmg = (int)Mathf.Abs(a_Damage);   //절대값 함수
            DamageText.text = "- " + a_Dmg;
        }
        else
        {
            DamageText.text = "+ " + (int)a_Damage;
        }

        a_Color.a = 1.0f;
        DamageText.color = a_Color;
    }
}
