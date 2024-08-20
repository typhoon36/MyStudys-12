using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCtrl : MonoBehaviour
{
    //����ũ ��ƼŬ ������ ������ ����
    public GameObject sparkEffect;

    [HideInInspector] public bool m_IsColl = false;
    Material m_WallMaterial = null;

    // Start is called before the first frame update
    void Start()
    {
        m_WallMaterial = gameObject.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //�浹�� ������ �� �߻��ϴ� �̺�Ʈ
    void OnCollisionEnter(Collision coll)
    {
        //�浹�� ���ӿ�����Ʈ�� �±װ� ��
        if(coll.collider.tag == "BULLET")
        {
            //����ũ ��ƼŬ�� �������� ����
            GameObject spark = Instantiate(sparkEffect, coll.transform.position, Quaternion.identity);

            //ParticleSystem ������Ʈ�� ����ð�(duration)�� ���� �� ���� ó��
            Destroy(spark, spark.GetComponent<ParticleSystem>().main.duration + 0.2f);

            //�浹�� ���ӿ�����Ʈ ����
            Destroy(coll.gameObject);
        }
    }

    public void WallAlphaOnOff(bool isOn = true)
    {
        if (m_WallMaterial == null)
            return;

        if (isOn == true)  //����ȭ
        {
            m_WallMaterial.SetFloat("_Mode", 3);  //Transparent
            m_WallMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            m_WallMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m_WallMaterial.SetInt("_ZWrite", 0);
            m_WallMaterial.DisableKeyword("_ALPHATEST_ON");
            m_WallMaterial.DisableKeyword("_ALPHABLEND_ON");
            m_WallMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            m_WallMaterial.renderQueue = 3000;
            m_WallMaterial.color = new Color(1, 1, 1, 0.3f);
        }
        else  //������ȭ
        {
            m_WallMaterial.SetFloat("_Mode", 0);  //Opaque
            m_WallMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            m_WallMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            m_WallMaterial.SetInt("_ZWrite", 1);
            m_WallMaterial.DisableKeyword("_ALPHATEST_ON");
            m_WallMaterial.DisableKeyword("_ALPHABLEND_ON");
            m_WallMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m_WallMaterial.renderQueue = -1;
            m_WallMaterial.color = new Color(1, 1, 1, 1);
        }
    }//public void WallAlphaOnOff(bool isOn = true)
}
