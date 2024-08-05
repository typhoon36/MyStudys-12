using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MonType
{
    MT_Zombi,
    MT_Missile,
    MT_Boss
}

public enum BossState
{
    BS_APPEAR_MOVE,     //등장 이동 상태 
    BS_NORMAL_ATT,      //기본 공격 상태
    BS_FEVER_ATT        //피버 타입 공격 상태
}

public class Monster_Ctrl : MonoBehaviour
{
    public MonType m_MonType = MonType.MT_Zombi;

    //--- 몬스터 체력 변수
    float m_MaxHp = 200.0f;
    float m_CurHp = 200.0f;
    public Image m_HpBar = null;
    //--- 몬스터 체력 변수

    float m_Speed = 4.0f;   //이동속도
    Vector3 m_CurPos;       //위치 계산용 변수
    Vector3 m_SpawnPos;     //스폰 위치

    float m_CacPosY = 0.0f; //싸인 함수에 들어갈 누적 각도 계산용 변수
    float m_RandY   = 0.0f; //랜덤한 진폭값 저장용 변수
    float m_CycleSpeed = 0.0f;  //랜덤한 진동 속도 변수
    float m_SvCycleSpeed = 0.0f;  //랜덤한 진동 속도 Save 변수

    //--- 총알 발사 관련 변수 선언
    public GameObject m_ShootPos = null;
    public GameObject m_BulletPrefab = null;
    float shoot_Time = 0.0f;     //총알 발사 주기 계산용 변수
    float shoot_Delay = 1.5f;    //총알 쿨 타임
    float BulletMySpeed = 10.0f; //총알 이동 속도
    //--- 총알 발사 관련 변수 선언

    //--- 미사일 행동 패턴에 필요한 변수
    HeroCtrl m_RefHero = null;  //몬스터가 추적하게 될 주인공 객체 변수
    Vector3 m_DirVec;
    //--- 미사일 행동 패턴에 필요한 변수

    //--- 보스의 행동 패턴 관련 변수
    BossState m_BossState = BossState.BS_APPEAR_MOVE;   //등장 이동 상태
    int m_ShootCount = 0;
    //--- 보스의 행동 패턴 관련 변수

    [HideInInspector] public GameObject m_HomingLockOn = null;
    //이 몬스터를 추적하고 있는 유도탄의 참조 변수
    //이 변수의 상태를 보고 락온되어 있는지 아닌지를 확인한다.

    // Start is called before the first frame update
    void Start()
    {
        m_RefHero = GameObject.FindObjectOfType<HeroCtrl>();

        m_SpawnPos = transform.position;    //몬스터의 스폰 위치 저장
        m_RandY = Random.Range(0.5f, 2.0f); //Sin 함수의 랜덤 진폭
        m_CycleSpeed = Random.Range(1.8f, 5.0f);    //진동수 랜덤값
        m_SvCycleSpeed = m_CycleSpeed;  //랜덤한 진동 속도 Save 변수

        if (m_MonType == MonType.MT_Boss)
        {
            m_MaxHp = 3000.0f;  //최대 체력치
            m_CurHp = m_MaxHp;  //현재 체력
        }
    }

    //void FixedUpdate()
    //{
    //    m_Speed = 4.0f;   //이동속도
    //    m_CycleSpeed = m_SvCycleSpeed;  //랜덤한 진동 속도 Save 변수
    //}

    //void OnTriggerStay2D(Collider2D coll)
    //{
    //    if (coll.gameObject.name.Contains("ShieldField") == true)
    //    //if (coll.gameObject.tag == "Shield")
    //    {
    //        m_Speed = 4.0f * 0.2f;   //이동속도
    //        m_CycleSpeed = m_SvCycleSpeed * 0.2f;  //랜덤한 진동 속도 Save 변수
    //    }
    //}

    // Update is called once per frame
    void Update()
    {
        if (m_MonType == MonType.MT_Zombi)
            Zombi_AI_Update();
        else if (m_MonType == MonType.MT_Missile)
            Missile_AI_Update();
        else if (m_MonType == MonType.MT_Boss)
            Boss_AI_Update();

        if (this.transform.position.x < CameraResolution.m_ScWMin.x - 2.0f)
            Destroy(gameObject);    //왼쪽 화면을 벗어나면 즉시 제거
    }

    void Zombi_AI_Update()
    {
        m_CurPos = transform.position;
        m_CurPos.x += (-1.0f * m_Speed * Time.deltaTime);
        m_CacPosY += (Time.deltaTime * m_CycleSpeed);
        m_CurPos.y = m_SpawnPos.y + Mathf.Sin(m_CacPosY) * m_RandY;
        transform.position = m_CurPos;

        //--- 총알 발사
        if (m_BulletPrefab == null)
            return;

        shoot_Time += Time.deltaTime;
        if(shoot_Delay <= shoot_Time)
        {
            shoot_Time = 0.0f;

            //GameObject a_NewObj = Instantiate(m_BulletPrefab);
            //Bullet_Ctrl a_BulletSc = a_NewObj.GetComponent<Bullet_Ctrl>();
            //a_BulletSc.BulletSpawn(m_ShootPos.transform.position, Vector3.left, BulletMySpeed);

            Bullet_Ctrl a_BulletSc = BulletPool_Mgr.Inst.GetEnBulletPool();
            a_BulletSc.gameObject.SetActive(true);
            a_BulletSc.BulletSpawn(m_ShootPos.transform.position,
                                                Vector3.left, BulletMySpeed);
            a_BulletSc.transform.eulerAngles = Vector3.zero;

        }//if(shoot_Delay <= shoot_Time)
        //--- 총알 발사
    }

    void Missile_AI_Update()
    {
        m_CurPos = transform.position;
        m_DirVec = Vector3.left;

        if(m_RefHero != null)
        {
            Vector3 a_CacVec = m_RefHero.transform.position - transform.position;
            m_DirVec = a_CacVec;

            //미사일이 주인공과의 거리가 우측방향으로 3.5m 이상이면 높낮이 변화없이
            //좌측으로만 이동시키려는 의도
            if (a_CacVec.x < -3.5f)
                m_DirVec.y = 0.0f;
        }

        m_DirVec.Normalize();
        m_DirVec.x = -1.0f; //주인공을 지나치고 난 후에는 무조건 왼쪽 방향으로 이동하게...
        m_DirVec.z = 0.0f;

        m_CurPos += (m_DirVec * Time.deltaTime * m_Speed);
        transform.position = m_CurPos;
    }

    void Boss_AI_Update()
    {
        if(m_BossState == BossState.BS_APPEAR_MOVE)  //등장 이동 상태
        {
            m_CurPos = transform.position;
            float a_ArrivePos = CameraResolution.m_ScWMax.x - 1.9f; //도착 위치
            if(a_ArrivePos < m_CurPos.x)
            {
                m_CurPos.x += (-1.0f * Time.deltaTime * m_Speed);   //왼쪽으로 이동
                if(m_CurPos.x <= a_ArrivePos) //도착위치까지 도착했을 대 
                {
                    shoot_Time = 1.28f;
                    m_BossState = BossState.BS_FEVER_ATT;
                }
            }//if(a_ArrivePos < m_CurPos.x)

            transform.position = m_CurPos;

        }//if(m_BossState == BossState.BS_APPEAR_MOVE)  //등장 이동 상태
        else if(m_BossState == BossState.BS_NORMAL_ATT) //일반 공격
        {
            shoot_Time -= Time.deltaTime;
            if (shoot_Time <= 0.0f)
            {
                //일반공격
                Vector3 a_TargetV =
                        m_RefHero.transform.position - m_ShootPos.transform.position;
                a_TargetV.z = 0.0f;
                a_TargetV.Normalize();

                //GameObject a_NewObj = Instantiate(m_BulletPrefab);
                //Bullet_Ctrl a_BulletSc = a_NewObj.GetComponent<Bullet_Ctrl>();
                //a_BulletSc.BulletSpawn(m_ShootPos.transform.position,
                //                                        a_TargetV, BulletMySpeed);

                Bullet_Ctrl a_BulletSc = BulletPool_Mgr.Inst.GetEnBulletPool();
                a_BulletSc.gameObject.SetActive(true);
                a_BulletSc.BulletSpawn(m_ShootPos.transform.position,
                                                        a_TargetV, BulletMySpeed);

                //--- 총알이 날아가는 방향으로 회전시키기...
                a_BulletSc.transform.right = new Vector3(-a_TargetV.x, -a_TargetV.y, 0.0f);
                //a_CacAngle = Mathf.Atan2(a_TargetV.y, a_TargetV.x) * Mathf.Rad2Deg;
                //a_CacAngle += 180.0f;   //Flip x 에 체크되어 있기 때문에...
                //a_NewObj.transform.eulerAngles = new Vector3(0.0f, 0.0f, a_CacAngle);
                //--- 총알이 날아가는 방향으로 회전시키기...

                m_ShootCount++;
                if (m_ShootCount < 7) //일반공격 7번까지의 공격 주기
                {
                    shoot_Time = 0.7f;
                }
                else
                {
                    m_ShootCount = 0;
                    shoot_Time = 2.0f;
                    m_BossState = BossState.BS_FEVER_ATT;
                }
            }//if (shoot_Time <= 0.0f)

        }//else if(m_BossState == BossState.BS_NORMAL_ATT) //일반 공격
        else if(m_BossState == BossState.BS_FEVER_ATT)  //피버 공격
        {
            shoot_Time -= Time.deltaTime;
            if(shoot_Time <= 0.0f)
            {
                //궁극기 공격
                float Radius = 1.0f;
                Vector3 a_TargetV = Vector3.zero;
                GameObject a_NewObj = null;
                Bullet_Ctrl a_BulletSc = null;
                //float a_CacAngle = 0.0f;
                for(float Angle = 0.0f; Angle < 360.0f; Angle += 15.0f)
                {
                    a_TargetV.x = Mathf.Sin(Angle * Mathf.Deg2Rad) * Radius;
                    a_TargetV.y = Mathf.Cos(Angle * Mathf.Deg2Rad) * Radius;
                    a_TargetV.Normalize();
                    //a_NewObj = Instantiate(m_BulletPrefab);
                    //a_BulletSc = a_NewObj.GetComponent<Bullet_Ctrl>();
                    //a_BulletSc.BulletSpawn(m_ShootPos.transform.position,
                    //                                    a_TargetV, BulletMySpeed);
                    a_BulletSc = BulletPool_Mgr.Inst.GetEnBulletPool();
                    a_BulletSc.gameObject.SetActive(true);
                    a_BulletSc.BulletSpawn(m_ShootPos.transform.position,
                                                        a_TargetV, BulletMySpeed);

                    //--- 총알이 날아가는 방향으로 회전시키기...
                    a_BulletSc.transform.right = new Vector3(-a_TargetV.x, -a_TargetV.y, 0.0f);
                    //a_CacAngle = Mathf.Atan2(a_TargetV.y, a_TargetV.x) * Mathf.Rad2Deg;
                    //a_CacAngle += 180.0f;   //Flip x 에 체크되어 있기 때문에...
                    //a_NewObj.transform.eulerAngles = new Vector3(0.0f, 0.0f, a_CacAngle);
                    //--- 총알이 날아가는 방향으로 회전시키기...
                }//for(float Angle = 0.0f; Angle < 360.0f; Angle += 15.0f)

                Sound_Mgr.Inst.PlayEffSound("explosion_large_01", 0.8f);

                m_ShootCount++;
                if(m_ShootCount < 3)
                {
                    shoot_Time = 1.0f;
                }
                else  
                {
                    m_ShootCount = 0;
                    shoot_Time = 1.5f; //궁극기에서 기본 공격으로 넘어갈 때 1.5초 딜레이 후 공격 위해 
                    m_BossState = BossState.BS_NORMAL_ATT;
                }

            }//if(shoot_Time <= 0.0f)
        }//else if(m_BossState == BossState.BS_FEVER_ATT)  //피버 공격

    }//void Boss_AI_Update()

    void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.tag == "AllyBullet")
        {
            if (coll.gameObject.name.Contains("HomingMissile") == true)
            {
                TakeDamage(150.0f);
                Destroy(coll.gameObject);
            }
            else
            {
                TakeDamage(80.0f);
                //Destroy(coll.gameObject);
                coll.gameObject.SetActive(false); //<-- 메모리 풀 방식이기 때문에...
            }
        }
    }//void OnTriggerEnter2D(Collider2D coll)

    public void TakeDamage(float a_Value)
    {
        if (m_CurHp <= 0.0f)  //이 몬스터가 이미 죽어 있으면...
            return;           //데미지를 차감할 필요 없으니 리턴 시키겠다는 뜻

        float a_CacDmg = a_Value;
        if (m_CurHp < a_Value)
            a_CacDmg = m_CurHp;

        Game_Mgr.Inst.DamageText(-a_CacDmg, transform.position, Color.red);

        m_CurHp -= a_Value;
        if (m_CurHp < 0.0f)
            m_CurHp = 0.0f;

        if (m_HpBar != null)
            m_HpBar.fillAmount = m_CurHp / m_MaxHp;

        if(m_CurHp <= 0.0f)
        { //몬스터 사망 처리
            //보상주기

            //--- 점수 보상
            Game_Mgr.Inst.AddScore();
            //--- 점수 보상

            //--- 유저 경험치 증가
            if (m_MonType == MonType.MT_Boss)
                Game_Mgr.Inst.AddExpLevel(20);
            else
                Game_Mgr.Inst.AddExpLevel();
            //--- 유저 경험치 증가

            //--- 골드 보상
            Game_Mgr.Inst.SpawnCoin(transform.position);
            //--- 골드 보상

            //--- 하트 보상
            if(m_MonType == MonType.MT_Boss)
                Game_Mgr.Inst.SpawnHeart(transform.position);
            //--- 하트 보상

            //--- 사망한 몬스터가 보스면 다음번 스폰 주기 설정
            if (m_MonType == MonType.MT_Boss)
            {
                MonsterGenerator.Inst.m_SpBossTimer = Random.Range(25.0f, 30.0f);
            }
            //--- 사망한 몬스터가 보스면 다음번 스폰 주기 설정

            Destroy(gameObject);
        }
    }//public void TakeDamage(float a_Value)
}
