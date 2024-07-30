using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubHero_Ctrl : MonoBehaviour
{
    HeroCtrl m_RefHero = null;  //주인공 객체의 참조 변수
    float angle = 0.0f;     //회전 각도 계산용 변수(주인공을 중심으로 주변을 돌게 하기 위함)
    float radius = 1.0f;    //회전 변경
    float speed = 100.0f;   //회전 속도

    Vector3 Parent_Pos = Vector3.zero; //부모가 될 오브젝트 좌표를 받아올 변수

    float m_LifeTime = 0.0f;    //생명 타이머

    //--- 공격 관련 변수
    GameObject m_BulletObj = null;
    Bullet_Ctrl m_BulletSc = null;
    float m_AttSpeed = 0.5f;        //공격 속도(공속)
    float m_ShootCool = 0.0f;       //총알 발사 주기 계산용 변수

    GameObject m_CloneObj = null;
    bool IsDouble = false;
    //--- 공격 관련 변수

    // Start is called before the first frame update
    void Start()
    {
        m_RefHero = transform.root.GetComponent<HeroCtrl>(); 
                    //GameObject.FindObjectOfType<HeroCtrl>();

        m_BulletObj = Resources.Load("BulletPrefab") as GameObject;
    }

    // Update is called once per frame
    void Update()
    {
        m_LifeTime -= Time.deltaTime;
        if(m_LifeTime <= 0.0f)
        {
            Destroy(gameObject);
            return;
        }

        if(m_RefHero == null || transform.parent == null)
        {
            Destroy(gameObject);
            return;
        }

        angle += Time.deltaTime * speed;
        if (360.0f < angle)
            angle -= 360.0f;   //0 ~ 360 도를 순환시키기 위한 코드
        
        Parent_Pos = transform.parent.position;
        transform.position = Parent_Pos +
                            new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                                        Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                                        0.0f);

        FireUpdate();

    }//void Update()

    public void SubHeroSpawn(float a_Angle, float a_LifeTime)
    {
        angle = a_Angle;
        m_LifeTime = a_LifeTime;
    }

    void FireUpdate()
    {
        if(m_RefHero != null)
        {
            if (0.0f < m_RefHero.m_Double_OnTime)  //더블샷 상태 가져오기
                IsDouble = true;
            else
                IsDouble = false;
        }

        if (0.0f < m_ShootCool)
            m_ShootCool -= Time.deltaTime;

        if(m_ShootCool <= 0.0f)
        {
            m_ShootCool = m_AttSpeed;   //공격속도 0.5초 주기

            if(IsDouble == true) //더블샷
            {
                Vector3 a_Pos;
                for(int i = 0; i < 2; i++)
                {
                    //m_CloneObj = Instantiate(m_BulletObj);
                    m_BulletSc = BulletPool_Mgr.Inst.GetALBulletPool();
                    m_BulletSc.gameObject.SetActive(true);
                    a_Pos = transform.position;
                    a_Pos.y += 0.2f - (i * 0.4f);
                    m_BulletSc.transform.position = a_Pos;
                }

            }
            else  //일반총알
            {
                //m_CloneObj = Instantiate(m_BulletObj);
                //m_CloneObj.transform.position = transform.position;
                m_BulletSc = BulletPool_Mgr.Inst.GetALBulletPool();
                m_BulletSc.gameObject.SetActive(true);
                m_BulletSc.transform.position = transform.position;
            }

        }//if(m_ShootCool <= 0.0f)

    }//void FireUpdate()
}
