using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sky_BG_Ctrl : MonoBehaviour
{
    float ScrollSpeed = 0.2f;
    float Offset = 0.0f;

    SpriteRenderer m_Render;

    // Start is called before the first frame update
    void Start()
    {
        m_Render = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Offset += Time.deltaTime * ScrollSpeed;
        if (10000.0f < Offset)
            Offset = Offset - 10000.0f;
        m_Render.material.mainTextureOffset = new Vector2(Offset, 0);
    }
}
