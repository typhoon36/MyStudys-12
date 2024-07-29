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

    //--- 효과음 최적화를 위한 버퍼 변수
    int m_EffSdCount = 5;       //지금은 5개의 레이어로 플레이...
    int m_SoundCount = 0;       //최대 5개까지 재생되게 제어(렉방지 위해...)
    GameObject[] m_SndObjList = new GameObject[10];
    AudioSource[] m_SndSrcList = new AudioSource[10];
    float[] m_EffVolume = new float[10];
    //--- 효과음 최적화를 위한 버퍼 변수

    protected override void Init()  //Awake() 함수 대신 사용
    {
        base.Init(); //부모쪽에 있는 Init() 함수 호출

        LoadChildGameObj();
    }

    // Start is called before the first frame update
    void Start()
    {
        //--- 사운드 리소스 미리 로딩
        AudioClip a_GAudioClip = null;
        object[] temp = Resources.LoadAll("Sounds");
        for (int i = 0; i < temp.Length; i++)
        {
            a_GAudioClip = temp[i] as AudioClip;

            if (m_AdClipList.ContainsKey(a_GAudioClip.name) == true)
                continue;

            m_AdClipList.Add(a_GAudioClip.name, a_GAudioClip);
        }
        //--- 사운드 리소스 미리 로딩
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

        //--- 게임 효과음 플레이를 위한 5개의 레이어 생성 코드
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
        //--- 게임 효과음 플레이를 위한 5개의 레이어 생성 코드

        //--- 게임 시작되면 사운드 OnOff, 사운드 볼륨 로컬 로딩 후 적용
        int a_SoundOnOff = PlayerPrefs.GetInt("SoundOnOff", 1);
        if (a_SoundOnOff == 1)
            SoundOnOff(true);
        else
            SoundOnOff(false);

        float a_Value = PlayerPrefs.GetFloat("SoundVolume", 1.0f);
        SoundVolume(a_Value);
        //--- 게임 시작되면 사운드 OnOff, 사운드 볼륨 로컬 로딩 후 적용
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
    {  //GUI 효과음 플레이 하기 위한 함수

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
            m_AudioSrc.mute = a_MuteOnOff;  //mute == true 끄기 mute == false 켜기
            //if (a_MuteOnOff == false)
            //    m_AudioSrc.time = 0;      //처음부터 다시 플레이
        }//if(m_AudioSrc != null)

        for(int i = 0; i < m_EffSdCount; i++)
        {
            if (m_SndSrcList[i] != null)
            {
                m_SndSrcList[i].mute = a_MuteOnOff;

                if (a_MuteOnOff == false)
                    m_SndSrcList[i].time = 0;   //처음부터 다시 플레이
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
