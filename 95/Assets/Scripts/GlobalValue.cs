using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Skill_0 = 0,        //���� ���� ��ų
    Skill_1,            //�ν��� 
    Skill_2,            //���
    SkCount
}

public class GlobalValue 
{
    public static string g_Unique_ID = "";      //������ ������ȣ 

    public static string g_NickName = "";       //������ ����
    public static int g_BestScore = 0;          //��������
    public static int g_UserGold = 0;           //���ӸӴ�
    public static int g_Round    = 1;           //����
    public static int g_Exp = 0;                //������ ����ġ Experience
    public static int g_Level = 0;              //������ ����

    public static int[] g_SkillCount = new int[3];  //������ ������

    public static void LoadGameData()
    {
        //PlayerPrefs.DeleteAll();

        //PlayerPrefs.SetInt("UserGold", 99999);

        //g_NickName  = PlayerPrefs.GetString("NickName", "SBS����");
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
