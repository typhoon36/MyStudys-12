using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Skill_0 = 0,        //버블 쉴드 스킬
    Skill_1,            //부스터 
    Skill_2,            //허상
    SkCount
}

public class GlobalValue 
{
    public static string g_Unique_ID = "";      //유저의 고유번호 

    public static string g_NickName = "";       //유저의 별명
    public static int g_BestScore = 0;          //게임점수
    public static int g_UserGold = 0;           //게임머니
    public static int g_Round    = 1;           //라운드
    public static int g_Exp = 0;                //유저의 경험치 Experience
    public static int g_Level = 0;              //유저의 레벨

    public static int[] g_SkillCount = new int[3];  //아이템 보유수

    public static void LoadGameData()
    {
        //PlayerPrefs.DeleteAll();

        //PlayerPrefs.SetInt("UserGold", 99999);

        //g_NickName  = PlayerPrefs.GetString("NickName", "SBS영웅");
        //g_BestScore = PlayerPrefs.GetInt("BestScore", 0);
        //g_UserGold  = PlayerPrefs.GetInt("UserGold", 0);
        g_Round     = PlayerPrefs.GetInt("GameRound", 1);

        //string a_MkKey = "";
        //for(int i = 0; i < g_SkillCount.Length; i++)
        //{
        //    a_MkKey = "SkItem_" + i.ToString();
        //    g_SkillCount[i] = PlayerPrefs.GetInt(a_MkKey, 1);
        //}
    }

}//public class GlobalValue 
