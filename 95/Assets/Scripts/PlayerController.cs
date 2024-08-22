using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    float h = 0.0f;
    float v = 0.0f;

    float moveSpeed = 10.0f;    //�̵��ӵ�
    Vector3 moveDir = Vector3.zero; //�̵�����

    //--- ī�޶� ȸ���� ���� ����
    float rotSpeed = 350.0f;
    Vector3 m_CacVec = Vector3.zero;
    //--- ī�޶� ȸ���� ���� ����

    public GameObject m_MM_Arrow = null; //�̴ϸʿ��� ǥ���� �� ��ũ

    //--- ���� ���� ��ų
    public GameObject ShieldPrefab;
    float m_BsOnTime = 0.0f;        //BubbleShield
    float m_BsDuration = 20.0f;     //20�� ���� �ߵ�
    //--- ���� ���� ��ų

    //--- �ν��� ��ų
    float m_BstOnTime = 0.0f;       //Booster
    float m_BstDuration = 20.0f;
    //--- �ν��� ��ų

    //--- ��� ��ų
    public GameObject HallucinPrefab;
    [HideInInspector] public GameObject m_HLcinClone = null;
    float m_HcOnTime = 0.0f;
    float m_HcDuration = 20.0f;     //20�� ���� �ߵ�
    //--- ��� ��ų

    // Start is called before the first frame update
    void Start()
    {
        m_HLcinClone = Instantiate(HallucinPrefab);
        m_HLcinClone.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //--- ī�޶� ȸ�� ���� �κ�
        if(Input.GetMouseButton(1) == true)  //���콺 ������ ��ư ����� �ִ� ����
        {
            m_CacVec = transform.eulerAngles;

            m_CacVec.y += (rotSpeed * Time.deltaTime * Input.GetAxisRaw("Mouse X"));
            m_CacVec.x -= (rotSpeed * Time.deltaTime * Input.GetAxisRaw("Mouse Y"));

            if (180.0f < m_CacVec.x && m_CacVec.x < 340.0f)
                m_CacVec.x = 340.0f;

            if (12.0f < m_CacVec.x && m_CacVec.x <= 180.0f)
                m_CacVec.x = 12.0f;

            transform.eulerAngles = m_CacVec;
        }
        //--- ī�޶� ȸ�� ���� �κ�

        //--- �̵� ���� �κ�
        h = Input.GetAxis("Horizontal");    // -1.0f ~ 1.0f
        v = Input.GetAxis("Vertical");      // -1.0f ~ 1.0f

        //1�� ��� �����¿� �̵� ���� ���� ���
        moveDir = (Vector3.forward * v) + (Vector3.right * h);
        if (1.0f < moveDir.magnitude)
            moveDir.Normalize();

        if(0.0f < m_BstOnTime)
        {
            transform.Translate(moveDir * Time.deltaTime * moveSpeed * 2.7f, Space.Self);
        }
        else
        {
            transform.Translate(moveDir * Time.deltaTime * moveSpeed, Space.Self);
        }     
        //--- �̵� ���� �κ�
        //�ӵ� = �̵��Ÿ� / �ð�  --> �ӵ� * �ð� = �̵��Ÿ� --> �ð� * �ӵ� = �̵��Ÿ�

        ////--- 2�� ��� ���� ��ǥ�� �������� �̵���Ű�� ���
        //moveDir = (transform.forward * v) + (transform.right * h);
        //if (1.0f < moveDir.magnitude)
        //    moveDir.Normalize();

        //transform.position += moveDir * Time.deltaTime * moveSpeed;
        ////--- 2�� ��� ���� ��ǥ�� �������� �̵���Ű�� ���

        ////--- 3�� ��� ���� ��ǥ�踦 �������� �������� �̵���Ű�� ���
        //moveDir = (transform.forward * v) + (transform.right * h);
        //if (1.0f < moveDir.magnitude)
        //    moveDir.Normalize();

        //transform.Translate(moveDir * Time.deltaTime * moveSpeed, Space.World);
        ////--- 3�� ��� ���� ��ǥ�踦 �������� �������� �̵���Ű�� ���

        //--- ĳ���� ���̰� ����
        transform.position = new Vector3(transform.position.x,
                                Game_Mgr.Inst.m_RefMap.SampleHeight(transform.position) + 5.0f,
                                transform.position.z);
        //--- ĳ���� ���̰� ����

        //--- �̴ϸʿ��� ǥ���� �� ������ ��ġ ������
        m_MM_Arrow.transform.position = new Vector3(transform.position.x, 110.0f, transform.position.z);
        m_MM_Arrow.transform.eulerAngles = new Vector3(90.0f, transform.eulerAngles.y, 0.0f);
        //--- �̴ϸʿ��� ǥ���� �� ������ ��ġ ������

        Update_Skill();

    }//void Update()

    void OnTriggerEnter(Collider coll)
    {
        if(coll.gameObject.tag == "Enemy")  //���̶���� �浹
        {
            Game_Mgr.Inst.DecreaseHp(); //���ΰ� �ϵ� ���� ��Ű��...
            Destroy(coll.gameObject); //�浹�� ���� ����
        }
        else if(coll.gameObject.tag == "Pet")
        {
            string a_CloneName = coll.gameObject.name;
            string a_AmName = a_CloneName.Replace("(Clone)", "");
            AnimalType a_AmType = (AnimalType)System.Enum.Parse(typeof(AnimalType), a_AmName);
            //a_AmName ���ڿ��� enum�� Ÿ������ ��ȯ�� �ֱ� 
            Game_Mgr.Inst.AnimalIcon[(int)a_AmType].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            Destroy(coll.gameObject);
            GameClearCheck();
        }

        //else if(coll.gameObject.name.Contains("CatPrefab") == true)
        //{
        //    Game_Mgr.Inst.AnimalIcon[0].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        //    Destroy(coll.gameObject);
        //    GameClearCheck();
        //}
        //else if (coll.gameObject.name.Contains("DuckPrefab") == true)
        //{
        //    Game_Mgr.Inst.AnimalIcon[1].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        //    Destroy(coll.gameObject);
        //    GameClearCheck();
        //}
        //else if (coll.gameObject.name.Contains("molePrefab") == true)
        //{
        //    Game_Mgr.Inst.AnimalIcon[2].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        //    Destroy(coll.gameObject);
        //    GameClearCheck();
        //}
        //else if (coll.gameObject.name.Contains("penguinPrefab") == true)
        //{
        //    Game_Mgr.Inst.AnimalIcon[3].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        //    Destroy(coll.gameObject);
        //    GameClearCheck();
        //}
        //else if (coll.gameObject.name.Contains("sheepPrefab") == true)
        //{
        //    Game_Mgr.Inst.AnimalIcon[4].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        //    Destroy(coll.gameObject);
        //    GameClearCheck();
        //}

    }//void OnTriggerEnter(Collider coll)

    void GameClearCheck()
    {
        //--- �����ֱ�
        Game_Mgr.Inst.AddGold(20);
        //--- �����ֱ�

        bool IsAllSave = true;
        for(int i = 0; i < Game_Mgr.Inst.AnimalIcon.Length; i++)
        {
            if (Game_Mgr.Inst.AnimalIcon[i].color.a < 1.0f)
            {
                IsAllSave = false;
                break;
            }
        }//for(int i = 0; i < Game_Mgr.Inst.AnimalIcon.Length; i++)

        if(IsAllSave == true)
        {
            Game_Mgr.IsClear = true; // true �̸� �̼� Ŭ���� ����
            SceneManager.LoadScene("GameOver");
        }

    }//void GameClearCheck()

    void Update_Skill()
    {
        //--- �ν��� ��ų
        if(0.0f < m_BstOnTime)
        {
            m_BstOnTime -= Time.deltaTime;
        }
        //--- �ν��� ��ų

        //--- ���(�ҷ�ó��̼�) ���� ������Ʈ
        if(0.0f < m_HcOnTime)
        {
            m_HcOnTime -= Time.deltaTime;
            if (m_HLcinClone != null && m_HLcinClone.activeSelf == false)
                m_HLcinClone.SetActive(true);
        }
        else
        {
            if (m_HLcinClone != null && m_HLcinClone.activeSelf == true)
                m_HLcinClone.SetActive(false);
        }
        //--- ���(�ҷ�ó��̼�) ���� ������Ʈ

    }//void Update_Skill()

    public void UseSkill_Item(SkillType a_SkType)
    {
        if(a_SkType == SkillType.Skill_0) //���� ��ź ��ų
        {
            GameObject ShieldClone = Instantiate(ShieldPrefab);
            ShieldClone.transform.position = transform.position + transform.forward * 15.0f;

            m_BsOnTime = m_BsDuration;

            //UI ��Ÿ�� �ߵ�
            Game_Mgr.Inst.SkillCoolMethod(a_SkType, m_BsOnTime, m_BsDuration);

        }//if(a_SkType == SkillType.Skill_0) //���� ��ź ��ų
        else if(a_SkType == SkillType.Skill_1) //�ν��� ��ų
        {
            if (0.0f < m_BstOnTime)
                return;

            m_BstOnTime = m_BstDuration;

            //UI ��Ÿ�� �ߵ�
            Game_Mgr.Inst.SkillCoolMethod(a_SkType, m_BstOnTime, m_BstDuration);

        }//else if(a_SkType == SkillType.Skill_1) //�ν��� ��ų
        else if(a_SkType == SkillType.Skill_2)  //��� ��ų
        {
            if(0.0f < m_HcOnTime)
                return;

            m_HcOnTime = m_HcDuration;

            m_HLcinClone.SetActive(true);
            Vector3 a_VecPos = transform.position + transform.forward * 10.0f;
            m_HLcinClone.transform.position = new Vector3(a_VecPos.x,
                                    Game_Mgr.Inst.m_RefMap.SampleHeight(a_VecPos),
                                    a_VecPos.z);
            m_HLcinClone.transform.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, 0.0f);
            Transform a_Child = m_HLcinClone.transform.Find("MiniMapIcon");
            if(a_Child != null)
            {
                Vector3 a_CurPos = a_Child.position;
                if (110.0f < a_CurPos.y)
                    a_CurPos.y = 110.0f;

                a_Child.position = a_CurPos;
            }//if(a_Child != null)

            //UI ��Ÿ�� �ߵ�
            Game_Mgr.Inst.SkillCoolMethod(a_SkType, m_HcOnTime, m_HcDuration);

        }//else if(a_SkType == SkillType.Skill_2)  //��� ��ų

        int a_SkIdx = (int)a_SkType;
        GlobalValue.g_SkillCount[a_SkIdx]--;
        //string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        //PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);
        NetworkMgr.Inst.PushPacket(PacketType.UpdateItem);   //<-- ������ ������ �� ���� ��û

    }//public void UseSkill_Item(SkillType a_SkType)
}
