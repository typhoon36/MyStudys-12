using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMgr : MonoBehaviour
{
    public Text scoreText;
    public Button Replay_Btn = null;
    public Button GoLobby_Btn = null;

    public Text HelpText;
    public Text ReplayText;

    // Start is called before the first frame update
    void Start()
    {
        if (Game_Mgr.IsClear == true)  //미션 클리어 상태로 넘어온 경우
        {
            scoreText.text = "<color=#9EB8FF><size=63>Mission Clear</size></color>" + "\n" +
                            "Round : " + GlobalValue.g_Round + "\n" +
                            "Play Time : " + Game_Mgr.m_Timer.ToString("N1") + " Second\n" +
                            "Gold : " + Game_Mgr.s_CurGold.ToString();

            HelpText.text = "Tab - 다음 라운드";
            ReplayText.text = "다음 라운드";

            GlobalValue.g_Round++;
            PlayerPrefs.SetInt("GameRound", GlobalValue.g_Round);

        } //if(Game_Mgr.IsClear == true)  //미션 클리어 상태로 넘어온 경우
        else
        {
            scoreText.text = "<color=#ff2d2d><size=63>Mission Failed</size></color>" + "\n" +
                             "Gold : " + Game_Mgr.s_CurGold.ToString();
        }

        //--- 버튼 처리 코드
        if (Replay_Btn != null)
            Replay_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("GameScene");
            });

        if (GoLobby_Btn != null)
            GoLobby_Btn.onClick.AddListener(() =>
            {
                //로비씬 가기
                SceneManager.LoadScene("LobbyScene");
            });
        //--- 버튼 처리 코드

    }//void Start()

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab) == true)
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}
