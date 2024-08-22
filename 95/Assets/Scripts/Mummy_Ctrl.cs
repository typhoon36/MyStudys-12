using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mummy_Ctrl : MonoBehaviour
{
    Transform PlayerTr;
    [HideInInspector] public float m_MoveVelocity = 13.0f;  //초당 이동 속도
    float m_SvMoveSpeed = 13.0f; //초당 이동 속도
    Transform m_MM_IconTr = null;
    Vector3 m_MM_CacPos = Vector3.zero;
    Animator m_Animator = null;

    // Start is called before the first frame update
    void Start()
    {
        m_SvMoveSpeed = m_MoveVelocity;

        //PlayerTr = GameObject.Find("Main Camera").GetComponent<Transform>();
        PlayerTr = Camera.main.transform;

        m_MM_IconTr = transform.Find("MiniMapIcon");
        m_Animator = GetComponentInChildren<Animator>();

    }//void Start()

    #region --- 버블 폭탄안에 들어왔을 때 처리 코드
    void FixedUpdate()
    {
        m_MoveVelocity = m_SvMoveSpeed; //이동속도를 저장되어 있던 기본값으로 적용 후
        m_Animator.speed = 1.0f;
    }

    void OnTriggerStay(Collider coll)
    {
        if(coll.gameObject.name.Contains("ShieldPrefab") == true)
        {
            m_MoveVelocity = m_SvMoveSpeed * 0.1f;  //이동속도를 10%로 감소시킴
            m_Animator.speed = 0.08f;
        }
    }
    #endregion --- 버블 폭탄안에 들어왔을 때 처리 코드

    void OnTriggerEnter(Collider coll)
    {
        if(coll.gameObject.name.Contains("HallucinPrefab") == true)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //--- 추적 이동 구현
        Vector3 a_TargetPos = PlayerTr.position;

        //--- 허상 추적 코드
        PlayerController a_RefPlayer = PlayerTr.GetComponent<PlayerController>();
        if(a_RefPlayer != null && a_RefPlayer.m_HLcinClone != null &&
            a_RefPlayer.m_HLcinClone.activeSelf == true)
        {
            a_TargetPos = a_RefPlayer.m_HLcinClone.transform.position;
        }
        //--- 허상 추적 코드

        Vector3 a_MoveDir = a_TargetPos - this.transform.position;
        a_MoveDir.y = 0.0f;

        transform.forward = a_MoveDir.normalized;
        Vector3 a_StepVec = transform.forward * m_MoveVelocity * Time.deltaTime;
        transform.Translate(a_StepVec, Space.World);

        float a_CacPosY = Game_Mgr.Inst.m_RefMap.SampleHeight(transform.position);
        transform.position = new Vector3(transform.position.x,
                                        a_CacPosY,
                                        transform.position.z);
        //--- 추적 이동 구현
        
        //--- 미니맵 아이콤 위치 재조정
        if(m_MM_IconTr != null)
        {
            m_MM_CacPos = m_MM_IconTr.position;
            m_MM_CacPos.y = 110.0f;
            m_MM_IconTr.position = m_MM_CacPos;
        }
        //--- 미니맵 아이콤 위치 재조정

    }//void Update()
}
