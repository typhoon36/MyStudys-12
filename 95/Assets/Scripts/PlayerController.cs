using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    float h = 0.0f;
    float v = 0.0f;

    float moveSpeed = 10.0f;    //이동속도
    Vector3 moveDir = Vector3.zero; //이동방향

    //--- 카메라 회전을 위한 변수
    float rotSpeed = 350.0f;
    Vector3 m_CacVec = Vector3.zero;
    //--- 카메라 회전을 위한 변수

    public GameObject m_MM_Arrow = null; //미니맵에서 표시해 줄 마크

    //--- 버블 쉴드 스킬
    public GameObject ShieldPrefab;
    float m_BsOnTime = 0.0f;        //BubbleShield
    float m_BsDuration = 20.0f;     //20초 동안 발동
    //--- 버블 쉴드 스킬

    //--- 부스터 스킬
    float m_BstOnTime = 0.0f;       //Booster
    float m_BstDuration = 20.0f;
    //--- 부스터 스킬

    //--- 허상 스킬
    public GameObject HallucinPrefab;
    [HideInInspector] public GameObject m_HLcinClone = null;
    float m_HcOnTime = 0.0f;
    float m_HcDuration = 20.0f;     //20초 동안 발동
    //--- 허상 스킬

    // Start is called before the first frame update
    void Start()
    {
        m_HLcinClone = Instantiate(HallucinPrefab);
        m_HLcinClone.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //--- 카메라 회전 구현 부분
        if(Input.GetMouseButton(1) == true)  //마우스 오른쪽 버튼 누라고 있는 동안
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
        //--- 카메라 회전 구현 부분

        //--- 이동 구현 부분
        h = Input.GetAxis("Horizontal");    // -1.0f ~ 1.0f
        v = Input.GetAxis("Vertical");      // -1.0f ~ 1.0f

        //1번 방법 전후좌우 이동 방향 벡터 계산
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
        //--- 이동 구현 부분
        //속도 = 이동거리 / 시간  --> 속도 * 시간 = 이동거리 --> 시간 * 속도 = 이동거리

        ////--- 2번 방법 직접 좌표를 누적시켜 이동시키는 방식
        //moveDir = (transform.forward * v) + (transform.right * h);
        //if (1.0f < moveDir.magnitude)
        //    moveDir.Normalize();

        //transform.position += moveDir * Time.deltaTime * moveSpeed;
        ////--- 2번 방법 직접 좌표를 누적시켜 이동시키는 방식

        ////--- 3번 방법 월드 좌표계를 기준으로 누적시켜 이동시키는 방식
        //moveDir = (transform.forward * v) + (transform.right * h);
        //if (1.0f < moveDir.magnitude)
        //    moveDir.Normalize();

        //transform.Translate(moveDir * Time.deltaTime * moveSpeed, Space.World);
        ////--- 3번 방법 월드 좌표계를 기준으로 누적시켜 이동시키는 방식

        //--- 캐릭터 높이값 조정
        transform.position = new Vector3(transform.position.x,
                                Game_Mgr.Inst.m_RefMap.SampleHeight(transform.position) + 5.0f,
                                transform.position.z);
        //--- 캐릭터 높이값 조정

        //--- 미니맵에서 표시해 줄 아이콤 위치 재조정
        m_MM_Arrow.transform.position = new Vector3(transform.position.x, 110.0f, transform.position.z);
        m_MM_Arrow.transform.eulerAngles = new Vector3(90.0f, transform.eulerAngles.y, 0.0f);
        //--- 미니맵에서 표시해 줄 아이콤 위치 재조정

        Update_Skill();

    }//void Update()

    void OnTriggerEnter(Collider coll)
    {
        if(coll.gameObject.tag == "Enemy")  //미이라와의 충돌
        {
            Game_Mgr.Inst.DecreaseHp(); //주인공 하드 감소 시키기...
            Destroy(coll.gameObject); //충돌된 몬스터 제거
        }
        else if(coll.gameObject.tag == "Pet")
        {
            string a_CloneName = coll.gameObject.name;
            string a_AmName = a_CloneName.Replace("(Clone)", "");
            AnimalType a_AmType = (AnimalType)System.Enum.Parse(typeof(AnimalType), a_AmName);
            //a_AmName 문자열을 enum형 타입으로 변환해 주기 
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
        //--- 보상주기
        Game_Mgr.Inst.AddGold(20);
        //--- 보상주기

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
            Game_Mgr.IsClear = true; // true 이면 미션 클리어 상태
            SceneManager.LoadScene("GameOver");
        }

    }//void GameClearCheck()

    void Update_Skill()
    {
        //--- 부스터 스킬
        if(0.0f < m_BstOnTime)
        {
            m_BstOnTime -= Time.deltaTime;
        }
        //--- 부스터 스킬

        //--- 허상(할루시네이션) 상태 업데이트
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
        //--- 허상(할루시네이션) 상태 업데이트

    }//void Update_Skill()

    public void UseSkill_Item(SkillType a_SkType)
    {
        if(a_SkType == SkillType.Skill_0) //버블 폭탄 스킬
        {
            GameObject ShieldClone = Instantiate(ShieldPrefab);
            ShieldClone.transform.position = transform.position + transform.forward * 15.0f;

            m_BsOnTime = m_BsDuration;

            //UI 쿨타임 발동
            Game_Mgr.Inst.SkillCoolMethod(a_SkType, m_BsOnTime, m_BsDuration);

        }//if(a_SkType == SkillType.Skill_0) //버블 폭탄 스킬
        else if(a_SkType == SkillType.Skill_1) //부스터 스킬
        {
            if (0.0f < m_BstOnTime)
                return;

            m_BstOnTime = m_BstDuration;

            //UI 쿨타임 발동
            Game_Mgr.Inst.SkillCoolMethod(a_SkType, m_BstOnTime, m_BstDuration);

        }//else if(a_SkType == SkillType.Skill_1) //부스터 스킬
        else if(a_SkType == SkillType.Skill_2)  //허상 스킬
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

            //UI 쿨타임 발동
            Game_Mgr.Inst.SkillCoolMethod(a_SkType, m_HcOnTime, m_HcDuration);

        }//else if(a_SkType == SkillType.Skill_2)  //허상 스킬

        int a_SkIdx = (int)a_SkType;
        GlobalValue.g_SkillCount[a_SkIdx]--;
        //string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        //PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);
        NetworkMgr.Inst.PushPacket(PacketType.UpdateItem);   //<-- 서버에 아이템 수 갱신 요청

    }//public void UseSkill_Item(SkillType a_SkType)
}
