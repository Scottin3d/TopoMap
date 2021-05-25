using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeight : MonoBehaviour
{
    public GameObject PC_Player;
    public GameObject VR_Player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float Get_PC_CurHeight()
    {
        float H = PC_Player.transform.position.y;
        return H;
    }

    public float Get_VR_CurHeight()
    {
        float H = VR_Player.transform.position.y;
        return H;
    }
}
