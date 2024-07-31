using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound_Mgr : G_Singleton<Sound_Mgr>
{
    [HideInInspector] public AudioSource m_AudioSrc = null;
    Dictionary<string, AudioClip> m_AdClipList = new Dictionary<string, AudioClip>();

    float m_bgmVolume = 0.2f;
    [HideInInspector] public bool m_SoundOnOff = true;
    [HideInInspector] public float m_SoundVolume = 1.0f;

    //--- ȿ���� ����ȭ�� ���� ���� ����
    int m_EffSdCount = 5;       //������ 5���� ���̾�� �÷���...
    int m_SoundCount = 0;       //�ִ� 5������ ����ǰ� ����(������ ����...)
    GameObject[] m_SndObjList = new GameObject[10];
    AudioSource[] m_SndSrcList = new AudioSource[10];
    float[] m_EffVolume = new float[10];
    //--- ȿ���� ����ȭ�� ���� ���� ����

    protected override void Init()  //Awake() �Լ� ��� ���
    {
        base.Init(); //�θ��ʿ� �ִ� Init() �Լ� ȣ��

        LoadChildGameObj();
    }

    // Start is called before the first frame update
    void Start()
    {
        //--- ���� ���ҽ� �̸� �ε�
        AudioClip a_GAudioClip = null;
        object[] temp = Resources.LoadAll("Sounds");
        for (int i = 0; i < temp.Length; i++)
        {
            a_GAudioClip = temp[i] as AudioClip;

            if (m_AdClipList.ContainsKey(a_GAudioClip.name) == true)
                continue;

            m_AdClipList.Add(a_GAudioClip.name, a_GAudioClip);
        }
        //--- ���� ���ҽ� �̸� �ε�
    }


    //float m_TestVoluem = 1.0f; 
    //// Update is called once per frame
    //void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.LeftArrow) == true)
    //    {
    //        m_TestVoluem -= 0.1f;
    //        if (m_TestVoluem < 0.0f)
    //            m_TestVoluem = 0.0f;
    //        SoundVolume(m_TestVoluem);
    //    }

    //    if (Input.GetKeyDown(KeyCode.RightArrow) == true)
    //    {
    //        m_TestVoluem += 0.1f;
    //        if (1.0f < m_TestVoluem)
    //            m_TestVoluem = 1.0f;
    //        SoundVolume(m_TestVoluem);
    //    }
    //}

    void LoadChildGameObj()
    {
        m_AudioSrc = gameObject.AddComponent<AudioSource>();

        //--- ���� ȿ���� �÷��̸� ���� 5���� ���̾� ���� �ڵ�
        for(int i = 0; i < m_EffSdCount; i++)
        {
            GameObject newSndObj = new GameObject();
            newSndObj.transform.SetParent(this.transform);
            newSndObj.transform.localPosition = Vector3.zero;
            AudioSource a_AudioSrc = newSndObj.AddComponent<AudioSource>();
            a_AudioSrc.playOnAwake = false;
            a_AudioSrc.loop = false;
            newSndObj.name = "SoundEffObj";

            m_SndSrcList[i] = a_AudioSrc;
            m_SndObjList[i] = newSndObj;
        }
        //--- ���� ȿ���� �÷��̸� ���� 5���� ���̾� ���� �ڵ�

        //--- ���� ���۵Ǹ� ���� OnOff, ���� ���� ���� �ε� �� ����
        int a_SoundOnOff = PlayerPrefs.GetInt("SoundOnOff", 1);
        if (a_SoundOnOff == 1)
            SoundOnOff(true);
        else
            SoundOnOff(false);

        float a_Value = PlayerPrefs.GetFloat("SoundVolume", 1.0f);
        SoundVolume(a_Value);
        //--- ���� ���۵Ǹ� ���� OnOff, ���� ���� ���� �ε� �� ����
    }

    public void PlayBGM(string a_FileName, float fVolume = 0.2f)
    {
        AudioClip a_GAudioClip = null;
        if(m_AdClipList.ContainsKey(a_FileName) == true)
        {
            a_GAudioClip = m_AdClipList[a_FileName]; 
        }
        else
        {
            a_GAudioClip = Resources.Load("Sounds/" + a_FileName) as AudioClip; 
            m_AdClipList.Add(a_FileName, a_GAudioClip);
        }

        if (m_AudioSrc == null)
            return;

        if (m_AudioSrc.clip != null && m_AudioSrc.clip.name == a_FileName)
            return;

        m_AudioSrc.clip = a_GAudioClip;
        m_AudioSrc.volume = fVolume * m_SoundVolume;
        m_bgmVolume = fVolume;
        m_AudioSrc.loop = true;
        m_AudioSrc.Play();
    }// public void PlayBGM(string a_FileName, float fVolume = 0.2f)

    public void PlayGUISound(string a_FileName, float fVolume = 0.2f)
    {  //GUI ȿ���� �÷��� �ϱ� ���� �Լ�

        if (m_SoundOnOff == false)
            return;

        AudioClip a_GAudioClip = null;

        if(m_AdClipList.ContainsKey(a_FileName) == true)
        {
            a_GAudioClip = m_AdClipList[a_FileName];
        }
        else
        {
            a_GAudioClip = Resources.Load("Sounds/" + a_FileName) as AudioClip;
            m_AdClipList.Add(a_FileName, a_GAudioClip);
        }

        if(m_AudioSrc == null)
            return; 

        m_AudioSrc.PlayOneShot(a_GAudioClip, fVolume * m_SoundVolume);

    }//public void PlayGUISound(string a_FileName, float fVolume = 0.2f)

    public void PlayEffSound(string a_FileName, float fVolume = 0.2f)
    {
        if(m_SoundOnOff == false)
            return;

        AudioClip a_GAudioClip = null;
        if(m_AdClipList.ContainsKey(a_FileName) == true)
        {
            a_GAudioClip = m_AdClipList[a_FileName];
        }
        else
        {
            a_GAudioClip = Resources.Load("Sounds/" + a_FileName) as AudioClip;
            m_AdClipList.Add(a_FileName , a_GAudioClip);
        }

        if(a_GAudioClip == null)
            return;

        if (m_SndSrcList[m_SoundCount] != null)
        {
            m_SndSrcList[m_SoundCount].volume = 1.0f;
            m_SndSrcList[m_SoundCount].PlayOneShot(a_GAudioClip, fVolume * m_SoundVolume);
            m_EffVolume[m_SoundCount] = fVolume;

            m_SoundCount++;
            if(m_EffSdCount <= m_SoundCount)
                m_SoundCount = 0;
        }//if (m_SndSrcList[m_SoundCount] != null)

    }//public void PlayEffSound(string a_FileName, float fVolume = 0.2f)

    public void SoundOnOff(bool a_OnOff = true)
    {
        bool a_MuteOnOff = !a_OnOff;

        if(m_AudioSrc != null)
        {
            m_AudioSrc.mute = a_MuteOnOff;  //mute == true ���� mute == false �ѱ�
            //if (a_MuteOnOff == false)
            //    m_AudioSrc.time = 0;      //ó������ �ٽ� �÷���
        }//if(m_AudioSrc != null)

        for(int i = 0; i < m_EffSdCount; i++)
        {
            if (m_SndSrcList[i] != null)
            {
                m_SndSrcList[i].mute = a_MuteOnOff;

                if (a_MuteOnOff == false)
                    m_SndSrcList[i].time = 0;   //ó������ �ٽ� �÷���
            }
        }//for(int i = 0; i < m_EffSdCount; i++)

        m_SoundOnOff = a_OnOff;
    }


    public void SoundVolume(float fVolume)
    {
        if(m_AudioSrc != null)
           m_AudioSrc.volume = m_bgmVolume * fVolume;

        //for(int i = 0; i < m_EffSdCount; i++)
        //{
        //    if (m_SndSrcList[i] != null)
        //        m_SndSrcList[i].volume = 1.0f;
        //}

        m_SoundVolume = fVolume;
    }

}//public class Sound_Mgr : G_Singleton<Sound_Mgr>
