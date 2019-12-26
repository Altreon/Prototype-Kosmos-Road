using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cam : MonoBehaviour
{
	public Transform t;
	private Rigidbody rb;
	
    // Start is called before the first frame update
    void Start()
    {
		rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //transform.position = Vector3.Lerp(transform.position, t.position - transform.forward * 5f, 10f * Time.deltaTime);
		//transform.position = Vector3.Lerp(rb.position, tRb.position - transform.forward * 5f, 10 * Time.deltaTime);
		rb.MovePosition(Vector3.Lerp(transform.position, t.position - transform.forward * 5f, 10f * Time.deltaTime));
		
    }
}
