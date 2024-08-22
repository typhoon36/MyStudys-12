using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mummy_Ctrl : MonoBehaviour
{
    Transform PlayerTr;
    [HideInInspector] public float m_MoveVelocity = 13.0f;  //�ʴ� �̵� �ӵ�
    float m_SvMoveSpeed = 13.0f; //�ʴ� �̵� �ӵ�
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

    #region --- ���� ��ź�ȿ� ������ �� ó�� �ڵ�
    void FixedUpdate()
    {
        m_MoveVelocity = m_SvMoveSpeed; //�̵��ӵ��� ����Ǿ� �ִ� �⺻������ ���� ��
        m_Animator.speed = 1.0f;
    }

    void OnTriggerStay(Collider coll)
    {
        if(coll.gameObject.name.Contains("ShieldPrefab") == true)
        {
            m_MoveVelocity = m_SvMoveSpeed * 0.1f;  //�̵��ӵ��� 10%�� ���ҽ�Ŵ
            m_Animator.speed = 0.08f;
        }
    }
    #endregion --- ���� ��ź�ȿ� ������ �� ó�� �ڵ�

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
        //--- ���� �̵� ����
        Vector3 a_TargetPos = PlayerTr.position;

        //--- ��� ���� �ڵ�
        PlayerController a_RefPlayer = PlayerTr.GetComponent<PlayerController>();
        if(a_RefPlayer != null && a_RefPlayer.m_HLcinClone != null &&
            a_RefPlayer.m_HLcinClone.activeSelf == true)
        {
            a_TargetPos = a_RefPlayer.m_HLcinClone.transform.position;
        }
        //--- ��� ���� �ڵ�

        Vector3 a_MoveDir = a_TargetPos - this.transform.position;
        a_MoveDir.y = 0.0f;

        transform.forward = a_MoveDir.normalized;
        Vector3 a_StepVec = transform.forward * m_MoveVelocity * Time.deltaTime;
        transform.Translate(a_StepVec, Space.World);

        float a_CacPosY = Game_Mgr.Inst.m_RefMap.SampleHeight(transform.position);
        transform.position = new Vector3(transform.position.x,
                                        a_CacPosY,
                                        transform.position.z);
        //--- ���� �̵� ����
        
        //--- �̴ϸ� ������ ��ġ ������
        if(m_MM_IconTr != null)
        {
            m_MM_CacPos = m_MM_IconTr.position;
            m_MM_CacPos.y = 110.0f;
            m_MM_IconTr.position = m_MM_CacPos;
        }
        //--- �̴ϸ� ������ ��ġ ������

    }//void Update()
}
