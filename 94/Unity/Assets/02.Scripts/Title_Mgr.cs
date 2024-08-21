using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Title_Mgr : MonoBehaviour
{
    [Header("--- LoginPanel ---")]
    public GameObject m_LoginPanelObj;
    public Button m_LoginBtn = null;
    public Button m_CreateAccOpenBtn = null;
    public InputField IDInputField;
    public InputField PWInputField;

    [Header("--- CreateAccountPanel ---")]
    public GameObject m_CreateAccountPanelObj;
    public InputField New_IDInputField;
    public InputField New_PWInputField;
    public InputField New_NickInputField;
    public Button m_CreateAccBtn = null;
    public Button m_CancleBtn = null;

    [Header("Message")]
    public Text MessageTxt;
    float ShowTime = 0.0f;

    string LoginUrl = "";
    string CreateUrl = "";



    // Start is called before the first frame update
    void Start()
    {
        GlobalValue.LoadGameData();
        //--- LoginPanel
        if (m_LoginBtn != null)
            m_LoginBtn.onClick.AddListener(LoginBtnClick);

        if (m_CreateAccOpenBtn != null)
            m_CreateAccOpenBtn.onClick.AddListener(OpenCreateAccBtn);

        //## 계정생성 패널
        if (m_CancleBtn != null)
            m_CancleBtn.onClick.AddListener(CancleBtnClick);

        if (m_CreateAccBtn != null)
            m_CreateAccBtn.onClick.AddListener(CreateAccBtn);


        //## Url
        LoginUrl = "http://typhoon.dothome.co.kr/Login.php";

        CreateUrl = "http://typhoon.dothome.co.kr/CreateAccount.php";
    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f < ShowTime)
        {
            ShowTime -= Time.deltaTime;
            if (ShowTime <= 0.0f)
            {
                MessageOn("", false);
            }
        }

    }

    void LoginBtnClick()
    {
        // SceneManager.LoadScene("Lobby");

        string a_IdStr = IDInputField.text;
        string a_PwStr = PWInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();

        if (string.IsNullOrEmpty(a_IdStr) == true || string.IsNullOrEmpty(a_PwStr) == true)
        {
            MessageOn("Id,Pw는 정확히 채워주세요.");
            return;
        }

        //## 예외처리
        //아이디
        if (!(3 <= a_IdStr.Length && a_IdStr.Length <= 20))
        {
            MessageOn("Id는 3~20자리로 입력해주세요.");
            return;
        }

        //비밀번호
        if (!(4 <= a_PwStr.Length && a_PwStr.Length <= 20))
        {
            MessageOn("Pw는 4~20자리로 입력해주세요.");
            return;
        }

        //웹통신인지라 코루틴 함수 호출
        StartCoroutine(LoginCo(a_IdStr, a_PwStr));

    }

    IEnumerator LoginCo(string a_IdStr, string a_PwStr)
    {
        //## 웹통신
        WWWForm form = new WWWForm();

        form.AddField("Input_id", a_IdStr, System.Text.Encoding.UTF8);
        form.AddField("Input_pw", a_PwStr);

        UnityWebRequest a_Www = UnityWebRequest.Post(LoginUrl, form);

        //## 요청을 보내고 대기
        yield return a_Www.SendWebRequest();

        //## 오류
        if (a_Www.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_Www.downloadHandler.data);

            a_Www.Dispose();

            if (sz.Contains("Id does not exitst.")== true)
            {
                MessageOn("아이디가 존재하지않습니다.");
                yield break;
            }

            if (sz.Contains("Password does not Match.") == true)
            {
                MessageOn("비밀번호가 틀렸습니다.");
                yield break;
            }

            if (sz.Contains("Login_Success.") == false)
            {
                MessageOn("로그인 실패!알수없는 오류가 발생했습니다.잠시후 다시 시도해주세요.");
                yield break;
            }

            if (sz.Contains("{\"") == false)
            {
                MessageOn("서버가 응답없음." + sz);
                yield break;
            }

            GlobalValue.g_Unique_ID = a_IdStr;


            string a_GetStr = sz.Substring(sz.IndexOf("{\""));
            a_GetStr = a_GetStr.Replace("\nLogin_Success.", "");

            svRespon respon = JsonUtility.FromJson<svRespon>(a_GetStr);

            GlobalValue.g_NickName = respon.nick_name;
            GlobalValue.g_BestScore = respon.best_score;
            GlobalValue.g_UserGold = respon.game_gold;

            //## 층정보 로딩
            if (string.IsNullOrEmpty(respon.floor_info) == false)
            {
                Floor_Info a_FloorInfo = JsonUtility.FromJson<Floor_Info>(respon.floor_info);
                if (a_FloorInfo != null)
                {
                    GlobalValue.g_BestFloor = a_FloorInfo.BestFloor;
                    GlobalValue.g_CurFloorNum = a_FloorInfo.CurFloor;
                }
            }



            if (string.IsNullOrEmpty(respon.info)==false)
            {
                ItemList a_ItList = JsonUtility.FromJson<ItemList>(respon.info);

                //## 아이템리스트 파싱
                if (a_ItList != null && a_ItList.SkList != null)
                {
                    for (int i = 0; i < a_ItList.SkList.Length; i++)
                    {
                        if (GlobalValue.g_SkillCount.Length <= i) continue;

                        GlobalValue.g_SkillCount[i] = a_ItList.SkList[i];
                    }
                }
            }
            


            //## 로비로 이동
            SceneManager.LoadScene("Lobby");


        }
        else
        {
            MessageOn(a_Www.error);


        }


    }






    void MessageOn(string a_Msg = "", bool isOn = true)
    {
        if (isOn == true)
        {
            MessageTxt.text = a_Msg;
            MessageTxt.gameObject.SetActive(true);
            ShowTime = 5.0f;
        }
        else
        {
            MessageTxt.text = "";
            MessageTxt.gameObject.SetActive(false);
        }
    }


    void OpenCreateAccBtn()
    {
        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(false);

        if (m_CreateAccountPanelObj != null)
            m_CreateAccountPanelObj.SetActive(true);
    }

    void CancleBtnClick()
    {
        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(true);

        if (m_CreateAccountPanelObj != null)
            m_CreateAccountPanelObj.SetActive(false);
    }

    void CreateAccBtn()
    {
        string a_IdStr = New_IDInputField.text;
        string a_PwStr = New_PWInputField.text;
        string a_NickStr = New_NickInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();
        a_NickStr = a_NickStr.Trim();

        //## 예외처리
        if (string.IsNullOrEmpty(a_IdStr) == true ||
            string.IsNullOrEmpty(a_PwStr) == true ||
            string.IsNullOrEmpty(a_NickStr) == true)
        {
            MessageOn("Id,Pw,별명은 정확히 채워주세요.");
            return;
        }

        //아이디
        if (!(3 <= a_IdStr.Length && a_IdStr.Length <= 20))
        {
            MessageOn("Id는 3~20자리로 입력해주세요.");
            return;
        }

        //비밀번호
        if (!(4 <= a_PwStr.Length && a_PwStr.Length <= 20))
        {
            MessageOn("Pw는 4~20자리로 입력해주세요.");
            return;
        }

        //닉네임
        if (!(2 <= a_NickStr.Length && a_NickStr.Length <= 10))
        {
            MessageOn("NickName은  2~10자리로 입력해주세요.");
            return;
        }


        //웹통신인지라 코루틴 함수 호출
        StartCoroutine(CreateAccCo(a_IdStr, a_PwStr, a_NickStr));



    }

    IEnumerator CreateAccCo(string a_IdStr, string a_PwStr, string a_NickStr)
    {
        //## 웹통신
        WWWForm form = new WWWForm();

        //## 서버로 전송할 데이터
        form.AddField("Input_id", a_IdStr, System.Text.Encoding.UTF8);
        form.AddField("Input_pw", a_PwStr);
        form.AddField("Input_nick", a_NickStr, System.Text.Encoding.UTF8);

        UnityWebRequest a_Www = UnityWebRequest.Post(CreateUrl, form);
        //## 요청을 보내고 대기
        yield return a_Www.SendWebRequest();

        //## 오류
        if (a_Www.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_Www.downloadHandler.data);
            a_Www.Dispose();

            if (sz.Contains("Create Success.")==true)
                MessageOn("가입 성공!");
            else if(sz.Contains("Id is already exist.") == true)
                MessageOn("이미 존재하는 아이디입니다.");
            else if(sz.Contains("Nick is already exist.") == true)
                MessageOn("이미 존재하는 닉네임입니다.");
            else
                MessageOn(sz);
        }
        else
        {
            MessageOn("가입 실패!" + a_Www.error);
            a_Www.Dispose();
        }
    }


}
