using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BamsongiController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        //Shoot(new Vector3(0, 200, 2000));
        Destroy(gameObject, 10.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shoot(Vector3 dir)
    {
        GetComponent<Rigidbody>().AddForce(dir);
    }

    void OnCollisionEnter(Collision coll)
    {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<ParticleSystem>().Play();

        Destroy(gameObject, 4.0f);

        if(coll.gameObject.tag == "Enemy")
        {
            //--- 밤송이 외형 안보이게 정리...
            GetComponent<SphereCollider>().enabled = false;

            MeshRenderer[] a_ChildList =
                        gameObject.GetComponentsInChildren<MeshRenderer>();
            for(int i = 0; i < a_ChildList.Length; i++)
            {
                a_ChildList[i].enabled = false;
            }
            //--- 밤송이 외형 안보이게 정리...

            //--- 보상주기
            Game_Mgr.Inst.AddScore();
            Game_Mgr.Inst.AddGold();
            //--- 보상주기

            Destroy(coll.gameObject); //충돌된 적 캐릭터 즉시 삭제
        }//if(coll.gameObject.tag == "Enemy")

    }//void OnCollisionEnter(Collision coll)
}
