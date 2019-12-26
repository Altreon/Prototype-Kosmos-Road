using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jump : MonoBehaviour
{
    private Rigidbody hovercraft_rb;
    public float m_ForceyString = 0.1f;
    Vector3 m_NewForce;


    // Start is called before the first frame update
    void Start()
    {
        hovercraft_rb = GetComponent<Rigidbody>();
        m_NewForce = new Vector3(0, m_ForceyString, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {   

        if (Input.GetButton("jump"))
        {
            hovercraft_rb.AddForce(m_NewForce, ForceMode.Impulse);

        }
    }
}
