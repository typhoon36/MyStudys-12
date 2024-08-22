using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {
    public GameObject ShieldBody;
    public GameObject Ring1;
    public GameObject Ring2;
    public float RotateSpeed = 10;
    public float ShieldOutShineTimeSpace = 2;

    float curTime;
    bool isShine = false;
    Material shieldMat;
    float offsetY = 1;

    float m_CurSize = 5.0f;
    float m_MaxSize = 100.0f;

	// Use this for initialization
	void Start () {
        Destroy(gameObject, 20.0f);

        shieldMat = ShieldBody.GetComponent<MeshRenderer>().material;
        transform.localScale = new Vector3(m_CurSize, m_CurSize, m_CurSize);
    }
	
	// Update is called once per frame
	void Update () {
        Ring1.transform.Rotate(Vector3.right, Time.deltaTime * RotateSpeed);
        Ring2.transform.Rotate(Vector3.forward, Time.deltaTime * RotateSpeed);


        if (isShine)
        {
            offsetY = Mathf.Lerp(offsetY, -1, 0.02f);
            shieldMat.SetFloat("_ScanningOffsetY", offsetY);
            if (offsetY + 1 < 0.01)
            {
                offsetY = 1;
                 isShine = false;
                curTime = 0;
                
            }
        }
        else
        {
            //curTime += Time.deltaTime;
            //if (curTime >= ShieldOutShineTimeSpace)
            //{
            //    isShine = true;
            //}
        }

        if(m_CurSize < m_MaxSize)
        {
            m_CurSize += Time.deltaTime * 100.0f;
            transform.localScale = new Vector3(m_CurSize, m_CurSize, m_CurSize);
        }

    }//void Update ()
}
