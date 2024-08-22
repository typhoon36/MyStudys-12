using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum AnimalType
{
    CatPrefab = 0,
    DuckPrefab,
    molePrefab,
    penguinPrefab,
    sheepPrefab,
    AnimalTypeCount
}

public class Game_Mgr : MonoBehaviour
{
    //--- ���̰� ã�� ���� ����
    public Terrain m_RefMap = null;
    //--- ���̰� ã�� ���� ����

    [Header("--- UI ---")]
    int hp = 3;
    public Image[] hpImage;
    public Button BackBtn;

    public static float m_Timer = 0.0f;
    public static int s_CurGold = 0;    //�̹� ������������ ���� ��尪
    public Text TimerText;
    public Text GoldText;
    public static bool IsClear = false;
    int m_CurScore = 0;                 //�̹� ������������ ���� ���� ����
    public Text m_BestScoreText = null; //�ְ����� ǥ�� UI

    public Text RoundText = null;
    float m_CacEffRate = 0.0f;
    float m_CacEffTime = 1.0f;
    float m_EffDuring  = 1.0f;
    Color m_Color;     //�÷� ���� ����

    //--- ���̶� ���� ���� ����
    [Header("--- Mummy Spawn ---")]
    public GameObject Mummy_Root;   //���̶� ������ ���� ����
    float span = 3.0f;              //���̶� ���� �ֱ�
    float delta = 0.0f;             //���̶� ���� �ֱ� ���� ����

    float m_MvSpeedCtrl = 8.0f;     //��ü ���̶� �̵� �ӵ��� �����ϱ� ���� ����
    //--- ���̶� ���� ���� ����

    //--- ���� ���� ����
    [Header("--- Animal Spawn ---")]
    public GameObject[] AnimalArr;
    public Transform AnimalGroup;

    public Image[] AnimalIcon;
    //--- ���� ���� ����

    //--- MiniMap ó�� �κ�
    [Header("--- MiniMap ---")]
    public RawImage m_MM_BackGD = null;
    public Toggle m_MM_Toggle = null;
    public Slider m_MM_TrSlider = null;
    public Slider m_MM_SzSlider = null;
    //--- MiniMap ó�� �κ�

    //--- Skill Cool Time ���� �ڵ�
    [Header("--- Skill Timer ---")]
    public Transform m_SkillCoolRoot = null;
    public GameObject m_SkCoolNode = null;
    //--- Skill Cool Time ���� �ڵ�
    public SkInvenNode[] m_SkInvenNode; //Skill �κ��丮 ��ư ���� ����

    PlayerController PlayerCtrl = null; //���ΰ� ����

    [Header("--- Inventory Show OnOff ---")]
    public Button m_Inven_Btn = null;
    public Transform m_InvenScrollView = null;
    bool m_Inven_ScOnOff = true;
    float m_ScSpeed = 2000.0f;
    Vector3 m_ScOnPos = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 m_ScOffPos = new Vector3(-222.0f, 0.0f, 0.0f);

    //--- GameScene �ȿ����� ���Ǵ� �̱��� ����
    public static Game_Mgr Inst = null;

    void Awake()
    {
        Inst = this;
    }
    //--- GameScene �ȿ����� ���Ǵ� �̱��� ����

    // Start is called before the first frame update
    void Start()
    {
        m_Timer = 0.0f;         // static ���� �ʱ�ȭ
        s_CurGold = 0;
        IsClear = false;        // true �̸� �̼� Ŭ���� ����
        Time.timeScale = 1.0f;

        GlobalValue.LoadGameData();
        RefreshGameUI();

        PlayerCtrl = FindObjectOfType<PlayerController>();

        AnimalRandGen();

        StartCoroutine(MummyGenerator());

        BackBtn.onClick.AddListener(() =>
        {
            //SceneManager.LoadScene("LobbyScene");
            StartCoroutine(BackLobby());
        });

        //--- �̴ϸ� ó���� ���� �ڵ�
        if(m_MM_Toggle != null)
        {
            m_MM_Toggle.onValueChanged.AddListener((value) =>
            {
                if (m_MM_BackGD == null)
                    return;

                m_MM_BackGD.gameObject.SetActive(value);
            });
        }

        if(m_MM_TrSlider != null)
        {
            float a_Alpha = PlayerPrefs.GetFloat("SliderAlpha", 165.0f);
            m_MM_TrSlider.value = a_Alpha;
            if (m_MM_BackGD != null)
                m_MM_BackGD.color = new Color32(255, 255, 255, (byte)a_Alpha);
            m_MM_TrSlider.onValueChanged.AddListener(TrSliderMethod);
        }

        if(m_MM_SzSlider != null)
        {
            float a_Size = PlayerPrefs.GetFloat("SliderSize", 350.0f);
            m_MM_SzSlider.value = a_Size;
            if (m_MM_BackGD != null)
                m_MM_BackGD.GetComponent<RectTransform>().sizeDelta = new Vector2(a_Size, a_Size);
            m_MM_SzSlider.onValueChanged.AddListener(SzSliderMethod);
        }

        //--- �̴ϸ� ó���� ���� �ڵ�

        if (m_Inven_Btn != null)
            m_Inven_Btn.onClick.AddListener(() =>
            {
                m_Inven_ScOnOff = !m_Inven_ScOnOff;
            });

    }//void Start()

    // Update is called once per frame
    void Update()
    {
        m_Timer += Time.deltaTime;
        TimerText.text = "R " + GlobalValue.g_Round + " / " + m_Timer.ToString("N1");

        //--- Round Text ����
        if(0.0f < m_CacEffTime)
        {
            m_CacEffTime -= Time.deltaTime;
            m_CacEffRate = m_CacEffTime / (m_EffDuring - 0.4f);
            if (1.0f < m_CacEffRate)
                m_CacEffRate = 1.0f;
            m_Color = RoundText.color;
            m_Color.a = m_CacEffRate;
            RoundText.color = m_Color;

            if(m_CacEffTime <= 0.0f)
               RoundText.gameObject.SetActive(false);
        }//if(0.0f < m_CacEffTime)
        //--- Round Text ����

        //--- �̴ϸ� ����Ʈ ���·� ��������...
        if (Input.GetKeyDown(KeyCode.P) == true)
        {
            if(m_MM_TrSlider != null)
            {
                float a_Alpha = 165.0f;
                m_MM_TrSlider.value = a_Alpha;
                if (m_MM_BackGD != null)
                    m_MM_BackGD.color = new Color32(255, 255, 255, (byte)a_Alpha);
                PlayerPrefs.SetFloat("SliderAlpha", a_Alpha);
            }

            if(m_MM_SzSlider != null)
            {
                float a_Size = 350.0f;
                m_MM_SzSlider.value = a_Size;
                if (m_MM_BackGD != null)
                    m_MM_BackGD.GetComponent<RectTransform>().sizeDelta = new Vector2(a_Size, a_Size);
                PlayerPrefs.SetFloat("SliderSize", a_Size);
            }
        }
        //--- �̴ϸ� ����Ʈ ���·� ��������...

        if(Input.GetKeyDown(KeyCode.Alpha1) ||
           Input.GetKeyDown(KeyCode.Keypad1))  //����Ű 1
        {
            UseSkill_Key(SkillType.Skill_0);  //���� ���� ��ų ���
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) ||
                 Input.GetKeyDown(KeyCode.Keypad2))  //����Ű 2
        {
            UseSkill_Key(SkillType.Skill_1);  //�ν���
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) ||
                Input.GetKeyDown(KeyCode.Keypad3))  //����Ű 3
        {
            UseSkill_Key(SkillType.Skill_2);  //���
        }

        ScrollViewOnOff_Update();

    }//void Update()

    IEnumerator BackLobby()
    {
        //���� ��Ŷ ó�� ���� ���°ų� ��� ��Ŷ�� �����Ѵٸ�...
        while (NetworkMgr.Inst.IsBackLobbyOk() == false)            
        {
            yield return null;
        }

        SceneManager.LoadScene("LobbyScene");
    }

    IEnumerator MummyGenerator()
    {
        bool isFind = false;
        Vector3 RandomXYZ = Vector3.zero;
        Vector3 a_HeroPos = Vector3.zero;
        GameObject go = null;
        Vector3 a_CacPos = Vector3.zero;
        while (true)
        {
            isFind = false;
            RandomXYZ = Vector3.zero;
            a_HeroPos = Camera.main.transform.position;

            for(int i = 0; i < 100; i++)
            {
                RandomXYZ = new Vector3(Random.Range(-250.0f, 250.0f),
                                        10.0f,
                                        Random.Range(-250.0f, 250.0f));

                a_CacPos = a_HeroPos - RandomXYZ;
                a_CacPos.y = 0.0f;
                if(a_CacPos.magnitude <= 50.0f)
                {
                    continue;
                }

                isFind = true;
                break;

            }//for(int i = 0; i < 100; i++)

            if(isFind == true)
            {
                //���� ��Ĩ ���� �ڵ�
                GameObject[] a_Ems = GameObject.FindGameObjectsWithTag("Enemy");
                foreach(GameObject a_Enemy in a_Ems)
                {
                    if(a_Enemy == null)
                        continue;

                    if(RandomXYZ.x == a_Enemy.transform.position.x &&
                       RandomXYZ.z == a_Enemy.transform.position.z)
                    {
                        RandomXYZ.x += 0.01f;
                    }
                }//foreach(GameObject a_Enemy in a_Ems)
                //���� ��Ĩ ���� �ڵ�

                RandomXYZ.y = m_RefMap.SampleHeight(RandomXYZ);
                go = Instantiate(Mummy_Root);
                go.transform.position = RandomXYZ;
                go.GetComponent<Mummy_Ctrl>().m_MoveVelocity = m_MvSpeedCtrl;

            }//if(isFind == true)

            yield return new WaitForSeconds(span);

        }//while (true)

    }//IEnumerator MummyGenerator()

    public void DecreaseHp()
    {
        hp--;
        if (hp < 0)
            hp = 0;

        for(int i = 0; i < hpImage.Length; i++)
        {
            if (i < hp)
                hpImage[i].gameObject.SetActive(true);
            else
                hpImage[i].gameObject.SetActive(false);
        }

        if(hp <= 0)
        {
            //GameOver ó��
            //Time.timeScale = 0.0f; //�Ͻ����� ó��
            SceneManager.LoadScene("GameOver");
        }
    } //public void DecreaseHp()

    public void AddGold(int Value = 10)
    {
        s_CurGold += Value;

        if (Value < 0)
        {
            GlobalValue.g_UserGold += Value;
            if (GlobalValue.g_UserGold < 0)
                GlobalValue.g_UserGold = 0;
        }
        else if (GlobalValue.g_UserGold <= int.MaxValue - Value)
            GlobalValue.g_UserGold += Value;
        else
            GlobalValue.g_UserGold = int.MaxValue;

        if (GoldText != null)
            GoldText.text = "Gold : " + GlobalValue.g_UserGold.ToString();

        //PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
        NetworkMgr.Inst.PushPacket(PacketType.UserGold);

    }//public void AddGold(int Value = 10)

    public void AddScore(int Value = 1)
    {
        m_CurScore += Value;

        if (Value < 0)
        {
            GlobalValue.g_BestScore += Value;
            if (GlobalValue.g_BestScore < 0)
                GlobalValue.g_BestScore = 0;
        }
        else if (GlobalValue.g_BestScore <= int.MaxValue - Value)
            GlobalValue.g_BestScore += Value;
        else
            GlobalValue.g_BestScore = int.MaxValue;

        if (m_BestScoreText != null)
            m_BestScoreText.text = "Score : " + GlobalValue.g_BestScore.ToString();

        //PlayerPrefs.SetInt("UserGold", GlobalValue.g_BestScore);
        NetworkMgr.Inst.PushPacket(PacketType.BestScore);

    }//public void AddGold(int Value = 10)

    void AnimalRandGen()
    {
        for(int i = 0; i < AnimalArr.Length; i++)
        {
            GameObject go = Instantiate(AnimalArr[i]);
            go.transform.SetParent(AnimalGroup);
            Vector3 RandomXYZ = new Vector3(Random.Range(-250.0f, 250.0f),
                                            10.0f,
                                            Random.Range(-250.0f, 250.0f));
            ////--- �׽�Ʈ �ڵ�
            //Vector3 RandomXYZ = new Vector3(Random.Range(-20.0f, 20.0f),
            //                                10.0f,
            //                                Random.Range(10.0f, 30.0f));
            ////--- �׽�Ʈ �ڵ�

            RandomXYZ.y = m_RefMap.SampleHeight(RandomXYZ) + 2.0f;
            go.transform.position = RandomXYZ;
            Transform a_Child = go.transform.Find("MinimapIcon");
            Vector3 a_CurPos = a_Child.position;
            if (110.0f < a_CurPos.y)
                a_CurPos.y = 110.0f;
            a_Child.position = a_CurPos;
            go.transform.eulerAngles = new Vector3(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        }
    }//void AnimalRandGen()

    void RefreshGameUI()
    {
        if (GoldText != null)
            GoldText.text = "Gold : " + GlobalValue.g_UserGold.ToString();

        if (m_BestScoreText != null)
            m_BestScoreText.text = "Score : " + GlobalValue.g_BestScore.ToString();

        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            if (m_SkInvenNode.Length <= i)
                continue;

            m_SkInvenNode[i].Refresh_UI((SkillType)i, GlobalValue.g_SkillCount[i]);
        }

        RoundText.text = "Round " + GlobalValue.g_Round;
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

    void TrSliderMethod(float a_Alpha)
    {
        if (m_MM_BackGD != null)
            m_MM_BackGD.color = new Color32(255, 255, 255, (byte)a_Alpha);

        PlayerPrefs.SetFloat("SliderAlpha", a_Alpha);
    }

    void SzSliderMethod(float a_Size)
    {
        if (m_MM_BackGD != null)
            m_MM_BackGD.GetComponent<RectTransform>().sizeDelta = new Vector2(a_Size, a_Size);

        PlayerPrefs.SetFloat("SliderSize", a_Size);
    }

    public void UseSkill_Key(SkillType a_SkType)
    {
        if (GlobalValue.g_SkillCount[(int)a_SkType] <= 0)
            return;

        if (PlayerCtrl != null)
            PlayerCtrl.UseSkill_Item(a_SkType);

        if ((int)a_SkType < m_SkInvenNode.Length)
            m_SkInvenNode[(int)a_SkType].m_SkCountText.text =
                        GlobalValue.g_SkillCount[(int)a_SkType].ToString();
    }

    public void SkillCoolMethod(SkillType a_SkType, float a_Time, float a_During)
    {
        GameObject a_Obj = Instantiate(m_SkCoolNode);
        a_Obj.transform.SetParent(m_SkillCoolRoot, false);

        SkillCool_Ctrl a_SCtrl = a_Obj.GetComponent<SkillCool_Ctrl>();
        if (a_SCtrl != null)
            a_SCtrl.InitState(a_SkType, a_Time, a_During);
    }

    void ScrollViewOnOff_Update()
    {
        if (m_InvenScrollView == null)
            return;

        if(Input.GetKeyDown(KeyCode.V) == true)
        {
            m_Inven_ScOnOff = !m_Inven_ScOnOff;
        }

        if(m_Inven_ScOnOff == false)
        {
            if(m_InvenScrollView.localPosition.x > m_ScOffPos.x)
            {
                m_InvenScrollView.localPosition =
                    Vector3.MoveTowards(m_InvenScrollView.localPosition,
                                        m_ScOffPos, m_ScSpeed * Time.deltaTime);
                if(m_ScOffPos.x <= m_InvenScrollView.localPosition.x)
                {
                    m_Inven_Btn.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                }
            }//if(m_InvenScrollView.localPosition.x > m_ScOffPos.x)
        }//if(m_Inven_ScOnOff == false)
        else //if(m_Inven_ScOnOff == true)
        {
            if(m_ScOnPos.x > m_InvenScrollView.localPosition.x)
            {
                m_InvenScrollView.localPosition =
                    Vector3.MoveTowards(m_InvenScrollView.localPosition,
                                        m_ScOnPos, m_ScSpeed * Time.deltaTime);

                if(m_InvenScrollView.localPosition.x <= m_ScOnPos.x)
                {
                    m_Inven_Btn.transform.eulerAngles = new Vector3(0.0f, 0.0f, 180.0f);
                }
            }//if(m_ScOnPos.x > m_InvenScrollView.localPosition.x)
        }//else //if(m_Inven_ScOnOff == true)

    }//void ScrollViewOnOff_Update()

}//public class Game_Mgr : MonoBehaviour
