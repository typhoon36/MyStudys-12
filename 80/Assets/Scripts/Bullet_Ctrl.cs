using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_Ctrl : MonoBehaviour
{
    Vector3 m_DirVec = Vector3.right;       //날아가야 할 방향 벡터
    float m_MoveSpeed = 15.0f;              //이동속도

    float m_LifeTime = 3.0f;

    void OnEnable()  // Active가 활성화 될 때마다 호출되는 함수
    {
        m_LifeTime = 3.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Destroy(gameObject, 3.0f);
    }

    //void FixedUpdate()
    //{
    //    m_MoveSpeed = 15.0f;
    //}

    //void OnTriggerStay2D(Collider2D coll)
    //{
    //    if (tag == "AllyBullet")
    //        return;

    //    if (coll.gameObject.name.Contains("ShieldField") == true)
    //    {
    //        m_MoveSpeed = 15.0f * 0.2f;
    //    }
    //}

    // Update is called once per frame
    void Update()
    {
        if (0.0f < m_LifeTime)
        {
            m_LifeTime -= Time.deltaTime;
            if (m_LifeTime <= 0.0f)
                gameObject.SetActive(false);
        }

        transform.position += m_DirVec * Time.deltaTime * m_MoveSpeed;

        if(CameraResolution.m_ScWMax.x + 1.0f < transform.position.x ||
           transform.position.x < CameraResolution.m_ScWMin.x - 1.0f ||
           CameraResolution.m_ScWMax.y + 1.0f < transform.position.y ||
           transform.position.y < CameraResolution.m_ScWMin.y - 1.0f)
        { //총알이 화면을 벗어나면... 즉시 제거
            //Destroy(gameObject);
            gameObject.SetActive(false);
        }

    }//void Update()

    public void BulletSpawn(Vector3 a_StPos, Vector3 a_DirVec, 
                            float a_MvSpeed = 15.0f, float Att = 20.0f)
    {
        m_DirVec = a_DirVec;
        transform.position = new Vector3(a_StPos.x, a_StPos.y, 0.0f);
        m_MoveSpeed = a_MvSpeed;
    }
}
