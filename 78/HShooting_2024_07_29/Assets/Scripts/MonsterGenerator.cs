using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGenerator : MonoBehaviour
{
    public GameObject[] MonPrefab;

    float m_SpDelta = 0.0f;     //스폰 주기 계산용 변수
    float m_DiffSpawn = 1.0f;   //난이도에 따른 몬스터 스폰 주기 변수

    [HideInInspector] public float m_SpBossTimer = 20.0f;

    //--- 싱글톤 패턴
    public static MonsterGenerator Inst = null;

    void Awake()
    {
        Inst = this;
    }
    //--- 싱글톤 패턴

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_SpDelta -= Time.deltaTime;
        if(m_SpDelta < 0.0f)
        {
            m_SpDelta  = m_DiffSpawn;

            GameObject Go = null;

            int dice = Random.Range(1, 11);     //1 ~ 10 랜덤값 발생
            if (2 < dice)
            {
                Go = Instantiate(MonPrefab[0]); //좀비 스폰
            }
            else
            {
                Go = Instantiate(MonPrefab[1]); //미사일 스폰
            }
            float py = Random.Range(-3.0f, 3.0f);
            Go.transform.position = new Vector3(CameraResolution.m_ScWMax.x + 1.0f, py, 0.0f);

        }//if(m_SpDelta < 0.0f)

        //---- 보스 스폰
        if(0.0f < m_SpBossTimer)
        {
            m_SpBossTimer -= Time.deltaTime;
            if(m_SpBossTimer < 0.0f)
            {
                GameObject Go = Instantiate(MonPrefab[2]);
                float py = Random.Range(-3.0f, 3.0f);   //-3.0f ~ 3.0f 랜덤값 발생
                Go.transform.position =
                         new Vector3(CameraResolution.m_ScWMax.x + 1.0f, py, 0.0f);
            }
        }
        //---- 보스 스폰

    }//void Update()
}
