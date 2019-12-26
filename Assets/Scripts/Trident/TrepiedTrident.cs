using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrepiedTrident : MonoBehaviour
{
	public float speedRotation = 20f;
	public bool invertSens = false;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(invertSens){
			transform.Rotate(0, speedRotation * Time.deltaTime, 0);
			
			if(transform.eulerAngles.y > 340f){
				Destroy(gameObject);
			}
		}else{
			transform.Rotate(0, -speedRotation * Time.deltaTime, 0);
			
			if(transform.eulerAngles.y > 0f && transform.eulerAngles.y < 20f){
				Destroy(gameObject);
			}
		}
    }
}
