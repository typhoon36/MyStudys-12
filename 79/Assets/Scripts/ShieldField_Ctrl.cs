using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldField_Ctrl : MonoBehaviour
{
    HeroCtrl m_RefHero;

    // Start is called before the first frame update
    void Start()
    {
        m_RefHero = GameObject.FindObjectOfType<HeroCtrl>();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_RefHero != null)
            transform.position = m_RefHero.transform.position;
    }
}
