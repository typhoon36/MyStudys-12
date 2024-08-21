using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum CamCtrlMode
{
    CCM_Default,
    CCM_WithMsBtn
}

public class FollowCam : MonoBehaviour
{
    public GameObject[] CharObjs;   //ĳ���� ����
    int CharType = 0;

    public Transform targetTr;      //������ Ÿ�� ���ӿ�����Ʈ�� Transform ����
    FireCtrl m_FireCtrl = null;     //������ Ÿ���� ���� �ִ� FireCtrl ��ũ��Ʈ ���� ����
    public float dist = 10.0f;      //ī�޶���� ���� �Ÿ�
    public float height = 3.0f;     //ī�޶��� ���� ����
    public float dampTrace = 20.0f; //�ε巯�� ������ ���� ����

    Vector3 m_PlayerVec = Vector3.zero;
    float rotSpeed = 10.0f;

    //--- Wall ����ȭ ó���� ���� ����Ʈ ���� ����
    Vector3 a_CacVLen = Vector3.zero;
    Vector3 a_CacDirVec = Vector3.zero;

    LayerMask m_WallLyMask = -1;
    List<WallCtrl> m_SW_List = new List<WallCtrl>();
    //--- Wall ����ȭ ó���� ���� ����Ʈ ���� ����

    //--- ī�޶� ��ġ ���� ����
    float m_RotV = 0.0f;            //���콺 ���� ���۰� ���� ����
    float m_DefaultRotV = 25.2f;    //���� ������ ȸ�� ����
    float m_MarginRotV  = 22.3f;    //�ѱ����� ���� ����
    float m_MinLimitV   = -17.9f;   //�� �Ʒ� ���� ����
    float m_MaxLimitV   = 52.9f;    //�� �Ʒ� ���� ����
    float m_MaxDist     = 4.0f;     //���콺 �� �ƿ� �ִ� �Ÿ� ���� ��
    float m_MinDist     = 2.0f;     //���콺 �� �� �ִ� �Ÿ� ���� ��
    float m_ZoomSpeed   = 0.7f;     //���콺 �� ���ۿ� ���� �� �� �ƿ� ���ǵ� ���� ��

    Quaternion m_BuffRot;           //ī�޶� ȸ�� ���� ����
    Vector3 m_BuffPos;              //ī�޶� ȸ���� ���� ��ġ ��ǥ ���� ����
    Vector3 m_BasicPos = Vector3.zero; //��ġ ���� ����
    //--- ī�޶� ��ġ ���� ����

    //--- �� ���� ���� ���� ����
    public static Vector3 m_RifleDir = Vector3.zero;    //�� ���� ����
    Quaternion m_RFCacRot;
    Vector3 m_RFCacPos = Vector3.forward;
    //--- �� ���� ���� ���� ����

    //--- ī�޶� ��Ʈ�� ��� ���� ����
    public static CamCtrlMode m_CCMMode = CamCtrlMode.CCM_Default;
    public static float m_CCMDelay = 1.0f;
    //bool IsShowCursor = false;
    //--- ī�޶� ��Ʈ�� ��� ���� ����

    // Start is called before the first frame update
    void Start()
    {
        dist = 3.4f;
        height = 2.8f;

        //--- Side Wall ����Ʈ �����...
        m_WallLyMask = 1 << LayerMask.NameToLayer("SideWall");
        //"SideWall" ���̾ Lay üũ �ϱ� ���� ����ũ ���� ����

        GameObject[] a_SideWalls = GameObject.FindGameObjectsWithTag("SideWall");
        for(int i = 0; i < a_SideWalls.Length; i++)
        {
            WallCtrl a_WCtrl = a_SideWalls[i].GetComponent<WallCtrl>();
            a_WCtrl.m_IsColl = false;
            a_WCtrl.WallAlphaOnOff(false);  //������ȭ�� ����
            m_SW_List.Add(a_WCtrl);
        }
        //--- Side Wall ����Ʈ �����...

        //--- ī�޶� ��ġ ���
        m_RotV = m_DefaultRotV;
        //--- ī�޶� ��ġ ���

        if (SceneManager.GetActiveScene().name == "scLevel02")
            m_RotV = 10.2f;

        //--- ī�޶� ��Ʈ�� ��� �ʱ�ȭ
        //m_CCMMode = CamCtrlMode.CCM_Default;
        //GameMgr.Inst.m_Help_Text.text = "< Esc >Ű : Def_Mode";
        m_CCMMode = CamCtrlMode.CCM_WithMsBtn;
        GameMgr.Inst.m_Help_Text.text = "< Esc >Ű : Btn_Mode";

        m_CCMDelay = 0.3f;
        //--- ī�޶� ��Ʈ�� ��� �ʱ�ȭ

    }//void Start()

    void Update()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
        {
            Cursor.visible = true;  //���콺 Ŀ���� ���̰� �ϴ� �ɼ�
            Cursor.lockState = CursorLockMode.None; //���콺 Ŀ���� ȭ�� ������ ������ �ǰ� �ϴ� �ɼ�
            return;
        }

        if(Input.GetKeyDown(KeyCode.Escape) == true)
        {
            if(m_CCMMode == CamCtrlMode.CCM_Default)
            {
                m_CCMMode = CamCtrlMode.CCM_WithMsBtn;
                GameMgr.Inst.m_Help_Text.text = "< Esc >Ű : Btn_Mode";
            }
            else //if (m_CCMMode == CamCtrlMode.CCM_WithMsBtn)
            {
                m_CCMMode = CamCtrlMode.CCM_Default;
                GameMgr.Inst.m_Help_Text.text = "< Esc >Ű : Def_Mode";
            }
        }//if(Input.GetKeyDown(KeyCode.Escape) == true)

        if(m_CCMMode == CamCtrlMode.CCM_Default)
        {
            Cursor.visible = false;  //Ŀ���� �Ⱥ��̰� ó���ϴ� �ɼ�
            Cursor.lockState = CursorLockMode.Locked; //Ŀ���� ȭ�� ������ ��� �� ���� �ϴ� �ɼ�
        }
        else //if (m_CCMMode == CamCtrlMode.CCM_WithMsBtn)
        {
            Cursor.visible = true;  //Ŀ���� ���̰� ó���ϴ� �ɼ�
            Cursor.lockState = CursorLockMode.None; //Ŀ���� ȭ�� ������ ��� �� �ְ� �ϴ� �ɼ�
        }

        bool IsMsRot = false;
        float a_AddRotSpeed = 235.0f;
        if (0.0f < m_CCMDelay)
            m_CCMDelay -= Time.deltaTime;

        if (m_CCMMode == CamCtrlMode.CCM_Default)
        {
            if (m_CCMDelay <= 0.0f)
                IsMsRot = true;

            a_AddRotSpeed = 180.0f;
        }//if(m_CCMMode == CamCtrlMode.CCM_Default) 
        else
        {
            if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
            if (GameMgr.IsPointerOverUIObject() == false)
            {
               IsMsRot = true;
            }
        }//if (m_CCMMode == CamCtrlMode.CCM_WithMsBtn)

        //if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
        //if (GameMgr.IsPointerOverUIObject() == false)
        if (IsMsRot == true)
        {
            ////--- ī�޶� �� �Ʒ� �ٶ󺸴� ���� ������ ���� ������ ���� �ڵ�
            //height -= (rotSpeed * Time.deltaTime * Input.GetAxis("Mouse Y"));

            //if (height < 0.1f)
            //    height = 0.1f;

            //if (5.7f < height)
            //    height = 5.7f;
            ////--- ī�޶� �� �Ʒ� �ٶ󺸴� ���� ������ ���� ������ ���� �ڵ�

            //--- (����ǥ�踦 �̿��� ���� ȸ�� ó�� �ڵ�)
            //float a_AddRotSpeed = 235.0f;
            rotSpeed = a_AddRotSpeed;
            m_RotV -= (rotSpeed * Time.deltaTime * Input.GetAxisRaw("Mouse Y"));
            if (m_RotV < m_MinLimitV)
                m_RotV = m_MinLimitV;
            if (m_MaxLimitV < m_RotV)
                m_RotV = m_MaxLimitV;
            //--- (����ǥ�踦 �̿��� ���� ȸ�� ó�� �ڵ�)
        }

        //--- ī�޶� ���� �ܾƿ�
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && dist < m_MaxDist)
        {
            dist += m_ZoomSpeed;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0 && dist > m_MinDist)
        {
            dist -= m_ZoomSpeed;
        }
        //--- ī�޶� ���� �ܾƿ�

        //if(Input.GetKeyDown(KeyCode.C))
        //{
        //    CharacterChange();
        //}

    }//void Update()

    //Update �Լ� ȣ�� ���� �� ���� ȣ��Ǵ� �Լ��� LateUpdate ���
    //������ Ÿ���� �̵��� ����� ���Ŀ� ī�޶� �����ϱ� ���� LateUpdate ���
    // Update is called once per frame
    void LateUpdate()
    {
        m_PlayerVec = targetTr.position;
        m_PlayerVec.y += 1.2f;

        ////---ī�޶� ��ġ ��� �ִ� ���밭�� �ҽ�(���� ��ǥ��� ���)
        ////ī�޶��� ��ġ�� ��������� dist ������ŭ �������� ��ġ�ϰ�
        ////height ������ŭ ���� �ø�
        //transform.position = Vector3.Lerp(transform.position,
        //                                    targetTr.position
        //                                    - (targetTr.forward * dist)
        //                                    + (Vector3.up * height),
        //                                    Time.deltaTime * dampTrace);
        ////---ī�޶� ��ġ ��� �ִ� ���밭�� �ҽ�(���� ��ǥ��� ���)

        //--- (����ǥ�踦 ������ǥ��� ȯ���ؼ� ī�޶��� ��ġ�� ����ִ� �ڵ�)
        m_BuffRot = Quaternion.Euler(m_RotV, targetTr.eulerAngles.y, 0.0f);
        m_BasicPos.x = 0.0f;
        m_BasicPos.y = 0.0f;
        m_BasicPos.z = -dist;
        m_BuffPos = m_PlayerVec + (m_BuffRot * m_BasicPos);
        transform.position = Vector3.Lerp(transform.position, m_BuffPos,
                                                  Time.deltaTime * dampTrace);
        //--- (����ǥ�踦 ������ǥ��� ȯ���ؼ� ī�޶��� ��ġ�� ����ִ� �ڵ�)

        //ī�޶� Ÿ�� ���ӿ�����Ʈ�� �ٶ󺸰� ����
        transform.LookAt(m_PlayerVec);

        //--- Wall ī�޶� ���� �� ���� ó�� �κ�
        a_CacVLen = transform.position - m_PlayerVec;
        //���ΰ����� ī�޶� ���ϴ� ���� ���� 
        //������ ������ ���� ������ ���� ����� üũ�ϴ� �� �� ������ ���Ƽ�...
        a_CacDirVec = a_CacVLen.normalized;
        GameObject a_FindObj = null;
        RaycastHit a_HitInfo;
        if(Physics.Raycast(m_PlayerVec + (-a_CacDirVec * 1.0f),
                           a_CacDirVec, out a_HitInfo, a_CacVLen.magnitude + 4.0f,
                           m_WallLyMask.value))
        {
            a_FindObj = a_HitInfo.collider.gameObject;
        }

        for(int i = 0; i < m_SW_List.Count; i++) 
        {
            if (m_SW_List[i].gameObject == a_FindObj)
            {
                if (m_SW_List[i].m_IsColl == false)
                {
                    m_SW_List[i].WallAlphaOnOff(true);  //����ȭ
                    m_SW_List[i].m_IsColl = true;
                }
            }
            else
            {
                if (m_SW_List[i].m_IsColl == true)
                {
                    m_SW_List[i].WallAlphaOnOff(false);  //������ȭ
                    m_SW_List[i].m_IsColl = false;
                }
            }
        }//for(int i = 0; i < m_SW_List.Count; i++) 
         //--- Wall ī�޶� ���� �� ���� ó�� �κ�

        //--- Rifle ���� ���
        if (m_FireCtrl == null)
            m_FireCtrl = targetTr.GetComponent<FireCtrl>();

        Vector3 a_cPos = Vector3.zero;
        if(m_RotV < 6.0f)
        {
            a_cPos = m_FireCtrl.firePos.localPosition;
            a_cPos.y = 1.53f;
            m_FireCtrl.firePos.localPosition = a_cPos;
        }
        else
        {
            a_cPos = m_FireCtrl.firePos.localPosition;
            a_cPos.y = 1.42f;
            m_FireCtrl.firePos.localPosition = a_cPos;
        }

        m_RFCacRot = Quaternion.Euler(
            Camera.main.transform.eulerAngles.x - m_MarginRotV,
            targetTr.eulerAngles.y,
            0.0f);

        m_RifleDir = m_RFCacRot * m_RFCacPos;
        //--- Rifle ���� ���

    }//void LateUpdate()

    void CharacterChange()
    {
        Vector3 a_Pos = CharObjs[CharType].transform.position;
        Quaternion a_Rot = CharObjs[CharType].transform.rotation;
        CharObjs[CharType].SetActive(false);
        CharType++;
        if (1 < CharType)
            CharType = 0;
        CharObjs[CharType].SetActive(true);
        CharObjs[CharType].transform.position = a_Pos;
        CharObjs[CharType].transform.rotation = a_Rot;
        targetTr = CharObjs[CharType].transform;
    }
}
