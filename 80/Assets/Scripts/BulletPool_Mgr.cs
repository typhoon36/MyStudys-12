using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool_Mgr : MonoBehaviour
{
    [Header("------- Bullet Pool -------")]
    public GameObject AL_BulletPrefab;  //�Ʊ� �Ѿ� �������� ������ ����
    public GameObject En_BulletPrefab;  //���� �Ѿ� �������� ������ ����
    //�Ʊ� �Ѿ��� �̸� ������ ������ ����Ʈ �ڷ���
    [HideInInspector] public List<Bullet_Ctrl> m_AllyBulletPool = new List<Bullet_Ctrl>();
    //���� �Ѿ��� �̸� ������ ������ ����Ʈ �ڷ���
    [HideInInspector] public List<Bullet_Ctrl> m_EnBulletPool = new List<Bullet_Ctrl>();

    //--- �̱��� ����
    public static BulletPool_Mgr Inst = null;

    private void Awake()
    {
        Inst = this;
    }
    //--- �̱��� ����

    // Start is called before the first frame update
    void Start()
    {
        //--- Ally Bullet Pool
        //�Ѿ��� ������ ������Ʈ Ǯ�� ����
        for(int i = 0; i < 40; i++)
        {
            //�Ѿ� �������� ����
            GameObject a_Bullet = (GameObject)Instantiate(AL_BulletPrefab);
            //������ �Ѿ��� Bullet_Mgr ������ ���ϵ�ȭ �ϱ�
            a_Bullet.transform.SetParent(this.transform);
            //������ �Ѿ��� ��Ȱ��ȭ
            a_Bullet.SetActive(false);
            //������ �Ѿ��� ������Ʈ Ǯ�� �߰�
            m_AllyBulletPool.Add( a_Bullet.GetComponent<Bullet_Ctrl>() );   
        }
        //--- Ally Bullet Pool

        //--- Enemy Bullet Pool
        //�Ѿ��� ������ ������Ʈ Ǯ�� ����
        for(int i = 0; i < 50; i++)
        {
            //�Ѿ� �������� ����
            GameObject a_Bullet = (GameObject)Instantiate(En_BulletPrefab);
            //������ �Ѿ��� Bullet_Mgr ������ ���ϵ�ȭ �ϱ�
            a_Bullet.transform.SetParent(this.transform);
            //������ �Ѿ��� ��Ȱ��ȭ
            a_Bullet.SetActive(false);
            //������ �Ѿ��� ������Ʈ Ǯ�� �߰�
            m_EnBulletPool.Add( a_Bullet.GetComponent<Bullet_Ctrl>() );
        }
        //--- Enemy Bullet Pool
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public Bullet_Ctrl GetALBulletPool()
    {
        //������Ʈ Ǯ�� ó������ ������ ��ȸ
        foreach(Bullet_Ctrl a_BNode in m_AllyBulletPool)
        {
            //��Ȱ��ȭ ���η� ��� ������ Bullet�� �Ǵ�
            if(a_BNode.gameObject.activeSelf == false)
            {
                return a_BNode;
            }
        }

        //����ϰ� �ִ� �Ѿ��� �ϳ��� ������ �������� �Ѿ���� �ȴ�.
        //�׷� ��� �Ѿ��� ���� �ϳ� �� �߰��� ����� �ش�.

        //�Ѿ� �������� ����
        GameObject a_Bullet = (GameObject)Instantiate(AL_BulletPrefab);
        //������ �Ѿ��� this.transform ������ ���ϵ�ȭ �ϱ�
        a_Bullet.transform.SetParent(this.transform);
        //������ �Ѿ��� ��Ȱ��ȭ
        a_Bullet.SetActive(false);
        //������ �Ѿ��� BulletCtrl ������Ʈ ã�ƿ���
        Bullet_Ctrl a_BCtrl = a_Bullet.GetComponent<Bullet_Ctrl>();
        //������ �Ѿ��� ������Ʈ Ǯ�� �߰�
        m_AllyBulletPool.Add(a_BCtrl);

        return a_BCtrl;
    }

    public Bullet_Ctrl GetEnBulletPool()
    {
        //������Ʈ Ǯ�� ó������ ������ ��ȸ
        foreach(Bullet_Ctrl a_BNode in m_EnBulletPool)
        {
            //��Ȱ��ȭ ���η� ��� ������ Bullet�� �Ǵ�
            if(a_BNode.gameObject.activeSelf == false)
            {
                return a_BNode;
            }
        }

        //�Ѿ� �������� ����
        GameObject a_Bullet = (GameObject)Instantiate(En_BulletPrefab);
        //������ �Ѿ��� BulletGroup ������ ���ϵ�ȭ �ϱ�
        a_Bullet.transform.SetParent(this.transform);
        //������ �Ѿ��� ��Ȱ��ȭ
        a_Bullet.SetActive(false);
        //������ �Ѿ��� BulletCtrl ������Ʈ ã�ƿ���
        Bullet_Ctrl a_BCtrl = a_Bullet.GetComponent<Bullet_Ctrl>();
        //������ �Ѿ��� ������Ʈ Ǯ�� �߰�
        m_EnBulletPool.Add(a_BCtrl);

        return a_BCtrl; 
    }
}
