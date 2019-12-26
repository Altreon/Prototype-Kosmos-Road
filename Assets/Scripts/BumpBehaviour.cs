using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumpBehaviour : MonoBehaviour
{ 
    public float BumperForce = 0f;
    public GameObject Hovercraft;

    private Vector3 _direction;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == Hovercraft)
        {
            _direction = collision.transform.position - this.transform.position;
            collision.rigidbody.AddForce(_direction.normalized * BumperForce, ForceMode.VelocityChange);
        }
    }
}
