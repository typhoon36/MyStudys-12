using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelCtrl : MonoBehaviour
{
    //���� ȿ�� ��ƼŬ ���� ����
    public GameObject expEffect;
    //�������� ������ �ؽ�ó �迭
    public Texture[] textures;

    //�Ѿ� ���� ȸ���� ������ų ����
    private int hitCount = 0;
    float timer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        int idx = Random.Range(0, textures.Length);
        GetComponentInChildren<MeshRenderer>().material.mainTexture = textures[idx];
    }

    // Update is called once per frame
    void Update()
    {
        if(0.0f < timer)
        {
            timer -= Time.deltaTime;
            if(timer <= 0.0f)
            {
                Rigidbody rbody = this.GetComponent<Rigidbody>();
                if (rbody != null)
                    rbody.mass = 20.0f;
            }
        }//if(0.0f < timer)

    }//void Update()

    //�浹 �� �߻��ϴ� �ݹ� �Լ�(CallBack Function)
    void OnCollisionEnter(Collision coll)
    {
        if(coll.collider.tag == "BULLET")
        {
            //�浹�� �Ѿ� ����
            Destroy(coll.gameObject);

            //�Ѿ� ���� Ƚ���� ������Ű�� 3ȸ �̻��̸� ���� ó��
            if (++hitCount >= 3)
                ExpBarrel();
        }
    }//void OnCollisionEnter(Collision coll)

    //�巳�� ���߽�ų �Լ�
    void ExpBarrel()
    {
        //���� ȿ�� ��ƼŬ ����
        GameObject explosion = Instantiate(expEffect, transform.position, Quaternion.identity);
        Destroy(explosion, explosion.GetComponentInChildren<ParticleSystem>().main.duration + 2.0f);

        //������ ������ �߽����� 10.0f �ݰ� ���� ���� �ִ� Collider��ü ����
        Collider[] colls = Physics.OverlapSphere(transform.position, 10.0f);
        BarrelCtrl a_Barrel = null;
        Rigidbody rbody = null;
        //������ Collider ��ü�� ���߷� ����
        foreach(Collider coll in colls)
        {
            a_Barrel = coll.GetComponent<BarrelCtrl>();
            if (a_Barrel == null)
                continue;

            rbody = coll.GetComponent<Rigidbody>();
            if(rbody != null)
            {
                rbody.mass = 1.0f;
                rbody.AddExplosionForce(1000.0f, transform.position, 10.0f, 300.0f);
                a_Barrel.timer = 0.1f;
            }
        }

        //5�� �Ŀ� �巳�� ����
        Destroy(gameObject, 5.0f);
    }
}
