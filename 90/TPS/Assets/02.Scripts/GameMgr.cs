using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// <���̵� ����>
// 1, ���ʹ� 3������ �Ѿ��� �߻��ϰ� ����
// 2, ������ �Ѿ� �߻� �ֱ� : 2��(3��)���� ~ 1��(99��)���� ���ϰ�...
// 3, ������ ������ �Ѿ� �̵��ӵ� ���� :
//      ���̵� 4������ 15�� �þ���� 800 ~ 3000�� ����
// 4, ������ ���� ���� ������ ���� :
//      8������ 1������ �þ����...
//      ������ maxMonster = 10; (�ʵ忡 Ȱ�� ���� ���� ������ ����) : 10 ~ 25��������
//      ������ m_MonLimit = 20; (���� ī��Ʈ ������ : ������ ���̾Ƹ�� ����) : 20 ~ 30 ��������
// 5, ���� ������ �ö� ������ ���� ������ ������ ���� �� �ְ� �ؼ� ���� ������ �ö� ���� ���� �ֱ�

public enum GameState
{
    GameIng,
    GameEnd
}

public class GameMgr : MonoBehaviour
{
    public static GameState s_GameState = GameState.GameIng;

    //Text UI �׸� ������ ���� ����
    public Text txtScore;
    //���� ������ ����ϱ� ���� ����
    private int totScore = 0;
    int m_CurScore = 0;     //�̹� ������������ ���� ��������

    public Button BackBtn;

    //���Ͱ� ������ ��ġ�� ���� �迭
    public Transform[] points;
    //���� �������� �Ҵ��� ����
    public GameObject monsterPrefab;
    //���͸� �̸� ������ ������ ����Ʈ �ڷ���
    public List<GameObject> monsterPool = new List<GameObject>();

    //���͸� �߻���ų �ֱ�
    public float createTime = 2.0f;
    //������ �ִ� �߻� ����
    public int maxMonster = 10;
    //���� ������ ������ ���� ī��Ʈ ����
    int m_MonCurNum = 0;
    //���� ������ ���� �ִ� ���� ������
    int m_MonLimit = 20;
    //���̾Ƹ��� ������ �Ǿ��� �� �ѹ��� ���� ��Ű�� ���Ͽ�...
    bool m_IsSpawnDiamond = false;
    //���� ������ ������ ���� ī��Ʈ ����
    [HideInInspector] public int m_CurKillNum = 0;
    //���� ������ �����ؾ� �� ���� ī��Ʈ ����
    [HideInInspector] public int m_TargetKillNum = 10;

    //���� ���� ���� ����
    public bool isGameOver = false;

    [HideInInspector] public GameObject m_CoinItem = null;
    [Header("--- Gold UI ---")]
    public Text m_UserGoldText = null;  //�̹� ������������ ���� ��尪 ǥ�� UI
    int m_CurGold = 0;

    //--- �Ӹ� ���� ���ؽ�Ʈ ����� ���� ����
    [Header("--- HealText ---")]
    public Transform m_Heal_Canvas = null;
    public GameObject m_HTextPrefab = null;
    //--- �Ӹ� ���� ���ؽ�Ʈ ����� ���� ����

    [Header("--- W Damage Text ---")]
    public Transform W_Damage_Canvas = null;
    public GameObject W_DamagePrefab = null;

    [Header("--- Skill Cool Timer ---")]
    public GameObject m_SkCoolPrefab = null;
    public Transform m_SkCoolRoot = null;
    public SkInvenNode[] m_SkInvenNode;   //Skill �κ��丮 ��ư ���� ����

    [Header("--- Door Ctrl ---")]
    public Text m_FL_Tm_Text = null;
    public Text m_LastFloorText = null;
    public Text m_DoorOpenText = null;
    float m_Floor_TimeOut = 0.0f;   //�̹� �� Ż�� �ð� Ÿ�̸�
    GameObject[] m_DoorObj = new GameObject[3];
    public static GameObject m_DiamondItem = null;

    [Header("--- GameOver ---")]
    public GameObject GameOverPanel = null;
    public Text Title_Text = null;
    public Text Result_Text = null;
    public Button Replay_Btn = null;
    public Button RstLobby_Btn = null;

    public Text m_Help_Text = null;

    PlayerCtrl m_RefHero = null;

    //--- �̱��� ����
    public static GameMgr Inst = null;

    void Awake()
    {
        Inst = this;   
    }
    //--- �̱��� ����

    // Start is called before the first frame update
    void Start()
    {
        if (IsEndingScene() == true)
            return;

        Time.timeScale = 1.0f;
        s_GameState = GameState.GameIng;
        GlobalValue.LoadGameData();
        RefreshGameUI();

        DispScore(0);

        if(BackBtn != null)
            BackBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Lobby");
            });

        //--- ���̵� �� ���� ���� �ִ� ���� ������ ����
        // 8������ 1������ �þ����... 20���� ~ 30�������� ���� �ǵ���...
        // ������ maxMonster = 10; (�ʵ忡 Ȱ�� ���� ���� ������ ����) : 10 ~ 20��������
        // ������ m_MonLimit = 20; (���� ī��Ʈ ������ : ������ ���̾Ƹ�� ����) : 20 ~ 35��������
        int a_CacMaxMon = GlobalValue.g_CurFloorNum - 7;
        if (a_CacMaxMon < 0)
            a_CacMaxMon = 0;
        a_CacMaxMon = 10 + a_CacMaxMon;
        if (25 < a_CacMaxMon)
            a_CacMaxMon = 25;

        maxMonster = a_CacMaxMon;   //10 ~ 25 �������� 8������ �Ѹ����� �þ
        m_MonLimit = 10 + a_CacMaxMon;  //20 ~ 35 �������� 8������ �Ѹ����� �þ
        m_TargetKillNum = a_CacMaxMon;  //�̹������� ��ƾ� �� ���� 10 ~ 25 �������� ���� �ǰ�... 
        //--- ���̵� �� ���� ���� �ִ� ���� ������ ����

        // Hierachy ���� SpawnPoint�� ã�� ������ �ִ� ��� Transform ������Ʈ�� ã�ƿ�
        points = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();

        //���͸� ������ ������Ʈ Ǯ�� ����
        for(int i = 0; i < maxMonster; i++)
        {
            //���� �������� ����
            GameObject monster = (GameObject)Instantiate(monsterPrefab);
            //������ ������ �̸� ����
            monster.name = "Monster_" + i.ToString();
            //������ ���͸� ��Ȱ��ȭ
            monster.SetActive(false);
            //������ ���͸� ������Ʈ Ǯ�� �߰�
            monsterPool.Add(monster);
        }

        if(points.Length > 0)
        {
            //���� ���� �ڷ�ƾ �Լ� ȣ��
            StartCoroutine(this.CreateMonster());
        }

        m_CoinItem = Resources.Load("CoinItem/CoinPrefab") as GameObject;

        //--- GameOver ��ư ó�� �ڵ�
        if (Replay_Btn != null)
            Replay_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLevel01");
                SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
            });

        if (RstLobby_Btn != null)
            RstLobby_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Lobby");
            });
        //--- GameOver ��ư ó�� �ڵ�

        //--- Door ���� ���� �ڵ�
        m_FL_Tm_Text.text = GlobalValue.g_CurFloorNum + "��(����:" +
                            GlobalValue.g_BestFloor + "��)";

        GameObject a_DoorObj = GameObject.Find("Gate_In_1");
        if(a_DoorObj != null)
            m_DoorObj[0] = a_DoorObj;

        a_DoorObj = GameObject.Find("Gate_Exit_1");
        if(a_DoorObj != null)
        {
            m_DoorObj[1] = a_DoorObj;
            m_DoorObj[1].SetActive(false);
        }

        a_DoorObj = GameObject.Find("Gate_Exit_2");
        if(a_DoorObj != null)
        {
            m_DoorObj[2] = a_DoorObj;
            m_DoorObj[2].SetActive(false);
        }

        if (GlobalValue.g_CurFloorNum <= 1)
            m_DoorObj[0].SetActive(false);

        if(GlobalValue.g_CurFloorNum < GlobalValue.g_BestFloor)
        { //�ְ� Ŭ���� ���� ���̸� �׳� ������ ���� �����ش�.
            ShowDoor();
        }

        m_DiamondItem = Resources.Load("DiamondItem/DiamondPrefab") as GameObject;
        //--- Door ���� ���� �ڵ�

        m_RefHero = GameObject.FindObjectOfType<PlayerCtrl>();

    }//void Start()

    // Update is called once per frame
    void Update()
    {
        //--- ����Ʈ ���� ���� �ڵ�
        if(0.0f < m_Floor_TimeOut)
        {
            m_Floor_TimeOut -= Time.deltaTime;
            m_FL_Tm_Text.text = GlobalValue.g_CurFloorNum + "��(����:" +
                                GlobalValue.g_BestFloor + "��) / " +
                                m_Floor_TimeOut.ToString("F1");
            if(m_Floor_TimeOut <= 0.0f)
            {
                s_GameState = GameState.GameEnd;
                Time.timeScale = 0.0f;  //�Ͻ�����
                GameOverMethod();
            }
        }//if(0.0f < m_Floor_TimeOut)

        MissionUIUpdate();
        //--- ����Ʈ ���� ���� �ڵ�

        //���콺 �߾ӹ�ư(�� Ŭ��)
        if (Input.GetMouseButtonDown(2) == true)
        {
            UseSkill_Key(SkillType.Skill_1);
        }

        //--- ����Ű �̿����� ��ų ����ϱ�...
        if(Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            UseSkill_Key(SkillType.Skill_0);    //30% ���� ������ ��ų
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2) ||
                Input.GetKeyDown(KeyCode.Keypad2))
        {
            UseSkill_Key(SkillType.Skill_1);    //����ź ���
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) ||
                Input.GetKeyDown(KeyCode.Keypad3))
        {
            UseSkill_Key(SkillType.Skill_2);    //��ȣ�� �ߵ�
        }
        //--- ����Ű �̿����� ��ų ����ϱ�...

    }//void Update()

    //���� ���� �� ȭ�� ǥ��
    public void DispScore(int score)
    {
        //totScore += score;
        //txtScore.text = "score <color=#ff0000>" + totScore.ToString() + "</color>";

        m_CurScore += score;
        if (m_CurScore < 0)
            m_CurScore = 0;

        if (score < 0)
        {
            GlobalValue.g_BestScore += score;
            if (GlobalValue.g_BestScore < 0)
                GlobalValue.g_BestScore = 0;
        }
        else if(GlobalValue.g_BestScore <= int.MaxValue - score)
        {
            GlobalValue.g_BestScore += score;
        }
        else
        {
            GlobalValue.g_BestScore = int.MaxValue;
        }

        txtScore.text = "SCORE <color=#ff0000>" + m_CurScore.ToString() +
                "</color> / BEST <color=#ff0000>" +
                GlobalValue.g_BestScore.ToString() + "</color>";

        //txtScore.text = "SCORE <color=#ff0000>" + m_CurScore.ToString("0000") +
        //                "</color> / BEST <color=#ff0000>" + 
        //                GlobalValue.g_BestScore.ToString("0000") + "</color>";

        PlayerPrefs.SetInt("BestScore", GlobalValue.g_BestScore);
    }

    //���� ���� �ڷ�ƾ �Լ�
    IEnumerator CreateMonster()
    {
        //���� ���� �ñ��� ���� ����
        while( !isGameOver )
        {
            //���� ���� �ֱ� �ð���ŭ ���� ������ �纸
            yield return new WaitForSeconds(createTime);

            if (m_MonLimit <= m_MonCurNum)
                continue;

            //�÷��̾ ������� �� �ڷ�ƾ�� ������ ���� ��ƾ�� �������� ����
            if (GameMgr.s_GameState == GameState.GameEnd) 
                yield break; //�ڷ�ƾ �Լ����� �Լ��� ���������� ���

            //������Ʈ Ǯ�� ó������ ������ ��ȸ
            foreach(GameObject monster in monsterPool)
            {
                //��Ȱ��ȭ ���η� ��� ������ ���͸� �Ǵ�
                if(monster.activeSelf == false)
                {
                    //���͸� ������ų ��ġ�� �ε������� ����
                    int idx = Random.Range(1, points.Length);

                    //--- ���� ī��Ʈ �� ������ ���� ���� ���� üũ�ϴ� �Լ�
                    if(CheckMonsterCount(idx) == true)
                        break;
                    //--- ���� ī��Ʈ �� ������ ���� ���� ���� üũ�ϴ� �Լ�

                    //������ ������ġ�� ����
                    monster.transform.position = points[idx].position;
                    //���͸� Ȱ��ȭ��
                    monster.SetActive(true);

                    //������Ʈ Ǯ���� ���� ������ �ϳ��� Ȱ��ȭ�� �� for ������ ��������
                    break;
                }//if(monster.activeSelf == false)
            }//foreach(GameObject monster in monsterPool)
        }// while( !isGameOver )

    }//IEnumerator CreateMonster()

    public void AddGold(int value = 10)
    {
        //�̹� ������������ ���� ��尪
        if(value < 0)
        {
            m_CurGold += value;
            if (m_CurGold < 0)
                m_CurGold = 0;
        }
        else if (m_CurGold <= int.MaxValue - value)
            m_CurGold += value;
        else
            m_CurGold = int.MaxValue;

        //���ÿ� ����Ǿ� �ִ� ���� ���� ��尪
        if (value < 0)
        {
            GlobalValue.g_UserGold += value;
            if (GlobalValue.g_UserGold < 0)
                GlobalValue.g_UserGold = 0;
        }
        else if (GlobalValue.g_UserGold <= int.MaxValue - value)
            GlobalValue.g_UserGold += value;
        else
            GlobalValue.g_UserGold = int.MaxValue;

        if (m_UserGoldText != null)
            m_UserGoldText.text = "Gold <color=#ffff00>" + GlobalValue.g_UserGold + "</color>";

        PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);

    }//public void AddGold(int value = 10)

    public void SpawnCoin(Vector3 a_Pos)
    {
        GameObject a_CoinObj = Instantiate(m_CoinItem);
        a_CoinObj.transform.position = a_Pos;
        Destroy(a_CoinObj, 10.0f);      //10�� ���� �Ծ�� �Ѵ�.
    }

    void RefreshGameUI()
    {
        if (m_UserGoldText != null)
            m_UserGoldText.text = "Gold <color=#ffff00>" + GlobalValue.g_UserGold + "</color>";

        for(int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            if (m_SkInvenNode.Length <= i)
                continue;

            m_SkInvenNode[i].InitState((SkillType)i);

        }//for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)

    }//void RefreshGameUI()

    public static bool IsPointerOverUIObject() //UGUI�� UI���� ���� ��ŷ�Ǵ��� Ȯ���ϴ� �Լ�
    {
        PointerEventData a_EDCurPos = new PointerEventData(EventSystem.current);

#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)

			List<RaycastResult> results = new List<RaycastResult>();
			for (int i = 0; i < Input.touchCount; ++i)
			{
				a_EDCurPos.position = Input.GetTouch(i).position;  
				results.Clear();
				EventSystem.current.RaycastAll(a_EDCurPos, results);
                if (0 < results.Count)
                    return true;
			}

			return false;
#else
        a_EDCurPos.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(a_EDCurPos, results);
        return (0 < results.Count);
#endif
    }//public bool IsPointerOverUIObject()

    public void UseSkill_Key(SkillType a_SkType)
    {
        if (GlobalValue.g_SkillCount[(int)a_SkType] <= 0)
            return;

        if (m_RefHero != null)
            m_RefHero.UseSkill_Item(a_SkType);

        if ((int)a_SkType < m_SkInvenNode.Length)
            m_SkInvenNode[(int)a_SkType].m_SkCountText.text =
                   GlobalValue.g_SkillCount[(int)a_SkType].ToString();
    }

    public void SpawnHealText(int cont, Vector3 a_WSpawnPos, Color a_Color)
    {
        if(m_Heal_Canvas == null && m_HTextPrefab == null)
            return;

        GameObject a_HealObj = Instantiate(m_HTextPrefab) as GameObject;
        HealTextCtrl a_HealText = a_HealObj.GetComponent<HealTextCtrl>();
        if(a_HealText != null)
           a_HealText.InitState(cont, a_WSpawnPos, m_Heal_Canvas, a_Color);        
    }

    public void W_SpawnDamageText(int cont, Vector3 a_SpPos, Color a_Color)
    {
        if(W_Damage_Canvas == null || W_DamagePrefab == null) 
            return;

        GameObject a_DmgObj = Instantiate(W_DamagePrefab);
        a_DmgObj.transform.SetParent(W_Damage_Canvas, false);

        W_DamageText a_DamageTx = a_DmgObj.GetComponent<W_DamageText>();
        if(a_DamageTx != null)
            a_DamageTx.InitState(cont, a_SpPos, a_Color);
    }

    public void SkillTimeMethod(float a_Time, float a_Dur)
    {
        GameObject obj = Instantiate(m_SkCoolPrefab);
        obj.transform.SetParent(m_SkCoolRoot, false);
        SkCool_NodeCtrl skNode = obj.GetComponent<SkCool_NodeCtrl>();
        skNode.InitState(a_Time, a_Dur);
    }

    public void GameOverMethod()
    {
        GameOverPanel.SetActive(true);
        Result_Text.text = "NickName\n" + GlobalValue.g_NickName + "\n\n" +
                            "ȹ�� ����\n" + m_CurScore + "\n\n" +
                            "ȹ�� ���\n" + m_CurGold;

        if(SceneManager.GetActiveScene().name == "scLevel02")
        {
            Title_Text.text = "< Game Ending >";
            Result_Text.text = "NickName\n" + GlobalValue.g_NickName + "\n\n" +
                "Made by\n" + "SBS Game Acadamy" + "\n\n" + "Date\n" + "2024.7.11";
        }
    }

    public void ShowDoor()
    {
        int a_Idx = (GlobalValue.g_CurFloorNum % 2) + 1;
        if ((1 <= a_Idx && a_Idx <= 2) && m_DoorObj[a_Idx] != null)
            m_DoorObj[a_Idx].SetActive(true);

        if (m_LastFloorText != null)
            m_LastFloorText.gameObject.SetActive(false);

        if(m_DoorOpenText != null)
            m_DoorOpenText.gameObject.SetActive(true);
    }

    bool IsEndingScene()
    {
        if(SceneManager.GetActiveScene().name != "scLevel02")
        {
            return false;  //�̹��� �ε��� ���� �������� �ƴ϶��...
        }

        Time.timeScale = 1.0f;
        s_GameState = GameState.GameIng;

        GlobalValue.LoadGameData();
        RefreshGameUI();

        //ó�� ���� �� ����� ���ھ� ���� �ε�
        DispScore(0);

        if (BackBtn != null)
            BackBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Lobby");
            });

        m_FL_Tm_Text.text = GlobalValue.g_CurFloorNum + "��(����:" +
                            GlobalValue.g_BestFloor + "��)";

        m_RefHero = GameObject.FindAnyObjectByType<PlayerCtrl>();

        if (Replay_Btn != null)
            Replay_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Lobby");
            });

        if (RstLobby_Btn != null)
            RstLobby_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Lobby");
            });

        return true;
    }

    //--- ���� ī��Ʈ �� ������ ���� ���� ���� üũ�ϴ� �Լ�
    bool CheckMonsterCount(int idx)
    {
        if (m_IsSpawnDiamond == false && m_TargetKillNum <= m_CurKillNum)
        { //�̹������� ���������� ������ ���Ͷ�� ���� ��� ���̾Ƹ�� ����

            if(GlobalValue.g_BestFloor <= GlobalValue.g_CurFloorNum)
            {   //����Ʈ�� �����ؾ� �ϴ� ���������� Ȱ���ϰ� ���� ����

                m_IsSpawnDiamond = true;

                //���̾Ƹ�� ����
                if (m_DiamondItem != null)
                {
                    GameObject a_DmdObj = (GameObject)Instantiate(m_DiamondItem);
                    a_DmdObj.transform.position = points[idx].position;
                }
                m_Floor_TimeOut = 60.0f;  //60�� Ÿ�̸� ������

                return true;
            }//if(GlobalValue.g_BestFloor <= GlobalValue.g_CurFloorNum)
        }//if(m_MonLimit <= m_MonCurNum)

        m_MonCurNum++;

        return false;
    }

    void MissionUIUpdate()
    {
        if (m_LastFloorText == null)
            return;

        if(m_LastFloorText.gameObject.activeSelf == false)
            return;

        if(0.0f < m_Floor_TimeOut)
        {
            m_LastFloorText.text = "<color=#00ffff>���̾Ƹ�尡 �� ��򰡿� �����Ǿ����ϴ�.</color>";
        }
        else
        {
            m_LastFloorText.text = "<color=#ffff00>(" + m_CurKillNum +
                                    " / " + m_TargetKillNum + " Mon) " +
                                    "���� 100��</color>";
        }
    }

}//public class GameMgr : MonoBehaviour
