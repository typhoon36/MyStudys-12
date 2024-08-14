using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;

//# https�� �ƴ� �ּ� ����û ���� ����
//�������Ϸ��� ������ �޴����� Project Settings -> Player -> Other Settings -> Configuration -> Allowdownloads over Http üũ

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
    public List<Item> itemArr; /// Json ���Ͽ����� �迭�̸����� ����Ʈ�� �����ϰ� ���
}

public class Game_Mgr : MonoBehaviour
{
    string JsonUrl = "";
    string FileName = "item_data.json";//���ϸ�

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
            // ���ҽ� ���� ��� ����
            string folderPath = Path.Combine(Application.dataPath, "Resources");

            // ��� ���
            Debug.Log("Folder Path: " + folderPath);

            // ������ ����
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
            //## ���ҽ� �������� Json ���� �ε�
            TextAsset JsonText = Resources.Load<TextAsset>("item_data");

            if(JsonText != null)
            {
                //### JsonUtility�� Json ������ �Ľ�
                ItemList itemList = JsonUtility.FromJson<ItemList>(JsonText.text);

                //�� ������ ���� ���
                Print_Txt.text = "";

                for(int i=0; i<itemList.itemArr.Count; i++)
                {
                    Print_Txt.text += "���� ��ȣ(" + itemList.itemArr[i].idx + " ) :";
                    Print_Txt.text += "������ �̸�(" + itemList.itemArr[i].ItemName + " ) :";
                    Print_Txt.text += "������ ����(" + itemList.itemArr[i].ItemLevel + " ) :";
                    Print_Txt.text += "������ ���ݻ�·�(" + float.Parse(itemList.itemArr[i].ItemAttRate).ToString("N2") + " ) :";
                    Print_Txt.text += "������ ����(" + itemList.itemArr[i].ItemPrice + " ) ";
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
        //������ ���� �ٿ�ε� ��û
        UnityWebRequest request = UnityWebRequest.Get(JsonUrl);
        yield return request.SendWebRequest();

        //## �ٿ��� ������ �߻��ߴ°�? üũ
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error Downloading file: " + request.error);
        }
        else
        {
            //## �ٿ�ε� �Ϸ� �� ������ string ����
            string JsonData = request.downloadHandler.text;

            //## ���ҽ� ���Ͽ� ��� ����(�����Ϳ����� ��ȿ)
            string floderPath = Path.Combine(Application.dataPath, "Resources");

            //## Json ������ ���Ϸ� ����
            string filePath = Path.Combine(floderPath, FileName);
            File.WriteAllText(filePath, JsonData);

            //## ����Ƽ �����Ϳ��� ���� ������ ���̽� ����
            UnityEditor.AssetDatabase.Refresh();//������Ʈ ���� ����

            //## �α� ���
            Debug.Log("File Downloaded & save : " + filePath);
        }
    }

   


#endif
}
