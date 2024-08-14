using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;

//# https가 아닌 주소 웹요청 차단 방지
//사용허용하려면 에디터 메뉴에서 Project Settings -> Player -> Other Settings -> Configuration -> Allowdownloads over Http 체크

[System.Serializable]
public class Item
{
    public int idx;
    public string ItemName;
    public string ItemLevel;
    public string ItemAttRate;
    public int ItemPrice;
    public string info;
}

[System.Serializable]
public class ItemList
{
    public List<Item> itemArr; /// Json 파일에서의 배열이름으로 리스트를 동일하게 사용
}

public class Game_Mgr : MonoBehaviour
{
    string JsonUrl = "";
    string FileName = "item_data.json";//파일명

    public Text Print_Txt;

    // Start is called before the first frame update
    void Start()
    {
        JsonUrl = "http://typhoon.dothome.co.kr/item_data.json";
    }

    // Update is called once per frame
    void Update()
    {

    }

#if UNITY_EDITOR
    void OnGUI()
    {
        if (GUI.Button(new Rect(100, 50, 500, 63),
                "<size=32>" + "Download Json to Resources" + "</size>") == true)
        {
            // 리소스 폴더 경로 생성
            string folderPath = Path.Combine(Application.dataPath, "Resources");

            // 경로 출력
            Debug.Log("Folder Path: " + folderPath);

            // 없으면 생성
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                //UnityEditor.AssetDatabase.Refresh();
            }

            StartCoroutine(DownloadJsonFile());
        }
        if(GUI.Button(new Rect(100,150,500,63),
            "<size=32>" + "Json File Load" + "</size>") == true)
        {
            //## 리소스 폴더에서 Json 파일 로드
            TextAsset JsonText = Resources.Load<TextAsset>("item_data");

            if(JsonText != null)
            {
                //### JsonUtility로 Json 파일을 파싱
                ItemList itemList = JsonUtility.FromJson<ItemList>(JsonText.text);

                //각 아이템 정보 출력
                Print_Txt.text = "";

                for(int i=0; i<itemList.itemArr.Count; i++)
                {
                    Print_Txt.text += "고유 번호(" + itemList.itemArr[i].idx + " ) :";
                    Print_Txt.text += "아이템 이름(" + itemList.itemArr[i].ItemName + " ) :";
                    Print_Txt.text += "아이템 레벨(" + itemList.itemArr[i].ItemLevel + " ) :";
                    Print_Txt.text += "아이템 공격상승률(" + float.Parse(itemList.itemArr[i].ItemAttRate).ToString("N2") + " ) :";
                    Print_Txt.text += "아이템 가격(" + itemList.itemArr[i].ItemPrice + " ) ";
                    Print_Txt.text += "\n";
                }



            }

            else
            {
                Debug.LogError("Json File Load Error" + FileName);
            }
        }
    }
    
    IEnumerator DownloadJsonFile()
    {
        //웹에서 파일 다운로드 요청
        UnityWebRequest request = UnityWebRequest.Get(JsonUrl);
        yield return request.SendWebRequest();

        //## 다운중 에러가 발생했는가? 체크
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error Downloading file: " + request.error);
        }
        else
        {
            //## 다운로드 완료 후 데이터 string 저장
            string JsonData = request.downloadHandler.text;

            //## 리소스 파일에 경로 지정(에디터에서만 유효)
            string floderPath = Path.Combine(Application.dataPath, "Resources");

            //## Json 데이터 파일로 저장
            string filePath = Path.Combine(floderPath, FileName);
            File.WriteAllText(filePath, JsonData);

            //## 유니티 에디터에서 에셋 데이터 베이스 갱신
            UnityEditor.AssetDatabase.Refresh();//프로젝트 폴더 갱신

            //## 로그 출력
            Debug.Log("File Downloaded & save : " + filePath);
        }
    }

   


#endif
}
