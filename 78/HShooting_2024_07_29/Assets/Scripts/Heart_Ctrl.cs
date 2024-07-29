using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart_Ctrl : MonoBehaviour
{
    int m_DirVecX = 1;     //날아갈 X 방향 값
    int m_DirVecY = 1;     //날아갈 Y 방향 값
    Vector3 m_DirVec;
    float m_MoveSpeed = 9.9f;   //날아 다니는 속도

    // Start is called before the first frame update
    void Start()
    {
        m_DirVec = new Vector3(m_DirVecX, m_DirVecY, 0.0f);
        Destroy(gameObject, 10.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.x < CameraResolution.m_ScWMin.x + 0.5f ||
           CameraResolution.m_ScWMax.x - 0.5f < transform.position.x)
        {
            m_DirVecX = -m_DirVecX;
        }

        if(transform.position.y < CameraResolution.m_ScWMin.y + 0.5f ||
           CameraResolution.m_ScWMax.y - 0.5f < transform.position.y)
        {
            m_DirVecY = -m_DirVecY;
        }

        //m_DirVec = new Vector3(m_DirVecX, m_DirVecY, 0.0f);
        m_DirVec.x = m_DirVecX;
        m_DirVec.y = m_DirVecY;
        m_DirVec.z = 0.0f;
        m_DirVec.Normalize();

        transform.position += (m_DirVec * Time.deltaTime * m_MoveSpeed);
    }
}
