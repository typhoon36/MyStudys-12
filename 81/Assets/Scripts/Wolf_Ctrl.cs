using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf_Ctrl : MonoBehaviour
{
    float m_MoveSpeed = 9.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.right * Time.deltaTime * m_MoveSpeed;

        if(CameraResolution.m_ScWMax.x + 0.5f < transform.position.x)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.tag == "Monster")
        {
            Monster_Ctrl a_Enemy = coll.gameObject.GetComponent<Monster_Ctrl>();
            if (a_Enemy != null)
                a_Enemy.TakeDamage(700);
        }
        else if(coll.tag == "EnemyBullet")
        {
            //Destroy(coll.gameObject);
            coll.gameObject.SetActive(false);
        }
    }
}
