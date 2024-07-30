using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMs_Ctrl : MonoBehaviour
{
    float m_MoveSpeed = 8.0f;   //이동속도
    float m_RotSpeed = 250.0f;  //200.0f;  //회전속도

    //--- 유도탄 변수
    [HideInInspector] public GameObject Target_Obj = null;  //타겟 참조 변수
    Vector3 m_DesireDir;    //타덱을 향하는 방향 변수
    //--- 유도탄 변수

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 5.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Target_Obj == null) //추적해야 할 타겟이 없으면...
            FindEnemy();     //타겟을 찾아주는 함수

        if (Target_Obj != null)
            BulletHoming();     //타겟을 향해 추적 이동하는 행동 패턴 함수
        else
            transform.Translate(transform.right * m_MoveSpeed * Time.deltaTime, Space.World);
        //미사일 머리쪽으로 이동
    }//void Update()

    void FindEnemy()  //타겟을 찾아주는 함수
    {
        GameObject[] a_EnemyList = GameObject.FindGameObjectsWithTag("Monster");

        if (a_EnemyList.Length <= 0) //등장해 있는 몬스터가 하나도 없으면...
            return;     //추적할 대상을 찾지 못한다.

        GameObject a_Find_Mon = null;
        Vector3 a_CacVec = Vector3.zero;
        Monster_Ctrl a_RefMon = null;
        for(int i = 0; i < a_EnemyList.Length; i++)
        {
            a_RefMon = a_EnemyList[i].GetComponent<Monster_Ctrl>();

            if (a_RefMon == null)
                continue;

            if (a_RefMon.m_HomingLockOn != null)
                continue;

            a_Find_Mon = a_EnemyList[i].gameObject;
            a_RefMon.m_HomingLockOn = this.gameObject;
            break;

        }//for(int i = 0; i < a_EnemyList.Length; i++)

        Target_Obj = a_Find_Mon;

    }//void FindEnemy()  //타겟을 찾아주는 함수

    void BulletHoming()  //타겟을 향해 추적 이동하는 행동 패턴 함수
    {
        m_DesireDir = Target_Obj.transform.position - transform.position;
        m_DesireDir.z = 0.0f;
        m_DesireDir.Normalize();

        //타겟을 향해 유도탄 회전시키기
        float angle = Mathf.Atan2(m_DesireDir.y, m_DesireDir.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward); //각도를 쿼터니온으로
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
                                        targetRot, m_RotSpeed * Time.deltaTime);

        //유도탄이 바라보는 방향쪽으로 움직이게 하기...
        transform.Translate(transform.right * m_MoveSpeed * Time.deltaTime, Space.World);

    }//void BulletHoming()  //타겟을 향해 추적 이동하는 행동 패턴 함수
}
