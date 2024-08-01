using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubHero_Ctrl : MonoBehaviour
{
    HeroCtrl m_RefHero = null;  //���ΰ� ��ü�� ���� ����
    float angle = 0.0f;     //ȸ�� ���� ���� ����(���ΰ��� �߽����� �ֺ��� ���� �ϱ� ����)
    float radius = 1.0f;    //ȸ�� ����
    float speed = 100.0f;   //ȸ�� �ӵ�

    Vector3 Parent_Pos = Vector3.zero; //�θ� �� ������Ʈ ��ǥ�� �޾ƿ� ����

    float m_LifeTime = 0.0f;    //���� Ÿ�̸�

    //--- ���� ���� ����
    GameObject m_BulletObj = null;
    Bullet_Ctrl m_BulletSc = null;
    float m_AttSpeed = 0.5f;        //���� �ӵ�(����)
    float m_ShootCool = 0.0f;       //�Ѿ� �߻� �ֱ� ���� ����

    GameObject m_CloneObj = null;
    bool IsDouble = false;
    //--- ���� ���� ����

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
            angle -= 360.0f;   //0 ~ 360 ���� ��ȯ��Ű�� ���� �ڵ�
        
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
            if (0.0f < m_RefHero.m_Double_OnTime)  //���� ���� ��������
                IsDouble = true;
            else
                IsDouble = false;
        }

        if (0.0f < m_ShootCool)
            m_ShootCool -= Time.deltaTime;

        if(m_ShootCool <= 0.0f)
        {
            m_ShootCool = m_AttSpeed;   //���ݼӵ� 0.5�� �ֱ�

            if(IsDouble == true) //����
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
            else  //�Ϲ��Ѿ�
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
