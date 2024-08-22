using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BamsongiGenerator : MonoBehaviour
{
    public GameObject bamsogiPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && Game_Mgr.IsPointerOverUIObject() == false)
        {
            GameObject bamsongi = Instantiate(bamsogiPrefab);

            bamsongi.transform.position =
                        Camera.main.transform.position + Camera.main.transform.forward * 1.0f;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 worldDir = ray.direction;
            bamsongi.GetComponent<BamsongiController>().Shoot(worldDir.normalized * 3500);
        }
    }
}
