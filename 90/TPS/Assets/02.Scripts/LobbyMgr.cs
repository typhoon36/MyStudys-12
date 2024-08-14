using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMgr : MonoBehaviour
{
    public Button m_Start_Btn;
    public Button m_Store_Btn;
    public Button m_Logout_Btn;
    public Button m_Clear_Save_Btn;

    public Text UserInfoText;

    [HideInInspector] public int m_MyRank = 0;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f; //�Ͻ������� ���� �ӵ���...
        GlobalValue.LoadGameData();

        if (m_Start_Btn != null)
            m_Start_Btn.onClick.AddListener(StartBtnClick);

        if (m_Store_Btn != null)
            m_Store_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("StoreScene");
            });

        if (m_Logout_Btn != null)
            m_Logout_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("TitleScene");
            });

        if (m_Clear_Save_Btn != null)
            m_Clear_Save_Btn.onClick.AddListener(Clear_Save_Click);

        RefreshUserInfo();
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    void StartBtnClick()
    {
        if(100 <= GlobalValue.g_CurFloorNum)
        {
            //������ ���� ������ ���¿��� ������ ���� �ߴٸ�...
            //�ٷ� ���� ��(99��)���� �����ϰ� �ϱ�...
            GlobalValue.g_CurFloorNum = 99;
            PlayerPrefs.SetInt("CurFloorNum", GlobalValue.g_CurFloorNum);
        }

        SceneManager.LoadScene("scLevel01");
        SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
    }

    void Clear_Save_Click()
    {
        PlayerPrefs.DeleteAll();
        GlobalValue.LoadGameData();
        RefreshUserInfo();
    }

    public void RefreshUserInfo()
    {
        UserInfoText.text = "������ : ����(" + GlobalValue.g_NickName +
                            ") : ����(" + m_MyRank + "��) : ����(" +
                            GlobalValue.g_BestScore.ToString("N0") + "��) : ���(" +
                            GlobalValue.g_UserGold.ToString("N0") + ")";
    }
}
